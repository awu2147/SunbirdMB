using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

namespace SunbirdMB.Core
{
    [Serializable]
    public class SpriteSheet
    {
        /// <summary>
        /// Returns a map of frame positions relative to the texture; key = frameNumber, value = position.
        /// </summary>
        [XmlIgnore]
        public Dictionary<int, Point> PositionMap { get; set; }

        /// <summary>
        /// The <see cref ="SpriteSheet"/> texture.
        /// </summary>
        [XmlIgnore]
        public Texture2D Texture;

        /// <summary>
        /// Path to the .xnb texture. Used during content loading and to retrieve cached textures.
        /// </summary>
        public string TexturePath { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int FrameHeight { get { return Texture.Height / Rows; } }
        public int FrameWidth { get { return Texture.Width / Columns; } }

        private SpriteSheet() { }

        public SpriteSheet(Texture2D texture, int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Texture = texture;
            PositionMap = ConstructPositionMap();
        }

        public void LoadContent(ContentManager content)
        {
            Texture = content.Load<Texture2D>(TexturePath);
            PositionMap = ConstructPositionMap();
        }

        public static SpriteSheet CreateNew(Texture2D texture, string path, int row, int columns)
        {
            return new SpriteSheet(texture, row, columns) { TexturePath = path };
        }

        public static SpriteSheet CreateNew(Texture2D texture, string path)
        {
            return CreateNew(texture, path, 1, 1);
        }

        /// <summary>
        /// This overload creates garbage if called more than once. Use with Care.
        /// </summary>
        public static SpriteSheet CreateNew(SunbirdMBGame mainGame, string path, int row, int columns)
        {
            //FileStream fileStream = new FileStream($@"..\..\..\..\Content\{path}.png", FileMode.Open);
            //Texture2D t = Texture2D.FromStream(mainGame.GraphicsDevice, fileStream);
            //fileStream.Dispose();
            //return new SpriteSheet(t, row, columns) { TexturePath = path };
            return new SpriteSheet(mainGame.Content.Load<Texture2D>(path), row, columns) { TexturePath = path };
        }

        /// <summary>
        /// This overload creates garbage if called more than once. Use with Care.
        /// </summary>
        public static SpriteSheet CreateNew(SunbirdMBGame mainGame, string path)
        {
            return CreateNew(mainGame, path, 1, 1);
        }

        public Dictionary<int, Point> ConstructPositionMap()
        {
            var positionMap = new Dictionary<int, Point>();

            var columnlist = new List<int>() { };
            for (int j = 0; j < Rows; j++)
            {
                for (int i = 0; i < Columns; i++)
                    columnlist.Add(i);
            }

            var rowlist = new List<int>() { };
            for (int j = 0; j < Rows; j++)
            {
                for (int i = 0; i < Columns; i++)
                    rowlist.Add(j);
            }

            for (int i = 0; i < columnlist.Count(); i++)
            {
                positionMap.Add(i, new Point(columnlist[i] * FrameWidth, rowlist[i] * FrameHeight));
            }

            return positionMap;
        }

    }
}

