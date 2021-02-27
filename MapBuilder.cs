using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
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
        private static readonly XmlSerializer MapBuilderSerializer = Serializer.CreateNew(typeof(MapBuilder));

        public WalkableTileTable WalkableTileTable { get; set; } = new WalkableTileTable();
        public XDictionary<Coord3D, Sprite> LayerMap { get; set; } = new XDictionary<Coord3D, Sprite>();
        public SpriteList<Sprite> DynamicSprites { get; set; } = new SpriteList<Sprite>();
        private IOrderedEnumerable<Sprite> OrderedLayerMap { get; set; }
        private List<Sprite> SL { get; set; } = new List<Sprite>() { };

        public bool IsLoading { get; set; }
        public int Altitude { get; set; }
        public static GhostMarker GhostMarker { get; set; }
        public Player Player { get; set; }
        public Sprite ClickAnimation { get; set; }
        
        [XmlIgnore] public Authorization Authorization 
        { 
            get { return MainToolbarViewModel.Authorization; }
            set { MainToolbarViewModel.Authorization = value; } 
        }        

        [XmlIgnore] public BuildMode BuildMode
        {
            get { return MainToolbarViewModel.BuildMode; }
            set { MainToolbarViewModel.BuildMode = value; }
        }

        [XmlIgnore] internal SunbirdMBGame SunbirdMBGame { get; set; }

        public MapBuilder() { }

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

            // Create the player and add to the DynamicSprites.
            var playerSheet = SpriteSheet.CreateNew(MainGame, "Temp/PirateGirlSheet", 1, 16);
            var playerAnimArgs = new AnimArgs(9, 1, 0.2f, AnimationState.None);
            Player = new Player(MainGame, playerSheet, playerAnimArgs);
            Player.Altitude = 1;
            DynamicSprites.Add(Player);

            // Instantiate ghost marker with its default texture and add it to DynamicSprites. Image is null at this point, but will be set
            // by CubeDesignerViewModel.OnMainGameLoaded() as soon as SunbirdMBGame's constructor resolves.
            GhostMarker = new GhostMarker(MainGame, SpriteSheet.CreateNew(MainGame, "Temp/TopFaceSelectionMarker")) { DrawPriority = 1 };
            DynamicSprites.Add(GhostMarker);

            ClickAnimation = new Sprite(MainGame, SpriteSheet.CreateNew(MainGame, "Temp/WalkTargetAnimation", 1, 7)) { Alpha = 0.8f };
            DynamicSprites.Add(ClickAnimation);

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

            // Variable assignment here:
            Altitude = XmlData.Altitude;
            WalkableTileTable = XmlData.WalkableTileTable;
            LayerMap = XmlData.LayerMap;
            DynamicSprites = XmlData.DynamicSprites;

            bool createNewGhostMarker = false;
            for (int i = 0; i < DynamicSprites.Count(); i++)
            {
                Sprite sprite = DynamicSprites[i];
                if (sprite is Player player) { Player = player; }
                else if (sprite is GhostMarker ghostMarker) { GhostMarker = ghostMarker; }
                else if (sprite.Animator?.SpriteSheet?.TexturePath == "Temp/WalkTargetAnimation") 
                { 
                    ClickAnimation = sprite; 
                }

                // Load content can fail if the content file for the sprite no longer exists.
                try { sprite.LoadContent(MainGame); }
                catch (Exception e)
                {
                    e.Message.Log();
                    if (sprite is GhostMarker ghostMarker)
                    {
                        // If the content file was required for the ghost marker, remove the current ghost marker and get ready to make a new one.
                        createNewGhostMarker = true;
                        DynamicSprites.Remove(ghostMarker);
                    }
                    i--;
                }
            }

            if (createNewGhostMarker)
            {
                // The content file for the ghost marker was removed, so make a new one and add it to the layer map.
                GhostMarker = new GhostMarker(MainGame, SpriteSheet.CreateNew(MainGame, "Temp/TopFaceSelectionMarker")) { DrawPriority = 1 };
                DynamicSprites.Add(GhostMarker);
            }

            List<Coord3D> purgedSprites = new List<Coord3D>();
            List<Coord3D> decoReferences = new List<Coord3D>();
            foreach (var layerMapItem in LayerMap)
            {
                var sprite = layerMapItem.Value;
                if (sprite is DecoReference)
                {
                    decoReferences.Add(layerMapItem.Key);
                }
                // Load content can fail if the content file for the sprite no longer exists.
                try { sprite.LoadContent(MainGame); }
                catch (Exception e)
                {
                    e.Message.Log();
                    if (sprite is Deco deco)
                    {
                        foreach (var coord in deco.OccupiedCoords)
                        {
                            purgedSprites.Add(coord);
                        }
                    }
                    else
                    {
                        purgedSprites.Add(layerMapItem.Key);
                    }
                }
            }
            foreach (var coord in purgedSprites)
            {
                LayerMap.Remove(coord);
            }
            foreach (var coord in decoReferences)
            {
                DecoReference decoReference = LayerMap[coord] as DecoReference;
                decoReference.Reference = (Deco)LayerMap[decoReference.ReferenceCoord];
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
                }
            }
        }

        public override void SaveAndSerialize()
        {
            Serializer.WriteXML<MapBuilder>(MapBuilderSerializer, this, saveFilePath);
        }


        public void BuildWorldObject(Coord2D mouseIsoFlatCoord, Coord2D mouseIsoCoord)
        {
            Coord3D mouseIsoCoord3D = new Coord3D(mouseIsoCoord, Altitude);
            if (BuildMode == BuildMode.Cube)
            {
                // Create cube and add it to the layer map.
                var cube = CubeFactory.CreateCurrentCube(mouseIsoFlatCoord, mouseIsoCoord3D);
                if (!LayerMap.ContainsKey(mouseIsoCoord3D))
                {
                    LayerMap.Add(mouseIsoCoord3D, cube);
                }
                // Deal with the walkable tile table.
                foreach (var coord in mouseIsoCoord.AdjacentCoords())
                {
                    // If there are no solid objects above us,
                    if (!(LayerMap.ContainsKey(new Coord3D(mouseIsoCoord, Altitude + 1)) && LayerMap[new Coord3D(mouseIsoCoord, Altitude + 1)].IsSolid)) // not- contains key and sprite is solid
                    {
                        WalkableTileTable.Add(Altitude + 1, coord, mouseIsoCoord);
                    }
                    // Assuming the added cube is solid, the tile under us is no longer walkable.
                    if (cube.IsSolid)
                    {
                        WalkableTileTable.Remove(Altitude, coord, mouseIsoCoord);
                    }
                }

            }
            else if (BuildMode == BuildMode.Deco)
            {
                // Create deco and add it to the layer map.
                var deco = DecoFactory.CreateCurrentDeco(mouseIsoFlatCoord, mouseIsoCoord, Altitude);
                if (deco.OccupiedCoords.Any((c) => LayerMap.ContainsKey(c)) == false)
                {
                    LayerMap.Add(mouseIsoCoord3D, deco);
                    foreach (var coord in deco.OccupiedCoords)
                    {
                        if (coord != mouseIsoCoord3D)
                        {
                            LayerMap.Add(coord, new DecoReference(deco, mouseIsoCoord3D) { Coords = coord.To2D(), Altitude = coord.Z }) ;
                        }

                        foreach (var _coord in new Coord2D(coord.X, coord.Y).AdjacentCoords())
                        {
                            // Assuming the added deco is solid, the tiles under us are no longer walkable.
                            if (deco.IsSolid)
                            {
                                WalkableTileTable.Remove(Altitude, _coord, new Coord2D(coord.X, coord.Y));
                            }
                        }
                    }
                }
            }
        }

        public void RemoveWorldObject(Coord2D mouseIsoFlatCoord, Coord2D mouseIsoCoord)
        {
            Coord3D mouseIsoCoord3D = new Coord3D(mouseIsoCoord, Altitude);
            if (BuildMode == BuildMode.Cube && LayerMap.ContainsKey(mouseIsoCoord3D))
            {
                var sprite = LayerMap[mouseIsoCoord3D];
                if (sprite is Cube)
                {
                    LayerMap.Remove(mouseIsoCoord3D);

                    foreach (var coord in mouseIsoCoord.AdjacentCoords())
                    {
                        WalkableTileTable.Remove(Altitude + 1, coord, mouseIsoCoord);
                    }

                    // If there is a cube below,
                    if (LayerMap.ContainsKey(new Coord3D(mouseIsoCoord, Altitude - 1)) && LayerMap[new Coord3D(mouseIsoCoord, Altitude - 1)] is Cube)
                    {
                        foreach (var coord in mouseIsoCoord.AdjacentCoords())
                        {
                            WalkableTileTable.Add(Altitude, coord, mouseIsoCoord);
                        }
                    }
                }
            }
            else if (BuildMode == BuildMode.Deco && LayerMap.ContainsKey(mouseIsoCoord3D))
            {
                var sprite = LayerMap[mouseIsoCoord3D];
                if (sprite is Deco deco)
                {
                    foreach (var coord in deco.OccupiedCoords)
                    {
                        LayerMap.Remove(coord);

                        // If there are cubes below,
                        var c = new Coord3D(new Coord2D(coord.X, coord.Y), Altitude - 1);
                        if (LayerMap.ContainsKey(c) && LayerMap[c] is Cube)
                        {
                            foreach (var _coord in new Coord2D(coord.X, coord.Y).AdjacentCoords())
                            {
                                WalkableTileTable.Add(Altitude, _coord, new Coord2D(coord.X, coord.Y));
                            }
                        }
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsLoading)
            {
                // Defined with respect to current mouse position.
                Coord2D mouseIsoFlatCoord = World.MousePositionToIsoFlatCoord(MainGame);
                Coord2D mouseIsoCoord = World.MousePositionToIsoCoord(MainGame, Altitude);

                // User input actions.
                if (Peripherals.KeyTapped(Keys.Q) && MainGame.IsActive)
                {
                    var i = (int)Authorization + 1;
                    if (i >= Enum.GetNames(typeof(Authorization)).Length) { i = 0; }
                    Authorization = (Authorization)(i);
                }

                if (Peripherals.KeyTapped(Keys.L) && MainGame.IsActive)
                {
                    $"Altitude = {Altitude}".Log();
                    mouseIsoCoord.ToString().Log();
                }

                if (Authorization == Authorization.Builder)
                {
                    if (Peripherals.LeftButtonPressed() && MainGame.IsActive)
                    {
                        BuildWorldObject(mouseIsoFlatCoord, mouseIsoCoord);
                    }

                    if (Peripherals.RightButtonPressed() && MainGame.IsActive)
                    {
                        RemoveWorldObject(mouseIsoFlatCoord, mouseIsoCoord);
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
                    for (int i = 30; i > -30; i--)
                    {
                        var targetedCoord = World.GetIsoCoord(mouseIsoFlatCoord, i);
                        if (LayerMap.ContainsKey(new Coord3D(targetedCoord, i)) && !(LayerMap[new Coord3D(targetedCoord, i)] is Deco))
                        {
                            Altitude = i;
                            GhostMarker.Altitude = Altitude;
                            break;
                        }
                    }
                    GhostMarker.Coords = World.MousePositionToIsoCoord(MainGame, Altitude);
                }

                // Test
                GhostMarker.Position = World.IsoFlatCoordToWorldPosition(mouseIsoFlatCoord);

                if (LayerMap.ContainsKey(new Coord3D(mouseIsoCoord, Altitude)) || Authorization == Authorization.None)
                {
                    GhostMarker.DrawDefaultMarker = true;
                }
                else if (Authorization == Authorization.Builder)
                {
                    GhostMarker.DrawDefaultMarker = false;
                }

                if (LayerMap.ContainsKey(new Coord3D(mouseIsoCoord, Altitude)))
                {
                    GhostMarker.DrawPriority = 1;
                }
                else if (Authorization == Authorization.Builder)
                {
                    GhostMarker.DrawPriority = 0;
                }
                else if (Authorization == Authorization.None)
                {
                    // If !LayerMap[Altitude].OccupiedCoords.Contains(mouseIsoCoord), we are in empty space
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
                //var altitudeList = LayerMap.Keys.ToList();
                //altitudeList.Sort();

                //foreach (var altitude in altitudeList)
                //{
                //    for (int i = 0; i < LayerMap[altitude].Count(); i++)
                //    {
                //        var sprite = LayerMap[altitude][i];
                //        if (sprite.Altitude != altitude)
                //        {
                //            LayerMap[altitude].Remove(sprite); i--;
                //            if (!LayerMap.ContainsKey(sprite.Altitude))
                //            {
                //                LayerMap.Add(sprite.Altitude, new SpriteList<Sprite>() { sprite });
                //            }
                //            else
                //            {
                //                if (!(sprite is IWorldObject))
                //                {
                //                    LayerMap[sprite.Altitude].Add(sprite);
                //                }
                //                else
                //                {
                //                    throw new NotImplementedException("Cube/Deco trying to move between layers, is this correct? Use AddCheck if so.");
                //                }
                //            }
                //        }
                //    }
                //}

                #endregion

                #region Main Loop

                Sort();
                foreach (var sprite in OrderedLayerMap)
                {
                    sprite.Update(gameTime);
                }

                #endregion
            }
        }

        /// <summary>
        /// The master sorting algorithm for any collection sprites.
        /// </summary>
        public void Sort()
        {
            SL.Clear();
            foreach (var layerMapItem in LayerMap)
            {
                SL.Add(layerMapItem.Value);
            }
            foreach (var sprite in DynamicSprites)
            {
                SL.Add(sprite);
            }
            OrderedLayerMap = SL.OrderBy(x => (x.Coords.X - x.Coords.Y)).ThenBy(x => x.Altitude).ThenBy(x => x.DrawPriority);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsLoading)
            {
                var rect = new Rectangle((int)MainGame.Camera.DragTargetPosition.X - 2000, (int)MainGame.Camera.DragTargetPosition.Y - 2000, 4000, 4000);
                #region Main Loop

                // Draw sorted sprites;
                foreach (var sprite in OrderedLayerMap)
                {
                    if (rect.Contains(sprite.Position))
                    {
                        // Game
                        if (Altitude != sprite.Altitude && sprite is IWorldObject && Authorization == Authorization.Builder)
                        {
                            sprite.Alpha = 0.2f;
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
