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

namespace SunbirdMB
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Actor> ActorList { get; set; } = new ObservableCollection<Actor>();

        internal MainWindow View { get; set; }

        public MainWindowViewModel(MainWindow view)
        {
            View = view;
        }

        internal void Initialize()
        {

            var appPath = Assembly.GetExecutingAssembly().Location;
            string contentPath = Path.Combine(appPath, "..", "..", "..", "Content");
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
    }

    public class Actor
    {
        public string Image { get; set; }

        public Int32Rect SourceRect { get; set; }

        public ICommand C_MouseDown { get; set; }

        public Actor(string imagePath) : this(imagePath, Int32Rect.Empty) { }

        public Actor(string imagePath, Int32Rect sourceRect)
        {
            Image = imagePath;
            SourceRect = sourceRect;
            C_MouseDown = new RelayCommand((o) => MouseDown());
        }

        private void MouseDown()
        {
            "clicked".Log();
            Image.Log();
        }
    }
}
