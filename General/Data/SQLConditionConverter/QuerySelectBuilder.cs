using General.Data.SQLConditionConverter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace General.Data.SQLConditionConverter
{
    public class QuerySelectBuilder<TEntity> where TEntity : class, new()
    {
        private readonly List<string> _columns = new List<string>();
        private readonly Dictionary<string, string> _conditionColMapping = new Dictionary<string, string>();
        private bool _mappingInitialized = false;

        /// <summary>
        /// 選取單一欄位，支援覆蓋 table alias 和 output alias。
        /// 未指定 tableAlias 時走 ColumnMappingAttribute / IConditionMappingAttribute 預設值。
        /// </summary>
        public QuerySelectBuilder<TEntity> Column(
            Expression<Func<TEntity, object>> column,
            string tableAlias = null,
            string alias = null)
        {
            EnsureMappingInitialized();
            var memberName = GetMemberName(column);
            var resolved = ResolveColumnName(memberName, tableAlias);
            _columns.Add(alias != null ? $"{resolved} AS {alias}" : resolved);
            return this;
        }

        /// <summary>
        /// 群組選取：同一 table alias 下批次選取多個欄位。
        /// </summary>
        public QuerySelectBuilder<TEntity> From(
            string tableAlias,
            params Expression<Func<TEntity, object>>[] columns)
        {
            EnsureMappingInitialized();
            foreach (var column in columns)
            {
                var memberName = GetMemberName(column);
                var resolved = ResolveColumnName(memberName, tableAlias);
                _columns.Add(resolved);
            }
            return this;
        }

        /// <summary>
        /// 加入原始 SQL 欄位表達式（如聚合函數）。
        /// </summary>
        public QuerySelectBuilder<TEntity> RawColumn(string rawSql)
        {
            _columns.Add(rawSql);
            return this;
        }

        /// <summary>
        /// 選取全部欄位。可指定 table alias。
        /// </summary>
        public QuerySelectBuilder<TEntity> ColumnAll(string tableAlias = null)
        {
            _columns.Add(tableAlias != null ? $"{tableAlias}.*" : "*");
            return this;
        }

        /// <summary>
        /// 產出 SELECT 欄位字串。
        /// </summary>
        public string Build()
        {
            if (_columns.Count == 0)
                return "*";
            return string.Join(", ", _columns);
        }

        /// <summary>
        /// 清除已選取的欄位，可重複使用。
        /// </summary>
        public QuerySelectBuilder<TEntity> Reset()
        {
            _columns.Clear();
            return this;
        }

        private void EnsureMappingInitialized()
        {
            if (_mappingInitialized) return;
            _mappingInitialized = true;

            var attr = typeof(TEntity).GetCustomAttributes(true)
                .FirstOrDefault(x => x is IConditionMappingAttribute<TEntity>);
            if (attr != null)
            {
                var interceptor = attr as IConditionMappingAttribute<TEntity>;
                interceptor.Mapping();
                foreach (var m in interceptor.MappingCollection)
                {
                    var name = GetMemberNameFromBody(m.Key.Body);
                    if (name != null && !_conditionColMapping.ContainsKey(name))
                    {
                        _conditionColMapping.Add(name, m.Value);
                    }
                }
            }
        }

        private string ResolveColumnName(string memberName, string tableAliasOverride)
        {
            // 1. IConditionMappingAttribute mapping
            if (_conditionColMapping.TryGetValue(memberName, out var mappedName))
            {
                if (tableAliasOverride != null)
                {
                    // 去掉 mapping 中原有的 alias prefix，改用覆蓋值
                    var dotIndex = mappedName.LastIndexOf('.');
                    var colOnly = dotIndex >= 0 ? mappedName.Substring(dotIndex + 1) : mappedName;
                    return $"{tableAliasOverride}.{colOnly}";
                }
                return mappedName;
            }

            // 2. ColumnMappingAttribute on property
            var prop = typeof(TEntity).GetProperty(memberName);
            if (prop != null)
            {
                var colAttr = prop.GetCustomAttributes(true)
                    .FirstOrDefault(x => x is ColumnMappingAttribute) as ColumnMappingAttribute;
                if (colAttr != null)
                {
                    var colName = colAttr.ColumnName;
                    var tAlias = tableAliasOverride ?? colAttr.AliasName;
                    return string.IsNullOrEmpty(tAlias) ? colName : $"{tAlias}.{colName}";
                }
            }

            // 3. 原始 property name
            return tableAliasOverride != null ? $"{tableAliasOverride}.{memberName}" : memberName;
        }

        private string GetMemberName(Expression<Func<TEntity, object>> expression)
        {
            return GetMemberNameFromBody(expression.Body);
        }

        private string GetMemberNameFromBody(Expression body)
        {
            if (body is MemberExpression member)
                return member.Member.Name;
            if (body is UnaryExpression unary && unary.Operand is MemberExpression innerMember)
                return innerMember.Member.Name;
            return null;
        }
    }
}
