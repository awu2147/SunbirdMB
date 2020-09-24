using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace SunbirdMB.Gui
{
    public class CubeDesignerViewModel : PropertyChangedBase
    {
        public static ObservableCollection<CubeCatalogItem> CubeTopCollection { get; set; } = new ObservableCollection<CubeCatalogItem>();
        public static ObservableCollection<CubeCatalogItem> CubeBaseCollection { get; set; } = new ObservableCollection<CubeCatalogItem>();

        public static Grid CubePropertiesGrid { get; set; }

        public CubeDesignerViewModel(Grid cubePropertiesGrid)
        {
            CubePropertiesGrid = cubePropertiesGrid;
            // Start off by populating the cube designer. We can also load/create metadata files here, and add the resulting 
            // cube metadata objects to cube factory metadata collection. From the file paths, we can deduce the cube part type.
            Import(CubePart.All);
            // Rebuild the .pngs into .xnb files.
            ContentBuilder.RebuildContent();
        }

        internal static void SetPropertyGridDataContext(CubeMetaData cmd)
        {
            CubePropertiesGrid.DataContext = cmd;
        }

        internal void OnMainGameLoaded(MainGame mainGame)
        {
            // Once the main game has loaded, we have access to the content manager. We can now build our cube factory
            // library by creating textures from paths.
            CubeFactory.BuildLibrary(mainGame);
            // Initial current selections are set here.
            CubeFactory.CurrentCubeTopMetaData = CubeFactory.CubeMetaDataLibrary[CubeTopCollection[0].ContentPath];
            CubeFactory.CurrentCubeBaseMetaData = CubeFactory.CubeMetaDataLibrary[CubeBaseCollection[0].ContentPath];

            CubePropertiesGrid.DataContext = CubeFactory.CurrentCubeTopMetaData;
        }

        /// <summary>
        /// Import a single cube part into the cube designer and cube factory metadata collection (unbuilt).
        /// </summary>
        internal static void Import(string path, CubePart part)
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
                CubeTopCollection.Add(new CubeCatalogItem(path, contentPath));
            }
            else if (part == CubePart.Base)
            {
                CubeBaseCollection.Add(new CubeCatalogItem(path, contentPath));
            }
            // Find or create cube metadata.
            var metadata = Path.ChangeExtension(path, ".metadata");
            CubeMetaData cmd;
            if (File.Exists(metadata))
            {
                cmd = Serializer.ReadXML<CubeMetaData>(CubeMetaData.CubeMetaDataSerializer, metadata);
            }
            else
            {
                cmd = new CubeMetaData() { ContentPath = contentPath, Part = part, SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None };
                cmd.Serialize(metadata);
                $"Creating {Path.GetFileName(metadata)}...".Log();
            }
            CubeFactory.CubeMetaDataCollection.Add(cmd);
        }

        /// <summary>
        /// Import cube parts into the cube designer and cube library (unbuilt).
        /// </summary>
        internal static void Import(CubePart part)
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
                        CubeTopCollection.Add(new CubeCatalogItem(file, contentPath));
                    }
                    else if (part == CubePart.Base)
                    {
                        CubeBaseCollection.Add(new CubeCatalogItem(file, contentPath));
                    }
                    var metadata = Path.ChangeExtension(file, ".metadata");
                    CubeMetaData cmd;
                    if (File.Exists(metadata))
                    {
                        cmd = Serializer.ReadXML<CubeMetaData>(CubeMetaData.CubeMetaDataSerializer, metadata);
                    }
                    else
                    {
                        cmd = new CubeMetaData() { ContentPath = contentPath, Part = part, SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None };
                        cmd.Serialize(metadata);
                        $"Creating {Path.GetFileName(metadata)}...".Log();
                    }
                    CubeFactory.CubeMetaDataCollection.Add(cmd);
                }
            }
        }
    }
}
