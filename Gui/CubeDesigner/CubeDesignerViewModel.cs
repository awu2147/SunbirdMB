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

namespace SunbirdMB.Gui
{
    public class CubeDesignerViewModel : PropertyChangedBase
    {
        public ObservableCollection<CubeDesignerItem> CubeTopCollection { get; set; } = new ObservableCollection<CubeDesignerItem>();
        public ObservableCollection<CubeDesignerItem> CubeBaseCollection { get; set; } = new ObservableCollection<CubeDesignerItem>();

        private TabItem selectedTab;
        public TabItem SelectedTab
        {
            get { return selectedTab; }
            set { SetProperty(ref selectedTab, value); }
        }

        private CubeMetadata currentMetadata;
        public CubeMetadata CurrentMetadata
        {
            get { return currentMetadata; }
            set { SetProperty(ref currentMetadata, value); }
        }

        /// <summary>
        /// This should be followed by a single Initialize() call.
        /// </summary>
        public CubeDesignerViewModel() { }

        /// <summary>
        /// Call this after the constructor resolves. We leave the constructor empty to maintain compatibility with VS XAML designer.
        /// </summary>
        private void Initialize()
        {
            // We add to the collections below when importing, so it is important to subscribe these handlers before calling Import().
            // This will create garbage if Initialize() is called more than once.
            CubeTopCollection.CollectionChanged += new NotifyCollectionChangedEventHandler((sender, e) => CubeDesignerCollection_CollectionChanged(sender, e, CubePart.Top));
            CubeBaseCollection.CollectionChanged += new NotifyCollectionChangedEventHandler((sender, e) => CubeDesignerCollection_CollectionChanged(sender, e, CubePart.Base));
            // Start off by populating the cube designer. We can also load/create metadata files here, and add the resulting 
            // cube metadata objects to cube factory metadata collection. From the file paths, we can deduce the cube part type.
            Import(CubePart.All);
            // Rebuild the .pngs into .xnb files.
            ContentBuilder.RebuildContent();

            PropertyChanged += CubeDesignerViewModel_PropertyChanged;
        }

        internal void OnMainGameLoaded(SunbirdMBGame mainGame)
        {
            Initialize();
            // Once the main game has loaded, we have access to the content manager. We can now build our cube factory
            // library by creating textures from paths.
            CubeFactory.BuildLibrary(mainGame);
            // Initial current selections are set here.
            // TODO: Currently we just pick the first item in our tabs.
            SelectTop(CubeTopCollection[0]);
            SelectBase(CubeBaseCollection[0]);
            // Currently our metadata to display in the Properties window is just the current cube top.
            // This just depends on which tab is selected on start-up.
            CurrentMetadata = CubeFactory.CurrentCubeTopMetadata;
            // Now that a current cube exists, go ahead and create a ghost marker image from it. It is important that this gets ddone
            // before the first update and draw calls.
            MapBuilder.GhostMarker.MorphImage(CubeFactory.CreateCurrentCube(Coord.Zero, Coord.Zero, 0));
        }

        private void CubeDesignerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedTab")
            {
                // If the selected tab changes, then we need to switch the metadata displayed in the Properties window.
                switch (selectedTab.Header)
                {
                    case "Top":
                        CurrentMetadata = CubeFactory.CurrentCubeTopMetadata;
                        break;
                    case "Base":
                        CurrentMetadata = CubeFactory.CurrentCubeBaseMetadata;
                        break;
                }
            }
        }

        private void CubeDesignerCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CubePart part)
        {
            if (e.NewItems != null)
            {
                foreach (CubeDesignerItem item in e.NewItems)
                {
                    // CubeDesignerViewModel outlives CubeDesignerItem so no need to unsubscribe.
                    item.PropertyChanged += new PropertyChangedEventHandler((_sender, _e) => CubeDesignerCollectionItem_PropertyChanged(_sender, _e, part));
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

        private void SelectTop(CubeDesignerItem cubeDesignerItem)
        {
            CubeFactory.CurrentCubeTopMetadata = CubeFactory.CubeMetadataLibrary[cubeDesignerItem.ContentPath];
            cubeDesignerItem.Selected = true;
        }

        private void SelectBase(CubeDesignerItem cubeDesignerItem)
        {
            CubeFactory.CurrentCubeBaseMetadata = CubeFactory.CubeMetadataLibrary[cubeDesignerItem.ContentPath];
            cubeDesignerItem.Selected = true;
        }

        /// <summary>
        /// Import a single cube part into the cube designer and cube factory metadata collection (unbuilt).
        /// </summary>
        internal void Import(string path, CubePart part)
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            var appDirectory = appPath.TrimEnd(Path.GetFileName(appPath));
            Debug.Assert(appDirectory == @"D:\SunbirdMB\bin\Debug\");
            var contentPath = Path.ChangeExtension(path.Replace(appDirectory + @"Content\", ""), null);
            Debug.Assert(path.Length != contentPath.Length);
            if (path.Length == contentPath.Length)
            {
                "Import cancelled. Attempt was made to import from outside of the Content directory.".Log();
                return;
            }
            // Ask what cube part we are importing.
            if (part == CubePart.Top)
            {
                CubeTopCollection.Add(new CubeDesignerItem(this, path, contentPath));
            }
            else if (part == CubePart.Base)
            {
                CubeBaseCollection.Add(new CubeDesignerItem(this, path, contentPath));
            }
            // Find or create cube metadata.
            var metadata = Path.ChangeExtension(path, ".metadata");
            CubeMetadata cmd;
            if (File.Exists(metadata))
            {
                cmd = Serializer.ReadXML<CubeMetadata>(CubeMetadata.CubeMetaDataSerializer, metadata);
            }
            else
            {
                cmd = new CubeMetadata() { ContentPath = contentPath, Part = part, SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None };
                cmd.Serialize(metadata);
                $"Creating {Path.GetFileName(metadata)}...".Log();
            }
            CubeFactory.CubeMetadataCollection.Add(cmd);
        }

        /// <summary>
        /// Import cube parts into the cube designer and cube library (unbuilt).
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
                var appPath = Assembly.GetExecutingAssembly().Location;
                var appDirectory = appPath.TrimEnd(Path.GetFileName(appPath));
                Debug.Assert(appDirectory == @"D:\SunbirdMB\bin\Debug\");
                var contentDirectory = string.Empty;
                if (part == CubePart.Top)
                {
                    contentDirectory = Path.Combine(appDirectory, "Content", "Cubes", "Top");
                    Debug.Assert(contentDirectory == @"D:\SunbirdMB\bin\Debug\Content\Cubes\Top");
                }
                else if (part == CubePart.Base)
                {
                    contentDirectory = Path.Combine(appDirectory, "Content", "Cubes", "Base");
                    Debug.Assert(contentDirectory == @"D:\SunbirdMB\bin\Debug\Content\Cubes\Base");
                }
                var files = Directory.GetFiles(contentDirectory, "*.png", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var contentPath = Path.ChangeExtension(file.Replace(appDirectory + @"Content\", ""), null);
                    if (part == CubePart.Top)
                    {
                        CubeTopCollection.Add(new CubeDesignerItem(this, file, contentPath));
                    }
                    else if (part == CubePart.Base)
                    {
                        CubeBaseCollection.Add(new CubeDesignerItem(this, file, contentPath));
                    }
                    var metadata = Path.ChangeExtension(file, ".metadata");
                    CubeMetadata cmd;
                    if (File.Exists(metadata))
                    {
                        cmd = Serializer.ReadXML<CubeMetadata>(CubeMetadata.CubeMetaDataSerializer, metadata);
                    }
                    else
                    {
                        cmd = new CubeMetadata() { ContentPath = contentPath, Part = part, SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None };
                        cmd.Serialize(metadata);
                        $"Creating {Path.GetFileName(metadata)}...".Log();
                    }
                    CubeFactory.CubeMetadataCollection.Add(cmd);
                }
            }
        }
    }

}
