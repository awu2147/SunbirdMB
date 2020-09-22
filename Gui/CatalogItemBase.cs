using SunbirdMB.Framework;
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
        public string Image { get; set; }

        public Int32Rect SourceRect { get; set; }

        public ICommand C_MouseDown { get; set; }

        public CatalogItemBase(string imagePath) : this(imagePath, Int32Rect.Empty) { }

        public CatalogItemBase(string imagePath, Int32Rect sourceRect)
        {
            Image = imagePath;
            SourceRect = sourceRect;
            C_MouseDown = new RelayCommand((o) => MouseDown());
        }

        internal virtual void MouseDown()
        {
            $"Clicked {Path.GetFileName(Image)}".Log();
        }
    }

    public class CubeBaseItem : CatalogItemBase
    {
        public CubeBaseItem(string image) : base(image, new Int32Rect(0, 0, 72, 75))
        {

        }
    }
}
