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

namespace SunbirdMB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Actor> ActorList { get; set; } = new ObservableCollection<Actor>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Logger.DataContext = new LoggerViewModel();
            MainGameWindow.Loaded += MainGameWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            MainGameWindow.OnExit();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainGameWindow.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
        }

        private void MainGameWindow_Loaded(object sender, EventArgs e)
        {
            Debug.Print(MainGamePanel.ActualWidth.ToString());
            Debug.Print(MainGamePanel.ActualHeight.ToString());
            MainGameWindow.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
            SizeChanged += MainWindow_SizeChanged;

            var appPath = Assembly.GetExecutingAssembly().Location;
            string contentPath = Path.Combine(appPath, "..", "..", "..","Content");
            var test = Path.Combine(contentPath, CubeFactory.CubeTopMetaDataLibrary[0].Path + ".png");
            test.Log();

            ActorList.Add(new Actor(test, new Int32Rect(0, 0, 72, 75)));
            ActorList.Add(new Actor(test, new Int32Rect(0, 75, 72, 75)));
            ActorList.Add(new Actor(test, new Int32Rect(72, 0, 72, 75)));
            ActorList.Add(new Actor(test, new Int32Rect(72, 75, 72, 75)));
        }

        private void Clear_Log_Click(object sender, RoutedEventArgs e)
        {
            LoggerViewModel.Log.Clear();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            "clicked".Log();
        }
    }

    public class Actor
    {
        public string Image { get; set; }

        public Int32Rect SourceRect { get; set; }

        public Actor(string imagePath) : this(imagePath, Int32Rect.Empty)
        {
            Image = imagePath;
        }

        public Actor(string imagePath, Int32Rect sourceRect)
        {
            Image = imagePath;
            SourceRect = sourceRect;
        }
    }
}
