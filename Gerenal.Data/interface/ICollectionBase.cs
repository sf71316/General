using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;

namespace General.Data
{
    public interface ICollectionBase<T> where T:IEntity
    {
        void Add(T t);


    }

    

}
