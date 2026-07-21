using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Entity
{
   public class FileEntity
    {
       public string FileName { get; set; }
       public string NewFileName { get; set; }
       public Guid FileGrid { get; set; }
       public string FilePath { get; set; }
    }
}
