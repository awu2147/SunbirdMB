using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Core
{
    public class Deco : Sprite, IWorldObject
    {
        public XDictionary<int, HashSet<Coord>> OccupiedCoords { get; set; } = new XDictionary<int, HashSet<Coord>>();
        public Dimension Dimensions { get; set; }

        public Deco() { }

    }
}
