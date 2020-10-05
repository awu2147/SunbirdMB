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
                else 
                if (selection == SelectionMode.Active && DecoMetadata.ActiveFrames.Count > 1)
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
            Deselect(DecoCatalogViewModel._1x1x1, ViewModel.Deco1x1x1Collection);
            Deselect(DecoCatalogViewModel._1x1x2, ViewModel.Deco1x1x2Collection);
            Deselect(DecoCatalogViewModel._1x1x3, ViewModel.Deco1x1x3Collection);
        }

        internal void Deselect(string dimensions, ObservableCollection<DecoCatalogItem> collection)
        {
            if (ViewModel.SelectedTab.Header.ToString() == dimensions)
            {
                foreach (var item in collection)
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
            Deactivate(DecoCatalogViewModel._1x1x1, ViewModel.Deco1x1x1Collection);
            Deactivate(DecoCatalogViewModel._1x1x2, ViewModel.Deco1x1x2Collection);
            Deactivate(DecoCatalogViewModel._1x1x3, ViewModel.Deco1x1x3Collection);
        }

        internal void Deactivate(string dimensions, ObservableCollection<DecoCatalogItem> collection)
        {
            if (ViewModel.SelectedTab.Header.ToString() == dimensions)
            {
                foreach (var item in collection)
                {
                    if (item != this)
                    {
                        item.Selection = SelectionMode.None;
                        RemoveFromActive(item);
                    }
                }
            }
        }

        internal static DecoCatalogItem CreateNew(DecoCatalogViewModel viewModel, string imagePath, DecoMetadata dmd, string dimensions)
        {
            switch (dimensions)
            {
                case DecoCatalogViewModel._1x1x1:
                    return new DecoCatalogItem1x1x1(viewModel, imagePath, dmd);
                case DecoCatalogViewModel._1x1x2:
                    return new DecoCatalogItem1x1x2(viewModel, imagePath, dmd);
                case DecoCatalogViewModel._1x1x3:
                    return new DecoCatalogItem1x1x3(viewModel, imagePath, dmd);
                default:
                    return null;
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
    public class DecoCatalogItem1x1x2 : DecoCatalogItem
    {
        internal override int ItemWidth { get; set; } = 72;
        internal override int ItemHeight { get; set; } = 111;

        internal DecoCatalogItem1x1x2(DecoCatalogViewModel viewModel, string imagePath, DecoMetadata dmd) : base(viewModel, imagePath, dmd)
        {
            SourceRect = new Int32Rect(0, 0, 72, 111);
        }
    }
    public class DecoCatalogItem1x1x3 : DecoCatalogItem
    {
        internal override int ItemWidth { get; set; } = 72;
        internal override int ItemHeight { get; set; } = 147;

        internal DecoCatalogItem1x1x3(DecoCatalogViewModel viewModel, string imagePath, DecoMetadata dmd) : base(viewModel, imagePath, dmd)
        {
            SourceRect = new Int32Rect(0, 0, 72, 147);
        }
    }

}
