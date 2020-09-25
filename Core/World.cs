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

//TODO: LocalOriginToCoord().

namespace Sunbird.Core
{
    public static class World
    {
        public static float ZoomRatio { get { return (float)Zoom / (float)Scale; } }
        public static int Zoom { get; set; } = 3;
        public static int Scale { get; private set; } = 3;
        private static int TopFaceGridWidth { get; set; } = 24;
        private static int TopFaceGridHeight { get; set; } = 12;

        private static List<Rectangle> TopFaceAreaC { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPointZoom(1, 5), ScaledPointZoom(2, 2)),
            new Rectangle(ScaledPointZoom(3, 4), ScaledPointZoom(2, 4)),
            new Rectangle(ScaledPointZoom(5, 3), ScaledPointZoom(2, 6)),
            new Rectangle(ScaledPointZoom(7, 2), ScaledPointZoom(2, 8)),
            new Rectangle(ScaledPointZoom(9, 1), ScaledPointZoom(2, 10)),
            new Rectangle(ScaledPointZoom(11, 0), ScaledPointZoom(2, 12)),
            new Rectangle(ScaledPointZoom(13, 1), ScaledPointZoom(2, 10)),
            new Rectangle(ScaledPointZoom(15, 2), ScaledPointZoom(2, 8)),
            new Rectangle(ScaledPointZoom(17, 3), ScaledPointZoom(2, 6)),
            new Rectangle(ScaledPointZoom(19, 4), ScaledPointZoom(2, 4)),
            new Rectangle(ScaledPointZoom(21, 5), ScaledPointZoom(2, 2))
        };
        private static List<Rectangle> TopFaceAreaTL { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPointZoom(0, 0), ScaledPointZoom(1, 6)),
            new Rectangle(ScaledPointZoom(1, 0), ScaledPointZoom(2, 5)),
            new Rectangle(ScaledPointZoom(3, 0), ScaledPointZoom(2, 4)),
            new Rectangle(ScaledPointZoom(5, 0), ScaledPointZoom(2, 3)),
            new Rectangle(ScaledPointZoom(7, 0), ScaledPointZoom(2, 2)),
            new Rectangle(ScaledPointZoom(9, 0), ScaledPointZoom(2, 1))
        };
        private static List<Rectangle> TopFaceAreaTR { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPointZoom(13, 0), ScaledPointZoom(2, 1)),
            new Rectangle(ScaledPointZoom(15, 0), ScaledPointZoom(2, 2)),
            new Rectangle(ScaledPointZoom(17, 0), ScaledPointZoom(2, 3)),
            new Rectangle(ScaledPointZoom(19, 0), ScaledPointZoom(2, 4)),
            new Rectangle(ScaledPointZoom(21, 0), ScaledPointZoom(2, 5)),
            new Rectangle(ScaledPointZoom(23, 0), ScaledPointZoom(1, 6)),
        };
        private static List<Rectangle> TopFaceAreaBL { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPointZoom(0, 6), ScaledPointZoom(1, 6)),
            new Rectangle(ScaledPointZoom(1, 7), ScaledPointZoom(2, 5)),
            new Rectangle(ScaledPointZoom(3, 8), ScaledPointZoom(2, 4)),
            new Rectangle(ScaledPointZoom(5, 9), ScaledPointZoom(2, 3)),
            new Rectangle(ScaledPointZoom(7, 10), ScaledPointZoom(2, 2)),
            new Rectangle(ScaledPointZoom(9, 11), ScaledPointZoom(2, 1))
        };
        private static List<Rectangle> TopFaceAreaBR { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPointZoom(13, 11), ScaledPointZoom(2, 1)),
            new Rectangle(ScaledPointZoom(15, 10), ScaledPointZoom(2, 2)),
            new Rectangle(ScaledPointZoom(17, 9), ScaledPointZoom(2, 3)),
            new Rectangle(ScaledPointZoom(19, 8), ScaledPointZoom(2, 4)),
            new Rectangle(ScaledPointZoom(21, 7), ScaledPointZoom(2, 5)),
            new Rectangle(ScaledPointZoom(23, 6), ScaledPointZoom(1, 6)),
        };

        public static void ReconstructTopFaceArea()
        {
            TopFaceAreaC = new List<Rectangle>()
            {
                new Rectangle(ScaledPointZoom(1, 5), ScaledPointZoom(2, 2)),
                new Rectangle(ScaledPointZoom(3, 4), ScaledPointZoom(2, 4)),
                new Rectangle(ScaledPointZoom(5, 3), ScaledPointZoom(2, 6)),
                new Rectangle(ScaledPointZoom(7, 2), ScaledPointZoom(2, 8)),
                new Rectangle(ScaledPointZoom(9, 1), ScaledPointZoom(2, 10)),
                new Rectangle(ScaledPointZoom(11, 0), ScaledPointZoom(2, 12)),
                new Rectangle(ScaledPointZoom(13, 1), ScaledPointZoom(2, 10)),
                new Rectangle(ScaledPointZoom(15, 2), ScaledPointZoom(2, 8)),
                new Rectangle(ScaledPointZoom(17, 3), ScaledPointZoom(2, 6)),
                new Rectangle(ScaledPointZoom(19, 4), ScaledPointZoom(2, 4)),
                new Rectangle(ScaledPointZoom(21, 5), ScaledPointZoom(2, 2))
            };
            TopFaceAreaTL = new List<Rectangle>()
            {
                new Rectangle(ScaledPointZoom(0, 0), ScaledPointZoom(1, 6)),
                new Rectangle(ScaledPointZoom(1, 0), ScaledPointZoom(2, 5)),
                new Rectangle(ScaledPointZoom(3, 0), ScaledPointZoom(2, 4)),
                new Rectangle(ScaledPointZoom(5, 0), ScaledPointZoom(2, 3)),
                new Rectangle(ScaledPointZoom(7, 0), ScaledPointZoom(2, 2)),
                new Rectangle(ScaledPointZoom(9, 0), ScaledPointZoom(2, 1))
            };
            TopFaceAreaTR = new List<Rectangle>()
            {
                new Rectangle(ScaledPointZoom(13, 0), ScaledPointZoom(2, 1)),
                new Rectangle(ScaledPointZoom(15, 0), ScaledPointZoom(2, 2)),
                new Rectangle(ScaledPointZoom(17, 0), ScaledPointZoom(2, 3)),
                new Rectangle(ScaledPointZoom(19, 0), ScaledPointZoom(2, 4)),
                new Rectangle(ScaledPointZoom(21, 0), ScaledPointZoom(2, 5)),
                new Rectangle(ScaledPointZoom(23, 0), ScaledPointZoom(1, 6)),
            };
            TopFaceAreaBL = new List<Rectangle>()
            {
                new Rectangle(ScaledPointZoom(0, 6), ScaledPointZoom(1, 6)),
                new Rectangle(ScaledPointZoom(1, 7), ScaledPointZoom(2, 5)),
                new Rectangle(ScaledPointZoom(3, 8), ScaledPointZoom(2, 4)),
                new Rectangle(ScaledPointZoom(5, 9), ScaledPointZoom(2, 3)),
                new Rectangle(ScaledPointZoom(7, 10), ScaledPointZoom(2, 2)),
                new Rectangle(ScaledPointZoom(9, 11), ScaledPointZoom(2, 1))
            };
            TopFaceAreaBR = new List<Rectangle>()
            {
                new Rectangle(ScaledPointZoom(13, 11), ScaledPointZoom(2, 1)),
                new Rectangle(ScaledPointZoom(15, 10), ScaledPointZoom(2, 2)),
                new Rectangle(ScaledPointZoom(17, 9), ScaledPointZoom(2, 3)),
                new Rectangle(ScaledPointZoom(19, 8), ScaledPointZoom(2, 4)),
                new Rectangle(ScaledPointZoom(21, 7), ScaledPointZoom(2, 5)),
                new Rectangle(ScaledPointZoom(23, 6), ScaledPointZoom(1, 6)),
            };
        }


        public static Coord GetRelativeCoord(Coord coord, int altitude)
        {
            return coord + (new Coord(1, -1) * altitude);
        }

        public static Coord TopFace_PointToRelativeCoord(Point point, int altitude)
        {
            var normalizedPoint = TopFace_NormalizedPoint(point);
            var offset = TopFace_CoordOffset(normalizedPoint, point);

            var gridCoord = TopFace_PointToGridCoord(point);
            var coord = TopFace_GridCoordToCoord(gridCoord);

            return GetRelativeCoord(coord, altitude) + offset;
        }

        public static Coord TopFace_PointToRelativeCoord(SunbirdMBGame mainGame, int altitude)
        {
            return TopFace_PointToRelativeCoord(Peripherals.GetMouseWorldPosition(mainGame, mainGame.Camera), altitude);
        }

        public static Coord TopFace_PositionToRelativeCoord(Vector2 position, int altitude)
        {
            return TopFace_PointToRelativeCoord(position.ToPoint() * new Point(Zoom, Zoom) / new Point(Scale, Scale), altitude);
        }

        public static Coord TopFace_PointToCoord(Point point)
        {
            return TopFace_PointToRelativeCoord(point, 0);
        }

        public static Coord TopFace_PointToCoord(SunbirdMBGame mainGame)
        {
            return TopFace_PointToRelativeCoord(Peripherals.GetMouseWorldPosition(mainGame, mainGame.Camera), 0);
        }

        public static Coord TopFace_PositionToCoord(Vector2 position)
        {
            return TopFace_PointToCoord(position.ToPoint());
        }

        /// <summary>
        /// Maps a point in the world to a position in rectangle of size (Scaled(TopFaceGridWidth), Scaled(TopFaceGridWidth)), with the top left corner at the origin.
        /// </summary>
        /// <param name="gridCoord">Top Face grid coord.</param>
        /// <returns></returns>
        public static Point TopFace_NormalizedPoint(Point point)
        {
            var x = point.X % ScaledZoom(TopFaceGridWidth);
            if (x < 0)
            {
                x = ScaledZoom(TopFaceGridWidth) + x;
            }
            var y = point.Y % ScaledZoom(TopFaceGridHeight);
            if (y < 0)
            {
                y = ScaledZoom(TopFaceGridHeight) + y;
            }
            return new Point(x, y);
        }

        private static Coord TopFace_CoordOffset(Point point, Point worldPoint)
        {
            if (TopFaceAreaC.Any(rect => rect.Contains(point)))
            {
                return new Coord(0, 0);
            }
            else if (TopFaceAreaTL.Any(rect => rect.Contains(point)))
            {
                return new Coord(-1, 0);
            }
            else if (TopFaceAreaTR.Any(rect => rect.Contains(point)))
            {
                return new Coord(0, 1);
            }
            else if (TopFaceAreaBL.Any(rect => rect.Contains(point)))
            {
                return new Coord(0, -1);
            }
            else if (TopFaceAreaBR.Any(rect => rect.Contains(point)))
            {
                return new Coord(1, 0);
            }
            else
            {
                throw new Exception("TopFaceRectangle does not contain point. Check point is normalized and/or TopFaceRectangle components are correct.");
            }
        }

        /// <summary>
        /// Takes a point in the world and returns the corresponding Top Face grid coord.
        /// </summary>
        /// <param name="point">World position as a point.</param>
        /// <returns></returns>
        public static Coord TopFace_PointToGridCoord(Point point)
        {
            var x = point.X / ScaledZoom(TopFaceGridWidth);
            var xRem = point.X % ScaledZoom(TopFaceGridWidth); // We need remainder check to preserve grid edge schema after translation.
            if (point.X < 0 && xRem != 0)
            {
                x -= 1;
            }
            var y = point.Y / ScaledZoom(TopFaceGridHeight);
            var yRem = point.Y % ScaledZoom(TopFaceGridHeight);
            if (point.Y < 0 && yRem != 0)
            {
                y -= 1;
            }
            return new Coord(x, y);
        }

        /// <summary>
        /// Performs a 2x2 matrix multiplication on <paramref name="gridCoord"/>, where M = ([1 , 1], [1, -1]).
        /// </summary>
        /// <param name="gridCoord">Top Face grid coord.</param>
        /// <returns></returns>
        private static Coord TopFace_GridCoordToCoord(Coord gridCoord)
        {
            return new Coord(gridCoord.Y + gridCoord.X, gridCoord.Y * -1 + gridCoord.X);
        }

        /// <summary>
        /// Maps coord to local origin with respect to a Top Face grid.
        /// </summary>
        /// <param name="gridCoord">Top Face grid coord.</param>
        /// <returns></returns>
        public static Vector2 TopFace_CoordToLocalOrigin(Coord coord)
        {
            return ScaledVector(TopFaceGridWidth / 2, TopFaceGridHeight / 2) * coord.X + ScaledVector(TopFaceGridWidth / 2, -TopFaceGridHeight / 2) * coord.Y;
        }

        private static Point ScaledPoint(int x, int y)
        {
            return new Point(x, y) * new Point(Scale, Scale);
        }

        public static Point ScaledPoint(float x, float y)
        {
            return new Point((int)(x * ZoomRatio), (int)(y * ZoomRatio));
        }

        public static Point AntiScaledPoint(float x, float y)
        {
            return new Point((int)(x / ZoomRatio), (int)(y / ZoomRatio));
        }

        public static Point AntiScaledPoint(Point point)
        {
            float x = point.X;
            float y = point.Y;
            return new Point((int)(x / ZoomRatio), (int)(y / ZoomRatio));
        }

        public static Point ScaledPoint(Point point)
        {
            float x = point.X;
            float y = point.Y;
            return new Point((int)(x * ZoomRatio), (int)(y * ZoomRatio));
        }

        private static Point ScaledPointZoom(int x, int y)
        {
            return new Point(x, y) * new Point(Zoom, Zoom);
        }

        private static Vector2 ScaledVector(int x, int y)
        {
            return new Vector2(x, y) * Scale;
        }

        private static int Scaled(int value)
        {
            return value * Scale;
        }

        private static int ScaledZoom(int value)
        {
            return value * Zoom;
        }

        /// <summary>
        /// The master sorting algorithm for any collection sprites.
        /// </summary>
        public static IOrderedEnumerable<Sprite> Sort(XDictionary<int, SpriteList<Sprite>> layerMap)
        {
            var SL = new List<Sprite>() { };
            foreach (var layer in layerMap)
            {
                foreach (var sprite in layer.Value)
                {
                    SL.Add(sprite);
                }
            }
            return SL.OrderBy(x => (x.Coords.X - x.Coords.Y)).ThenBy(x => x.Altitude).ThenBy(x => x.DrawPriority);
        }
    }
}

