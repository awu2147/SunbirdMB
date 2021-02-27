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
        public ICommand C_Sort { get; set; }
        public ICommand C_Save { get; set; }

        private CubeDesignerViewModel cubeDesignerViewModel;
        public CubeDesignerViewModel CubeDesignerViewModel
        {
            get { return cubeDesignerViewModel; }
            set { SetProperty(ref cubeDesignerViewModel, value); }
        }

        private DecoCatalogViewModel decoCatalogViewModel;
        public DecoCatalogViewModel DecoCatalogViewModel
        {
            get { return decoCatalogViewModel; }
            set { SetProperty(ref decoCatalogViewModel, value); }
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

        private int cubeDecoSelectedIndex = 0;
        public int CubeDecoSelectedIndex
        {
            get { return cubeDecoSelectedIndex; }
            set { SetProperty(ref cubeDecoSelectedIndex, value); }
        }

        public SunbirdMBGame SunbirdMBGame { get; set; }

        public SunbirdMBWindowViewModel(SunbirdMBGame sunbirdMBGame)
        {
            SunbirdMBGame = sunbirdMBGame;

            C_Sort = new RelayCommand((o) => SortCubes());
            C_Save = new RelayCommand((o) => SaveGame());

            CubeDesignerViewModel = new CubeDesignerViewModel(sunbirdMBGame);
            decoCatalogViewModel = new DecoCatalogViewModel(sunbirdMBGame);
            LoggerViewModel = new LoggerViewModel();
            MainToolbarViewModel = new MainToolbarViewModel(this);
        }

        private void SaveGame()
        {
            SunbirdMBGame.SaveAndSerialize();
        }

        private void SortCubes()
        {
            CubeDesignerViewModel.SortAll();
        }

       
    }

}
