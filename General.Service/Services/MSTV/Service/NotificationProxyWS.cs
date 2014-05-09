using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace General.Service
{
    public sealed class NotificationProxyWS:INotificationProxy
    {
        NotificationWSProxy.Service _notificationProxy = null;
        IServiceConfiguration _config;
        public NotificationProxyWS(IServiceConfiguration Config)
        {
            this._config = Config;
        }
        #region NotificationWSProxy

        public event NotificationWSProxy.DeleteMessageByIdCompletedEventHandler DeleteMessageByIdCompleted;

        public event NotificationWSProxy.GetAllMessagesFromBranchCompletedEventHandler GetAllMessagesFromBranchCompleted;

        public int DeleteMessageById(Guid[] guid)
        {
            return this._notificationProxy.DeleteMessageById(guid);
        }
        public void DeleteMessageByIdAsync(Guid[] guid)
        {
            
            this._notificationProxy.GetAllMessagesFromBranchAsync(guid);
        }
        public DataTable GetAllMessagesFromBranch()
        {
            return this._notificationProxy.GetAllMessagesFromBranch();
        }
        public void GetAllMessagesFromBranchAsync()
        {
            this._notificationProxy.GetAllMessagesFromBranchAsync();
        }
        #endregion

        public void Dispose()
        {
            if(this._notificationProxy!=null)
            this._notificationProxy.Dispose();
        }


        public void Initialize()
        {
            if (this._notificationProxy == null)
            {
                this._notificationProxy = new NotificationWSProxy.Service();
                this._notificationProxy.Url = this._config.Url;
                this._notificationProxy.Credentials = this._config.Credential;
                this._notificationProxy.DeleteMessageByIdCompleted +=
                    this.DeleteMessageByIdCompleted;
                this._notificationProxy.GetAllMessagesFromBranchCompleted +=
                    this.GetAllMessagesFromBranchCompleted;
            }
        }


        public IServiceConfiguration Config
        {
            get { return this._config; }
        }
    }
}
