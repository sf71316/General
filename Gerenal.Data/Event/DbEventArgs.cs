using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace General.Data
{
    public class DbEventArgs : EventArgs
    {
        public DbEventArgs()
        {
            this.Settings = new ConnectionStringSettings();
        }
        public DbEventArgs(ConnectionStringSettings settings)
        {
            this.Settings = settings;
        }
        public ConnectionStringSettings Settings { get; set; }
        public bool Modified { get; set; }
    }
}
