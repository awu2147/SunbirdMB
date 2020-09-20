using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Framework
{
    public class LogMessage
    {
        public string Time { get; set; }

        public string Message { get; set; }

        //private string time;
        //public string Time
        //{
        //    get { return time; }
        //    set { SetProperty(ref time, value); }
        //}

        //private string message;
        //public string Message
        //{
        //    get { return message; }
        //    set { SetProperty(ref message, value); }
        //}

        public LogMessage(string time, string message)
        {
            Time = time;
            Message = message;
        }
    }
}
