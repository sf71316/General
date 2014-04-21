using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using General;

namespace General.SMS
{
    internal sealed class ImmediateSendSMS:ApbwSMSBase
    {
        private string content;
        public ImmediateSendSMS()
        {
            this.Command = "APIRTHttpRequest";
        }
        public override IResponse Action(IRequest entity)
        {
            this.PrepareData((entity as ImmediateSMSEntity));
           return (IResponse)this.RequestService<ApbwResponseEntity>(content);
        }

        public override T Action<T>(IRequest entity)
        {
            this.PrepareData((entity as ImmediateSMSEntity));
            return this.RequestService<T>(content);
        }
        private void PrepareData(ImmediateSMSEntity entity)
        {
            content = string.Format("Content={0}&MDN={1}&UID={2}&UPASS={3}",
                entity.Serialize(true),entity.MDN,entity.UID,entity.UPASS);
        }
    }
}
