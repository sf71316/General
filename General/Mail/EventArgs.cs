using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.IO;

namespace General
{
    public sealed class SendAllCompletedArgs : EventArgs
    {
        public SendAllCompletedArgs()
        {
            Result = new List<MailEntity>();
        }
        public List<MailEntity> Result { get; set; }
    }
    public sealed class SendCompletedArgs : EventArgs
    {
        public MailEntity Result { get; set; }
        public object Container { get; set; }
    }
    public sealed class SendPreparingArg : EventArgs
    {
        public SendPreparingArg()
        {
            this.ToList = new List<MailAddress>();
            this.Attach = new List<FileInfo>();
            this.CC = new List<string>();
            this.Bcc = new List<string>();
        }
        public List<MailAddress> ToList { get; private set; }
        public string SmtpIP { get; set; }
        public int SmtpPort { get; set; }
        public MailAddress Form { get; set; }
        public string Subject { get; set; }
        public string Pin { get; set; }
        public string Pwd { get; set; }
        public string Content { get; set; }
        public bool Ssl { get; set; }
        public bool Cancel { get; set; }
        public List<FileInfo> Attach { get; set; }
        public List<string> CC { get; set; }
        public List<string> Bcc { get; set; }

        public bool DirectSend { get; set; }
    }
}
