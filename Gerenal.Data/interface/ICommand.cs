using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace General.Data
{

        /// <summary>
        /// 查詢方法介面
        /// </summary>
        internal interface ISelectCommand
        {
            #region SELECT

            /// <summary>
            /// 取得對應條件的該資料表所有欄位的資料
            /// </summary>
            /// <param name="c">條件物件</param>
            /// <returns></returns>
            DataTable Select(ICondition c);

            #endregion
        }
        /// <summary>
        /// 新增方法介面
        /// </summary>
        internal interface IInsertCommand
        {
            #region INSERT

            /// <summary>
            /// 新增
            /// </summary>
            /// <param name="e">資料物件</param>
            /// <returns></returns>
            bool Insert(ICommandEntity e);
            /// <summary>
            /// 新增
            /// </summary>
            /// <param name="e">資料物件</param>
            /// <param name="isReturnID">是否回傳編號</param>
            /// <returns></returns>
            object Insert(ICommandEntity e, bool IsReturnID);
            /// <summary>
            /// 新增
            /// </summary>
            /// <typeparam name="R">回傳值的型別</typeparam>
            /// <param name="e">資料物件</param>
            /// <returns></returns>
            R Insert<R>(ICommandEntity e);
            /// <summary>
            /// 新增
            /// </summary>
            /// <typeparam name="T">有實作ICommandEntity的資料物件</typeparam>
            /// <param name="es">資料物件集合</param>
            void Insert<T>(List<T> es) where T : ICommandEntity;
        
            #endregion
        }
        /// <summary>
        /// 刪除方法介面
        /// </summary>
        internal interface IDeleteCommand
        {
            #region DELETE

            /// <summary>
            /// 刪除
            /// </summary>
            /// <param name="c">條件物件</param>
            /// <returns></returns>
            bool Delete(ICondition c);
            /// <summary>
            /// 刪除
            /// </summary>
            /// <typeparam name="T">有實作ICommandCondition的資料物件</typeparam>
            /// <param name="cs">條件物件集合</param>
            void Delete<T>(List<T> cs) where T : ICondition;
          

            #endregion
        }
        /// <summary>
        /// 修改方法介面
        /// </summary>
        internal interface IUpdateCommand
        {
            #region UPDATE

            /// <summary>
            /// 修改
            /// </summary>
            /// <param name="e">資料物件</param>
            /// <returns></returns>
            bool Update(ICommandEntity e);
            /// <summary>
            /// 修改
            /// </summary>
            /// <param name="e">資料物件</param>
            /// <param name="c">條件物件</param>
            /// <returns></returns>
            bool Update(ICommandEntity e, ICondition c);
         

            #endregion
        }
	
}
