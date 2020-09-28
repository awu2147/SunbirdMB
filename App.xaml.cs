using SunbirdMB.Gui;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SunbirdMB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        SunbirdSplash SplashScreen;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Create the token source.
            CancellationTokenSource cts = new CancellationTokenSource();

            //var splashScreen = new SunbirdSplash();
            //splashScreen.Show();
            // TODO: Implement a timeout.
            Thread splashThread = new Thread(new ThreadStart(() => LaunchSplash(cts.Token)));
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.IsBackground = true;
            splashThread.Start();

            var mainWindow = new SunbirdMBWindow(cts, splashThread);
            mainWindow.Show(); // Show the main window
        }

        private void LaunchSplash(CancellationToken ct)
        {
            // Create our context, and install it:
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
            SunbirdSplash sunbirdSplash = new SunbirdSplash(ct);
            sunbirdSplash.Closed += (s, e) => Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            sunbirdSplash.Show();
            Dispatcher.Run();
        }
    }
}
