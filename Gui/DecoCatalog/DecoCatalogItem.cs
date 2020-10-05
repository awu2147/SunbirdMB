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
        internal override int ItemWidth { get; set; }
        internal override int ItemHeight { get; set; }

        private readonly DecoCatalogManager CatalogManager;

        internal DecoMetadata DecoMetadata { get; set; }

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
                    MapBuilder.GhostMarker.MorphImage(DecoFactory.CreateCurrentDeco(Coord.Zero, Coord.Zero, 0));
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

        //internal static DecoCatalogItem CreateNew(DecoCatalogManager collectionManager, string imagePath, DecoMetadata dmd, string dimensions)
        //{
        //    switch (dimensions)
        //    {
        //        case DecoCatalogViewModel._1x1x1:
        //            return new DecoCatalogItem1x1x1(collectionManager, imagePath, dmd);
        //        case DecoCatalogViewModel._1x1x2:
        //            return new DecoCatalogItem1x1x2(collectionManager, imagePath, dmd);
        //        case DecoCatalogViewModel._1x1x3:
        //            return new DecoCatalogItem1x1x3(collectionManager, imagePath, dmd);
        //        default:
        //            return null;
        //    }
        //}

    }

    //public class DecoCatalogItem1x1x1 : DecoCatalogItem
    //{
    //    internal override int ItemWidth { get; set; } = 72;
    //    internal override int ItemHeight { get; set; } = 75;

    //    internal DecoCatalogItem1x1x1(DecoCatalogManager collectionManager, string imagePath, DecoMetadata dmd) : base(collectionManager, imagePath, dmd) 
    //    {
    //        SourceRect = new Int32Rect(0, 0, 72, 75);
    //    }
    //}
    //public class DecoCatalogItem1x1x2 : DecoCatalogItem
    //{
    //    internal override int ItemWidth { get; set; } = 72;
    //    internal override int ItemHeight { get; set; } = 111;

    //    internal DecoCatalogItem1x1x2(DecoCatalogManager collectionManager, string imagePath, DecoMetadata dmd) : base(collectionManager, imagePath, dmd)
    //    {
    //        SourceRect = new Int32Rect(0, 0, 72, 111);
    //    }
    //}
    //public class DecoCatalogItem1x1x3 : DecoCatalogItem
    //{
    //    internal override int ItemWidth { get; set; } = 72;
    //    internal override int ItemHeight { get; set; } = 147;

    //    internal DecoCatalogItem1x1x3(DecoCatalogManager collectionManager, string imagePath, DecoMetadata dmd) : base(collectionManager, imagePath, dmd)
    //    {
    //        SourceRect = new Int32Rect(0, 0, 72, 147);
    //    }
    //}

}
