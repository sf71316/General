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
    public interface ISelectCommand<T> : ISelectQuery<T>, IQueryCommand
    {
        #region SELECT
        /// <summary>
        /// 取得對應條件的該資料表所有欄位的資料
        /// </summary>
        /// <param name="c">條件物件</param>
        /// <returns></returns>
        ISelectQuery<T> Where(Expression expr);
        ISelectQuery<T> Where(Expression<Func<T, bool>> expr);
        //ISelectQuery<T> Where<T2>(Expression<Func<T, T2, bool>> expr);
        //ISelectQuery<T> Where<T2, T3>(Expression<Func<T, T2, T3, bool>> expr);
        //ISelectQuery<T> Where<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expr);
        ISelectCommand<T> Select(Expression<Func<T, object>> selector);
        ISelectCommand<T> From(string tablename);
        #endregion
    }
    public interface ISelectQuery<T>
    {
        IEnumerable<T> Query();
        ISelectQuery<T> OrderBy(string fieldname);
    }
    /// <summary>
    /// 新增方法介面
    /// </summary>
    public interface IInsertCommand:IQueryCommand
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
    public interface IDeleteCommand : IQueryCommand
    {
        #region DELETE
        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="c">條件物件</param>
        /// <returns></returns>
        int Delete(Expression expr);
        int Delete<T>(Expression<Func<T,bool>> expr);
        #endregion
    }
    /// <summary>
    /// 修改方法介面
    /// </summary>
    public interface IUpdateCommand : IQueryCommand
    {
        #region UPDATE
        /// <summary>
        /// 批次修改
        /// </summary>
        /// <param name="e">資料物件</param>
        /// <param name="c">條件物件</param>
        /// <returns></returns>
        int Update(object e, Expression expr);
        int UpdateFunc<T>(object e,Expression<Func<T, bool>> expr);
        #endregion
    }
}
