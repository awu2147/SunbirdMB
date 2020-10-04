using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace SunbirdMB.Core
{
    public class DecoMetadata : MetadataBase
    {
        public static readonly XmlSerializer DecoMetadataSerializer = Serializer.CreateNew(typeof(DecoMetadata));

        public string TypeName { get; set; } = typeof(Deco).FullName;
        public Vector2 PositionOffset { get; set; }
        public Dimension Dimensions { get; set; }

        public DecoMetadata() { }

        internal override void Register()
        {
            PropertyChanged += DecoMetaData_PropertyChanged;
        }

        private void DecoMetaData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var metadataPath = Path.ChangeExtension(Path.Combine(UriHelper.ContentDirectory, ContentPath), ".metadata");
            Serialize(metadataPath);
            MapBuilder.GhostMarker.MorphImage(DecoFactory.CreateCurrentDeco(Coord.Zero, Coord.Zero, 0));
        }

        public void Serialize(string path)
        {
            Serializer.WriteXML<CubeMetadata>(DecoMetadataSerializer, this, path);
        }

    }
}
