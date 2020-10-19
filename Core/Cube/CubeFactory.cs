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
        public static Cube CreateCube(CubeMetadata cubeTopMD, CubeMetadata cubeBaseMD, Coord2D isoFlatCoord, Coord3D isoCoord3D)
        {
            var cube = new Cube() 
            { 
                Position = World.IsoFlatCoordToWorldPosition(isoFlatCoord), 
                Coords = new Coord2D(isoCoord3D.X, isoCoord3D.Y), 
                Altitude = isoCoord3D.Z
            };
            var rand = new Random();

            // Create cube top animator.
            var spriteSheet = SpriteSheet.CreateNew(cubeTopMD.Texture, cubeTopMD.ContentPath, cubeTopMD.SheetRows, cubeTopMD.SheetColumns);
            cube.AnimatorTop = new Animator(cube, spriteSheet, cubeTopMD.StartFrame, cubeTopMD.CurrentFrame, cubeTopMD.FrameCount, cubeTopMD.FrameSpeed, cubeTopMD.AnimState);
            if (cubeTopMD.AnimState == AnimationState.None)
            {
                var i = rand.Next(0, cubeTopMD.ActiveFrames.Count());
                var cf = cubeTopMD.ActiveFrames.ToArray()[i];
                cube.AnimatorTop.CurrentFrame = cf;
            }
            else
            {
                cube.AnimatorTop.CurrentFrame = cubeTopMD.CurrentFrame;
            }

            // Create cube base animator.
            var spriteSheetBase = SpriteSheet.CreateNew(cubeBaseMD.Texture, cubeBaseMD.ContentPath, cubeBaseMD.SheetRows, cubeBaseMD.SheetColumns);
            cube.AnimatorBase = new Animator(cube, spriteSheetBase, cubeBaseMD.StartFrame, cubeBaseMD.CurrentFrame, cubeBaseMD.FrameCount, cubeBaseMD.FrameSpeed, cubeBaseMD.AnimState);
            if (cubeBaseMD.AnimState == AnimationState.None)
            {
                var i = rand.Next(0, cubeBaseMD.ActiveFrames.Count());
                var cf = cubeBaseMD.ActiveFrames.ToArray()[i];
                cube.AnimatorBase.CurrentFrame = cf;
            }
            else
            {
                cube.AnimatorBase.CurrentFrame = cubeBaseMD.CurrentFrame;
            }

            return cube;
        }

        public static Cube CreateCurrentCube(Coord2D isoFlatCoord, Coord3D isoCoord3D)
        {
            return CreateCube(CubeDesignerViewModel.CurrentCubeTopMetadata, CubeDesignerViewModel.CurrentCubeBaseMetadata, isoFlatCoord, isoCoord3D);
        }

    }
}
