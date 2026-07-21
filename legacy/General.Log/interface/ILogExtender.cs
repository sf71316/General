using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Log
{
    interface ILogExtender
    {
        void log(string message, string UserID, LogType type,Level level);
        void log(string message, LogType type, Level level);
        void log(string message, LogType type,Exception expcetion, Level level);
    }
}
