using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Gui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SunbirdMB.Tools;
using System.Windows.Data;

namespace SunbirdMB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SunbirdMBWindow : Window
    {
        internal SunbirdMBGame SunbirdMBGame { get; set; }
        private SunbirdMBWindowViewModel SunbirdMBWindowViewModel { get; set; }
        private Config Config { get; set; }        

        public SunbirdMBWindow()
        {
            InitializeComponent();

            SunbirdMBGame = MainGame;
            SunbirdMBGame.Loaded += Game_Loaded;

            SunbirdMBWindowViewModel = new SunbirdMBWindowViewModel();
            SunbirdMBWindowViewModel.SunbirdMBGame = SunbirdMBGame;
            DataContext = SunbirdMBWindowViewModel;

            if (File.Exists("Config.xml"))
            {
                Config = Serializer.ReadXML<Config>(Config.ConfigSerializer, "Config.xml");
                Config.LoadApplicationParameters(this);
            }
            else
            {
                Config = new Config();
            }

        }

        private void Game_Loaded(object sender, EventArgs e)
        {
            Config.LoadGameParameters(SunbirdMBGame);

            SunbirdMBGame.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
            SizeChanged += Window_SizeChanged;
            Closed += Window_Closed;

            SunbirdMBWindowViewModel.OnMainGameLoaded(SunbirdMBGame);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SunbirdMBGame.SaveAndSerialize();
            Config.SaveApplicationParameters(this);
            Config.SaveGameParameters(SunbirdMBGame);
            Config.Serialize();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SunbirdMBGame.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BackgroundGrid.Focus();
        }
    }

}
