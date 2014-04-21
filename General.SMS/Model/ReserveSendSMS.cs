using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using General;

namespace General.SMS
{
    public class ReserveSendSMS : ApbwSMSBase
    {
        private string content;
        public ReserveSendSMS()
        {
            this.Command = "APIPOHttpRequest";
        }
        public override IResponse Action(IRequest entity)
        {
            this.PrepareData((entity as ReserveSendEntity));
            return (IResponse)this.RequestService<ApbwResponseEntity>(content);
        }

        public override T Action<T>(IRequest entity)
        {
            this.PrepareData((entity as ReserveSendEntity));
            return this.RequestService<T>(content);
        }
        private void PrepareData(ReserveSendEntity entity)
        {
            content = string.Format("Content={0}&MDN={1}&UID={2}&UPASS={3}",
                entity.Serialize(true), entity.MDN, entity.UID, entity.UPASS);
        }
    }
}
