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
    internal class SunbirdMBWindowViewModel : PropertyChangedBase
    {
        public ICommand C_Import { get; set; }
        public ICommand C_Build { get; set; }

        private CubeDesignerViewModel cubeDesignerViewModel;
        public CubeDesignerViewModel CubeDesignerViewModel
        {
            get { return cubeDesignerViewModel; }
            set { SetProperty(ref cubeDesignerViewModel, value); }
        }

        private LoggerViewModel loggerViewModel;
        public LoggerViewModel LoggerViewModel
        {
            get { return loggerViewModel; }
            set { SetProperty(ref loggerViewModel, value); }
        }

        private MainToolbarViewModel mainToolbarViewModel;
        public MainToolbarViewModel MainToolbarViewModel
        {
            get { return mainToolbarViewModel; }
            set { SetProperty(ref mainToolbarViewModel, value); }
        }

        private int gameWidth = 900;
        public int GameWidth
        {
            get { return gameWidth; }
            set { SetProperty(ref gameWidth, value); }
        }

        private int gameHeight = 600;
        public int GameHeight
        {
            get { return gameHeight; }
            set { SetProperty(ref gameHeight, value); }
        }

        public SunbirdMBGame SunbirdMBGame { get; set; }

        public SunbirdMBWindowViewModel(SunbirdMBGame sunbirdMBGame)
        {
            SunbirdMBGame = sunbirdMBGame;

            C_Import = new RelayCommand((o) => Import());
            C_Build = new RelayCommand((o) => Build());

            CubeDesignerViewModel = new CubeDesignerViewModel();
            LoggerViewModel = new LoggerViewModel();
            MainToolbarViewModel = new MainToolbarViewModel();
        }

        private void Build()
        {
            CubeDesignerViewModel.SortAll();
        }

        private void Import()
        {
            var selectedTab = CubeDesignerViewModel.SelectedTab;

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
                    if (File.Exists(newFilePath))
                    {
                        "Cannot copy to directory, file already exists.".Log();
                    }
                    else
                    {
                        File.Copy(openFileDialog.FileName, newFilePath);
                        CubeDesignerViewModel.Import(newFilePath);
                        // We don't need to rebuild everything, only the .png we just imported.
                        ContentBuilder.BuildFile(newFilePath);
                        CubeFactory.BuildLibrary(SunbirdMBGame);
                    }
                }
                else
                {
                    "Incorrect file format".Log();
                }
            }
            
        }
    }

}
