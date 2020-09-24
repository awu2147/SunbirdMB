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
using System.Diagnostics;
using SunbirdMB.Tools;

namespace SunbirdMB
{
    internal class MainWindowViewModel : PropertyChangedBase
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

        private void Build()
        {
            //View.CubeDesignerViewModel.ImportBase();
            //View.CubeDesignerViewModel.ImportTop();
        }

        private void Import()
        {
            var selectedTab = View.CubeDesigner.SelectedValue as TabItem;

            var appPath = Assembly.GetExecutingAssembly().Location;
            var appDirectory = appPath.TrimEnd(Path.GetFileName(appPath));
            Debug.Assert(appDirectory == @"D:\SunbirdMB\bin\Debug\");
            var contentDirectory = string.Empty;
            if (selectedTab.Header.ToString() == "Top")
            {
                contentDirectory = Path.Combine(appDirectory, "Content", "Cubes", "Top");
                Debug.Assert(contentDirectory == @"D:\SunbirdMB\bin\Debug\Content\Cubes\Top");
            }
            else if (selectedTab.Header.ToString() == "Base")
            {
                contentDirectory = Path.Combine(appDirectory, "Content", "Cubes", "Base");
                Debug.Assert(contentDirectory == @"D:\SunbirdMB\bin\Debug\Content\Cubes\Base");
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (Path.GetExtension(openFileDialog.FileName) == ".png")
                {
                    var newFilePath = Path.Combine(contentDirectory, Path.GetFileName(openFileDialog.FileName));
                    File.Copy(openFileDialog.FileName, newFilePath);
                    if (selectedTab.Header.ToString() == "Top")
                    {
                        CubeDesignerViewModel.Import(newFilePath, CubePart.Top);
                    }
                    else if (selectedTab.Header.ToString() == "Base")
                    {
                        CubeDesignerViewModel.Import(newFilePath, CubePart.Base);
                    }
                    // We don't need to rebuild everything, only the .png we just imported.
                    ContentBuilder.BuildFile(newFilePath);
                    CubeFactory.BuildLibrary(View.MainGame);
                }
                else
                {
                    "Incorrect file format".Log();
                }
            }
            
        }
    }

}
