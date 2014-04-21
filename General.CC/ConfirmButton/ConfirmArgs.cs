using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.CC.ConfirmButton
{
    public class ConfirmArgs : EventArgs
    {
        public bool DialogResult { get; set; }
    }
}
