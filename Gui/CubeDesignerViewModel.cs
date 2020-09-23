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

namespace SunbirdMB.Gui
{
    public class CubeDesignerViewModel : ViewModelBase
    {
        public enum Part
        { 
            Top,
            Base,
            All
        }

        public ObservableCollection<CubeCatalogItem> CubeTopCollection { get; set; } = new ObservableCollection<CubeCatalogItem>();
        public ObservableCollection<CubeCatalogItem> CubeBaseCollection { get; set; } = new ObservableCollection<CubeCatalogItem>();

        public CubeDesignerViewModel()
        {
            Import(Part.All);
            Rebuild();
        }

        internal void Import(Part part)
        {
            if (part == Part.All)
            {
                Import(Part.Top);
                Import(Part.Base);
            }
            else
            {
                var appPath = Assembly.GetExecutingAssembly().Location;
                var appDirectory = appPath.TrimEnd(Path.GetFileName(appPath));
                Debug.Assert(appDirectory == @"D:\SunbirdMB\bin\Debug\");
                var contentPath = string.Empty;
                if (part == Part.Top)
                {
                    contentPath = Path.Combine(appDirectory, "Content", "Cubes", "Top");
                    Debug.Assert(contentPath == @"D:\SunbirdMB\bin\Debug\Content\Cubes\Top");
                }
                else if (part == Part.Base)
                {
                    contentPath = Path.Combine(appDirectory, "Content", "Cubes", "Base");
                    Debug.Assert(contentPath == @"D:\SunbirdMB\bin\Debug\Content\Cubes\Base");
                }
                var files = Directory.GetFiles(contentPath, "*.png", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var pathNoExt = Path.ChangeExtension(file, null);
                    var metadata = Path.ChangeExtension(file, ".metadata");
                    CubeMetaData cmd;
                    if (File.Exists(metadata))
                    {
                        cmd = Serializer.ReadXML<CubeMetaData>(CubeMetaData.CubeMetaDataSerializer, metadata);
                    }
                    else
                    {
                        cmd = new CubeMetaData() { Path = pathNoExt, SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None };
                        cmd.Serialize(metadata);
                        $"Creating {Path.GetFileName(metadata)}...".Log();
                    }
                    if (part == Part.Top)
                    {
                        CubeTopCollection.Add(new CubeCatalogItem(file));
                        CubeFactory.CubeTopMetaDataLibrary.Add(cmd);
                    }
                    else if (part == Part.Base)
                    {
                        CubeBaseCollection.Add(new CubeCatalogItem(file));
                        CubeFactory.CubeBaseMetaDataLibrary.Add(cmd);
                    }
                }
            }
        }

        internal void Rebuild()
        {
            "Rebuilding content...".Log();
            var appPath = Assembly.GetExecutingAssembly().Location;
            var appDirectory = appPath.TrimEnd(Path.GetFileName(appPath));
            Debug.Assert(appDirectory == @"D:\SunbirdMB\bin\Debug\");
            var contentPath = Path.Combine(appDirectory, "Content");
            var files = Directory.GetFiles(contentPath, "*.png", SearchOption.AllDirectories);
            var cb = new TextureContentBuilder();
            foreach (var file in files)
            {
                var target = file.Replace(appDirectory, "");
                cb.Targets.Add(target);
            }
            cb.Build();
        }

    }
}
