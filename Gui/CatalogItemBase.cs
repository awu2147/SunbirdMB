using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SunbirdMB.Gui
{
    public abstract class CatalogItemBase : PropertyChangedBase
    {
        public PropertyChangedEventHandler PropertyChangedHandler;

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

        public CatalogItemBase(string imagePath)
        {
            ImagePath = imagePath;
            C_LeftClick = new RelayCommand((o) => LeftClick());
            C_LeftDoubleClick = new RelayCommand((o) => LeftDoubleClick());
            C_ShiftLeftClick = new RelayCommand((o) => ShiftLeftClick());
            C_RightClick = new RelayCommand((o) => RightClick());
            C_RightDoubleClick = new RelayCommand((o) => RightDoubleClick());
        }

        internal void Register()
        {
            PropertyChanged += PropertyChangedHandler;
        }

        internal void Unregister()
        {
            PropertyChanged -= PropertyChangedHandler;
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

    public abstract class MetadataItemBase : CatalogItemBase
    {
        internal abstract int ItemWidth { get; set; } 
        internal abstract int ItemHeight { get; set; }

        private readonly MetadataBase Metadata;

        public MetadataItemBase(string imagePath, MetadataBase md) : base(imagePath)
        {
            Metadata = md;
        }

        internal int GetIndex()
        {
            int xPos = SourceRect.X / ItemWidth;
            int yPos = SourceRect.Y / ItemHeight;
            int index = Metadata.SheetColumns * yPos + xPos;
            return index + 1;
        }

        internal int GetIndex(MetadataItemBase mib)
        {
            int xPos = mib.SourceRect.X / mib.ItemWidth;
            int yPos = mib.SourceRect.Y / mib.ItemHeight;
            int index = mib.Metadata.SheetColumns * yPos + xPos;
            return index + 1;
        }

        internal void RemoveFromActive(MetadataItemBase mib)
        {
            if (Metadata.ActiveFrames.Contains(GetIndex(mib)))
            {
                Metadata.ActiveFrames.Remove(GetIndex(mib));
            }
        }

    }
}
