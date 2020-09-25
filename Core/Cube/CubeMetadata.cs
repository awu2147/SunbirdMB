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
        public string ContentPath { get; set; }
        public CubePart Part { get; set; }

        private int sheetRows = 1;
        public int SheetRows
        {
            get { return sheetRows; }
            set { SetProperty(ref sheetRows, value); }
        }

        private int sheetColumns = 1;
        public int SheetColumns
        {
            get { return sheetColumns; }
            set { SetProperty(ref sheetColumns, value); }
        }

        private int frameCount = 1;
        public int FrameCount
        {
            get { return frameCount; }
            set { SetProperty(ref frameCount, value); }
        }

        private int startFrame = 0;
        public int StartFrame
        {
            get { return startFrame; }
            set { SetProperty(ref startFrame, value); }
        }

        private float frameSpeed = 0.133f;
        public float FrameSpeed
        {
            get { return frameSpeed; }
            set { SetProperty(ref frameSpeed, value); }
        }

        public int CurrentFrame { get; set; } = 0;

        public AnimationState AnimState { get; set; } = AnimationState.None;

        public CubeMetadata() { }

        internal void SubscribeHandlers()
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
        }

        /// <summary>
        /// Call this to load texture content from path.
        /// </summary>
        public void LoadContent(IMainGame mainGame)
        {
            Texture = mainGame.Content.Load<Texture2D>(ContentPath);
            SubscribeHandlers();
        }

        public void Serialize(string path)
        {
            Serializer.WriteXML<CubeMetadata>(CubeMetaDataSerializer, this, path);
        }
    }
}
