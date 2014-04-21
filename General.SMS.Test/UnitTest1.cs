using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using General.SMS;

namespace General.SMS.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void SMS_Test()
        {
            ApbwSMSBase sms = ApbwSMSBase.GetInstance(CommandKind.立即發送簡訊);
            ImmediateSMSEntity entity = new ImmediateSMSEntity();
            entity.AutoSplite = "Y";
            entity.PhoneList.Add("0928934360");
            entity.Retry = "Y";
            entity.StopDateTime = DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmm");
            entity.Subject = "test";
            entity.Message ="test";
            ApbwResponseEntity response = sms.Action<ApbwResponseEntity>(entity);
            
        }
    }
}
