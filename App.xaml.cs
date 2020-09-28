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
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Create the token source for closing the splash screen.
            CancellationTokenSource cancelSplashTokenSource = new CancellationTokenSource();

            // Run the splash screen in a new background thread.
            Thread splashThread = new Thread(new ThreadStart(() => LaunchSplash(cancelSplashTokenSource.Token)));
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.IsBackground = true;
            splashThread.Start();

            // Run the main window.
            var mainWindow = new SunbirdMBWindow(cancelSplashTokenSource, splashThread);
            mainWindow.Show();
        }

        private void LaunchSplash(CancellationToken cancelSplashToken)
        {
            // Create our context, and install it:
            // http://reedcopsey.com/2011/11/28/launching-a-wpf-window-in-a-separate-thread-part-1/#more-321
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
            SunbirdSplash sunbirdSplash = new SunbirdSplash(cancelSplashToken);
            sunbirdSplash.Closed += (s, e) => Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            sunbirdSplash.Show();
            Dispatcher.Run();
        }
    }
}
