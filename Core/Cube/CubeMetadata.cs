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
    public class CubeMetadata : MetadataBase
    {
        public static readonly XmlSerializer CubeMetadataSerializer = Serializer.CreateNew(typeof(CubeMetadata));    

        public CubeMetadata() { }

        internal override void Register()
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

        public void Serialize(string path)
        {
            Serializer.WriteXML<CubeMetadata>(CubeMetadataSerializer, this, path);
        }
    }
}
