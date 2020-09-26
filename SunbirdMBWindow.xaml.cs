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
using System.Windows.Media.Animation;
using System.Windows.Interop;

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

        private bool gameLoaded;

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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        const int WM_SIZING = 0x214;
        const int WM_EXITSIZEMOVE = 0x0232;
        private static bool WindowWasResized = false;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SIZING)
            {
                if (WindowWasResized == false)
                {
                    //    'indicate the the user is resizing and not moving the window
                    WindowWasResized = true;
                }
            }
            // Handle messages...
            if (msg == WM_EXITSIZEMOVE && WindowWasResized && gameLoaded)
            {
                SunbirdMBWindowViewModel.GameWidth = (int)MainGamePanel.ActualWidth;
                SunbirdMBWindowViewModel.GameHeight = (int)MainGamePanel.ActualHeight;
                SunbirdMBGame.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
                Console.WriteLine("WM_EXITSIZEMOVE");
                WindowWasResized = false;
            }

            return IntPtr.Zero;
        }

        private void Game_Loaded(object sender, EventArgs e)
        {
            Config.LoadGameParameters(SunbirdMBGame);

            SunbirdMBWindowViewModel.GameWidth = (int)MainGamePanel.ActualWidth;
            SunbirdMBWindowViewModel.GameHeight = (int)MainGamePanel.ActualHeight;
            SunbirdMBGame.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
            //SizeChanged += Window_SizeChanged;
            Closed += Window_Closed;

            SunbirdMBWindowViewModel.OnMainGameLoaded(SunbirdMBGame);

            gameLoaded = true;
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
