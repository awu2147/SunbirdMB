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
        public HashSet<Coord3D> OccupiedCoords { get; set; } = new HashSet<Coord3D>();
        public Dimension Dimensions { get; set; }

        public Deco() { }

    }
}
