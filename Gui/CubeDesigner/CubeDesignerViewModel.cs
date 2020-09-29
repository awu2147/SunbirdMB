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
using System.Windows.Controls;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using SunbirdMB.Interfaces;

namespace SunbirdMB.Gui
{
    public class CubeDesignerViewModel : PropertyChangedBase
    {
        private static readonly PropertyChangedEventArgs SelectedTabPropertyEventArgs = new PropertyChangedEventArgs(nameof(SelectedTab));
        private static readonly PropertyChangedEventArgs CurrentMetadataPropertyEventArgs = new PropertyChangedEventArgs(nameof(CurrentMetadata));

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        public static ObservableCollection<CubeDesignerItem> CubeTopCollection { get; private set; } = new ObservableCollection<CubeDesignerItem>();
        public static ObservableCollection<CubeDesignerItem> CubeBaseCollection { get; set; } = new ObservableCollection<CubeDesignerItem>();

        public static CubeMetadata CurrentCubeTopMetadata { get; private set; }
        public static CubeMetadata CurrentCubeBaseMetadata { get; private set; }

        private static TabItem selectedTab;
        public static TabItem SelectedTab
        {
            get { return selectedTab; }
            set
            {
                if (selectedTab == value) { return; }
                selectedTab = value;
                StaticPropertyChanged?.Invoke(null, SelectedTabPropertyEventArgs);
                StaticPropertyChanged?.Invoke(null, CurrentMetadataPropertyEventArgs);
            }
        }

        public static CubeMetadata CurrentMetadata
        {
            get 
            {
                if (SelectedTab?.Header.ToString() == "Top")
                {
                    return CurrentCubeTopMetadata;
                }
                else if (SelectedTab?.Header.ToString() == "Base")
                {
                    return CurrentCubeBaseMetadata;
                }
                return null; 
            }
            set
            {
                if (SelectedTab?.Header.ToString() == "Top")
                {
                    CurrentCubeTopMetadata = value;
                }
                else if (SelectedTab?.Header.ToString() == "Base")
                {
                    CurrentCubeBaseMetadata = value;
                }
                StaticPropertyChanged?.Invoke(null, CurrentMetadataPropertyEventArgs);
            }
        }

        public ICommand C_Import { get; set; }
        public ICommand C_Sort { get; set; }

        public CubeDesignerViewModel() : this(null) { }

        public CubeDesignerViewModel(IMainGame mainGame)
        {
            MainGame = mainGame;
            C_Import = new RelayCommand((o) => Import());
            C_Sort = new RelayCommand((o) => SortAll());
        }

        IMainGame MainGame { get; set; }

        internal void OnBeforeContentBuild()
        {
            // We add to the collections below when importing, so it is important to subscribe these handlers before calling Import().
            // We only subscribe handlers once so it is imprtant the observable collection is not re-instantiated.
            CubeTopCollection.CollectionChanged += new NotifyCollectionChangedEventHandler((sender, e) => CubeDesignerCollection_CollectionChanged(sender, e, CubePart.Top));
            CubeBaseCollection.CollectionChanged += new NotifyCollectionChangedEventHandler((sender, e) => CubeDesignerCollection_CollectionChanged(sender, e, CubePart.Base));

        }

        internal void OnAfterContentBuild()
        {
            SunbirdMBWindow.PumpToSplash(() => SunbirdMBSplash.ViewModel.Message = "Importing Cube Content...");
            // Start off by populating the cube designer. We can also load/create metadata files here, and add the resulting 
            // cube metadata objects to cube factory metadata collection. From the file paths, we can deduce the cube part type.
            Import(CubePart.All);

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

        private void CubeDesignerCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CubePart part)
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
            if (e.PropertyName == "Selected" && cdi.Selected == true)
            {
                if (part == CubePart.Top)
                {
                    foreach (var item in CubeTopCollection)
                    {
                        if (item != cdi)
                        {
                            item.Selected = false;
                        }
                    }
                }
                else if (part == CubePart.Base)
                {
                    foreach (var item in CubeBaseCollection)
                    {
                        if (item != cdi)
                        {
                            item.Selected = false;
                        }
                    }
                }
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
            cubeDesignerItem.Selected = true;
        }

        private void SelectBase(CubeDesignerItem cubeDesignerItem)
        {
            CurrentCubeBaseMetadata = cubeDesignerItem.CubeMetadata; 
            cubeDesignerItem.Selected = true;
        }

        private void Import()
        {
            var importDirectory = string.Empty;
            if (SelectedTab.Header.ToString() == "Top")
            {
                importDirectory = UriHelper.CubeTopDirectory;
            }
            else if (SelectedTab.Header.ToString() == "Base")
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
                        Import(newFilePath);
                    }
                }
                else
                {
                    "Incorrect file format".Log();
                }
            }

        }

        /// <summary>
        /// Import a single cube part into the cube designer and cube factory metadata collection (unbuilt).
        /// </summary>
        internal void Import(string path)
        {
            var contentPath = path.MakeContentRelative();
            if (contentPath == string.Empty) { return; }
            // We don't need to rebuild everything, only the .png we just imported.
            ContentBuilder.BuildFile(path);
            var _part = contentPath.Split('\\')[1];
            CubePart part = (CubePart)Enum.Parse(typeof(CubePart), _part, true);
            // Find or create cube metadata.
            var metadataPath = Path.ChangeExtension(path, ".metadata");
            CubeMetadata cmd;
            if (File.Exists(metadataPath))
            {
                cmd = Serializer.ReadXML<CubeMetadata>(CubeMetadata.CubeMetaDataSerializer, metadataPath);
                cmd.LoadContent(MainGame);
            }
            else
            {
                cmd = new CubeMetadata() { ContentPath = contentPath, Part = part, SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None };
                cmd.LoadContent(MainGame);
                cmd.Serialize(metadataPath);
                $"Creating {Path.GetFileName(metadataPath)}...".Log();
            }
            // Ask what cube part we are importing.
            if (part == CubePart.Top)
            {
                CubeTopCollection.Add(new CubeDesignerItem(path, cmd));
            }
            else if (part == CubePart.Base)
            {
                CubeBaseCollection.Add(new CubeDesignerItem(path, cmd));
            }
        }

        /// <summary>
        /// Import cube parts into the cube designer and cube library from the Content\Cubes folder.
        /// </summary>
        internal void Import(CubePart part)
        {
            if (part == CubePart.All)
            {
                Import(CubePart.Top);
                Import(CubePart.Base);
            }
            else
            {
                var importDirectory = string.Empty;
                if (part == CubePart.Top)
                {
                    importDirectory = UriHelper.CubeTopDirectory;
                }
                else if (part == CubePart.Base)
                {
                    importDirectory = UriHelper.CubeBaseDirectory;
                }
                var files = Directory.GetFiles(importDirectory, "*.png", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var contentPath = file.MakeContentRelative();
                    if (contentPath == string.Empty) { return; }
                    var metadata = Path.ChangeExtension(file, ".metadata");
                    CubeMetadata cmd;
                    if (File.Exists(metadata))
                    {
                        cmd = Serializer.ReadXML<CubeMetadata>(CubeMetadata.CubeMetaDataSerializer, metadata);
                        cmd.LoadContent(MainGame);
                    }
                    else
                    {
                        cmd = new CubeMetadata() { ContentPath = contentPath, Part = part, SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None };
                        cmd.LoadContent(MainGame);
                        cmd.Serialize(metadata);
                        $"Creating {Path.GetFileName(metadata)}...".Log();
                    }
                    if (part == CubePart.Top)
                    {
                        CubeTopCollection.Add(new CubeDesignerItem(file, cmd));
                    }
                    else if (part == CubePart.Base)
                    {
                        CubeBaseCollection.Add(new CubeDesignerItem(file, cmd));
                    }
                }
            }
        }
    }

}
