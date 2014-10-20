using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
     public interface  IQueryCommand
    {
         bool UseTableAlias { get; set; }
    }
}
