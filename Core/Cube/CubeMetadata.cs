using Microsoft.Xna.Framework.Graphics;
using SunbirdMB.Framework;
using SunbirdMB.Gui;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SunbirdMB.Core
{
    public class CubeMetadata : PropertyChangedBase
    {
        public static readonly XmlSerializer CubeMetaDataSerializer = Serializer.CreateNew(typeof(CubeMetadata));

        [XmlIgnore]
        public Texture2D Texture;

        [XmlIgnore]
        public string Name { get { return ContentPath.Split('\\').Last(); } }
        public string ContentPath { get; set; }
        public CubePart Part { get; set; }

        private int sheetRows = 1;
        public int SheetRows
        {
            get { return sheetRows; }
            set
            {
                if (value < 1)
                {
                    SetProperty(ref sheetRows, 1);
                }
                else
                {
                    SetProperty(ref sheetRows, value);
                }
            }
        }

        private int sheetColumns = 1;
        public int SheetColumns
        {
            get { return sheetColumns; }
            set 
            {
                if (value < 1)
                {
                    SetProperty(ref sheetColumns, 1);
                }
                else
                {
                    SetProperty(ref sheetColumns, value);
                }
            }
        }

        private int frameCount = 1;
        public int FrameCount
        {
            get { return frameCount; }
            set
            {
                if (value < 1)
                {
                    SetProperty(ref frameCount, 1);
                }
                else
                {
                    SetProperty(ref frameCount, value);
                }
            }
        }

        private int startFrame = 1;
        public int StartFrame
        {
            get { return startFrame; }
            set
            {
                if (value < 1)
                {
                    SetProperty(ref startFrame, 1);
                }
                else
                {
                    SetProperty(ref startFrame, value);
                }
            }
        }

        private int currentFrame = 1;
        public int CurrentFrame
        {
            get { return currentFrame; }
            set
            {
                if (value < 1)
                {
                    SetProperty(ref currentFrame, 1);
                }
                else
                {
                    SetProperty(ref currentFrame, value);
                }
            }
        }

        private float frameSpeed = 0.133f;
        public float FrameSpeed
        {
            get { return frameSpeed; }
            set
            {
                if (value < 0.0166f)
                {
                    SetProperty(ref frameSpeed, 0.0166f);
                }
                else
                {
                    SetProperty(ref frameSpeed, value);
                }
            }
        }

        private AnimationState animState;
        public AnimationState AnimState
        {
            get { return animState; }
            set { SetProperty(ref animState, value); }
        }

        public CubeMetadata() { }

        internal void Register()
        {
            PropertyChanged += CubeMetaData_PropertyChanged;
        }

        private void CubeMetaData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            var appDirectory = appPath.TrimEnd(Path.GetFileName(appPath));
            var contentDirectory = Path.Combine(appDirectory, @"Content\");
            var metadataPath = Path.ChangeExtension(Path.Combine(contentDirectory, ContentPath), ".metadata");
            Serialize(metadataPath);
            MapBuilder.GhostMarker.MorphImage(CubeFactory.CreateCurrentCube(Coord.Zero, Coord.Zero, 0));
        }

        /// <summary>
        /// Call this to load texture content from path.
        /// </summary>
        public void LoadContent(IMainGame mainGame)
        {
            Texture = mainGame.Content.Load<Texture2D>(ContentPath);
            Register();
        }

        public void Serialize(string path)
        {
            Serializer.WriteXML<CubeMetadata>(CubeMetaDataSerializer, this, path);
        }
    }
}
