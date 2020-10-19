using SunbirdMB.Core;
using SunbirdMB.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Framework
{
    public static class Extensions
    {
        public static void Log(this string message)
        {
            var time = LoggerViewModel.TimeNow();
            LoggerViewModel.Log.Add(new LogMessage(time, message));
            if (LoggerViewModel.Log.Count > 200)
            {
                LoggerViewModel.Log.RemoveAt(0);
            }
        }

        public static string TrimEnd(this string source, string value)
        {
            if (!source.EndsWith(value))
                return source;

            return source.Remove(source.LastIndexOf(value));
        }

        public static void Add(this XDictionary<Coord3D, SpriteList<Sprite>> layerMap, Coord3D coord, Sprite item)
        {
            if (!layerMap.ContainsKey(coord))
            {
                layerMap.Add(coord, new SpriteList<Sprite>());
            }
            layerMap[coord].Add(item);
        }

        public static void Remove(this XDictionary<Coord3D, SpriteList<Sprite>> layerMap, Coord3D coord, Sprite item)
        {
            if (layerMap[coord].Contains(item))
            {
                layerMap[coord].Remove(item);
            }
        }

        public static List<Coord2D> AdjacentCoords(this Coord2D walkableCoord)
        {
            List<Coord2D> adjacentCoords = new List<Coord2D>()
            {
                walkableCoord + new Coord2D(0, 1),
                walkableCoord + new Coord2D(0, -1),
                walkableCoord + new Coord2D(1, 0),
                walkableCoord + new Coord2D(-1, 0),
                walkableCoord + new Coord2D(1, 1),
                walkableCoord + new Coord2D(1, -1),
                walkableCoord + new Coord2D(-1, -1),
                walkableCoord + new Coord2D(-1, 1),
            };
            return adjacentCoords;
        }

    }

    public class StringChangedEventArgs : EventArgs
    {
        public string NewString { get; set; }

        public StringChangedEventArgs(string newString)
        {
            NewString = newString;
        }
    }

    public delegate void StringChangedEventHandler(object sender, StringChangedEventArgs args);
}
