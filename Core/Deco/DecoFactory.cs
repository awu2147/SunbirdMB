using SunbirdMB.Framework;
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
        public static Deco CreateDeco(SunbirdMBGame mainGame, DecoMetadata decoMD, Coord coords, Coord relativeCoords, int altitude)
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

            for (int k = 0; k < decoMD.Dimensions.Z; k++)
            {
                deco.OccupiedCoords.Add(altitude + k, new HashSet<Coord>() { });
                for (int i = 0; i < decoMD.Dimensions.X; i++)
                {
                    for (int j = 0; j < decoMD.Dimensions.Y; j++)
                    {
#if DEBUG
                        Debug.Assert(decoMD.Dimensions.X == decoMD.Dimensions.Y);
#endif
                        int offset = decoMD.Dimensions.Y / 2;
                        deco.OccupiedCoords[altitude + k].Add(deco.Coords + new Coord(i - offset, -j + offset));
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

        public static Deco CreateCurrentDeco(SunbirdMBGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            throw new NotImplementedException();
            //return CreateDeco(mainGame, CurrentDecoMetadata, coords, relativeCoords, altitude);
        }        

    }
}
