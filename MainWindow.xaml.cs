using SunbirdMB.Gui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SunbirdMB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
        }

        private void Clear_Log_Click(object sender, RoutedEventArgs e)
        {
            LoggerViewModel.Log.Clear();
        }
    }
}
