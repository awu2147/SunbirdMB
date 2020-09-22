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

namespace SunbirdMB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Actor> ActorList { get; set; } = new ObservableCollection<Actor>();

        private Config config;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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

        }

        private void MainGame_Loaded(object sender, EventArgs e)
        {
            config.LoadGameParameters(MainGame);

            MainGame.SetCameraTransformMatrix((int)MainGamePanel.ActualWidth, (int)MainGamePanel.ActualHeight);
            SizeChanged += MainWindow_SizeChanged;
            Closed += MainWindow_Closed;
           
            var appPath = Assembly.GetExecutingAssembly().Location;
            string contentPath = Path.Combine(appPath, "..", "..", "..","Content");
            var test = Path.Combine(contentPath, CubeFactory.CubeTopMetaDataLibrary[0].Path + ".png");
            test.Log();

            ActorList.Add(new Actor(test, new Int32Rect(0, 0, 72, 75)));
            ActorList.Add(new Actor(test, new Int32Rect(0, 75, 72, 75)));
            ActorList.Add(new Actor(test, new Int32Rect(72, 0, 72, 75)));
            ActorList.Add(new Actor(test, new Int32Rect(72, 75, 72, 75)));
            ActorList.Add(new Actor(test, new Int32Rect(0, 0, 72, 75)));
            ActorList.Add(new Actor(test, new Int32Rect(0, 75, 72, 75)));
            ActorList.Add(new Actor(test, new Int32Rect(72, 0, 72, 75)));
            ActorList.Add(new Actor(test, new Int32Rect(72, 75, 72, 75)));
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
