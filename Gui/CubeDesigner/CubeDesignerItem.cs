using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System.ComponentModel;
using System.Windows;

namespace SunbirdMB.Gui
{
    public class CubeDesignerItem : CatalogItemBase
    {
        private const int ViewWidth = 72;
        private const int ViewHeight = 75;

        public CubeMetadata CubeMetadata { get; set; }

        public PropertyChangedEventHandler PropertyChangedHandler;

        private SelectionMode selection;
        public SelectionMode Selection
        {
            get { return selection; }
            set { SetProperty(ref selection, value); }
        }

        public CubeDesignerItem(string imagePath, CubeMetadata cmd) : base(imagePath, new Int32Rect(0, 0, 72, 75))
        {
            CubeMetadata = cmd;
        }

        internal void Register()
        {
            PropertyChanged += PropertyChangedHandler;
        }

        internal void Unregister()
        {
            PropertyChanged -= PropertyChangedHandler;
        }

        internal int GetIndex()
        {
            int xPos = SourceRect.X / ViewWidth;
            int yPos = SourceRect.Y / ViewHeight;
            int index = CubeMetadata.SheetColumns * yPos + xPos;
            return index + 1;
        }

        internal int GetIndex(CubeDesignerItem cdi)
        {
            int xPos = cdi.SourceRect.X / ViewWidth;
            int yPos = cdi.SourceRect.Y / ViewHeight;
            int index = cdi.CubeMetadata.SheetColumns * yPos + xPos;
            return index + 1;
        }

        internal void RemoveFromActive(CubeDesignerItem cdi)
        {
            if (CubeMetadata.ActiveFrames.Contains(GetIndex(cdi)))
            {
                CubeMetadata.ActiveFrames.Remove(GetIndex(cdi));
            }
        }

        internal override void LeftClick()
        {
            if ((!CubeDesignerViewModel.IsTopSubLevel && CubeDesignerViewModel.SelectedTab.Header.ToString() == CubePart.Top.ToString()) ||
                (!CubeDesignerViewModel.IsBaseSubLevel && CubeDesignerViewModel.SelectedTab.Header.ToString() == CubePart.Base.ToString()))
            {
                CubeDesignerViewModel.CurrentMetadata = CubeMetadata;
                Selection = SelectionMode.Selected;
            }
            else
            {
                Selection = SelectionMode.Active;
                CubeMetadata.ActiveFrames.Add(GetIndex());
                DeactivateAllButThis();                
            }
            MapBuilder.GhostMarker.MorphImage(CubeFactory.CreateCurrentCube(Coord.Zero, Coord.Zero, 0));
        }

        internal override void RightClick()
        {

        }

        internal override void LeftDoubleClick()
        {

        }

        internal override void RightDoubleClick()
        {
            if ((!CubeDesignerViewModel.IsTopSubLevel && CubeDesignerViewModel.SelectedTab.Header.ToString() == CubePart.Top.ToString()) ||
                (!CubeDesignerViewModel.IsBaseSubLevel && CubeDesignerViewModel.SelectedTab.Header.ToString() == CubePart.Base.ToString()))
            {
                CubeDesignerViewModel.EnterSubLevel(this);
            }
            else
            {
                CubeDesignerViewModel.ExitSubLevel();
            }
        }

        internal override void ShiftLeftClick()
        {
            if ((CubeDesignerViewModel.IsTopSubLevel && CubeDesignerViewModel.SelectedTab.Header.ToString() == CubePart.Top.ToString()) ||
                (CubeDesignerViewModel.IsBaseSubLevel && CubeDesignerViewModel.SelectedTab.Header.ToString() == CubePart.Base.ToString()))
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

        internal void DeactivateAllButThis()
        {
            if (CubeDesignerViewModel.SelectedTab.Header.ToString() == CubePart.Top.ToString())
            {
                foreach (var item in CubeDesignerViewModel.CubeTopCollection)
                {
                    if (item != this)
                    {
                        item.Selection = SelectionMode.None;
                        RemoveFromActive(item);
                    }
                }
            }
            else if (CubeDesignerViewModel.SelectedTab.Header.ToString() == CubePart.Base.ToString())
            {
                foreach (var item in CubeDesignerViewModel.CubeBaseCollection)
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
