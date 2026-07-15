using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data.ColumnMapper
{
    public sealed class ColumnMappingCollection : IEnumerable<IMappedType>
    {
        /// <summary>
        ///     An instance of <see cref="IDictionary{TKey, TValue}"/> containing each of the column mappings.
        /// </summary>
        public IDictionary<Type, IMappedType> Mappings { get; } = new Dictionary<Type, IMappedType>();

        IEnumerator IEnumerable.GetEnumerator() { return Mappings.GetEnumerator(); }

        IEnumerator<IMappedType> IEnumerable<IMappedType>.GetEnumerator() { return Mappings.Values.GetEnumerator(); }
    }
}
