using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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

        internal override void Register()
        {
            base.Register();
        }

        public DecoMetadata() { }

        public void Serialize(string path)
        {
            Serializer.WriteXML<CubeMetadata>(DecoMetadataSerializer, this, path);
        }

    }
}
