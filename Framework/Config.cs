using Microsoft.Xna.Framework.Input;
using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace SunbirdMB.Core
{
    public class Config
    {
        public static readonly XmlSerializer ConfigSerializer = Serializer.CreateNew(typeof(Config));

        public Config() { }

        internal void Serialize()
        {
            Serializer.WriteXML<Config>(ConfigSerializer, this, "Config.xml");
        }

        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public double WindowLeft { get; set; }
        public double WindowTop { get; set; }

        internal void LoadApplicationParameters(SunbirdMBWindow window)
        {
            window.Height = WindowHeight;
            window.Width = WindowWidth;
            window.Left = WindowLeft;
            window.Top = WindowTop;
        }

        internal void SaveApplicationParameters(SunbirdMBWindow window)
        {
            WindowHeight = window.Height;
            WindowWidth = window.Width;
            WindowLeft = window.Left;
            WindowTop = window.Top;
        }

        public int WorldZoom { get; set; } = 3;

        internal void LoadGameParameters(SunbirdMBGame game)
        {
            World.Zoom = WorldZoom;
        }

        internal void SaveGameParameters(SunbirdMBGame game)
        {
            WorldZoom = World.Zoom;
        }

    }
}
