using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.MPF.Controls
{
    public interface IKBContainer
    {
        string LayoutID { get; }
        string DatasourceID { get; }
        string TargetID { get; set; }
    }
}
