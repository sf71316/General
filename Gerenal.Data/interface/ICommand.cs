using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace General.Data
{
    /// <summary>
    /// 查詢方法介面
    /// </summary>
    public interface ISelectCommand:ISelectQuery
    {
        #region SELECT
        /// <summary>
        /// 取得對應條件的該資料表所有欄位的資料
        /// </summary>
        /// <param name="c">條件物件</param>
        /// <returns></returns>
        ISelectQuery Where(Expression expr);
        ISelectQuery Where<T1>(Expression<Func<T1, bool>> expr);
        ISelectQuery Where<T1, T2>(Expression<Func<T1, T2, bool>> expr);
        ISelectQuery Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expr);
        ISelectQuery Where<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expr);
        ISelectCommand Select(string field = "*");
         ISelectCommand From(string tablename);
        #endregion
    }
    public interface ISelectQuery
    {
        IEnumerable<T> Query<T>();
        ISelectQuery OrderBy(string fieldname);
    }
    /// <summary>
    /// 新增方法介面
    /// </summary>
    public interface IInsertCommand
    {
        #region INSERT

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="e">資料物件</param>
        /// <returns></returns>
        object Insert(object e);
        #endregion
    }
    /// <summary>
    /// 刪除方法介面
    /// </summary>
    public interface IDeleteCommand
    {
        #region DELETE
        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="c">條件物件</param>
        /// <returns></returns>
        int Delete(Expression expr);
        #endregion
    }
    /// <summary>
    /// 修改方法介面
    /// </summary>
    public interface IUpdateCommand
    {
        #region UPDATE
        /// <summary>
        /// 批次修改
        /// </summary>
        /// <param name="e">資料物件</param>
        /// <param name="c">條件物件</param>
        /// <returns></returns>
        int Update(object e, Expression expr);
        #endregion
    }
}
