using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Gui;
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

        public XDictionary<int, XDictionary<Coord, HashSet<Coord>>> WalkableTileTable { get; set; } = new XDictionary<int, XDictionary<Coord, HashSet<Coord>>>();

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
        public Player Player { get; set; }
        public Sprite ClickAnimation { get; set; }

        [XmlIgnore]
        public Authorization Authorization 
        { 
            get { return MainToolbarViewModel.Authorization; }
            set { MainToolbarViewModel.Authorization = value; } 
        }

        [XmlIgnore]
        public BuildMode BuildMode
        {
            get { return MainToolbarViewModel.BuildMode; }
            set { MainToolbarViewModel.BuildMode = value; }
        }

        public static GhostMarker GhostMarker { get; set; }

        public static string ClickedSpriteName = string.Empty;

        [XmlIgnore]
        internal SunbirdMBGame SunbirdMBGame { get; set; }

        public MapBuilder()
        {

        }

        public MapBuilder(SunbirdMBGame mainGame, string path) : base(mainGame)
        {
            SunbirdMBGame = mainGame;
            this.saveFilePath = path;
            var sfp = Path.Combine(UriHelper.AppDirectory, path);
            Debug.Assert(sfp == @"D:\SunbirdMB\bin\Debug\MapBuilderSave.xml");
            if (mainGame.cleanLoad || !File.Exists(sfp))
            {
                CreateContent();
            }
            else
            {
                LoadContentFromFile();
            }
        }

        private void CreateContent()
        {
            IsLoading = true;
            // Create first layer at 0 Altitude.
            LayerMap.Add(Altitude, new SpriteList<Sprite>());

            // Create the player and add to this layer.
            var playerSheet = SpriteSheet.CreateNew(MainGame, "Temp/PirateGirlSheet", 1, 16);
            var playerAnimArgs = new AnimArgs(9, 1, 0.2f, AnimationState.None);
            Player = new Player(MainGame, playerSheet, playerAnimArgs);
            // Should this be Add or AddCheck?
            LayerMap[Altitude].Add(Player);

            // Instantiate ghost marker with its default texture and add it to the layer map. Image is null at this point, but will be set
            // by CubeDesignerViewModel.OnMainGameLoaded() as soon as SunbirdMBGame's constructor resolves.
            GhostMarker = new GhostMarker(MainGame, SpriteSheet.CreateNew(MainGame, "Temp/TopFaceSelectionMarker")) { DrawPriority = 1 };
            LayerMap[Altitude].Add(GhostMarker);

            ClickAnimation = new Sprite(MainGame, SpriteSheet.CreateNew(MainGame, "Temp/WalkTargetAnimation")) { Alpha = 0.8f };
            LayerMap[Altitude].Add(ClickAnimation);

            IsLoading = false;
            MainGame.CurrentState = this;

            Peripherals.ScrollWheelUp += Peripherals_ScrollWheelUp;
            Peripherals.ScrollWheelDown += Peripherals_ScrollWheelDown;

        }

        private void LoadContentFromFile()
        {
            IsLoading = true;
            // Most time is spent here...
            var XmlData = Serializer.ReadXML<MapBuilder>(MapBuilderSerializer, saveFilePath);
            Altitude = XmlData.Altitude;
            WalkableTileTable = XmlData.WalkableTileTable;
            LayerMap = XmlData.LayerMap;
            bool createNewGhostMarker = false;
            foreach (var layer in LayerMap)
            {
                for (int i = 0; i < layer.Value.Count(); i++)
                {
                    var sprite = layer.Value[i];
                    if (sprite is Player player)
                    {
                        Player = player;
                    }
                    else if (sprite is GhostMarker ghostMarker)
                    {
                        // The ghost marker should be somewhere in the serialized collection of sprites.
                        // Find it and set it as our ghost marker instead of creating a new one.
                        GhostMarker = ghostMarker;
                    }
                    else if (sprite.Animator?.SpriteSheet?.TexturePath == "Temp/WalkTargetAnimation")
                    {
                        ClickAnimation = sprite;
                    }

                    try
                    {
                        // Load content can fail if the content file for the sprite no longer exists.
                        sprite.LoadContent(MainGame);
                    }
                    catch (Exception e)
                    {
                        e.Message.Log();
                        if (sprite is GhostMarker ghostMarker)
                        {
                            // If the content file was required for the ghost marker, remove the current ghost mark
                            // and get ready to make a new one.
                            createNewGhostMarker = true;
                            layer.Value.Remove(ghostMarker);
                        }
                        else
                        {
                            // Otherwise if the content file is required by the sprite (Cube/Deco), remove the sprite.
                            layer.Value.RemoveCheck(sprite, sprite.Altitude);                            
                        }
                        i--;
                    }
                }
            }

            if (createNewGhostMarker)
            {
                // The content file for the ghost marker was removed, so make a new one and add it to the layer map.
                GhostMarker = new GhostMarker(MainGame, SpriteSheet.CreateNew(MainGame, "Temp/TopFaceSelectionMarker")) { DrawPriority = 1 };
                LayerMap[Altitude].Add(GhostMarker);
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
                Coord mouseIsoCoord = World.MousePositionToIsoCoord(MainGame, Altitude);
                Coord mouseIsoFlatCoord = World.MousePositionToIsoFlatCoord(MainGame);

                // User input actions.
                if (Peripherals.KeyTapped(Keys.Q) && MainGame.IsActive)
                {
                    var i = (int)Authorization + 1;
                    if (i >= Enum.GetNames(typeof(Authorization)).Length) { i = 0; }
                    Authorization = (Authorization)(i);
                }

                if (Peripherals.KeyTapped(Keys.L) && MainGame.IsActive)
                {
                    //$"Window Position = {Peripherals.GetMouseWindowPosition(MainGame)}".Log();
                    //$"World Position = {Peripherals.GetMouseWorldPosition(MainGame, MainGame.Camera)}".Log();
                    $"Altitude = {Altitude}".Log();
                    //var scale = Vector3.Zero;
                    //var rotation = Quaternion.Identity;
                    //var translation = Vector3.Zero;
                    //SunbirdMBGame.Camera.CurrentTransform.Decompose(out scale, out rotation, out translation);
                    //$"{scale} {rotation} {translation }".Log();
                    mouseIsoCoord.ToString().Log();
                }

                if (Authorization == Authorization.Builder)
                {
                    if (Peripherals.LeftButtonPressed() && MainGame.IsActive)
                    {
                        if (BuildMode == BuildMode.Cube)
                        {
                            var cube = CubeFactory.CreateCurrentCube(mouseIsoFlatCoord, mouseIsoCoord, Altitude);
                            LayerMap[Altitude].AddCheck(cube, Altitude);
                            var altitude = Altitude + 1;

                            // If table doesnt contain entry for altiude, add it.
                            if (!WalkableTileTable.ContainsKey(altitude))
                            {
                                WalkableTileTable.Add(altitude, new XDictionary<Coord, HashSet<Coord>>());
                            }
                            // The following coords are adjacent to the selected coord, and will ALWAYS allow walking from said coord to selected coord.
                            List<Coord> adjacentCoords = new List<Coord>() 
                            { 
                                mouseIsoCoord + new Coord(0, 1),
                                mouseIsoCoord + new Coord(0, -1),
                                mouseIsoCoord + new Coord(1, 0),
                                mouseIsoCoord + new Coord(-1, 0),
                                mouseIsoCoord + new Coord(1, 1),
                                mouseIsoCoord + new Coord(1, -1),
                                mouseIsoCoord + new Coord(-1, -1),
                                mouseIsoCoord + new Coord(-1, 1),
                            };
                            foreach (var coord in adjacentCoords)
                            {
                                // If table doesnt contain entry for coord, add it, and add the selected coord to its set (because we can walk from the former to the latter).
                                if (!WalkableTileTable[altitude].ContainsKey(coord) && cube.IsWalkable)
                                {
                                    WalkableTileTable[altitude].Add(coord, new HashSet<Coord>() { mouseIsoCoord });
                                }
                                // If table DOES contain entry for coord, go ahead and add the selected coord to its set (because we can walk from the former to the latter).
                                else if (!WalkableTileTable[altitude][coord].Contains(mouseIsoCoord) && cube.IsWalkable)
                                {                                  
                                    WalkableTileTable[altitude][coord].Add(mouseIsoCoord);
                                }
                            }
                        }
                        else if (BuildMode == BuildMode.Deco)
                        {
                            var deco = DecoFactory.CreateCurrentDeco(mouseIsoFlatCoord, mouseIsoCoord, Altitude);
                            LayerMap[Altitude].AddCheck(deco, Altitude);
                            //for (int i = 0; i < 10; i ++)
                            //{
                            //    for (int j = 0; j < 10; j++)
                            //    {
                            //        var deco = DecoFactory.CreateCurrentDeco(mouseIsoFlatCoord + new Coord(i, j), mouseIsoCoord + new Coord(i, j), Altitude);
                            //        LayerMap[Altitude].AddCheck(deco, Altitude);
                            //    }
                            //}
                        }
                    }

                    if (Peripherals.RightButtonPressed() && MainGame.IsActive)
                    {
                        //var rect = new Rectangle(Peripherals.GetScaledMouseWorldPosition(MainGame.Camera), new Point(200,200));
                        if (BuildMode == BuildMode.Cube)
                        {
                            for (int i = 0; i < LayerMap[Altitude].Count(); i++)
                            {
                                var sprite = LayerMap[Altitude][i];
                                if (sprite is Cube cube && sprite.Coords == mouseIsoCoord)
                                {
                                    LayerMap[Altitude].RemoveCheck(sprite, Altitude); i--;

                                    var altitude = Altitude + 1;
                                    List<Coord> adjacentCoords = new List<Coord>() 
                                    { 
                                        mouseIsoCoord + new Coord(0, 1), 
                                        mouseIsoCoord + new Coord(0, -1), 
                                        mouseIsoCoord + new Coord(1, 0), 
                                        mouseIsoCoord + new Coord(-1, 0),
                                        mouseIsoCoord + new Coord(1, 1),
                                        mouseIsoCoord + new Coord(1, -1),
                                        mouseIsoCoord + new Coord(-1, -1),
                                        mouseIsoCoord + new Coord(-1, 1)
                                    };
                                    foreach (var coord in adjacentCoords)
                                    {
                                        if (WalkableTileTable[altitude].ContainsKey(coord) && WalkableTileTable[altitude][coord].Contains(mouseIsoCoord))
                                        {
                                            WalkableTileTable[altitude][coord].Remove(mouseIsoCoord);
                                            if (WalkableTileTable[altitude][coord].Count() == 0)
                                            {
                                                WalkableTileTable[altitude].Remove(coord);
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        else if (BuildMode == BuildMode.Deco)
                        {
                            for (int i = 0; i < LayerMap[Altitude].Count(); i++)
                            {
                                var sprite = LayerMap[Altitude][i];
                                if (sprite is Deco)
                                {
                                    var mc = sprite as Deco;
                                    if (mc.OccupiedCoords[Altitude].Contains(mouseIsoCoord))
                                    {
                                        LayerMap[Altitude].RemoveCheck(mc, Altitude); i--;
                                    }
                                }
                            }
                        }
                    }
                }

                // Ghost marker management.
                if (Authorization == Authorization.Builder)
                {
                    GhostMarker.Altitude = Altitude;
                    GhostMarker.Coords = mouseIsoCoord;
                }
                else if (Authorization == Authorization.None)
                {
                    // Top down look.
                    var l = LayerMap.Keys.ToList();
                    l.Sort();
                    l.Reverse();

                    foreach (var key in l)
                    {
                        var targetedCoord = World.GetIsoCoord(mouseIsoFlatCoord, key);
                        if (LayerMap[key].OccupiedCoords.Contains(targetedCoord))
                        {
                            Altitude = key;
                            GhostMarker.Altitude = Altitude;
                            break;
                        }
                    }
                    GhostMarker.Coords = World.MousePositionToIsoCoord(MainGame, Altitude);
                }

                // Test
                GhostMarker.Position = World.IsoFlatCoordToWorldPosition(mouseIsoFlatCoord);

                if (LayerMap[Altitude].OccupiedCoords.Contains(mouseIsoCoord) || Authorization == Authorization.None)
                {
                    GhostMarker.DrawDefaultMarker = true;
                }
                else if (Authorization == Authorization.Builder)
                {
                    GhostMarker.DrawDefaultMarker = false;
                }

                if (LayerMap[Altitude].OccupiedCoords.Contains(mouseIsoCoord))
                {
                    GhostMarker.DrawPriority = 1;
                }
                else if (Authorization == Authorization.Builder)
                {
                    GhostMarker.DrawPriority = 0;
                }
                else if (Authorization == Authorization.None)
                {
                    // !LayerMap[Altitude].OccupiedCoords.Contains(mouseIsoCoord), we are in empty space
                    // and always draw last.
                    GhostMarker.DrawPriority = -1000;
                }

                GhostMarker.IsHidden = !MainGame.IsMouseOver;


                if (Authorization == Authorization.None)
                {
                    if (Peripherals.LeftButtonTapped())
                    {
                        ClickAnimation.Position = GhostMarker.Position;
                        ClickAnimation.Coords = GhostMarker.Coords;
                        ClickAnimation.Altitude = GhostMarker.Altitude;
                        ClickAnimation.DrawPriority = GhostMarker.DrawPriority;
                        ClickAnimation.Animator.AnimationFinished = false;
                        ClickAnimation.Animator.Reconfigure(new AnimArgs(1, 7, 0.066f, AnimationState.Once));
                    }
                }
                ClickAnimation.Update(gameTime);

                // Player management.
                //Player.Altitude = 1;

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

                World.Sort(LayerMap);
                foreach (var sprite in World.OrderedLayerMap)
                {
                    sprite.Update(gameTime);
                }

                #endregion
            }
        }

        

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsLoading)
            {
                var rect = new Rectangle((int)MainGame.Camera.DragTargetPosition.X - 2000, (int)MainGame.Camera.DragTargetPosition.Y - 2000, 4000, 4000);
                #region Main Loop

                // Draw sorted sprites;
                foreach (var sprite in World.OrderedLayerMap)
                {
                    if (rect.Contains(sprite.Position))
                    {
                        // Game
                        if (Altitude != sprite.Altitude && sprite is IWorldObject && Authorization == Authorization.Builder)
                        {
                            sprite.Alpha = 0.15f;
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
