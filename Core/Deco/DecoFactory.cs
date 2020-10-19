using SunbirdMB.Framework;
using SunbirdMB.Gui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Core
{
    public static class DecoFactory
    {
        public static Deco CreateDeco(DecoMetadata decoMD, Coord2D coords, Coord2D relativeCoords, int altitude)
        {
            Type type = Type.GetType(decoMD.TypeName);
            var deco = Activator.CreateInstance(type) as Deco;
            deco.Position = World.IsoFlatCoordToWorldPosition(coords);
            deco.PositionOffset = decoMD.PositionOffset;
            deco.Coords = relativeCoords;
            deco.Altitude = altitude;
            // Odd NxN takes priority over even NxN when coords along same horizontal line.
            deco.Dimensions = decoMD.Dimensions;
            if (deco.Dimensions.X % 2 == 0)
            {
                deco.DrawPriority = -1;
            }
            var rand = new Random();

            // Create deco animator.
            var spriteSheet = SpriteSheet.CreateNew(decoMD.Texture, decoMD.ContentPath, decoMD.SheetRows, decoMD.SheetColumns);
            deco.Animator = new Animator(deco, spriteSheet, decoMD.StartFrame, decoMD.CurrentFrame, decoMD.FrameCount, decoMD.FrameSpeed, decoMD.AnimState);

            for (int i = 0; i < decoMD.Dimensions.X; i++)
            {
                for (int j = 0; j < decoMD.Dimensions.Y; j++)
                {
                    for (int k = 0; k < decoMD.Dimensions.Z; k++)
                    {
                        deco.OccupiedCoords.Add(new Coord3D(deco.Coords, deco.Altitude) + new Coord3D(i - decoMD.Dimensions.X / 2, -j + decoMD.Dimensions.Y / 2, k));
                    }
                }
            }

            if (decoMD.AnimState == AnimationState.None)
            {
                var i = rand.Next(0, decoMD.ActiveFrames.Count());
                var cf = decoMD.ActiveFrames.ToArray()[i];
                deco.Animator.CurrentFrame = cf;
            }
            else
            {
                deco.Animator.CurrentFrame = decoMD.CurrentFrame;
            }

            return deco;
        }

        public static Deco CreateCurrentDeco(Coord2D coords, Coord2D relativeCoords, int altitude)
        {
            return CreateDeco(DecoCatalogViewModel.CurrentDecoMetadata, coords, relativeCoords, altitude);
        }        

    }
}
