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

        public CubeDesignerViewModel() { }

        internal void Initialize()
        {
            //CubeTopCollection = new ObservableCollection<CubeDesignerItem>();
            //CubeBaseCollection = new ObservableCollection<CubeDesignerItem>();
            CubeTopCollection.CollectionChanged += CubeTopCollection_CollectionChanged;
            CubeBaseCollection.CollectionChanged += CubeBaseCollection_CollectionChanged;
            // Start off by populating the cube designer. We can also load/create metadata files here, and add the resulting 
            // cube metadata objects to cube factory metadata collection. From the file paths, we can deduce the cube part type.
            Import(CubePart.All);
            // Rebuild the .pngs into .xnb files.
            ContentBuilder.RebuildContent();
        }

        private void CubeBaseCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                    item.PropertyChanged -= CubeBaseCollectionItem_PropertyChanged;
            }
            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += CubeBaseCollectionItem_PropertyChanged;
            }
        }

        private void CubeBaseCollectionItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CubeDesignerItem cci = sender as CubeDesignerItem;
            if (e.PropertyName == "Selected" && cci.Selected == true)
            {
                foreach (var item in CubeBaseCollection)
                {
                    if (item != cci)
                    {
                        item.Selected = false;
                    }
                }
            }
        }

        private void CubeTopCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                    item.PropertyChanged -= CubeTopCollectionItem_PropertyChanged;
            }
            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += CubeTopCollectionItem_PropertyChanged;
            }
        }

        private void CubeTopCollectionItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CubeDesignerItem cci = sender as CubeDesignerItem;
            if (e.PropertyName == "Selected" && cci.Selected == true)
            {
                foreach (var item in CubeTopCollection)
                {
                    if (item != cci)
                    {
                        item.Selected = false;
                    }
                }
            }
        }

        internal void OnMainGameLoaded(SunbirdMBGame mainGame)
        {
            // Once the main game has loaded, we have access to the content manager. We can now build our cube factory
            // library by creating textures from paths.
            CubeFactory.BuildLibrary(mainGame);
            // Initial current selections are set here.
            CubeFactory.CurrentCubeTopMetaData = CubeFactory.CubeMetaDataLibrary[CubeTopCollection[0].ContentPath];
            CubeFactory.CurrentCubeBaseMetaData = CubeFactory.CubeMetaDataLibrary[CubeBaseCollection[0].ContentPath];

            CubeTopCollection[0].Selected = true;
            CubeBaseCollection[0].Selected = true;
            CurrentMetadata = CubeFactory.CurrentCubeTopMetaData;
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
            CubeFactory.CubeMetaDataCollection.Add(cmd);
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
                    CubeFactory.CubeMetaDataCollection.Add(cmd);
                }
            }
        }
    }

    public class StyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? dataValue = values[0] as bool?;
            Style firstStyle = values[1] as Style;
            Style secondStyle = values[2] as Style;

            return dataValue.Equals(false) ? firstStyle : secondStyle;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
