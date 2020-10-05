using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace SunbirdMB.Gui
{
    public abstract class CatalogItemBase : PropertyChangedBase
    {
        protected int itemWidth;
        protected int itemHeight;
        private string imagePath;
        private Int32Rect sourceRect;

        /// <summary>
        /// The width of the item element when displayed in the gui.
        /// </summary>
        public abstract int ItemWidth { get; set; }

        /// <summary>
        /// The height of the item element when displayed in the gui.
        /// </summary>
        public abstract int ItemHeight { get; set; }

        /// <summary>
        /// The full path to the item element image.
        /// </summary>
        public string ImagePath
        {
            get { return imagePath; }
            set { SetProperty(ref imagePath, value); }
        }

        /// <summary>
        /// The source rectangle used to view the item element image.
        /// </summary>
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

        internal virtual void LeftClick() { $"LeftClick".Log(); }

        internal virtual void LeftDoubleClick() { $"LeftDoubleClick".Log(); }

        internal virtual void ShiftLeftClick() { $"ShiftLeftClick".Log(); }

        internal virtual void RightClick() { $"RightClick".Log(); }

        internal virtual void RightDoubleClick() { $"RightDoubleClick".Log(); }

    }
   
}
