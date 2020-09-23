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
        }

        private void Build()
        {
            //View.CubeDesignerViewModel.ImportBase();
            //View.CubeDesignerViewModel.ImportTop();
        }

        private void Import()
        {
            var selectedTab = View.CubeDesigner.SelectedValue as TabItem;
            
            var appPath = Assembly.GetExecutingAssembly().Location;
            var destinationPath = string.Empty;
            if (selectedTab.Header.ToString() == "Base")
            {
                destinationPath = Path.Combine(appPath, "..", "Content", "Cubes", "Base");
            }
            else if (selectedTab.Header.ToString() == "Top")
            {
                destinationPath = Path.Combine(appPath, "..", "Content", "Cubes", "Top");
            }
            "import".Log();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                openFileDialog.FileName.Log();
                Path.GetPathRoot(openFileDialog.FileName).Log();
                File.Copy(openFileDialog.FileName, Path.Combine(destinationPath, Path.GetFileName(openFileDialog.FileName)));
                // .png
                var path = Path.Combine(destinationPath, Path.GetFileName(openFileDialog.FileName));
                var cmd = new CubeMetaData() { Path = path, SheetRows = 1, SheetColumns = 1, FrameCount = 1, AnimState = AnimationState.None };
                cmd.Serialize(Path.ChangeExtension(path, ".metadata"));
            }
            
        }
    }

}
