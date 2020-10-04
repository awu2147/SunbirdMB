using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows;

namespace SunbirdMB.Gui
{
    public class CubeDesignerItem : MetadataItemBase
    {
        private CubeDesignerViewModel ViewModel;
        internal override int ItemWidth { get; set; } = 72;
        internal override int ItemHeight { get; set; } = 75;

        public readonly CubeMetadata CubeMetadata;

        private SelectionMode selection;
        public SelectionMode Selection
        {
            get { return selection; }
            set { SetProperty(ref selection, value); }
        }

        public CubeDesignerItem(CubeDesignerViewModel viewModel, string imagePath, CubeMetadata cmd) : base(imagePath, cmd)
        {
            SourceRect = new Int32Rect(0, 0, 72, 75);
            ViewModel = viewModel;
            CubeMetadata = cmd;
        }

        internal override void LeftClick()
        {
            if (!ViewModel.IsSubLevel)
            {
                ViewModel.CurrentMetadata = CubeMetadata;
                Selection = SelectionMode.Selected;
                DeselectAllButThis();
            }
            else
            {
                Selection = SelectionMode.Active;
                CubeMetadata.ActiveFrames.Add(GetIndex());
                DeactivateAllButThis();                
            }
            MapBuilder.GhostMarker.MorphImage(CubeFactory.CreateCurrentCube(Coord.Zero, Coord.Zero, 0));
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
                    CubeMetadata.ActiveFrames.Add(GetIndex());
                }
                else if (selection == SelectionMode.Active && CubeMetadata.ActiveFrames.Count > 1)
                {
                    Selection = SelectionMode.None;
                    CubeMetadata.ActiveFrames.Remove(GetIndex());
                }
                if (CubeMetadata.ActiveFrames.Count == 1)
                {
                    MapBuilder.GhostMarker.MorphImage(CubeFactory.CreateCurrentCube(Coord.Zero, Coord.Zero, 0));
                }
            }
        }

        internal void DeselectAllButThis()
        {
            if (ViewModel.SelectedTab.Header.ToString() == CubePart.Top.ToString())
            {
                foreach (var item in ViewModel.CubeTopCollection)
                {
                    if (item != this)
                    {
                        item.Selection = SelectionMode.None;
                    }
                }
            }
            else if (ViewModel.SelectedTab.Header.ToString() == CubePart.Base.ToString())
            {
                foreach (var item in ViewModel.CubeBaseCollection)
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
            if (ViewModel.SelectedTab.Header.ToString() == CubePart.Top.ToString())
            {
                foreach (var item in ViewModel.CubeTopCollection)
                {
                    if (item != this)
                    {
                        item.Selection = SelectionMode.None;
                        RemoveFromActive(item);
                    }
                }
            }
            else if (ViewModel.SelectedTab.Header.ToString() == CubePart.Base.ToString())
            {
                foreach (var item in ViewModel.CubeBaseCollection)
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
}
