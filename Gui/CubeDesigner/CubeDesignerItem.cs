using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SunbirdMB.Gui
{
    public class CubeDesignerItem : CatalogItemBase
    {
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
            int xPos = SourceRect.X / 72;
            int yPos = SourceRect.Y / 75;
            int index = CubeMetadata.SheetColumns * yPos + xPos;
            return index + 1;
        }

        internal int GetIndex(CubeDesignerItem cdi)
        {
            int xPos = cdi.SourceRect.X / 72;
            int yPos = cdi.SourceRect.Y / 75;
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
            if ((!CubeDesignerViewModel.IsTopSubLevel && CubeDesignerViewModel.SelectedTab.Header.ToString() == "Top") ||
                (!CubeDesignerViewModel.IsBaseSubLevel && CubeDesignerViewModel.SelectedTab.Header.ToString() == "Base"))
            {
                CubeDesignerViewModel.CurrentMetadata = CubeMetadata;
                MapBuilder.GhostMarker.MorphImage(CubeFactory.CreateCurrentCube(Coord.Zero, Coord.Zero, 0));
                Selection = SelectionMode.Selected;
            }
            else
            {
                if (Selection == SelectionMode.None)
                {
                    Selection = SelectionMode.Active;
                    CubeMetadata.ActiveFrames.Add(GetIndex());
                    DeactivateAllButThis();
                }
                //else if (selection == SelectionMode.Active && CubeMetadata.ActiveFrames.Count > 1)
                //{
                //    Selection = SelectionMode.None;
                //    CubeMetadata.ActiveFrames.Remove(GetIndex());
                //}
            }
        }

        internal override void LeftDoubleClick()
        {
            CubeDesignerViewModel.EnterSubLevel(this);
        }

        internal override void RightClick()
        {
            CubeDesignerViewModel.ExitSubLevel();
        }

        internal override void ShiftLeftClick()
        {
            if ((CubeDesignerViewModel.IsTopSubLevel && CubeDesignerViewModel.SelectedTab.Header.ToString() == "Top") ||
                (CubeDesignerViewModel.IsBaseSubLevel && CubeDesignerViewModel.SelectedTab.Header.ToString() == "Base"))
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
