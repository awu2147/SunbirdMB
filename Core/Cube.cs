using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.Core;
using SunbirdMB.Interfaces;
using SunbirdMB.Framework;
using SunbirdMB.Gui;
using System.Reflection;

namespace SunbirdMB.Core
{
    /// <summary>
    /// A single unit building block.
    /// </summary>
    public class Cube : Sprite, IWorldObject
    {
        public Animator AnimatorTop { get; set; }
        public Animator AnimatorBase { get; set; }

        public Cube() { }

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This can create garbage if called during runtime.
        /// </summary>
        public override void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            AnimatorTop.LoadContent(content);
            AnimatorTop.Owner = this;
            AnimatorBase.LoadContent(content);
            AnimatorBase.Owner = this;
        }

        public override void Update(GameTime gameTime)
        {
            AnimatorTop.Update(gameTime);
            AnimatorBase.Update(gameTime);

            Debug.Assert(AnimatorTop.Position == Position);
            Debug.Assert(AnimatorBase.Position == Position);

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsHidden == false)
            {
                AnimatorBase.Draw(gameTime, spriteBatch, Alpha);
                AnimatorTop.Draw(gameTime, spriteBatch, Alpha);
            }
        }

    }

    public class CubeMetaData : PropertyChangedBase
    {
        public static readonly XmlSerializer CubeMetaDataSerializer = Serializer.CreateNew(typeof(CubeMetaData));

        [XmlIgnore]
        public Texture2D Texture;
        public string ContentPath { get; set; }
        public CubePart Part { get; set; }

        private int sheetRows = 1;
        public int SheetRows
        {
            get { return sheetRows; }
            set { SetProperty(ref sheetRows, value); }
        }

        private int sheetColumns = 1;
        public int SheetColumns
        {
            get { return sheetColumns; }
            set { SetProperty(ref sheetColumns, value); }
        }

        private int frameCount = 1;
        public int FrameCount
        {
            get { return frameCount; }
            set { SetProperty(ref frameCount, value); }
        }

        private int startFrame = 0;
        public int StartFrame
        {
            get { return startFrame; }
            set { SetProperty(ref startFrame, value); }
        }

        private float frameSpeed = 0.133f;
        public float FrameSpeed
        {
            get { return frameSpeed; }
            set { SetProperty(ref frameSpeed, value); }
        }

        public int CurrentFrame { get; set; } = 0;

        public AnimationState AnimState { get; set; } = AnimationState.None;

        public CubeMetaData() { }

        internal void SubscribeHandlers()
        {
            PropertyChanged += CubeMetaData_PropertyChanged;
        }

        private void CubeMetaData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            var appDirectory = appPath.TrimEnd(Path.GetFileName(appPath));
            var contentDirectory = Path.Combine(appDirectory, @"Content\");
            var metadataPath = Path.ChangeExtension(Path.Combine(contentDirectory, ContentPath), ".metadata");
            Serialize(metadataPath);
        }

        /// <summary>
        /// Call this to load texture content from path.
        /// </summary>
        public void LoadContent(MainGame mainGame)
        {
            Texture = mainGame.Content.Load<Texture2D>(ContentPath);
            SubscribeHandlers();
        }

        public void Serialize(string path)
        {
            Serializer.WriteXML<CubeMetaData>(CubeMetaDataSerializer, this, path);
        }
    }

    public static class CubeFactory
    {
        public static bool IsRandomTop { get; set; }
        public static bool IsRandomBase { get; set; }

        public static CubeMetaData CurrentCubeTopMetaData { get; set; }
        public static CubeMetaData CurrentCubeBaseMetaData { get; set; }

        public static List<CubeMetaData> CubeMetaDataCollection { get; set; } = new List<CubeMetaData>();

        public static XDictionary<string, CubeMetaData> CubeMetaDataLibrary { get; set; }

        public static void SetCurrent(string contentPath)
        {
            var cmd = CubeMetaDataLibrary[contentPath];
            if (cmd.Part == CubePart.Top)
            {
                CurrentCubeTopMetaData = cmd;
            }
            else if (cmd.Part == CubePart.Base)
            {
                CurrentCubeBaseMetaData = cmd;
            }
        }

        public static Cube CreateCube(MainGame mainGame, CubeMetaData cubeTopMD, CubeMetaData cubeBaseMD, Coord coords, Coord relativeCoords, int altitude)
        {
            var cube = new Cube() { Position = World.TopFace_CoordToLocalOrigin(coords), Coords = relativeCoords, Altitude = altitude };
            var rand = new Random();

            // Create cube top animator.
            var spriteSheet = SpriteSheet.CreateNew(cubeTopMD.Texture, cubeTopMD.ContentPath, cubeTopMD.SheetRows, cubeTopMD.SheetColumns);
            cube.AnimatorTop = new Animator(cube, spriteSheet, cubeTopMD.StartFrame, cubeTopMD.CurrentFrame, cubeTopMD.FrameCount, cubeTopMD.FrameSpeed, cubeTopMD.AnimState);
            if (IsRandomTop == true && cubeTopMD.AnimState == AnimationState.None)
            {
                cube.AnimatorTop.CurrentFrame = rand.Next(0, cube.AnimatorTop.FramesInLoop);
            }
            else
            {
                cube.AnimatorTop.CurrentFrame = cubeTopMD.CurrentFrame;
            }

            // Create cube base animator.
            var spriteSheetBase = SpriteSheet.CreateNew(cubeBaseMD.Texture, cubeBaseMD.ContentPath, cubeBaseMD.SheetRows, cubeBaseMD.SheetColumns);
            cube.AnimatorBase = new Animator(cube, spriteSheetBase, cubeBaseMD.StartFrame, cubeBaseMD.CurrentFrame, cubeBaseMD.FrameCount, cubeBaseMD.FrameSpeed, cubeBaseMD.AnimState);
            if (IsRandomBase == true && cubeBaseMD.AnimState == AnimationState.None)
            {
                cube.AnimatorBase.CurrentFrame = rand.Next(0, cube.AnimatorBase.FramesInLoop);
            }
            else
            {
                cube.AnimatorBase.CurrentFrame = cubeBaseMD.CurrentFrame;
            }

            return cube;
        }

        public static Cube CreateCurrentCube(MainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateCube(mainGame, CurrentCubeTopMetaData, CurrentCubeBaseMetaData, coords, relativeCoords, altitude);
        }

        /// <summary>
        /// Build the cube metadata library by using path to load textures into memory.
        /// </summary>
        public static void BuildLibrary(MainGame mainGame)
        {
            CubeMetaDataLibrary = new XDictionary<string, CubeMetaData>();
            foreach (var cmd in CubeMetaDataCollection)
            {
                cmd.LoadContent(mainGame);
                if (!CubeMetaDataLibrary.ContainsKey(cmd.ContentPath))
                {
                    CubeMetaDataLibrary.Add(cmd.ContentPath, cmd);
                }
            }
        }

        internal static void GenerateLibraryTextures(MainGame mainGame)
        {
            throw new NotImplementedException();
        }
    }    

}

