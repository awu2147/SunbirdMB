using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SunbirdMB.Gui
{
    public class CatalogItemBase : PropertyChangedBase
    {
        private string imagePath;
        public string ImagePath
        {
            get { return imagePath; }
            set { SetProperty(ref imagePath, value); }
        }

        private Int32Rect sourceRect;
        public Int32Rect SourceRect
        {
            get { return sourceRect; }
            set { SetProperty(ref sourceRect, value); }
        }

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set { SetProperty(ref selected, value); }
        }

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
            Selected = true;
        }
    }

}
