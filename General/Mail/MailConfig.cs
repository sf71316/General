using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General
{
    public abstract class MailConfig
    {
        public abstract string SMTP_IP { get;   }
        public abstract int SMTP_PORT { get;   }
        public abstract string SMTP_FORM_MAIL { get;   }
        public abstract bool SMTP_USE_SSL { get;   }
        public abstract string SMTP_PIN { get;   }
        public abstract string SMTP_Pwd { get;   }

    }
}
