using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Gerneral.Helper
{
    public class ValidateGroup:IList<Validation>,IEnumerable<Validation>
    {
        List<Validation> _list = new List<Validation>();
        public int IndexOf(Validation item)
        {
            return this._list.IndexOf(item);
        }

        public void Insert(int index, Validation item)
        {
            this._list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this._list.RemoveAt(index);
        }

        public Validation this[int index]
        {
            get
            {
                return this._list[index];
            }
            set
            {
                this._list[index] = value;
            }
        }

        public void Add(Validation item)
        {
            
        }

        public void Clear()
        {
            this._list.Clear();
        }

        public bool Contains(Validation item)
        {
            return this._list.Contains(item);
        }

        public void CopyTo(Validation[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get {return this._list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Validation item)
        {
            return this._list.Remove(item);
        }

        public IEnumerator<Validation> GetEnumerator()
        {
            return this._list.GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
