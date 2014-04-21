using System;
using System.Data.Common;
namespace General.Data
{
   public interface IDACTransaction
    {
        DbConnection Connection { get; set; }
        DbTransaction Transaction { get; set; }
    }
}
