using Sunbird.Core;
using SunbirdMB.Framework;
using SunbirdMB.Gui;
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

        //public static XDictionary<string, CubeMetadata> CubeMetadataLibrary { get; set; }

        public static Cube CreateCube(CubeMetadata cubeTopMD, CubeMetadata cubeBaseMD, Coord coords, Coord relativeCoords, int altitude)
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

        public static Cube CreateCurrentCube(Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateCube(CubeDesignerViewModel.CurrentCubeTopMetadata, CubeDesignerViewModel.CurrentCubeBaseMetadata, coords, relativeCoords, altitude);
        }

        ///// <summary>
        ///// Build the cube metadata library by using path to load textures into memory.
        ///// </summary>
        //public static void BuildLibrary(IMainGame mainGame)
        //{
        //    CubeMetadataLibrary = new XDictionary<string, CubeMetadata>();
        //    foreach (var cmd in CubeMetadataCollection)
        //    {
        //        if (!CubeMetadataLibrary.ContainsKey(cmd.ContentPath))
        //        {
        //            cmd.LoadContent(mainGame);
        //            CubeMetadataLibrary.Add(cmd.ContentPath, cmd);
        //        }
        //    }
        //    foreach (var cdi in CubeDesignerViewModel.CubeTopCollection)
        //    {
        //        if (!CubeMetadataLibrary.ContainsKey(cdi.CubeMetadata.ContentPath))
        //        {
        //            cmd.LoadContent(mainGame);
        //            CubeMetadataLibrary.Add(cmd.ContentPath, cmd);
        //        }
        //    }
        //}
    }
}
