using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General
{
    internal class DefaultConfig:MailConfig
    {

        public override string SMTP_IP
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["Mail.SMTP"];
            }
        }

        public override int SMTP_PORT
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["Mail.PORT"].ConvertTo<int>();
            }
        }

        public override string SMTP_FORM_MAIL
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["Mail.MailForm"];
            }
        }

        public override bool SMTP_USE_SSL
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["Mail.SSL"].ConvertTo<bool>();
            }
        }

        public override string SMTP_PIN
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["Mail.VerAcc"];
            }
        }

        public override string SMTP_Pwd
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["Mail.VerPwd"];
            }
        }
    }
}
