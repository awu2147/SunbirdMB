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
using System.Xml.Schema;
using SunbirdMB.Framework;

namespace SunbirdMB.Core
{
    public class SpriteList<T> : List<T>, IXmlSerializable
    {   
        public static readonly XmlSerializer SpriteListSerializer = Serializer.CreateNew(typeof(T));

        /// <summary>
        /// Current set of occupied coords.
        /// </summary>
        public HashSet<Coord> OccupiedCoords { get; set; } = new HashSet<Coord>();

        /// <summary>
        /// Add to list if coord unoccupied.
        /// </summary>
        /// <param name="sprite">The sprite to be added.</param>
        public void AddCheck(T sprite, int altitude)
        {
            if (sprite is Cube)
            {
                var cube = sprite as Cube;
                if (!OccupiedCoords.Contains(cube.Coords))
                {
                    Add(sprite);
                    OccupiedCoords.Add(cube.Coords);
                }
            }
            else if (sprite is Deco)
            {
                var deco = sprite as Deco;
                if (deco.OccupiedCoords[altitude].Any((x) => OccupiedCoords.Contains(x)) == false)
                {
                    Add(sprite);
                    // FIXME: only adds current altitude level.
                    foreach (var coord in deco.OccupiedCoords[altitude])
                    {
                        OccupiedCoords.Add(coord);
                    }
                }
            }
            else
            {
                throw new Exception("AddCheck called on a sprite that is not a Cube or Deco, is this correct?");
            }
        }

        /// <summary>
        /// Remove from list if coord occupied. Call this instead of Remove() where appropriate for extra safety.
        /// </summary>
        /// <param name="sprite">The sprite to be removed.</param>
        public void RemoveCheck(T sprite, int altitude)
        {
            if (sprite is Cube)
            {
                var cube = sprite as Cube;
                if (OccupiedCoords.Contains(cube.Coords))
                {
                    Remove(sprite);
                    OccupiedCoords.Remove(cube.Coords);
                }
            }
            else if (sprite is Deco)
            {
                var deco = sprite as Deco;
                if (OccupiedCoords.Contains(deco.Coords))
                {
                    Remove(sprite);
                    foreach (var coord in deco.OccupiedCoords[altitude])
                    {
                        OccupiedCoords.Remove(coord);
                    }
                }
            }
            else
            {
                throw new Exception("RemoveCheck called on a sprite that is not a Cube or Deco, is this correct?");
            }
        }

        public XmlSchema GetSchema() { return null; }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer occupiedCoordsSerializer = new XmlSerializer(typeof(HashSet<Coord>));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.Name == "Sprite")
                {
                    reader.ReadStartElement("Sprite");
                    T sprite = (T)SpriteListSerializer.Deserialize(reader);
                    reader.ReadEndElement();
                    this.Add(sprite);
                }

                else if (reader.Name == "OccupiedCoords")
                {
                    reader.ReadStartElement("OccupiedCoords");
                    OccupiedCoords = (HashSet<Coord>)occupiedCoordsSerializer.Deserialize(reader);
                    reader.ReadEndElement();
                }

                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer occupiedCoordsSerializer = new XmlSerializer(typeof(HashSet<Coord>));

            foreach (var sprite in this)
            {
                writer.WriteStartElement("Sprite");
                SpriteListSerializer.Serialize(writer, sprite);
                writer.WriteEndElement();
            }

            writer.WriteStartElement("OccupiedCoords");
            occupiedCoordsSerializer.Serialize(writer, OccupiedCoords);
            writer.WriteEndElement();
        }

    }
}

