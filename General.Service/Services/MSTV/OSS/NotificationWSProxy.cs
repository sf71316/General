using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using General.Service.MSTV;
using General.Service.NotificationWSProxy;

namespace General.Service.Services.MSTV.OSS
{
    public abstract class NotificationWSProxy : IServiceConfiguration, General.Service.Services.MSTV.OSS.INotificationWSProxy
    {
        General.Service.NotificationWSProxy.Service service;
        public NotificationWSProxy()
        {
            service = new Service.NotificationWSProxy.Service();
            this.service.GetAllMessagesFromBranchCompleted += this.GetAllMessagesFromBranchCompleted;
            this.service.DeleteMessageByIdCompleted += this.DeleteMessageByIdCompleted;
            
        }
        public int DeleteMessageById(Guid[] guid)
        {
            return this.service.DeleteMessageById(guid);
        }
        public void DeleteMessageByIdAsync(Guid[] guid)
        {
            this.service.GetAllMessagesFromBranchAsync(guid);
        }
        public DataTable GetAllMessagesFromBranch()
        {
            return this.service.GetAllMessagesFromBranch();
        }
        public void GetAllMessagesFromBranchAsync()
        {
            this.service.GetAllMessagesFromBranchAsync();
        }
        public event GetAllMessagesFromBranchCompletedEventHandler GetAllMessagesFromBranchCompleted;
        public event DeleteMessageByIdCompletedEventHandler DeleteMessageByIdCompleted;
        public string Url
        {
            get { return this.service.Url; }
            set { this.service.Url = value; }
        }

        public NetworkCredential Credential
        {
            get { return this.service.Credentials as NetworkCredential; }
            set { this.service.Credentials = value; }
        }

        public void Dispose()
        {
            if (this != null)
            {
                this.service.Dispose();
            }
        }
    }
}
