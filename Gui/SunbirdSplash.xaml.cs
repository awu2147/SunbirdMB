using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SunbirdMB.Gui
{
    /// <summary>
    /// Interaction logic for SunbirdSplash.xaml
    /// </summary>
    public partial class SunbirdSplash : Window
    {
        internal static SunbirdSplashViewModel ViewModel;

        public SunbirdSplash(CancellationToken cancelSplashToken)
        {
            InitializeComponent();
            ViewModel = new SunbirdSplashViewModel(this, Dispatcher);
            DataContext = ViewModel;

            //Task.Run(() => ViewModel.StartPolling(cancelSplashToken));
        }
    }

    public class SunbirdSplashViewModel : PropertyChangedBase
    {
        private int progress;
        public int Progress
        {
            get { return progress; }
            set { SetProperty(ref progress, value); }
        }

        public int Target;

        public readonly Window splashScreen;
        private readonly Dispatcher splashScreenDispatcher;

        public SunbirdSplashViewModel(Window window, Dispatcher dispatcher) 
        { 
            splashScreen = window;
            splashScreenDispatcher = dispatcher;
        }

        internal async void StartPolling(CancellationToken cancelSplashToken)
        {
            for (int i = 0; i < 50; i++)
            {
                if (cancelSplashToken.IsCancellationRequested)
                {
                    break;
                }
                Progress = Target;
                await Task.Delay(100);
            }
            splashScreenDispatcher.Invoke(() => splashScreen.Close());
        }

    }

}
