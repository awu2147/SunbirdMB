﻿using SunbirdMB.Core;
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
    public partial class MainWindow : Window
    {
        private MainWindowViewModel MainWindowViewModel { get; set; }
        internal LoggerViewModel LoggerViewModel { get; set; }
        internal CubeDesignerViewModel CubeDesignerViewModel { get; set; }

        private Config config;

        public MainWindow()
        {
            InitializeComponent();

            MainWindowViewModel = new MainWindowViewModel(this);
            DataContext = MainWindowViewModel;

            LoggerViewModel = new LoggerViewModel();
            Logger.DataContext = LoggerViewModel;

            CubeDesignerViewModel = new CubeDesignerViewModel(CubePropertiesGrid);
            CubeDesigner.DataContext = CubeDesignerViewModel;

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

            var b = CubeDesigner.Items[0] as Button;
            //b.Template.FindName()

        }

        private void MainGame_Loaded(object sender, EventArgs e)
        {
            config.LoadGameParameters(MainGame);

            MainGame.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
            SizeChanged += MainWindow_SizeChanged;
            Closed += MainWindow_Closed;

            CubeDesignerViewModel.OnMainGameLoaded(MainGame);
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

        private void TextBox_IntegerOnly(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Convert.ToDouble(e.Text);
            }
            catch
            {
                e.Handled = true;
            }
        }

        private void TextBox_DoubleOnly(object sender, TextCompositionEventArgs e)
        {
            if (e.Text != ".")
            {
                try
                {
                    Convert.ToDouble(e.Text);
                }
                catch
                {
                    e.Handled = true;
                }
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BackgroundGrid.Focus();
        }
    }

}
