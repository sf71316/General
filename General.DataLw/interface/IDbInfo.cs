using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace General.Data
{
    public interface IDbInfo
    {
        DbCommand Command { get; }
        DbDataAdapter Adapter { get; set; }
        DbConnection Connection { get; set; }
    }
}
