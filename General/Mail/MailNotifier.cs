using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.IO;

namespace General
{
    public class MailNotifier : IDisposable
    {
        private object _container;
        private MailAddress _extMail;
        private MailMessage _mail;
        protected MailConfig _config;
        private List<MailEntity> collection;
        public SendPreparingArg PreparingArg { get; set; }

        public MailNotifier()
        {
            this.PreparingArg = new SendPreparingArg();
        }
        public MailNotifier(MailConfig Config) : this()
        {
            this._config = Config;
        }
        public void Send()
        {
            if (this._extMail != null)
            {
                PreparingArg.ToList.Add(this._extMail);
            }
            this.OnSendPreparing(PreparingArg);
            if (!PreparingArg.Cancel)
            {
                if (PreparingArg.DirectSend)
                {
                    this.sendMail(PreparingArg, null);
                }
                else
                {
                    foreach (var item in PreparingArg.ToList)
                    {
                        this.sendMail(PreparingArg, item);
                    }
                }
            }
            this.OnSendAllCompleted(collection);

        }
        public void SendByStep(object value, string mailAddress)
        {
            this._container = value;
            this._extMail = new MailAddress(mailAddress);
            this.Send();
        }

        #region IDisposable 成員

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._mail != null)
                {
                    this._mail.Dispose();
                    collection.Clear();
                    PreparingArg = null;
                }
            }
            else
            {
            }
        }
        #endregion

        #region Event

        public delegate void SendCompletedHandler(object sender, SendAllCompletedArgs e);
        public event SendCompletedHandler SendAllCompleted;
        public event EventHandler<SendPreparingArg> Preparing;
        public event EventHandler<SendCompletedArgs> Sended;
        private void OnSendPreparing(SendPreparingArg args)
        {
            collection = new List<MailEntity>();
            if (this._config != null)
            {
                args.Form = new MailAddress(this._config.SMTP_FORM_MAIL);
                args.Pin = this._config.SMTP_PIN;
                args.Password = this._config.SMTP_Pwd;
                args.SmtpIP = this._config.SMTP_IP;
                args.SmtpPort = this._config.SMTP_PORT;
                args.EnableSsl = this._config.SMTP_USE_SSL;
            }
            if (Preparing != null)
            {
                Preparing(this, args);
            }

        }
        private void OnSendAllCompleted(List<MailEntity> collection)
        {
            if (SendAllCompleted != null)
            {
                SendAllCompletedArgs args = new SendAllCompletedArgs();
                args.Result = collection;
                SendAllCompleted(this, args);
            }
        }
        public void OnSended(object value, MailEntity entity)
        {
            if (Sended != null)
            {
                SendCompletedArgs args = new SendCompletedArgs();
                args.Container = value;
                args.Result = entity;
                Sended(this, args);
            }
        }
        #endregion
        public static MailConfig DefaultConfig
        {
            get
            {
                return new DefaultConfig();
            }
        }

        private void sendMail(SendPreparingArg arg, MailAddress mail)
        {

            MailEntity e = new MailEntity();
            _mail = new MailMessage();
            try
            {
                _mail.BodyEncoding = Encoding.UTF8;
                _mail.SubjectEncoding = Encoding.UTF8;
                _mail.IsBodyHtml = true;
                _mail.From = arg.Form;
                if (arg.CC.Count > 0)
                    _mail.CC.Add(string.Join(",", arg.CC.ToArray()));
                if (arg.Bcc.Count > 0)
                    _mail.Bcc.Add(string.Join(",", arg.Bcc.ToArray()));
                if (mail != null)
                    _mail.To.Add(mail);
                else
                {
                    arg.ToList.ForEach(p => _mail.To.Add(p));
                }
                _mail.Subject = arg.Subject;
                _mail.Body = arg.Content;
                foreach (FileInfo fi in arg.Attach)
                {
                    if (fi.Exists)
                    {
                        _mail.Attachments.Add(new Attachment(fi.FullName));
                    }
                }
                SmtpClient smtpClient = new SmtpClient(arg.SmtpIP, arg.SmtpPort);
                smtpClient.Credentials =
                    new System.Net.NetworkCredential(arg.Pin, arg.Password);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.EnableSsl = arg.EnableSsl;
                smtpClient.Timeout = 30000;
                smtpClient.Send(_mail);
                e.Mail = _mail;
                e.Status = MailStatus.成功;

            }
            catch (Exception ex)
            {
                e.Mail = _mail;
                e.Status = MailStatus.失敗;
                e.Error = ex;

            }
            this.OnSended(this._container, e);
            collection.Add(e);

        }
    }

}
