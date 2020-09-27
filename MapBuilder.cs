using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sunbird.Core;
using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;

namespace SunbirdMB
{
    public class MapBuilder : State
    {
        private readonly string saveFilePath;

        public static readonly XmlSerializer MapBuilderSerializer = Serializer.CreateNew(typeof(MapBuilder));

        /// <summary>
        /// For optimization reasons, create and assign value to this field during an update() loop so it is ready to be used in the draw() loop.
        /// </summary>
        private Dictionary<Coord, List<Sprite>> ShadowDict = new Dictionary<Coord, List<Sprite>>();

        public XDictionary<int, SpriteList<Sprite>> LayerMap { get; set; } = new XDictionary<int, SpriteList<Sprite>>();

        [XmlIgnore]
        public List<Sprite> Overlay { get; set; } = new List<Sprite>();

        /// <summary>
        /// When adding or removing sprites from the Overlay during runtime, move them here first. 
        /// This ensures that addition or removal occurs before enumeration over Overlay begins.
        /// </summary>
        [XmlIgnore]
        public List<KeyValuePair<Sprite, DeferAction>> DeferredOverlay { get; set; } = new List<KeyValuePair<Sprite, DeferAction>>();

        public bool InFocus;
        public bool IsLoading { get; set; }
        public int Altitude { get; set; }
        public GhostMarker GhostMarker { get; set; }
        public Cube CubePreview { get; set; }
        public Deco DecoPreview { get; set; }
        public Authorization Authorization { get; set; }
        public BuildMode BuildMode { get; set; } = BuildMode._Cube;

        public static string ClickedSpriteName = string.Empty;

        [XmlIgnore]
        internal SunbirdMBGame SunbirdMBGame { get; set; }

        public MapBuilder()
        {

        }

        public MapBuilder(SunbirdMBGame mainGame, GraphicsDevice graphicsDevice, ContentManager content, string path) 
            : base(mainGame, graphicsDevice, content)
        {
            SunbirdMBGame = mainGame;
            this.saveFilePath = path;
            var appPath = Assembly.GetExecutingAssembly().Location;
            var appDirectory = appPath.TrimEnd(Path.GetFileName(appPath));
            Debug.Assert(appDirectory == @"D:\SunbirdMB\bin\Debug\");
            var svp = Path.Combine(appDirectory, path);
            Debug.Assert(svp == @"D:\SunbirdMB\bin\Debug\MapBuilderSave.xml");
            if (mainGame.cleanLoad || !File.Exists(svp))
            {
                CreateContent();
            }
            else
            {
                LoadContentFromFile();
            }
            CubeFactory.IsRandomTop = true;
        }

        private void CreateContent()
        {
            IsLoading = true;
            // Create first layer at 0 Altitude.
            LayerMap.Add(Altitude, new SpriteList<Sprite>());

            IsLoading = false;
            MainGame.CurrentState = this;

            Peripherals.ScrollWheelUp += Peripherals_ScrollWheelUp;
            Peripherals.ScrollWheelDown += Peripherals_ScrollWheelDown;

        }

        internal void CreateGhostMarker()
        {

            CubePreview = CubeFactory.CreateCurrentCube(MainGame, Coord.Zero, Coord.Zero, 0);
            // Instantiated BuildMode is _Cube. GhostMarker needs CubePreview to exist to morph so we must create it after the latter (which belongs to the overlay).
            GhostMarker = new GhostMarker(MainGame, SpriteSheet.CreateNew(MainGame, "Temp/TopFaceSelectionMarker")) { DrawPriority = 1 };
            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
            LayerMap[Altitude].Add(GhostMarker);
        }

        private void LoadContentFromFile()
        {
            IsLoading = true;
            // Most time is spent here...
            var XmlData = Serializer.ReadXML<MapBuilder>(MapBuilderSerializer, saveFilePath);
            Altitude = XmlData.Altitude;
            BuildMode = XmlData.BuildMode;
            LayerMap = XmlData.LayerMap;
            foreach (var layer in LayerMap)
            {
                foreach (var sprite in layer.Value)
                {
                    if (sprite is GhostMarker)
                    {
                        GhostMarker = sprite as GhostMarker;
                    }
                    sprite.LoadContent(MainGame, GraphicsDevice, Content);
                }
            }

            IsLoading = false;
            MainGame.CurrentState = this;

            Peripherals.ScrollWheelUp += Peripherals_ScrollWheelUp;
            Peripherals.ScrollWheelDown += Peripherals_ScrollWheelDown;
        }

        // Should this have both generic and state specific components?
        private void Peripherals_ScrollWheelDown(object sender, EventArgs e)
        {
            if (MainGame.IsActive == true)
            {
                if (Peripherals.KeyPressed(Keys.LeftControl) && World.Zoom > 1)
                {
                    World.Zoom--;
                    World.ReconstructTopFaceArea();
                    if (MainGame.Camera.CurrentMode == CameraMode.Drag)
                    {
                        MainGame.Camera.DragTransform = MainGame.Camera.CreateDragTransform();
                    }
                }
                else if (!Peripherals.KeyPressed(Keys.LeftControl) && Authorization == Authorization.Builder)
                {
                    Altitude--;
                    if (LayerMap.ContainsKey(Altitude) == false)
                    {
                        LayerMap.Add(Altitude, new SpriteList<Sprite>());
                    }
                }
            }

        }

        private void Peripherals_ScrollWheelUp(object sender, EventArgs e)
        {
            if (MainGame.IsActive == true)
            {
                if (Peripherals.KeyPressed(Keys.LeftControl) && World.Zoom < 5)
                {
                    World.Zoom++;
                    World.ReconstructTopFaceArea();
                    if (MainGame.Camera.CurrentMode == CameraMode.Drag)
                    {
                        MainGame.Camera.DragTransform = MainGame.Camera.CreateDragTransform();
                    }
                }
                else if (!Peripherals.KeyPressed(Keys.LeftControl) && Authorization == Authorization.Builder)
                {
                    Altitude++;
                    if (LayerMap.ContainsKey(Altitude) == false)
                    {
                        LayerMap.Add(Altitude, new SpriteList<Sprite>());
                    }
                }
            }
        }

        public override void SaveAndSerialize()
        {
            Serializer.WriteXML<MapBuilder>(MapBuilderSerializer, this, saveFilePath);
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsLoading)
            {
                // Defined with respect to current mouse position.
                var relativeTopFaceCoords = World.TopFace_PointToRelativeCoord(MainGame, Altitude);
                var topFaceCoords = World.TopFace_PointToCoord(MainGame);

                // User input actions.
                if (Peripherals.KeyTapped(Keys.Q) && MainGame.IsActive)
                {
                    var i = (int)Authorization + 1;
                    if (i >= Enum.GetNames(typeof(Authorization)).Length) { i = 0; }
                    Authorization = (Authorization)(i);
                }

                if (Peripherals.KeyTapped(Keys.L) && MainGame.IsActive)
                {
                    $"Window Position = {Peripherals.GetMouseWindowPosition(MainGame)}".Log();
                    $"World Position = {Peripherals.GetMouseWorldPosition(MainGame, MainGame.Camera)}".Log();
                    $"Altitude = {Altitude}".Log();
                    var scale = Vector3.Zero;
                    var rotation = Quaternion.Identity;
                    var translation = Vector3.Zero;
                    SunbirdMBGame.Camera.CurrentTransform.Decompose(out scale, out rotation, out translation);
                    $"{scale} {rotation} {translation }".Log();
                }

                if (Authorization == Authorization.Builder)
                {
                    if (Peripherals.LeftButtonPressed() && MainGame.IsActive)
                    {
                        if (BuildMode == BuildMode._Cube)
                        {
                            var cube = CubeFactory.CreateCurrentCube(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                            LayerMap[Altitude].AddCheck(cube, Altitude);
                        }
                    }

                    if (Peripherals.RightButtonPressed() && MainGame.IsActive)
                    {
                        //var rect = new Rectangle(Peripherals.GetScaledMouseWorldPosition(MainGame.Camera), new Point(200,200));
                        if (BuildMode == BuildMode._Cube)
                        {
                            for (int i = 0; i < LayerMap[Altitude].Count(); i++)
                            {
                                var sprite = LayerMap[Altitude][i];
                                if (sprite is Cube && sprite.Coords == relativeTopFaceCoords)
                                {
                                    LayerMap[Altitude].RemoveCheck(sprite, Altitude); i--;
                                }
                                //if (sprite is Cube && rect.Contains(sprite.Position.ToPoint()))
                                //{
                                //    LayerMap[Altitude].RemoveCheck(sprite, Altitude); i--;
                                //}
                            }
                        }
                    }
                }

                // Ghost marker management.
                if (Authorization == Authorization.Builder)
                {
                    GhostMarker.Altitude = Altitude;
                    GhostMarker.Coords = relativeTopFaceCoords;
                }
                else if (Authorization == Authorization.None)
                {
                    var l = LayerMap.Keys.ToList();
                    l.Sort();
                    l.Reverse();

                    foreach (var key in l)
                    {
                        var targetedCoord = World.GetRelativeCoord(topFaceCoords, key);
                        if (LayerMap[key].OccupiedCoords.Contains(targetedCoord))
                        {
                            Altitude = key;
                            GhostMarker.Altitude = Altitude;
                            break;
                        }
                    }
                    GhostMarker.Coords = World.TopFace_PointToRelativeCoord(MainGame, Altitude);
                }

                GhostMarker.Position = World.TopFace_CoordToLocalOrigin(topFaceCoords);

                if (LayerMap[Altitude].OccupiedCoords.Contains(relativeTopFaceCoords) || Authorization == Authorization.None)
                {
                    GhostMarker.DrawDefaultMarker = true;
                }
                else if (Authorization == Authorization.Builder)
                {
                    GhostMarker.DrawDefaultMarker = false;
                }

                if (LayerMap[Altitude].OccupiedCoords.Contains(relativeTopFaceCoords))
                {
                    GhostMarker.DrawPriority = 1;
                }
                else if (Authorization == Authorization.Builder)
                {
                    GhostMarker.DrawPriority = 0;
                }
                else if (Authorization == Authorization.None)
                {
                    GhostMarker.DrawPriority = -1000;
                }

                GhostMarker.IsHidden = !MainGame.IsMouseOver;

                #region Pre Loop

                // Rearrange sprites into their correct altitude layer.
                var altitudeList = LayerMap.Keys.ToList();
                altitudeList.Sort();

                foreach (var altitude in altitudeList)
                {
                    for (int i = 0; i < LayerMap[altitude].Count(); i++)
                    {
                        var sprite = LayerMap[altitude][i];
                        if (sprite.Altitude != altitude)
                        {
                            LayerMap[altitude].Remove(sprite); i--;
                            if (!LayerMap.ContainsKey(sprite.Altitude))
                            {
                                LayerMap.Add(sprite.Altitude, new SpriteList<Sprite>() { sprite });
                            }
                            else
                            {
                                if (!(sprite is IWorldObject))
                                {
                                    LayerMap[sprite.Altitude].Add(sprite);
                                }
                                else
                                {
                                    throw new NotImplementedException("Cube/Deco trying to move between layers, is this correct? Use AddCheck if so.");
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Main Loop

                ShadowDict = new Dictionary<Coord, List<Sprite>>();

                foreach (var sprite in World.Sort(LayerMap))
                {
                    sprite.Update(gameTime);
                    // Create dict: Key = coords, Value = column (list) of sprites with same coord different altitude.
                    if (ShadowDict.ContainsKey(sprite.Coords) == false)
                    {
                        ShadowDict.Add(sprite.Coords, new List<Sprite>() { });
                    }
                    else
                    {
                        ShadowDict[sprite.Coords].Add(sprite);
                    }
                }

                #endregion
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsLoading)
            {
                #region Main Loop

                // Draw sorted sprites;
                foreach (var sprite in World.Sort(LayerMap))
                {
                    // Game
                    if (Altitude != sprite.Altitude && sprite is IWorldObject && Authorization == Authorization.Builder)
                    {
                        sprite.Alpha = 0.1f;
                        sprite.Draw(gameTime, spriteBatch);
                    }
                    else if ((Altitude == sprite.Altitude || Authorization == Authorization.None) && sprite is IWorldObject)
                    {
                        sprite.Alpha = 1f;
                        sprite.Draw(gameTime, spriteBatch);
                    }
                    else
                    {
                        sprite.Draw(gameTime, spriteBatch);
                    }                  
                }
                #endregion
            }
        }

        public override void DrawOverlay(GameTime gameTime, SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }

    }
}
