using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace General.Data
{
    public interface IDACORM
    {
        T GetEntity<T>() where T : IEntity;
        IEnumerable<TEntity> GetEntities<TEntity>()
            where TEntity : IEntity;
    }
}
