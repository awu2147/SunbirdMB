using SunbirdMB.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Framework
{
    public static class Extensions
    {
        public static void Log(this string message)
        {
            var time = LoggerViewModel.TimeNow();
            LoggerViewModel.Log.Add(new LogMessage(time, message));
            if (LoggerViewModel.Log.Count > 200)
            {
                LoggerViewModel.Log.RemoveAt(0);
            }
        }
    }
}
