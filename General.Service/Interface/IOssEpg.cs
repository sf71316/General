using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Service
{
    public interface IOssEpg : IDisposable, IServiceCommon
    {
       string ReadEPGByDateRange(DateTime starttime, DateTime endtime);
    }
}
