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
    public class CubeDesignerItem : CatalogItemBase, IContent //, IDisposable
    {
        public CubeDesignerViewModel CubeDesigner { get; set; }
        public string ContentPath { get; set; }

        //internal PropertyChangedEventHandler CubeDesignerItem_PropertyChanged;

        public CubeDesignerItem(CubeDesignerViewModel cubeDesigner, string imagePath, string contentPath) : base(imagePath, new Int32Rect(0, 0, 72, 75))
        {
            CubeDesigner = cubeDesigner;
            ContentPath = contentPath;
        }

        internal override void MouseDown()
        {
            CubeFactory.SetCurrent(ContentPath);
            CubeDesigner.CurrentMetadata = CubeFactory.CubeMetadataLibrary[ContentPath];
            Selected = true;
        }

        //public void Dispose()
        //{
        //    PropertyChanged -= CubeDesignerItem_PropertyChanged;
        //}
    }
}
