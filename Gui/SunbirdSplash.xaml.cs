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

        CancellationToken CT;

        public SunbirdSplash(CancellationToken ct)
        {
            CT = ct;
            InitializeComponent();
            ViewModel = new SunbirdSplashViewModel(this, Dispatcher);
            //Loaded += SunbirdSplash_Loaded;
            DataContext = ViewModel;

            Task.Run(() => ViewModel.FakeUpdate(ct));
        }

        //private void SunbirdSplash_Loaded(object sender, RoutedEventArgs e)
        //{
        //    for (int i = 0; i < 30; i++)
        //    {
        //        if (CT.IsCancellationRequested)
        //        {
        //            Close();
        //            return;
        //        }
        //        Console.WriteLine("yo");
        //        MyProgressBar.Value += 10;
        //        Thread.Sleep(200);
        //    }
        //}
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

        Window View;
        Dispatcher D;

        public SunbirdSplashViewModel(Window view, Dispatcher dis) 
        { 
            View = view;
            D = dis;
        }

        internal async void FakeUpdate(CancellationToken ct)
        {
            for (int i = 0; i < 30; i++)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                Console.WriteLine("yo");
                Progress = Target;
                await Task.Delay(100);
            }
            D.Invoke(() => View.Close());
        }

    }

}
