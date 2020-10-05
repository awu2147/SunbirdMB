using SunbirdMB.Core;
using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SunbirdMB.Gui
{
    public class DecoCatalogItem : MetadataItemBase
    {
        private readonly DecoCatalogManager CatalogManager;
        internal readonly DecoMetadata DecoMetadata;

        public override int ItemWidth
        {
            get { return itemWidth; }
            set { SetProperty(ref itemWidth, value); }
        }

        public override int ItemHeight
        {
            get { return itemHeight; }
            set { SetProperty(ref itemHeight, value); }
        }

        internal override MetadataBase Metadata { get { return DecoMetadata; } }

        internal DecoCatalogItem(DecoCatalogManager collectionManager, string imagePath, DecoMetadata dmd, int itemWidth, int itemHeight) : base(imagePath, dmd)
        {
            CatalogManager = collectionManager;
            DecoMetadata = dmd;
            ItemWidth = itemWidth;
            ItemHeight = itemHeight;
            SourceRect = new Int32Rect(0, 0, itemWidth, itemHeight);
        }

        internal override void LeftClick()
        {
            if (!CatalogManager.IsLocalSubLevel)
            {
                CatalogManager.ViewModel.CurrentMetadata = DecoMetadata;
                Selection = SelectionMode.Selected;
                DeselectAllButThis();
            }
            else
            {
                Selection = SelectionMode.Active;
                DecoMetadata.ActiveFrames.Add(GetIndex());
                DeactivateAllButThis();
            }
            MapBuilder.GhostMarker.MorphCurrentDeco();
        }

        internal override void RightClick() { }

        internal override void LeftDoubleClick() { }

        internal override void RightDoubleClick()
        {
            if (!CatalogManager.IsLocalSubLevel)
            {
                CatalogManager.EnterSubLevel(this);
            }
            else
            {
                CatalogManager.ExitSubLevel();
            }
        }

        internal override void ShiftLeftClick() 
        {
            if (CatalogManager.IsLocalSubLevel)
            {
                if (Selection == SelectionMode.None)
                {
                    Selection = SelectionMode.Active;
                    DecoMetadata.ActiveFrames.Add(GetIndex());
                }
                else 
                if (Selection == SelectionMode.Active && DecoMetadata.ActiveFrames.Count > 1)
                {
                    Selection = SelectionMode.None;
                    DecoMetadata.ActiveFrames.Remove(GetIndex());
                }
                if (DecoMetadata.ActiveFrames.Count == 1)
                {
                    MapBuilder.GhostMarker.MorphCurrentDeco();
                }
            }
        }

        internal void DeselectAllButThis()
        {
            foreach (var item in CatalogManager.DecoCollection)
            {
                if (item != this)
                {
                    item.Selection = SelectionMode.None;
                }
            }
        }

        internal void DeactivateAllButThis()
        {
            foreach (var item in CatalogManager.DecoCollection)
            {
                if (item != this)
                {
                    item.Selection = SelectionMode.None;
                    RemoveFromActive(item);
                }
            }
        }        

    }   

}
