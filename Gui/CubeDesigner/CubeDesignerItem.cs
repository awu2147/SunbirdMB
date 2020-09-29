using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SunbirdMB.Gui
{
    public class CubeDesignerItem : CatalogItemBase
    {
        public CubeMetadata CubeMetadata { get; set; }

        public PropertyChangedEventHandler PropertyChangedHandler;

        public CubeDesignerItem(string imagePath, CubeMetadata cmd) : base(imagePath, new Int32Rect(0, 0, 72, 75))
        {
            CubeMetadata = cmd;
        }

        internal void Register()
        {
            PropertyChanged += PropertyChangedHandler;
        }

        internal void Unregister()
        {
            PropertyChanged -= PropertyChangedHandler;
        }

        internal override void MouseDown()
        {
            CubeDesignerViewModel.CurrentMetadata = CubeMetadata; //CubeFactory.CubeMetadataLibrary[ContentPath];
            MapBuilder.GhostMarker.MorphImage(CubeFactory.CreateCurrentCube(Coord.Zero, Coord.Zero, 0));
            Selected = true;
        }

    }
}
