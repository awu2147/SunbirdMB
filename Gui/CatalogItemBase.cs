using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SunbirdMB.Gui
{
    public class CatalogItemBase
    {
        public string ImagePath { get; set; }

        public Int32Rect SourceRect { get; set; }

        public ICommand C_MouseDown { get; set; }

        public CatalogItemBase(string imagePath) : this(imagePath, Int32Rect.Empty) { }

        public CatalogItemBase(string imagePath, Int32Rect sourceRect)
        {
            ImagePath = imagePath;
            SourceRect = sourceRect;
            C_MouseDown = new RelayCommand((o) => MouseDown());
        }

        internal virtual void MouseDown()
        {
            $"Clicked {Path.GetFileName(ImagePath)}".Log();
        }
    }

    public class CubeCatalogItem : CatalogItemBase, IContent
    {
        public string ContentPath { get; set; }
        public CubeCatalogItem(string imagePath, string contentPath) : base(imagePath, new Int32Rect(0, 0, 72, 75))
        {
            ContentPath = contentPath;
        }

        internal override void MouseDown()
        {
            $"Clicked {Path.GetFileName(ImagePath)}".Log();
            CubeFactory.SetCurrent(ContentPath);
            CubeDesignerViewModel.SetPropertyGridDataContext(CubeFactory.CubeMetaDataLibrary[ContentPath]);
        }
    }
}
