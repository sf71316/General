using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using General.Service.BSS;
using General.Service.COSS;
using General.Service.OssDiagnostics;

namespace General.Service
{
    public abstract class MSTVService:IDisposable
    {
        #region Variable
        protected IPrincipalManagement _bss;
        protected ICossService _coss;
        protected INotificationProxy _notification;
        protected IOssDiagnostics _ossDiagnostics;
        protected IOssConfiguration _ossconfiguration;
        protected IOssChannel _osschannel;
        protected IOssEpg _ossepg;
        protected IBilling _billing;
        #endregion

        #region Property
        /// <summary>
        /// BSS-PrincipalManagement
        /// </summary>
        public IPrincipalManagement PrincipalManagement
        {
            get
            {
                this._bss.Initialize();
                return this._bss;
            }
        }
        /// <summary>
        /// BSS-BillingRecordManagement
        /// </summary>
        public IBilling Billing
        {
            get
            {
                this._billing.Initialize();
                return this._billing;
            }
        }
        public  ICossService COSS
        {
            get
            {
                this._coss.Initialize();
                return this._coss;
            }
        }
        public INotificationProxy Notification
        {
            get
            {
                this._notification.Initialize();
                return this._notification;
            }
        }
        public IOssDiagnostics OssDiagnostics
        {
            get
            {
                this._ossDiagnostics.Initialize();
                return this._ossDiagnostics;
            }
        }
        public IOssConfiguration OssConfiguration
        {
            get
            {
                this._ossconfiguration.Initialize();
                return this._ossconfiguration;
            }
        }
        public IOssChannel OssChannel
        {
            get
            {
                this._osschannel.Initialize();
                return this._osschannel;
            }
        }
        public IOssEpg OssEpg
        {
            get
            {
                this._ossepg.Initialize();
                return this._ossepg;
            }
        }
        #endregion
        public static MSTVService DefaultService()
        {
            return new DefaultAdapter();
        }

        public virtual void Dispose()
        {
            this._bss.Dispose();
            this._coss.Dispose();
            this._notification.Dispose();
            this._ossDiagnostics.Dispose();
            this._osschannel.Dispose();
            this._ossepg.Dispose();
        }
    }
}
