using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    public interface ISQLResult
    {
        IDictionary<string, object> Parameters { get; set; }
        string ToSting();
        
    }
}
