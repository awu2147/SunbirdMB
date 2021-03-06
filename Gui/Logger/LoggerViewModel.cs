﻿using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SunbirdMB.Gui
{
    public class LoggerViewModel : PropertyChangedBase
    {
        public static ObservableCollection<LogMessage> Log { get; set; } = new ObservableCollection<LogMessage>();

        public ICommand C_ClearLog { get; set; }

        public LoggerViewModel()
        {
            C_ClearLog = new RelayCommand((o) => ClearLog());
        }

        private void ClearLog()
        {
            Log.Clear();
        }

        public static string TimeNow()
        {
            var timeNow = DateTime.Now;
            var time24 = timeNow.ToString("HH:mm:ss");
            return time24;
        }

    }

    public static class AutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, AutoScrollPropertyChanged));

        public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var scrollViewer = obj as ScrollViewer;
            if (scrollViewer != null && (bool)args.NewValue)
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                scrollViewer.ScrollToEnd();
            }
            else
            {
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Only scroll to bottom when the extent changed. Otherwise you can't scroll up
            if (e.ExtentHeightChange != 0)
            {
                var scrollViewer = sender as ScrollViewer;
                scrollViewer?.ScrollToBottom();
            }
        }

        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }
    }
}
