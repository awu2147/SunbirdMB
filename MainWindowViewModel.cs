using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Gui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace SunbirdMB
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<CubeBaseItem> CubeBaseCollection { get; set; } = new ObservableCollection<CubeBaseItem>();

        internal MainWindow View { get; set; }
        public ICommand C_Import { get; set; }
        public ICommand C_Build { get; set; }

        public MainWindowViewModel(MainWindow view)
        {
            View = view; 
            C_Import = new RelayCommand((o) => Import());
            C_Build = new RelayCommand((o) => Build());
        }

        internal void Initialize()
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            string contentPath = Path.Combine(appPath, "..", "..", "..", "Content");
            var test = Path.Combine(contentPath, CubeFactory.CubeTopMetaDataLibrary[0].Path + ".png");
            test.Log();

            Build();
        }

        private void Import()
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            var basePath = Path.Combine(appPath, "..", "Content", "Cubes", "Base");
            "import".Log();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                openFileDialog.FileName.Log();
                Path.GetPathRoot(openFileDialog.FileName).Log();
                File.Copy(openFileDialog.FileName, Path.Combine(basePath, Path.GetFileName(openFileDialog.FileName)));
            }
        }

        private void Build()
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            var basePath = Path.Combine(appPath, "..", "Content", "Cubes", "Base");
            var files = Directory.GetFiles(basePath);
            foreach (var file in files)
            {
                CubeBaseCollection.Add(new CubeBaseItem(file));
            }
        }
    }

}
