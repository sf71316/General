using System;
using General.Crypto;
using System.DirectoryServices.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace General.Test
{
    [TestClass]
    public class UnitTest1
    {
        string context = "Hello World!!";
       static string IV, KEY,Enc_context;
        [TestMethod]
        public void TestAES()
        {
            CryptoProvider crypto = CryptoProvider.GetModule(CryptoEum.Aes);
            crypto.GenerateIV();
            crypto.GenerateKey();
            IV =Convert.ToBase64String( crypto.IV);
            KEY =Convert.ToBase64String( crypto.Key);
           
            Enc_context = crypto.Encrypt(context);

            crypto.IV = Convert.FromBase64String(IV);
            crypto.Key = Convert.FromBase64String(KEY);
            Assert.AreEqual(context, crypto.Decrypt(Enc_context));
        }
        [TestMethod]
        public void TestRijndael()
        {
            CryptoProvider crypto = CryptoProvider.GetModule(CryptoEum.Rijndael);
            crypto.GenerateIV();
            crypto.GenerateKey();
            IV = Convert.ToBase64String(crypto.IV);
            KEY = Convert.ToBase64String(crypto.Key);

            Enc_context = crypto.Encrypt(context);

            crypto.IV = Convert.FromBase64String(IV);
            crypto.Key = Convert.FromBase64String(KEY);
            Assert.AreEqual(context, crypto.Decrypt(Enc_context));
        }
        [TestMethod]
        public void ADTest()
        {
            General.Security.ADServer ad = new Security.ADServer("it_information@markwell.com.tw",
                "123456", "markwell.com.tw","markwell.com.tw");
            ad.Searching += ad_Searching;
            var rs = ad.SearchOne();
        }

        void ad_Searching(object sender, Security.ADServerSearchingArgs e)
        {
            e.Searcher.Filter = "(SAMAccountName=ivy.kuo)";
            e.Searcher.PropertiesToLoad.Add("company");
        }
        [TestMethod]
        public void MailTest()
        {
            MailNotifier instance = new MailNotifier();
            instance.Preparing += instance_Preparing;
            instance.Sended += instance_Sended;
            instance.Send();
        }

        void instance_Sended(object sender, SendCompletedArgs e)
        {
            
        }

        void instance_Preparing(object sender, SendPreparingArg e)
        {
            e.Content = "test";
            e.SmtpIP = "203.217.97.25";
            e.SmtpPort = 25;
            e.Form =new System.Net.Mail.MailAddress( "service@vee.com.tw");
            e.Subject = "test";
            e.ToList.Add(new System.Net.Mail.MailAddress("willy.cheng@markwell.com.tw"));
        }
    }
}
