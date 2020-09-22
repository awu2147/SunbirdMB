using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Gui
{
    public class CubeDesignerViewModel : ViewModelBase
    {
        private HashSet<string> CubeTopPaths = new HashSet<string>();
        public ObservableCollection<CubeCatalogItem> CubeTopCollection { get; set; } = new ObservableCollection<CubeCatalogItem>();

        private HashSet<string> CubeBasePaths = new HashSet<string>();
        public ObservableCollection<CubeCatalogItem> CubeBaseCollection { get; set; } = new ObservableCollection<CubeCatalogItem>();

        public CubeDesignerViewModel()
        {
            BuildBase();
            BuildTop();
        }
        internal void BuildTop()
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            var basePath = Path.Combine(appPath, "..", "Content", "Cubes", "Top");
            var files = Directory.GetFiles(basePath);
            foreach (var file in files)
            {
                if (!CubeTopPaths.Contains(file))
                {
                    CubeTopPaths.Add(file);
                    CubeTopCollection.Add(new CubeCatalogItem(file));
                }
            }
        }

        internal void BuildBase()
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            var basePath = Path.Combine(appPath, "..", "Content", "Cubes", "Base");
            var files = Directory.GetFiles(basePath);
            foreach (var file in files)
            {
                if (!CubeBasePaths.Contains(file))
                {
                    CubeBasePaths.Add(file);
                    CubeBaseCollection.Add(new CubeCatalogItem(file));
                }
            }
        }
    }
}
