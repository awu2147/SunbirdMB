using SunbirdMB.Core;
using SunbirdMB.Framework;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using SunbirdMB.Interfaces;
using SelectionMode = SunbirdMB.Framework.SelectionMode;

namespace SunbirdMB.Gui
{
    public class CubeDesignerViewModel : PropertyChangedBase, IImporter
    {
        private ObservableCollection<CubeDesignerItem> cubeTopCollection = new ObservableCollection<CubeDesignerItem>();
        private ObservableCollection<CubeDesignerItem> cubeBaseCollection = new ObservableCollection<CubeDesignerItem>();
        
        private bool isTopSubLevel;
        private bool isBaseSubLevel;

        private TabItem selectedTab;
        private IMainGame MainGame { get; set; }
        public static bool IsCurrentCubeWalkable { get; set; } = true;
        public static CubeMetadata CurrentCubeTopMetadata { get; private set; }
        public static CubeMetadata CurrentCubeBaseMetadata { get; private set; }
        public ObservableCollection<CubeDesignerItem> CachedCubeBaseCollection { get; set; }
        public ObservableCollection<CubeDesignerItem> CachedCubeTopCollection { get; set; }

        public ObservableCollection<CubeDesignerItem> CubeTopCollection
        {
            get { return cubeTopCollection; }
            set { SetProperty(ref cubeTopCollection, value); }
        }

        public ObservableCollection<CubeDesignerItem> CubeBaseCollection
        {
            get { return cubeBaseCollection; }
            set { SetProperty(ref cubeBaseCollection, value); }
        }

        public TabItem SelectedTab
        {
            get { return selectedTab; }
            set { SetProperty(ref selectedTab, value, nameof(SelectedTab), nameof(CurrentMetadata)); }
        }

        public bool IsTopSubLevel
        {
            get { return isTopSubLevel; }
            set { SetProperty(ref isTopSubLevel, value, nameof(IsSubLevel), nameof(IsAnimationComboBoxEnabled)); }
        }

        public bool IsBaseSubLevel
        {
            get { return isBaseSubLevel; }
            set { SetProperty(ref isBaseSubLevel, value, nameof(IsSubLevel), nameof(IsAnimationComboBoxEnabled)); }
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

        internal void EnterSubLevel(CubeDesignerItem cdi)
        {
            if (!IsTopSubLevel && SelectedTab.Header.ToString() == CubePart.Top.ToString())
            {
                CachedCubeTopCollection = CubeTopCollection;
                CubeTopCollection = new ObservableCollection<CubeDesignerItem>();
                CreateSubCollection(cdi, CubeTopCollection);
                IsTopSubLevel = true;
            }
            else if (!IsBaseSubLevel && SelectedTab.Header.ToString() == CubePart.Base.ToString())
            {
                CachedCubeBaseCollection = CubeBaseCollection;
                CubeBaseCollection = new ObservableCollection<CubeDesignerItem>();
                CreateSubCollection(cdi, CubeBaseCollection);
                IsBaseSubLevel = true;
            }
        }

        private void CreateSubCollection(CubeDesignerItem cdi, ObservableCollection<CubeDesignerItem> collection)
        {
            int count = 0;
            for (int y = 0; y < cdi.CubeMetadata.SheetRows; y++)
            {
                for (int x = 0; x < cdi.CubeMetadata.SheetColumns; x++)
                {
                    count++;
                    SelectionMode selection = cdi.CubeMetadata.ActiveFrames.Contains(count) ? SelectionMode.Active : SelectionMode.None;
                    var newCdi = new CubeDesignerItem(this, cdi.ImagePath, cdi.CubeMetadata);
                    newCdi.SourceRect = new Int32Rect(newCdi.ItemWidth * x, newCdi.ItemHeight * y, newCdi.ItemWidth, newCdi.ItemHeight);
                    newCdi.Selection = selection;
                    collection.Add(newCdi);
                    if (count == cdi.CubeMetadata.FrameCount)
                    {
                        break;
                    }
                }
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

        internal void SortAll()
        {
            MetadataItemBase.Sort(CubeTopCollection);
            MetadataItemBase.Sort(CubeBaseCollection);
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
            Importer.CopyBuildImport(importDirectory, this);
            SortAll();
        }

        /// <summary> 
        /// Import a single cube part into the cube designer from the Content\Cubes folder.
        /// </summary>
        public void Import(string path)
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
                    cmd = new CubeMetadata() { ContentPath = contentPath };
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

    }

}
