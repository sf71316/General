using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;

namespace General.Security
{
    public sealed class ADServerSearchingArgs:EventArgs
    {
        public ADServerSearchingArgs(DirectorySearcher ds)
        {
            this.Searcher = ds;
        }
        public DirectorySearcher Searcher { get; set; }
    }
}
