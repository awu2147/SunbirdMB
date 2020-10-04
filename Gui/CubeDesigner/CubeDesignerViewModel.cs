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
    public class CubeDesignerViewModel : PropertyChangedBase
    {
        private ObservableCollection<CubeDesignerItem> cubeTopCollection = new ObservableCollection<CubeDesignerItem>();
        private ObservableCollection<CubeDesignerItem> cubeBaseCollection = new ObservableCollection<CubeDesignerItem>();
        private TabItem selectedTab;
        private bool isTopSubLevel;
        private bool isBaseSubLevel;

        private IMainGame MainGame { get; set; }
        public static CubeMetadata CurrentCubeTopMetadata { get; private set; }
        public static CubeMetadata CurrentCubeBaseMetadata { get; private set; }
        public ObservableCollection<CubeDesignerItem> CachedCubeBaseCollection { get; set; }
        public ObservableCollection<CubeDesignerItem> CachedCubeTopCollection { get; set; }

        public ObservableCollection<CubeDesignerItem> CubeTopCollection
        {
            get { return cubeTopCollection; }
            set
            {
                if (cubeTopCollection == value) { return; }
                cubeTopCollection = value;
                NotifyPropertyChanged(nameof(CubeTopCollection));
            }
        }

        public ObservableCollection<CubeDesignerItem> CubeBaseCollection
        {
            get { return cubeBaseCollection; }
            set
            {
                if (cubeBaseCollection == value) { return; }
                cubeBaseCollection = value;
                NotifyPropertyChanged(nameof(CubeBaseCollection));
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

        public bool IsTopSubLevel
        {
            get { return isTopSubLevel; }
            set
            {
                isTopSubLevel = value;
                NotifyPropertyChanged(nameof(IsSubLevel));
                NotifyPropertyChanged(nameof(IsAnimationComboBoxEnabled));
            }
        }

        public bool IsBaseSubLevel
        {
            get { return isBaseSubLevel; }
            set
            {
                isBaseSubLevel = value;
                NotifyPropertyChanged(nameof(IsSubLevel));
                NotifyPropertyChanged(nameof(IsAnimationComboBoxEnabled));
            }
        }

        public CubeMetadata CurrentMetadata
        {
            get
            {
                if (SelectedTab?.Header.ToString() == CubePart.Top.ToString())
                {
                    return CurrentCubeTopMetadata;
                }
                else if (SelectedTab?.Header.ToString() == CubePart.Base.ToString())
                {
                    return CurrentCubeBaseMetadata;
                }
                return null;
            }
            set
            {
                if (SelectedTab?.Header.ToString() == CubePart.Top.ToString())
                {
                    CurrentCubeTopMetadata = value;
                }
                else if (SelectedTab?.Header.ToString() == CubePart.Base.ToString())
                {
                    CurrentCubeBaseMetadata = value;
                }
                NotifyPropertyChanged(nameof(CurrentMetadata));
            }
        }

        public bool IsSubLevel
        {
            get
            {
                return ((IsTopSubLevel && SelectedTab.Header.ToString() == CubePart.Top.ToString()) ||
                        (IsBaseSubLevel && SelectedTab.Header.ToString() == CubePart.Base.ToString()));
            }
        }

        public bool IsAnimationComboBoxEnabled { get { return !IsSubLevel; } }

        public ICommand C_Import { get; set; }
        public ICommand C_Sort { get; set; }

        public CubeDesignerViewModel() : this(null) { }

        public CubeDesignerViewModel(IMainGame mainGame)
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
            CubeTopCollection.CollectionChanged += new NotifyCollectionChangedEventHandler((sender, e) => CubeDesignerCollection_CollectionChanged(sender, e, CubePart.Top));
            CubeBaseCollection.CollectionChanged += new NotifyCollectionChangedEventHandler((sender, e) => CubeDesignerCollection_CollectionChanged(sender, e, CubePart.Base));

        }

        internal void OnAfterContentBuild()
        {
            SunbirdMBWindow.PumpToSplash(() => SunbirdMBSplash.ViewModel.Message = "Importing Cube Content...");
            // Import all cube parts into the cube designer from the Content\Cubes folder.
            ImportAll();

            //SunbirdMBWindow.PumpToSplash(() => SunbirdMBSplash.ViewModel.Message = "Building Cube Designer Library...");
            // Build our cube factory library by creating textures from paths.
            //CubeFactory.BuildLibrary(MainGame);

            // Initial current selections are set here.
            // TODO: Currently we just pick the first item in our tabs.
            SelectTop(CubeTopCollection[0]);
            SelectBase(CubeBaseCollection[0]);
            // Currently our metadata to display in the Properties window is just the current cube top.
            // This just depends on which tab is selected on start-up.
            CurrentMetadata = CurrentCubeTopMetadata;
        }

        internal void OnGameLoaded()
        {
            // Now that a current cube and ghost marker exists go ahead and create a ghost marker image from it. It is important that this gets done
            // before the first update and draw calls.
            MapBuilder.GhostMarker.MorphImage(CubeFactory.CreateCurrentCube(Coord.Zero, Coord.Zero, 0));
        }

        public void CubeDesignerCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CubePart part)
        {
            if (e.NewItems != null)
            {
                foreach (CubeDesignerItem item in e.NewItems)
                {
                    // CubeDesignerViewModel outlives CubeDesignerItem so no need to unsubscribe.
                    item.PropertyChangedHandler = new PropertyChangedEventHandler((_sender, _e) => CubeDesignerCollectionItem_PropertyChanged(_sender, _e, part));
                    item.Register();
                }
            }
        }

        private void CubeDesignerCollectionItem_PropertyChanged(object sender, PropertyChangedEventArgs e, CubePart part)
        {
            CubeDesignerItem cdi = sender as CubeDesignerItem;
            // Manage the selection and deselction of cube designer items here.
            if (e.PropertyName == nameof(cdi.Selection) && cdi.Selection == SelectionMode.Selected)
            {
                if (part == CubePart.Top)
                {
                    foreach (var item in CubeTopCollection)
                    {
                        if (item != cdi)
                        {
                            item.Selection = SelectionMode.None;
                        }
                    }
                }
                else if (part == CubePart.Base)
                {
                    foreach (var item in CubeBaseCollection)
                    {
                        if (item != cdi)
                        {
                            item.Selection = SelectionMode.None;
                        }
                    }
                }
            }
        }

        internal void EnterSubLevel(CubeDesignerItem cdi)
        {
            if (!IsTopSubLevel && SelectedTab.Header.ToString() == CubePart.Top.ToString())
            {
                CachedCubeTopCollection = CubeTopCollection;
                CubeTopCollection = new ObservableCollection<CubeDesignerItem>();
                CubeTopCollection.CollectionChanged += new NotifyCollectionChangedEventHandler((sender, e) => CubeDesignerCollection_CollectionChanged(sender, e, CubePart.Top));

                int count = 0;
                for (int y = 0; y < cdi.CubeMetadata.SheetRows; y++)
                {
                    for (int x = 0; x < cdi.CubeMetadata.SheetColumns; x++)
                    {
                        count++;
                        SelectionMode selection = cdi.CubeMetadata.ActiveFrames.Contains(count) ? SelectionMode.Active : SelectionMode.None;
                        CubeTopCollection.Add(new CubeDesignerItem(this, cdi.ImagePath, cdi.CubeMetadata) { SourceRect = new Int32Rect(72 * x, 75 * y, 72, 75), Selection = selection });
                    }
                }

                IsTopSubLevel = true;
            }
            else if (!IsBaseSubLevel && SelectedTab.Header.ToString() == CubePart.Base.ToString())
            {
                CachedCubeBaseCollection = CubeBaseCollection;
                CubeBaseCollection = new ObservableCollection<CubeDesignerItem>();
                CubeBaseCollection.CollectionChanged += new NotifyCollectionChangedEventHandler((sender, e) => CubeDesignerCollection_CollectionChanged(sender, e, CubePart.Base));

                int count = 0;
                for (int y = 0; y < cdi.CubeMetadata.SheetRows; y++)
                {
                    for (int x = 0; x < cdi.CubeMetadata.SheetColumns; x++)
                    {
                        count++;
                        SelectionMode selection = cdi.CubeMetadata.ActiveFrames.Contains(count) ? SelectionMode.Active : SelectionMode.None;
                        CubeBaseCollection.Add(new CubeDesignerItem(this, cdi.ImagePath, cdi.CubeMetadata) { SourceRect = new Int32Rect(72 * x, 75 * y, 72, 75), Selection = selection });
                    }
                }

                IsBaseSubLevel = true;
            }
        }

        internal void ExitSubLevel()
        {
            // Set the cube collections back to their upper level cached copy.
            if (IsTopSubLevel && SelectedTab.Header.ToString() == CubePart.Top.ToString())
            {
                CubeTopCollection = CachedCubeTopCollection;
                IsTopSubLevel = false;
            }
            else if (IsBaseSubLevel && SelectedTab.Header.ToString() == CubePart.Base.ToString())
            {
                CubeBaseCollection = CachedCubeBaseCollection;
                IsBaseSubLevel = false;
            }
            
        }

        private void Sort(ObservableCollection<CubeDesignerItem> cubePartCollection)
        {
            var cache = new List<CubeDesignerItem>();
            foreach (var item in cubePartCollection)
            {
                item.Unregister();
                cache.Add(item);
            }
            cubePartCollection.Clear();
            cache.Sort((x, y) => string.Compare(x.CubeMetadata.Name, y.CubeMetadata.Name));
            foreach (var item in cache)
            {
                cubePartCollection.Add(item);
            }
        }

        internal void SortAll()
        {
            SortTop();
            SortBase();
        }

        internal void SortBase()
        {
            Sort(CubeBaseCollection);
        }

        internal void SortTop()
        {
            Sort(CubeTopCollection);
        }

        private void SelectTop(CubeDesignerItem cubeDesignerItem)
        {
            CurrentCubeTopMetadata = cubeDesignerItem.CubeMetadata;
            cubeDesignerItem.Selection = SelectionMode.Selected;
        }

        private void SelectBase(CubeDesignerItem cubeDesignerItem)
        {
            CurrentCubeBaseMetadata = cubeDesignerItem.CubeMetadata;
            cubeDesignerItem.Selection = SelectionMode.Selected;
        }

        /// <summary> 
        /// Copy a single cube part from another directory into the Content\Cubes folder, and import it.
        /// </summary>
        private void Import()
        {
            var importDirectory = string.Empty;
            if (SelectedTab.Header.ToString() == CubePart.Top.ToString())
            {
                importDirectory = UriHelper.CubeTopDirectory;
            }
            else if (SelectedTab.Header.ToString() == CubePart.Base.ToString())
            {
                importDirectory = UriHelper.CubeBaseDirectory;
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
                CubeMetadata cmd;
                if (File.Exists(metadataPath))
                {
                    cmd = Serializer.ReadXML<CubeMetadata>(CubeMetadata.CubeMetadataSerializer, metadataPath);
                    cmd.LoadContent(MainGame);
                }
                else
                {
                    cmd = new CubeMetadata() { ContentPath = contentPath, SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None };
                    cmd.LoadContent(MainGame);
                    cmd.Serialize(metadataPath);
                    $"Creating {Path.GetFileName(metadataPath)}...".Log();
                }

                // Deduce part type from path.
                var _part = contentPath.Split('\\')[1];
                CubePart part = (CubePart)Enum.Parse(typeof(CubePart), _part, true);

                // Add to part specific collection.
                if (part == CubePart.Top)
                {
                    CubeTopCollection.Add(new CubeDesignerItem(this, path, cmd));
                }
                else if (part == CubePart.Base)
                {
                    CubeBaseCollection.Add(new CubeDesignerItem(this, path, cmd));
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
            var cubeTops = Directory.GetFiles(UriHelper.CubeTopDirectory, "*.png", SearchOption.AllDirectories);
            foreach (var cubeTop in cubeTops)
            {
                Import(cubeTop);
            }
            var cubeBases = Directory.GetFiles(UriHelper.CubeBaseDirectory, "*.png", SearchOption.AllDirectories);
            foreach (var cubeBase in cubeBases)
            {
                Import(cubeBase);
            }
        }
    }

}
