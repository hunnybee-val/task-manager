using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Models
{
    public class LogMessageItem
    {
        public string LogMessage { get; set; }
        public string TimeOfMessage { get; set; }

        public LogMessageItem(string messageValue)
        {
            LogMessage = messageValue;
            TimeOfMessage = DateTime.Now.ToString("T");
        }
    }
}
