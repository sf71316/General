using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Service
{
    public interface INotificationProxy : IDisposable, IServiceCommon
    {
        /// <summary>
        /// 刪除跑馬燈資料
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        int DeleteMessageById(Guid[] guid);
        /// <summary>
        /// 刪除跑馬燈資料(非同步)
        /// </summary>
        /// <param name="guid"></param>
        void DeleteMessageByIdAsync(Guid[] guid);
        /// <summary>
        /// 取得所有跑馬燈資料
        /// </summary>
        /// <returns></returns>
        System.Data.DataTable GetAllMessagesFromBranch();
        /// <summary>
        /// 取得所有跑馬燈資料(非同步)
        /// </summary>
        /// <returns></returns>
        void GetAllMessagesFromBranchAsync();
        event General.Service.NotificationWSProxy.GetAllMessagesFromBranchCompletedEventHandler GetAllMessagesFromBranchCompleted;
        event General.Service.NotificationWSProxy.DeleteMessageByIdCompletedEventHandler DeleteMessageByIdCompleted; 
    }
}
