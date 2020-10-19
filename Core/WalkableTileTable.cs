using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Core
{
    public class WalkableTileTable : XDictionary<int, XDictionary<Coord2D, HashSet<Coord2D>>>
    {
        public WalkableTileTable()
        {

        }

        public void Add(int altitude, Coord2D adjacentCoord, Coord2D walkableCoord)
        {
            if (!base.ContainsKey(altitude))
            {
                base.Add(altitude, new XDictionary<Coord2D, HashSet<Coord2D>>());
            }
            if (!base[altitude].ContainsKey(adjacentCoord))
            {
                base[altitude].Add(adjacentCoord, new HashSet<Coord2D>());
            }
            base[altitude][adjacentCoord].Add(walkableCoord);
        }

        public void Remove(int altitude, Coord2D adjacentCoord, Coord2D walkableCoord)
        {
            if (base.ContainsKey(altitude) && base[altitude].ContainsKey(adjacentCoord) && base[altitude][adjacentCoord].Contains(walkableCoord))
            {             
                base[altitude][adjacentCoord].Remove(walkableCoord);
                if (base[altitude][adjacentCoord].Count() == 0)
                {
                    base[altitude].Remove(adjacentCoord);
                }
            }
        }

    }
}
