using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    internal interface ICommandBuilder
    {
        string TableName { get; set; }
    }
}
