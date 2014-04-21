using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace General
{
    /// <summary>
    /// mail 發送Entity
    /// </summary>
    public class MailEntity
    {
        public MailMessage Mail { get; set; }
        public MailStatus Status { get; set; }
        public Exception Error { get; set; }
    }
    /// <summary>
    /// Mail發送結果狀態
    /// </summary>
    public enum MailStatus
    {
        成功,失敗,
    }
}
