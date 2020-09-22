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

namespace SunbirdMB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel { get; set; }

        private Config config;

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel(this);
            DataContext = ViewModel;
            Logger.DataContext = new LoggerViewModel();
            MainGame.Loaded += MainGame_Loaded;

            if (File.Exists("Config.xml"))
            {
                config = Serializer.ReadXML<Config>(Config.ConfigSerializer, "Config.xml");
                config.LoadApplicationParameters(this);
            }
            else
            {
                config = new Config();
            }

            //var cb = new TextureContentBuilder(@"D:\", @"Test\bin", @"Test\obj");
            //cb.Targets.Add(@"BloodBowl.png");
            //cb.Build();
        }

        private void MainGame_Loaded(object sender, EventArgs e)
        {
            config.LoadGameParameters(MainGame);

            MainGame.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
            SizeChanged += MainWindow_SizeChanged;
            Closed += MainWindow_Closed;

            ViewModel.Initialize();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            MainGame.SaveAndSerialize();
            config.SaveApplicationParameters(this);
            config.SaveGameParameters(MainGame);
            config.Serialize();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainGame.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
        }

        private void Clear_Log_Click(object sender, RoutedEventArgs e)
        {
            LoggerViewModel.Log.Clear();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            "clicked".Log();
            sender.GetType().ToString().Log();

            Image image = sender as Image;
            var bmi = image.Source as CroppedBitmap;
            bmi.Source.ToString().Log();
        }
    }

}
