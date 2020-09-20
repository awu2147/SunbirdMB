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

namespace SunbirdMB.Core
{
    /// <summary>
    /// A single unit building block.
    /// </summary>
    [Serializable]
    public class Cube : Sprite, IWorldObject
    {
        public Animator AnimatorBase { get; set; }

        public Cube() { }

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This can create garbage if called during runtime.
        /// </summary>
        public override void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Animator.LoadContent(content);
            Animator.Owner = this;
            AnimatorBase.LoadContent(content);
            AnimatorBase.Owner = this;
#if DEBUG
            Debug.Assert(ShadowPath != null);
            Debug.Assert(AntiShadowPath != null);
#endif
            Shadow = content.Load<Texture2D>(ShadowPath);
            AntiShadow = content.Load<Texture2D>(AntiShadowPath);
            if (LightPath != null) { Light = content.Load<Texture2D>(LightPath); }
        }

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This is safe to call during runtime.
        /// </summary>
        public override void SafeLoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            LoadContent(mainGame, graphicsDevice, content);
        }

        public override void Update(GameTime gameTime)
        {
            Animator.Update(gameTime);
            AnimatorBase.Update(gameTime);
#if DEBUG
            Debug.Assert(Animator.Position == Position);
            Debug.Assert(AnimatorBase.Position == Position);
#endif
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsHidden == false)
            {
                AnimatorBase.Draw(gameTime, spriteBatch, Alpha);
                Animator.Draw(gameTime, spriteBatch, Alpha);
            }
        }

    }

    [Serializable]
    public class CubeMetaData
    {
        [XmlIgnore]
        public Texture2D Texture;
        public string Path { get; set; }

        public int SheetRows { get; set; } = 1;
        public int SheetColumns { get; set; } = 1;
        public int StartFrame { get; set; } = 0;
        public int CurrentFrame { get; set; } = 0;
        public int FrameCount { get; set; } = 1;
        public float FrameSpeed { get; set; } = 0.133f;
        public AnimationState AnimState { get; set; } = AnimationState.None;
        public CubeMetaData()
        {

        }

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This can create garbage if called during runtime.
        /// </summary>
        public void LoadContent(MainGame mainGame)
        {
            Texture = mainGame.Content.Load<Texture2D>(Path);
        }

        public void NextFrame()
        {
            if (AnimState == AnimationState.None)
            {
                CurrentFrame++;
                if (CurrentFrame >= FrameCount)
                {
                    CurrentFrame = 0;
                }
            }
        }

        public void PreviousFrame()
        {
            if (AnimState == AnimationState.None)
            {
                CurrentFrame--;
                if (CurrentFrame < 0)
                {
                    CurrentFrame = FrameCount - 1;
                }
            }
        }
    }

    public static class CubeFactory
    {
        public static bool IsRandomTop { get; set; }
        public static bool IsRandomBase { get; set; }

        public static CubeMetaData CurrentCubeTopMetaData { get; set; }
        public static CubeMetaData CurrentCubeBaseMetaData { get; set; }

        public static int CurrentTopIndex { get; set; }
        public static int CurrentBaseIndex { get; set; }

        public static List<CubeMetaData> CubeTopMetaDataLibrary { get; set; }
        public static List<CubeMetaData> CubeBaseMetaDataLibrary { get; set; }

        public static XDictionary<string, CubeMetaData> CubeMetaDataLibrary { get; set; }

        public static Cube CreateCube(MainGame mainGame, CubeMetaData cubeTopMD, CubeMetaData cubeBaseMD, Coord coords, Coord relativeCoords, int altitude)
        {
            var cube = new Cube() { Position = World.TopFace_CoordToLocalOrigin(coords), Coords = relativeCoords, Altitude = altitude };
            var rand = new Random();

            // Create cube top animator.
            var spriteSheet = SpriteSheet.CreateNew(cubeTopMD.Texture, cubeTopMD.Path, cubeTopMD.SheetRows, cubeTopMD.SheetColumns);
            cube.Animator = new Animator(cube, spriteSheet, cubeTopMD.StartFrame, cubeTopMD.CurrentFrame, cubeTopMD.FrameCount, cubeTopMD.FrameSpeed, cubeTopMD.AnimState);
            if (IsRandomTop == true && cubeTopMD.AnimState == AnimationState.None)
            {
                cube.Animator.CurrentFrame = rand.Next(0, cube.Animator.FramesInLoop);
            }
            else
            {
                cube.Animator.CurrentFrame = cubeTopMD.CurrentFrame;
            }

            // Create cube base animator.
            var spriteSheetBase = SpriteSheet.CreateNew(cubeBaseMD.Texture, cubeBaseMD.Path, cubeBaseMD.SheetRows, cubeBaseMD.SheetColumns);
            cube.AnimatorBase = new Animator(cube, spriteSheetBase, cubeBaseMD.StartFrame, cubeBaseMD.CurrentFrame, cubeBaseMD.FrameCount, cubeBaseMD.FrameSpeed, cubeBaseMD.AnimState);
            if (IsRandomBase == true && cubeBaseMD.AnimState == AnimationState.None)
            {
                cube.AnimatorBase.CurrentFrame = rand.Next(0, cube.AnimatorBase.FramesInLoop);
            }
            else
            {
                cube.AnimatorBase.CurrentFrame = cubeBaseMD.CurrentFrame;
            }

            // This actually prevents memory leak vs CreateMask() since content manager knows to reuse same texture. Should all cubes have the same shadow and antishadow?
            cube.Shadow = mainGame.Content.Load<Texture2D>("Temp/CubeShadow");
            cube.ShadowPath = "Temp/CubeShadow";
            cube.AntiShadow = mainGame.Content.Load<Texture2D>("Temp/CubeAntiShadow");
            cube.AntiShadowPath = "Temp/CubeAntiShadow";

            return cube;
        }

        public static Cube CreateCurrentCube(MainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateCube(mainGame, CurrentCubeTopMetaData, CurrentCubeBaseMetaData, coords, relativeCoords, altitude);
        }

        public static void FindNextTop()
        {
            CurrentTopIndex++;
            if (CurrentTopIndex >= CubeTopMetaDataLibrary.Count())
            {
                CurrentTopIndex = 0;
            }
            CurrentCubeTopMetaData = CubeTopMetaDataLibrary[CurrentTopIndex];
        }

        public static void FindPreviousTop()
        {
            CurrentTopIndex--;
            if (CurrentTopIndex < 0)
            {
                CurrentTopIndex = CubeTopMetaDataLibrary.Count() - 1;
            }
            CurrentCubeTopMetaData = CubeTopMetaDataLibrary[CurrentTopIndex];
        }

        public static void FindNextBase()
        {
            CurrentBaseIndex++;
            if (CurrentBaseIndex >= CubeBaseMetaDataLibrary.Count())
            {
                CurrentBaseIndex = 0;
            }
            CurrentCubeBaseMetaData = CubeBaseMetaDataLibrary[CurrentBaseIndex];
        }

        public static void FindPreviousBase()
        {
            CurrentBaseIndex--;
            if (CurrentBaseIndex < 0)
            {
                CurrentBaseIndex = CubeBaseMetaDataLibrary.Count() - 1;
            }
            CurrentCubeBaseMetaData = CubeBaseMetaDataLibrary[CurrentBaseIndex];
        }

    }

    /// <summary>
    /// Acts as a data store for the static class CubeFactory during serialization.
    /// </summary>
    [Serializable]
    public class CubeFactoryData
    {
        public static readonly XmlSerializer CubeFactoryDataSerializer = Serializer.CreateNew(typeof(CubeFactoryData), new Type[] { typeof(CubeMetaData) });

        public bool IsRandomTop { get; set; }
        public bool IsRandomBase { get; set; }

        public CubeMetaData CurrentCubeTopMetaData { get; set; }
        public CubeMetaData CurrentCubeBaseMetaData { get; set; }

        public int CurrentTopIndex { get; set; }
        public int CurrentBaseIndex { get; set; }

        public List<CubeMetaData> CubeTopMetaDataLibrary { get; set; }
        public List<CubeMetaData> CubeBaseMetaDataLibrary { get; set; }

        public CubeFactoryData()
        {
            SyncIn();
        }

        public void Serialize()
        {
            Serializer.WriteXML<CubeFactoryData>(CubeFactoryDataSerializer, this, "CubeFactoryData.xml");
        }

        /// <summary>
        /// Create a copy of CubeFactory's static properties;
        /// </summary>
        public void SyncIn()
        {
            IsRandomTop = CubeFactory.IsRandomTop;
            IsRandomBase = CubeFactory.IsRandomBase;

            CurrentCubeTopMetaData = CubeFactory.CurrentCubeTopMetaData;
            CurrentCubeBaseMetaData = CubeFactory.CurrentCubeBaseMetaData;

            CurrentTopIndex = CubeFactory.CurrentTopIndex;
            CurrentBaseIndex = CubeFactory.CurrentBaseIndex;

            CubeTopMetaDataLibrary = CubeFactory.CubeTopMetaDataLibrary;
            CubeBaseMetaDataLibrary = CubeFactory.CubeBaseMetaDataLibrary;
        }

        /// <summary>
        /// Reassign values to CubeFactory's static properties;
        /// </summary>
        public void SyncOut(MainGame mainGame)
        {
            CubeFactory.IsRandomTop = IsRandomTop;
            CubeFactory.IsRandomBase = IsRandomBase;

            CubeFactory.CurrentCubeTopMetaData = CurrentCubeTopMetaData;
            CubeFactory.CurrentCubeBaseMetaData = CurrentCubeBaseMetaData;

            CubeFactory.CurrentTopIndex = CurrentTopIndex;
            CubeFactory.CurrentBaseIndex = CurrentBaseIndex;

            CubeFactory.CubeTopMetaDataLibrary = CubeTopMetaDataLibrary;
            CubeFactory.CubeBaseMetaDataLibrary = CubeBaseMetaDataLibrary;

            // Generate library Textures from Path and populate CubeMetaDataLibrary (Dictionary).
            CubeFactory.CubeMetaDataLibrary = new XDictionary<string, CubeMetaData>();
            foreach (var ctmd in CubeFactory.CubeTopMetaDataLibrary)
            {
                ctmd.LoadContent(mainGame);
                if (!CubeFactory.CubeMetaDataLibrary.ContainsKey(ctmd.Path))
                {
                    CubeFactory.CubeMetaDataLibrary.Add(ctmd.Path, ctmd);
                }
            }
            foreach (var cbmd in CubeFactory.CubeBaseMetaDataLibrary)
            {
                cbmd.LoadContent(mainGame);
                if (!CubeFactory.CubeMetaDataLibrary.ContainsKey(cbmd.Path))
                {
                    CubeFactory.CubeMetaDataLibrary.Add(cbmd.Path, cbmd);
                }
            }

            CubeFactory.CurrentCubeTopMetaData.LoadContent(mainGame);
            CubeFactory.CurrentCubeBaseMetaData.LoadContent(mainGame);

        }

    }

}

