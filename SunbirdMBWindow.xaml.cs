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
        private SunbirdMBWindowViewModel SunbirdMBWindowViewModel { get; set; }
        internal SunbirdMBGame SunbirdMBGame { get; set; }

        private Config Config { get; set; }

        public SunbirdMBWindow()
        {
            InitializeComponent();

            SunbirdMBGame = MainGame;
            SunbirdMBGame.Loaded += SunbirdMBGame_Loaded;

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

        private void SunbirdMBGame_Loaded(object sender, EventArgs e)
        {
            Config.LoadGameParameters(SunbirdMBGame);

            SunbirdMBGame.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
            SizeChanged += MainWindow_SizeChanged;
            Closed += MainWindow_Closed;

            SunbirdMBWindowViewModel.OnMainGameLoaded(SunbirdMBGame);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            SunbirdMBGame.SaveAndSerialize();
            Config.SaveApplicationParameters(this);
            Config.SaveGameParameters(SunbirdMBGame);
            Config.Serialize();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SunbirdMBGame.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
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
