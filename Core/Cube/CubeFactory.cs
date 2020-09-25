using Sunbird.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Core
{
    public static class CubeFactory
    {
        public static bool IsRandomTop { get; set; }
        public static bool IsRandomBase { get; set; }

        public static CubeMetadata CurrentCubeTopMetaData { get; set; }
        public static CubeMetadata CurrentCubeBaseMetaData { get; set; }

        public static List<CubeMetadata> CubeMetaDataCollection { get; set; } = new List<CubeMetadata>();

        public static XDictionary<string, CubeMetadata> CubeMetaDataLibrary { get; set; }

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

        public static Cube CreateCube(IMainGame mainGame, CubeMetadata cubeTopMD, CubeMetadata cubeBaseMD, Coord coords, Coord relativeCoords, int altitude)
        {
            var cube = new Cube() { Position = World.TopFace_CoordToLocalOrigin(coords), Coords = relativeCoords, Altitude = altitude };
            var rand = new Random();

            // Create cube top animator.
            var spriteSheet = SpriteSheet.CreateNew(cubeTopMD.Texture, cubeTopMD.ContentPath, cubeTopMD.SheetRows, cubeTopMD.SheetColumns);
            cube.AnimatorTop = new Animator(cube, spriteSheet, cubeTopMD.StartFrame, cubeTopMD.CurrentFrame, cubeTopMD.FrameCount, cubeTopMD.FrameSpeed, cubeTopMD.AnimState);
            if (IsRandomTop == true && cubeTopMD.AnimState == AnimationState.None)
            {
                cube.AnimatorTop.CurrentFrame = rand.Next(1, cube.AnimatorTop.FramesInLoop + 1);
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
                cube.AnimatorBase.CurrentFrame = rand.Next(1, cube.AnimatorBase.FramesInLoop + 1);
            }
            else
            {
                cube.AnimatorBase.CurrentFrame = cubeBaseMD.CurrentFrame;
            }

            return cube;
        }

        public static Cube CreateCurrentCube(IMainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateCube(mainGame, CurrentCubeTopMetaData, CurrentCubeBaseMetaData, coords, relativeCoords, altitude);
        }

        /// <summary>
        /// Build the cube metadata library by using path to load textures into memory.
        /// </summary>
        public static void BuildLibrary(IMainGame mainGame)
        {
            CubeMetaDataLibrary = new XDictionary<string, CubeMetadata>();
            foreach (var cmd in CubeMetaDataCollection)
            {
                cmd.LoadContent(mainGame);
                if (!CubeMetaDataLibrary.ContainsKey(cmd.ContentPath))
                {
                    CubeMetaDataLibrary.Add(cmd.ContentPath, cmd);
                }
            }
        }
    }
}
