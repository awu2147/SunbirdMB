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
using SunbirdMB.Framework;
using SunbirdMB.Core;
using SunbirdMB;

/// <summary>
/// Terminology:
/// - GridCoord: Coord in the grid plane.
/// - IsoFlatCoord: Coord in the isometric Plane altitude = 0.
/// - IsoCoord: Coord in the alititude scaled isometric plane.
/// - 2DWorldPosition: Position in the 2D game world.
/// </summary>

namespace SunbirdMB.Core
{
    public static class World
    {
        /// <summary>
        /// Floating point representation of Zoom / Scale.
        /// </summary>
        public static float ZoomRatio { get { return (float)Zoom / (float)Scale; } }
        public static int Zoom { get; set; } = 3;
        public static int Scale { get; private set; } = 3;
        private static int TopFaceGridWidth { get; set; } = 24 * Scale;
        private static int TopFaceGridHeight { get; set; } = 12 * Scale;

        private static List<Rectangle> TopFaceAreaC { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPoint(1, 5, 3), ScaledPoint(2, 2, 3)),
            new Rectangle(ScaledPoint(3, 4, 3), ScaledPoint(2, 4, 3)),
            new Rectangle(ScaledPoint(5, 3, 3), ScaledPoint(2, 6, 3)),
            new Rectangle(ScaledPoint(7, 2, 3), ScaledPoint(2, 8, 3)),
            new Rectangle(ScaledPoint(9, 1, 3), ScaledPoint(2, 10, 3)),
            new Rectangle(ScaledPoint(11, 0, 3), ScaledPoint(2, 12, 3)),
            new Rectangle(ScaledPoint(13, 1, 3), ScaledPoint(2, 10, 3)),
            new Rectangle(ScaledPoint(15, 2, 3), ScaledPoint(2, 8, 3)),
            new Rectangle(ScaledPoint(17, 3, 3), ScaledPoint(2, 6, 3)),
            new Rectangle(ScaledPoint(19, 4, 3), ScaledPoint(2, 4, 3)),
            new Rectangle(ScaledPoint(21, 5, 3), ScaledPoint(2, 2, 3))
        };
        private static List<Rectangle> TopFaceAreaTL { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPoint(0, 0, 3), ScaledPoint(1, 6, 3)),
            new Rectangle(ScaledPoint(1, 0, 3), ScaledPoint(2, 5, 3)),
            new Rectangle(ScaledPoint(3, 0, 3), ScaledPoint(2, 4, 3)),
            new Rectangle(ScaledPoint(5, 0, 3), ScaledPoint(2, 3, 3)),
            new Rectangle(ScaledPoint(7, 0, 3), ScaledPoint(2, 2, 3)),
            new Rectangle(ScaledPoint(9, 0, 3), ScaledPoint(2, 1, 3))
        };
        private static List<Rectangle> TopFaceAreaTR { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPoint(13, 0, 3), ScaledPoint(2, 1, 3)),
            new Rectangle(ScaledPoint(15, 0, 3), ScaledPoint(2, 2, 3)),
            new Rectangle(ScaledPoint(17, 0, 3), ScaledPoint(2, 3, 3)),
            new Rectangle(ScaledPoint(19, 0, 3), ScaledPoint(2, 4, 3)),
            new Rectangle(ScaledPoint(21, 0, 3), ScaledPoint(2, 5, 3)),
            new Rectangle(ScaledPoint(23, 0, 3), ScaledPoint(1, 6, 3)),
        };
        private static List<Rectangle> TopFaceAreaBL { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPoint(0, 6, 3), ScaledPoint(1, 6, 3)),
            new Rectangle(ScaledPoint(1, 7, 3), ScaledPoint(2, 5, 3)),
            new Rectangle(ScaledPoint(3, 8, 3), ScaledPoint(2, 4, 3)),
            new Rectangle(ScaledPoint(5, 9, 3), ScaledPoint(2, 3, 3)),
            new Rectangle(ScaledPoint(7, 10, 3), ScaledPoint(2, 2, 3)),
            new Rectangle(ScaledPoint(9, 11, 3), ScaledPoint(2, 1, 3))
        };
        private static List<Rectangle> TopFaceAreaBR { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPoint(13, 11, 3), ScaledPoint(2, 1, 3)),
            new Rectangle(ScaledPoint(15, 10, 3), ScaledPoint(2, 2, 3)),
            new Rectangle( ScaledPoint(17, 9, 3), ScaledPoint(2, 3, 3)),
            new Rectangle( ScaledPoint(19, 8, 3), ScaledPoint(2, 4, 3)),
            new Rectangle( ScaledPoint(21, 7, 3), ScaledPoint(2, 5, 3)),
            new Rectangle( ScaledPoint(23, 6, 3), ScaledPoint(1, 6, 3)),
        };

        /// <summary>
        /// Get the coordinate of the top face that the mouse is currently over, taking into account altitude.
        /// </summary>
        public static Coord2D MousePositionToIsoCoord(SunbirdMBGame mainGame, int altitude)
        {
            return WorldPositionToIsoCoord(Peripherals.GetScaledMouseWorldPosition(mainGame, mainGame.Camera), altitude);
        }

        /// <summary>
        /// Get the coordinate of the top face that the mouse is currently over.
        /// </summary>
        public static Coord2D MousePositionToIsoFlatCoord(SunbirdMBGame mainGame)
        {
            return WorldPositionToIsoCoord(Peripherals.GetScaledMouseWorldPosition(mainGame, mainGame.Camera), 0);
        }

        public static Coord2D WorldPositionToIsoCoord(Point point, int altitude)
        {
            Coord2D gridCoord = WorldPositionToGridCoord(point);
            Coord2D isoCoord = GridCoordToIsoCoord(gridCoord);
            Coord2D isoCoordoffset = IsoCoordOffset(WorldPositionToNormalizedGridPosition(point), point);

            return GetIsoCoord(isoCoord, altitude) + isoCoordoffset;
        }

        /// <summary>
        /// Maps a position in the world to a position in a quadrant of size [TopFaceGridWidth, TopFaceGridHeight], with the top left corner alligned with the origin.
        /// </summary>
        public static Point WorldPositionToNormalizedGridPosition(Point point)
        {
            var x = point.X % TopFaceGridWidth;
            if (x < 0)
            {
                x = TopFaceGridWidth + x;
            }
            var y = point.Y % TopFaceGridHeight;
            if (y < 0)
            {
                y = TopFaceGridHeight + y;
            }
            return new Point(x, y);
        }

        /// <summary>
        /// Find the offset needed to be applied to the isometric coordinate if the position was outside the actual diamond face and in the corner regions instead.
        /// </summary>
        private static Coord2D IsoCoordOffset(Point point, Point worldPoint)
        {
            if (TopFaceAreaC.Any(rect => rect.Contains(point)))
            {
                return new Coord2D(0, 0);
            }
            else if (TopFaceAreaTL.Any(rect => rect.Contains(point)))
            {
                return new Coord2D(-1, 0);
            }
            else if (TopFaceAreaTR.Any(rect => rect.Contains(point)))
            {
                return new Coord2D(0, 1);
            }
            else if (TopFaceAreaBL.Any(rect => rect.Contains(point)))
            {
                return new Coord2D(0, -1);
            }
            else if (TopFaceAreaBR.Any(rect => rect.Contains(point)))
            {
                return new Coord2D(1, 0);
            }
            else
            {
                throw new Exception("TopFaceRectangle does not contain point. Check point is normalized and/or TopFaceRectangle components are correct.");
            }
        }

        /// <summary>
        /// Takes a point in the world and returns the corresponding un-offset Top Face grid coord.
        /// </summary>
        /// <param name="point">World position as a point.</param>
        /// <returns></returns>
        public static Coord2D WorldPositionToGridCoord(Point point)
        {
            var x = point.X / TopFaceGridWidth;
            var xRem = point.X % TopFaceGridWidth; // We need remainder check to preserve grid edge schema after translation.
            if (point.X < 0 && xRem != 0)
            {
                x -= 1;
            }
            var y = point.Y / TopFaceGridHeight;
            var yRem = point.Y % TopFaceGridHeight;
            if (point.Y < 0 && yRem != 0)
            {
                y -= 1;
            }
            return new Coord2D(x, y);
        }

        /// <summary>
        /// Performs a 2x2 matrix multiplication on <paramref name="gridCoord"/>, where M = ([1 , 1], [1, -1]).
        /// </summary>
        /// <param name="gridCoord">Grid coord</param>
        /// <returns>World coord</returns>
        private static Coord2D GridCoordToIsoCoord(Coord2D gridCoord)
        {
            // This is where the magic happens.
            return new Coord2D(gridCoord.Y + gridCoord.X, gridCoord.Y * -1 + gridCoord.X);
        }

        /// <summary>
        /// Find the coord in the altitude plane.
        /// </summary>
        public static Coord2D GetIsoCoord(Coord2D coord, int altitude)
        {
            return coord + (new Coord2D(1, -1) * altitude);
        }

        /// <summary>
        /// Return grid quadrant top left corner world position from isometric flat coord.
        /// </summary>
        public static Vector2 IsoFlatCoordToWorldPosition(Coord2D isoFlatCoord)
        {
            return new Vector2(TopFaceGridWidth / 2, TopFaceGridHeight / 2) * isoFlatCoord.X + new Vector2(TopFaceGridWidth / 2, -TopFaceGridHeight / 2) * isoFlatCoord.Y;
        }

        public static Coord2D WorldPositionToIsoCoord(Vector2 point, int altitude)
        {
            return WorldPositionToIsoCoord(point.ToPoint(), altitude);
        }

        public static Point ZoomScaledPoint(Point point)
        {
            float x = point.X;
            float y = point.Y;
            return new Point((int)(x / ZoomRatio), (int)(y / ZoomRatio));
        }

        private static Point ScaledPoint(int x, int y, int scale)
        {
            return new Point(x, y) * new Point(scale, scale);
        }

        //public static IOrderedEnumerable<Sprite> OrderedLayerMap;

        ///// <summary>
        ///// The master sorting algorithm for any collection sprites.
        ///// </summary>
        //public static void Sort(XDictionary<int, SpriteList<Sprite>> layerMap)
        //{
        //    var SL = new List<Sprite>() { };
        //    foreach (var layer in layerMap)
        //    {
        //        foreach (var sprite in layer.Value)
        //        {
        //            SL.Add(sprite);
        //        }
        //    }
        //    OrderedLayerMap = SL.OrderBy(x => (x.Coords.X - x.Coords.Y)).ThenBy(x => x.Altitude).ThenBy(x => x.DrawPriority);
        //}

        //public static void Sort(XDictionary<Coord3D, List<Sprite>> layerMap)
        //{
        //    var SL = new List<Sprite>() { };
        //    foreach (var layerBlock in layerMap)
        //    {
        //        foreach (var sprite in layerBlock.Value)
        //        {
        //            SL.Add(sprite);
        //        }
        //    }
        //    OrderedLayerMap = SL.OrderBy(x => (x.Coords.X - x.Coords.Y)).ThenBy(x => x.Altitude).ThenBy(x => x.DrawPriority);
        //}

    }
}

