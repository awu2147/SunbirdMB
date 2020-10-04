using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Win32;
using SunbirdMB.Interfaces;
using SelectionMode = SunbirdMB.Framework.SelectionMode;

namespace SunbirdMB.Gui
{
    class DecoCatalogViewModel : PropertyChangedBase
    {
        private ObservableCollection<DecoCatalogItem> deco1x1x1Collection = new ObservableCollection<DecoCatalogItem>();
        private TabItem selectedTab;
        private bool is1x1x1SubLevel;

        internal const string _1x1x1 = "1x1x1";

        private IMainGame MainGame { get; set; }
        public static DecoMetadata CurrentDecoMetadata { get; private set; }
        public ObservableCollection<DecoCatalogItem> CachedDeco1x1x1Collection { get; set; }

        public ObservableCollection<DecoCatalogItem> Deco1x1x1Collection
        {
            get { return deco1x1x1Collection; }
            set
            {
                if (deco1x1x1Collection == value) { return; }
                deco1x1x1Collection = value;
                NotifyPropertyChanged(nameof(Deco1x1x1Collection));
            }
        }

        public TabItem SelectedTab
        {
            get { return selectedTab; }
            set
            {
                if (selectedTab == value) { return; }
                selectedTab = value;
                NotifyPropertyChanged(nameof(SelectedTab));
                NotifyPropertyChanged(nameof(CurrentMetadata));
            }
        }

        public bool Is1x1x1SubLevel
        {
            get { return is1x1x1SubLevel; }
            set
            {
                is1x1x1SubLevel = value;
                NotifyPropertyChanged(nameof(IsSubLevel));
                NotifyPropertyChanged(nameof(IsAnimationComboBoxEnabled));
            }
        }

        public DecoMetadata CurrentMetadata
        {
            get
            {
                return CurrentDecoMetadata;
            }
            set
            {
                CurrentDecoMetadata = value;
                NotifyPropertyChanged(nameof(CurrentMetadata));
            }
        }

        public bool IsSubLevel
        {
            get
            {
                return ((Is1x1x1SubLevel && SelectedTab.Header.ToString() == _1x1x1));
            }
        }

        public bool IsAnimationComboBoxEnabled { get { return !IsSubLevel; } }

        public ICommand C_Import { get; set; }
        public ICommand C_Sort { get; set; }

        public DecoCatalogViewModel() : this(null) { }

        public DecoCatalogViewModel(IMainGame mainGame)
        {
            MainGame = mainGame;
            C_Import = new RelayCommand((o) => Import());
            C_Sort = new RelayCommand((o) => Uncache());
        }

        private void Uncache()
        {

        }

        internal void OnBeforeContentBuild()
        {
            // We add to the collections below when importing, so it is important to subscribe these handlers before calling Import().
            //Deco1x1x1Collection.CollectionChanged += new NotifyCollectionChangedEventHandler((sender, e) => DecoCatalogCollection_CollectionChanged(sender, e));
        }

        internal void OnAfterContentBuild()
        {
            SunbirdMBWindow.PumpToSplash(() => SunbirdMBSplash.ViewModel.Message = "Importing Deco Content...");
            // Import all decos into the deco catalog from the Content\Decos folder.
            ImportAll();

            // Initial current selections are set here.
            // TODO: Currently we just pick the first item in our tab.
            SelectDeco(Deco1x1x1Collection[0]);
            CurrentMetadata = Deco1x1x1Collection[0].DecoMetadata;
        }

        internal void OnGameLoaded() { }

        internal void EnterSubLevel(DecoCatalogItem dci)
        {
            if (!Is1x1x1SubLevel && SelectedTab.Header.ToString() == _1x1x1)
            {
                CachedDeco1x1x1Collection = Deco1x1x1Collection;
                Deco1x1x1Collection = new ObservableCollection<DecoCatalogItem>();

                int count = 0;
                for (int y = 0; y < dci.DecoMetadata.SheetRows; y++)
                {
                    for (int x = 0; x < dci.DecoMetadata.SheetColumns; x++)
                    {
                        count++;
                        SelectionMode selection = dci.DecoMetadata.ActiveFrames.Contains(count) ? SelectionMode.Active : SelectionMode.None;
                        Deco1x1x1Collection.Add(new DecoCatalogItem1x1x1(this, dci.ImagePath, dci.DecoMetadata) { SourceRect = new Int32Rect(72 * x, 75 * y, 72, 75), Selection = selection });
                    }
                }

                Is1x1x1SubLevel = true;
            }
        }

        internal void ExitSubLevel()
        {
            // Set the cube collections back to their upper level cached copy.
            if (Is1x1x1SubLevel && SelectedTab.Header.ToString() == _1x1x1)
            {
                Deco1x1x1Collection = CachedDeco1x1x1Collection;
                Is1x1x1SubLevel = false;
            }

        }

        private void SelectDeco(DecoCatalogItem decoCatalogItem)
        {
            decoCatalogItem.Selection = SelectionMode.Selected;
        }     

        private void Sort(ObservableCollection<DecoCatalogItem> decoCollection)
        {
            var cache = new List<DecoCatalogItem>();
            foreach (var item in decoCollection)
            {
                item.Unregister();
                cache.Add(item);
            }
            decoCollection.Clear();
            cache.Sort((x, y) => string.Compare(x.DecoMetadata.Name, y.DecoMetadata.Name));
            foreach (var item in cache)
            {
                decoCollection.Add(item);
            }
        }

        internal void SortAll()
        {
            Sort(Deco1x1x1Collection);
        }

        /// <summary> 
        /// Copy a single cube part from another directory into the Content\Cubes folder, and import it.
        /// </summary>
        private void Import()
        {
            var importDirectory = string.Empty;
            if (SelectedTab.Header.ToString() == _1x1x1)
            {
                importDirectory = UriHelper.Deco1x1x1Directory;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (Path.GetExtension(openFileDialog.FileName) == ".png")
                {
                    var newFilePath = Path.Combine(importDirectory, Path.GetFileName(openFileDialog.FileName));
                    if (File.Exists(newFilePath))
                    {
                        "Cannot copy to directory, file already exists.".Log();
                    }
                    else
                    {
                        File.Copy(openFileDialog.FileName, newFilePath);
                        // We don't need to rebuild everything, only the .png we just imported.
                        ContentBuilder.BuildFile(newFilePath);
                        Import(newFilePath);
                    }
                }
                else
                {
                    "Incorrect file format".Log();
                }
            }
            SortAll();
        }

        /// <summary> 
        /// Import a single cube part into the cube designer from the Content\Cubes folder.
        /// </summary>
        private void Import(string path)
        {
            try
            {
                var contentPath = path.MakeContentRelative();
                if (contentPath == string.Empty) { return; }

                // Find or create cube metadata.
                var metadataPath = Path.ChangeExtension(path, ".metadata");
                DecoMetadata dmd;
                if (File.Exists(metadataPath))
                {
                    dmd = Serializer.ReadXML<DecoMetadata>(DecoMetadata.DecoMetadataSerializer, metadataPath);
                    dmd.LoadContent(MainGame);
                }
                else
                {
                    dmd = new DecoMetadata() { ContentPath = contentPath, SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None };
                    dmd.LoadContent(MainGame);
                    dmd.Serialize(metadataPath);
                    $"Creating {Path.GetFileName(metadataPath)}...".Log();
                }

                // Deduce dimension from path.
                var dimension = contentPath.Split('\\')[1];

                // Add to part specific collection.
                if (dimension == _1x1x1)
                {
                    dmd.Dimensions = new Dimension(1, 1, 1);
                    Deco1x1x1Collection.Add(new DecoCatalogItem1x1x1(this, path, dmd));
                }

            }
            catch (Exception e)
            {
                e.Message.Log();
            }
        }

        /// <summary>
        /// Import all cube parts into the cube designer from the Content\Cubes folder.
        /// </summary>
        private void ImportAll()
        {
            var decos1x1x1 = Directory.GetFiles(UriHelper.Deco1x1x1Directory, "*.png", SearchOption.AllDirectories);
            foreach (var deco1x1x1 in decos1x1x1)
            {
                Import(deco1x1x1);
            }
        }
    }
}

