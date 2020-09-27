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

        public static CubeMetadata CurrentCubeTopMetadata { get; set; }
        public static CubeMetadata CurrentCubeBaseMetadata { get; set; }

        public static List<CubeMetadata> CubeMetadataCollection { get; set; } = new List<CubeMetadata>();

        public static XDictionary<string, CubeMetadata> CubeMetadataLibrary { get; set; }

        public static void SetCurrent(string contentPath)
        {
            var cmd = CubeMetadataLibrary[contentPath];
            if (cmd.Part == CubePart.Top)
            {
                CurrentCubeTopMetadata = cmd;
            }
            else if (cmd.Part == CubePart.Base)
            {
                CurrentCubeBaseMetadata = cmd;
            }
        }

        public static Cube CreateCube(IMainGame mainGame, CubeMetadata cubeTopMD, CubeMetadata cubeBaseMD, Coord coords, Coord relativeCoords, int altitude)
        {
            var cube = new Cube() { Position = World.IsoFlatCoordToWorldPosition(coords), Coords = relativeCoords, Altitude = altitude };
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
            return CreateCube(mainGame, CurrentCubeTopMetadata, CurrentCubeBaseMetadata, coords, relativeCoords, altitude);
        }

        /// <summary>
        /// Build the cube metadata library by using path to load textures into memory.
        /// </summary>
        public static void BuildLibrary(IMainGame mainGame)
        {
            CubeMetadataLibrary = new XDictionary<string, CubeMetadata>();
            foreach (var cmd in CubeMetadataCollection)
            {
                cmd.LoadContent(mainGame);
                if (!CubeMetadataLibrary.ContainsKey(cmd.ContentPath))
                {
                    CubeMetadataLibrary.Add(cmd.ContentPath, cmd);
                }
            }
        }
    }
}
