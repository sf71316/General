using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace General.Log
{
    public  class NLogExtender:ILogExtender
    {
        public void log(string message, string UserID, LogType type, Level level)
        {
            throw new NotImplementedException();
        }

        public void log(string message, LogType type, Level level)
        {
            throw new NotImplementedException();
        }

        public void log(string message, LogType type, Exception expcetion, Level level)
        {
            throw new NotImplementedException();
        }
        private void log()
        {
            Logger log = LogManager.GetLogger("");
            
        }
    }
}
