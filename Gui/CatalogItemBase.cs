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

        public ICommand C_LeftClick { get; set; }
        public ICommand C_LeftDoubleClick { get; set; }
        public ICommand C_ShiftLeftClick { get; set; }
        public ICommand C_RightClick { get; set; }
        public ICommand C_RightDoubleClick { get; set; }

        public CatalogItemBase(string imagePath) : this(imagePath, Int32Rect.Empty) { }

        public CatalogItemBase(string imagePath, Int32Rect sourceRect)
        {
            ImagePath = imagePath;
            SourceRect = sourceRect;
            C_LeftClick = new RelayCommand((o) => LeftClick());
            C_LeftDoubleClick = new RelayCommand((o) => LeftDoubleClick());
            C_ShiftLeftClick = new RelayCommand((o) => ShiftLeftClick());
            C_RightClick = new RelayCommand((o) => RightClick());
            C_RightDoubleClick = new RelayCommand((o) => RightDoubleClick());
        }

        internal virtual void LeftClick()
        {
            $"LeftClick".Log();
        }

        internal virtual void LeftDoubleClick()
        {
            $"LeftDoubleClick".Log();
        }

        internal virtual void ShiftLeftClick()
        {
            $"ShiftLeftClick".Log();
        }

        internal virtual void RightClick()
        {
            $"RightClick".Log();
        }

        internal virtual void RightDoubleClick()
        {
            $"RightDoubleClick".Log();
        }

    }
}
