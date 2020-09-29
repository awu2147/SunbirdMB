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

        internal override void LeftClick()
        {
            if (!CubeDesignerViewModel.IsSubLevel)
            {
                CubeDesignerViewModel.CurrentMetadata = CubeMetadata;
                MapBuilder.GhostMarker.MorphImage(CubeFactory.CreateCurrentCube(Coord.Zero, Coord.Zero, 0));
                Selection = SelectionMode.Selected;
            }
            else
            {
                Selection = SelectionMode.Active;
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
    }
}
