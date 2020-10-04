using SunbirdMB.Core;
using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SunbirdMB.Gui
{
    public abstract class DecoCatalogItem : MetadataItemBase
    {
        private DecoCatalogViewModel ViewModel;

        public DecoMetadata DecoMetadata { get; set; }

        private SelectionMode selection;
        public SelectionMode Selection
        {
            get { return selection; }
            set { SetProperty(ref selection, value); }
        }

        internal DecoCatalogItem(DecoCatalogViewModel viewModel, string imagePath, DecoMetadata dmd) : base(imagePath, dmd)
        {
            ViewModel = viewModel;
            DecoMetadata = dmd;
        }

        internal override void LeftClick() { }

        internal override void RightClick() { }

        internal override void LeftDoubleClick() { }

        internal override void RightDoubleClick() { }

        internal override void ShiftLeftClick() { }

    }

    public class DecoCatalogItem1x1x1 : DecoCatalogItem
    {
        internal override int ItemWidth { get; set; } = 72;
        internal override int ItemHeight { get; set; } = 75;

        internal DecoCatalogItem1x1x1(DecoCatalogViewModel viewModel, string imagePath, DecoMetadata dmd) : base(viewModel, imagePath, dmd) 
        {
            SourceRect = new Int32Rect(0, 0, 72, 75);
        }
    }

}
