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

        internal override void LeftClick()
        {
            if (!ViewModel.IsSubLevel)
            {
                ViewModel.CurrentMetadata = DecoMetadata;
                Selection = SelectionMode.Selected;
                DeselectAllButThis();
            }
            else
            {
                Selection = SelectionMode.Active;
                DecoMetadata.ActiveFrames.Add(GetIndex());
                DeactivateAllButThis();
            }
            MapBuilder.GhostMarker.MorphImage(DecoFactory.CreateCurrentDeco(Coord.Zero, Coord.Zero, 0));
        }

        internal override void RightClick() { }

        internal override void LeftDoubleClick() { }

        internal override void RightDoubleClick()
        {
            if (!ViewModel.IsSubLevel)
            {
                ViewModel.EnterSubLevel(this);
            }
            else
            {
                ViewModel.ExitSubLevel();
            }
        }

        internal override void ShiftLeftClick() 
        {
            if (ViewModel.IsSubLevel)
            {
                if (Selection == SelectionMode.None)
                {
                    Selection = SelectionMode.Active;
                    DecoMetadata.ActiveFrames.Add(GetIndex());
                }
                else if (selection == SelectionMode.Active && DecoMetadata.ActiveFrames.Count > 1)
                {
                    Selection = SelectionMode.None;
                    DecoMetadata.ActiveFrames.Remove(GetIndex());
                }
                if (DecoMetadata.ActiveFrames.Count == 1)
                {
                    MapBuilder.GhostMarker.MorphImage(DecoFactory.CreateCurrentDeco(Coord.Zero, Coord.Zero, 0));
                }
            }
        }

        internal void DeselectAllButThis()
        {
            if (ViewModel.SelectedTab.Header.ToString() == DecoCatalogViewModel._1x1x1)
            {
                foreach (var item in ViewModel.Deco1x1x1Collection)
                {
                    if (item != this)
                    {
                        item.Selection = SelectionMode.None;
                    }
                }
            }
        }

        internal void DeactivateAllButThis()
        {
            if (ViewModel.SelectedTab.Header.ToString() == DecoCatalogViewModel._1x1x1)
            {
                foreach (var item in ViewModel.Deco1x1x1Collection)
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
