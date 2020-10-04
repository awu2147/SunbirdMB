﻿using SunbirdMB.Core;
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
using Microsoft.Xna.Framework;

namespace SunbirdMB.Gui
{
    class DecoCatalogViewModel : PropertyChangedBase
    {
        internal const string _1x1x1 = "1x1x1";
        internal const string _1x1x2 = "1x1x2";
        internal const string _1x1x3 = "1x1x3";

        private ObservableCollection<DecoCatalogItem> deco1x1x1Collection = new ObservableCollection<DecoCatalogItem>();
        private ObservableCollection<DecoCatalogItem> deco1x1x2Collection = new ObservableCollection<DecoCatalogItem>();
        private ObservableCollection<DecoCatalogItem> deco1x1x3Collection = new ObservableCollection<DecoCatalogItem>();

        private bool is1x1x1SubLevel;
        private bool is1x1x2SubLevel;
        private bool is1x1x3SubLevel;
        
        private TabItem selectedTab;
        private IMainGame MainGame { get; set; }
        public static DecoMetadata CurrentDecoMetadata { get; private set; }
        public ObservableCollection<DecoCatalogItem> CachedDeco1x1x1Collection { get; set; }
        public ObservableCollection<DecoCatalogItem> CachedDeco1x1x2Collection { get; set; }
        public ObservableCollection<DecoCatalogItem> CachedDeco1x1x3Collection { get; set; }

        public ObservableCollection<DecoCatalogItem> Deco1x1x1Collection
        {
            get { return deco1x1x1Collection; }
            set {  SetProperty(ref deco1x1x1Collection, value); }
        }

        public ObservableCollection<DecoCatalogItem> Deco1x1x2Collection
        {
            get { return deco1x1x2Collection; }
            set { SetProperty(ref deco1x1x2Collection, value); }
        }

        public ObservableCollection<DecoCatalogItem> Deco1x1x3Collection
        {
            get { return deco1x1x3Collection; }
            set { SetProperty(ref deco1x1x3Collection, value); }
        }

        public bool Is1x1x1SubLevel
        {
            get { return is1x1x1SubLevel; }
            set { SetProperty(ref is1x1x1SubLevel, value, nameof(IsSubLevel), nameof(IsAnimationComboBoxEnabled)); }
        }

        public bool Is1x1x2SubLevel
        {
            get { return is1x1x2SubLevel; }
            set { SetProperty(ref is1x1x2SubLevel, value, nameof(IsSubLevel), nameof(IsAnimationComboBoxEnabled)); }
        }

        public bool Is1x1x3SubLevel
        {
            get { return is1x1x3SubLevel; }
            set { SetProperty(ref is1x1x3SubLevel, value, nameof(IsSubLevel), nameof(IsAnimationComboBoxEnabled)); }
        }

        public TabItem SelectedTab
        {
            get { return selectedTab; }
            set { SetProperty(ref selectedTab, value, nameof(SelectedTab), nameof(CurrentMetadata)); }
        }

        public DecoMetadata CurrentMetadata
        {
            get { return CurrentDecoMetadata; }
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
                return ((Is1x1x1SubLevel && SelectedTab.Header.ToString() == _1x1x1) ||
                        (Is1x1x2SubLevel && SelectedTab.Header.ToString() == _1x1x2) ||
                        (Is1x1x3SubLevel && SelectedTab.Header.ToString() == _1x1x3));
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

        }

        internal void OnAfterContentBuild()
        {
            SunbirdMBWindow.PumpToSplash(() => SunbirdMBSplash.ViewModel.Message = "Importing Deco Content...");
            // Import all decos into the deco catalog from the Content\Decos folder.
            ImportAll();

            // Initial current selections are set here.
            // TODO: Currently we just pick the first item in our tab.
            SelectDeco(Deco1x1x1Collection[0]);
            SelectDeco(Deco1x1x2Collection[0]);
            SelectDeco(Deco1x1x3Collection[0]);
            CurrentMetadata = Deco1x1x1Collection[0].DecoMetadata;
        }

        internal void OnGameLoaded() { }

        internal void EnterSubLevel(DecoCatalogItem dci)
        {
            if (!Is1x1x1SubLevel && SelectedTab.Header.ToString() == _1x1x1)
            {
                CachedDeco1x1x1Collection = Deco1x1x1Collection;
                Deco1x1x1Collection = new ObservableCollection<DecoCatalogItem>();
                CreateSubCollection(dci, _1x1x1, Deco1x1x1Collection);
                Is1x1x1SubLevel = true;
            }
            else if (!Is1x1x2SubLevel && SelectedTab.Header.ToString() == _1x1x2)
            {
                CachedDeco1x1x2Collection = Deco1x1x2Collection;
                Deco1x1x2Collection = new ObservableCollection<DecoCatalogItem>();
                CreateSubCollection(dci, _1x1x2, Deco1x1x2Collection);
                Is1x1x2SubLevel = true;
            }
            else if (!Is1x1x3SubLevel && SelectedTab.Header.ToString() == _1x1x3)
            {
                CachedDeco1x1x3Collection = Deco1x1x3Collection;
                Deco1x1x3Collection = new ObservableCollection<DecoCatalogItem>();
                CreateSubCollection(dci, _1x1x3, Deco1x1x3Collection);
                Is1x1x3SubLevel = true;
            }
        }

        private void CreateSubCollection(DecoCatalogItem dci, string dimensions, ObservableCollection<DecoCatalogItem> collection)
        {
            int count = 0;
            for (int y = 0; y < dci.DecoMetadata.SheetRows; y++)
            {
                for (int x = 0; x < dci.DecoMetadata.SheetColumns; x++)
                {
                    count++;
                    SelectionMode selection = dci.DecoMetadata.ActiveFrames.Contains(count) ? SelectionMode.Active : SelectionMode.None;
                    var newDci = DecoCatalogItem.CreateNew(this, dci.ImagePath, dci.DecoMetadata, dimensions);
                    newDci.SourceRect = new Int32Rect(newDci.ItemWidth * x, newDci.ItemHeight * y, newDci.ItemWidth, newDci.ItemHeight);
                    newDci.Selection = selection;
                    collection.Add(newDci);
                }
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
            if (Is1x1x2SubLevel && SelectedTab.Header.ToString() == _1x1x2)
            {
                Deco1x1x2Collection = CachedDeco1x1x2Collection;
                Is1x1x2SubLevel = false;
            }
            if (Is1x1x3SubLevel && SelectedTab.Header.ToString() == _1x1x3)
            {
                Deco1x1x3Collection = CachedDeco1x1x3Collection;
                Is1x1x3SubLevel = false;
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
            Sort(Deco1x1x2Collection);
            Sort(Deco1x1x3Collection);
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

                // Add to dimension specific collection.
                if (dimension == _1x1x1)
                {
                    dmd.Dimensions = new Dimension(1, 1, 1);
                    Deco1x1x1Collection.Add(new DecoCatalogItem1x1x1(this, path, dmd));
                }
                else if (dimension == _1x1x2)
                {
                    dmd.Dimensions = new Dimension(1, 1, 2);
                    dmd.PositionOffset = new Vector2(0, -36);
                    Deco1x1x2Collection.Add(new DecoCatalogItem1x1x2(this, path, dmd));
                }
                else if (dimension == _1x1x3)
                {
                    dmd.Dimensions = new Dimension(1, 1, 3);
                    dmd.PositionOffset = new Vector2(0, -72);
                    Deco1x1x3Collection.Add(new DecoCatalogItem1x1x3(this, path, dmd));
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
            var decos1x1x2 = Directory.GetFiles(UriHelper.Deco1x1x2Directory, "*.png", SearchOption.AllDirectories);
            foreach (var deco1x1x2 in decos1x1x2)
            {
                Import(deco1x1x2);
            }
            var decos1x1x3 = Directory.GetFiles(UriHelper.Deco1x1x3Directory, "*.png", SearchOption.AllDirectories);
            foreach (var deco1x1x3 in decos1x1x3)
            {
                Import(deco1x1x3);
            }
        }
    }
}

