using SunbirdMB.Core;
using SunbirdMB.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SunbirdMB.Gui
{
    public abstract class MetadataItemBase : CatalogItemBase
    {
        internal abstract MetadataBase Metadata { get; }

        private SelectionMode selection;
        public SelectionMode Selection
        {
            get { return selection; }
            set { SetProperty(ref selection, value); }
        }

        public MetadataItemBase(string imagePath, MetadataBase md) : base(imagePath) { }

        internal int GetIndex()
        {
            return GetIndex(this);
        }

        internal int GetIndex(MetadataItemBase mib)
        {
            int xPos = mib.SourceRect.X / mib.ItemWidth;
            int yPos = mib.SourceRect.Y / mib.ItemHeight;
            int index = mib.Metadata.SheetColumns * yPos + xPos;
            return index + 1;
        }

        internal void RemoveFromActive(MetadataItemBase mib)
        {
            if (Metadata.ActiveFrames.Contains(GetIndex(mib)))
            {
                Metadata.ActiveFrames.Remove(GetIndex(mib));
            }
        }

        internal static void Sort<T>(ObservableCollection<T> derivedItemCollection)
        {
            var cache = new List<T>();
            foreach (var item in derivedItemCollection)
            {
                cache.Add(item);
            }
            derivedItemCollection.Clear();
            cache.Sort((x, y) => string.CompareOrdinal((x as MetadataItemBase).Metadata.Name, (y as MetadataItemBase).Metadata.Name));
            foreach (var item in cache)
            {
                derivedItemCollection.Add(item);
            }
        }

    }
}
