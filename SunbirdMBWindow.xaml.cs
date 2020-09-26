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
using Microsoft.Xna.Framework;
using SharpDX.Direct3D11;

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

            SnapsToDevicePixels = true;
            KeyDown += SunbirdMBWindow_KeyDown;

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

        private void SunbirdMBWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                Console.WriteLine($"{Width} {Height}");
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
        const int WM_SYSCOMMAND = 0x0112;
        IntPtr SC_RESTORE = (IntPtr)0xF120;
        IntPtr SC_MINIMIZE = (IntPtr)0XF020;
        IntPtr SC_MAXIMIZE = (IntPtr)0xF030;
        private static bool IsResizing = false;
        private static bool WindowWasResized = false;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            IntPtr _wParam = (IntPtr)(wParam.ToInt32() & 0xFFF0);
            if (msg == WM_SIZING)
            {
                if (IsResizing == false)
                {
                    // Indicate the the user is resizing and not moving the window
                    IsResizing = true;
                }
            }
            // Handle messages...
            if (msg == WM_EXITSIZEMOVE && IsResizing && gameLoaded)
            {
                ResizeGameWindow();
                IsResizing = false;
            }
            if (msg == WM_SYSCOMMAND)
            {
                if (_wParam == SC_MAXIMIZE && gameLoaded)
                {
                    WindowWasResized = true;
                    Console.WriteLine("maximize");
                }
                if (_wParam == SC_MINIMIZE && gameLoaded)
                {
                    WindowWasResized = true;
                    Console.WriteLine("minimize");
                }
                if (_wParam == SC_RESTORE && gameLoaded)
                {
                    WindowWasResized = true;
                    Console.WriteLine("restore");
                }
            }

            return IntPtr.Zero;
        }

        private void ResizeGameWindow()
        {
            Console.WriteLine($"{(int)MainGamePanel.ActualWidth} {(int)MainGamePanel.ActualHeight}");
            SunbirdMBWindowViewModel.GameWidth = (int)MainGamePanel.ActualWidth;
            SunbirdMBWindowViewModel.GameHeight = (int)MainGamePanel.ActualHeight;
            SunbirdMBGame.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
        }

        private void Game_Loaded(object sender, EventArgs e)
        {
            Config.LoadGameParameters(SunbirdMBGame);

            ResizeGameWindow();
            SizeChanged += Window_SizeChanged;
            Closed += Window_Closed;

            SunbirdMBWindowViewModel.OnMainGameLoaded(SunbirdMBGame);

            gameLoaded = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {            
            if (WindowWasResized)
            {
                Console.WriteLine("size changed");
                ResizeGameWindow();
                WindowWasResized = false;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SunbirdMBGame.SaveAndSerialize();
            Config.SaveApplicationParameters(this);
            Config.SaveGameParameters(SunbirdMBGame);
            Config.Serialize();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BackgroundGrid.Focus();
        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            ResizeGameWindow();
        }
    }

}
