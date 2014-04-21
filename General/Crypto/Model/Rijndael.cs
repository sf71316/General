using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace General.Crypto
{
    internal sealed class RijndaelCrypto : CryptoProvider
    {
        MemoryStream mStream=null;
        CryptoStream cStream = null;
        public RijndaelCrypto()
        {
            Alorithm = RijndaelManaged.Create();
        }
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            mStream.Dispose();
            cStream.Dispose();
        }
            
        public override byte[] IV
        {
            get { return this.Alorithm.IV; }
            set
            {
                this.Alorithm.IV = value;
            }
        }

        public override byte[] Key
        {
            get { return this.Alorithm.Key; }
            set
            {
                this.Alorithm.Key = value;
            }
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="plainStr">明文字串</param>
        /// <returns>密文</returns>
        public override string Encrypt(string plainStr)
        {

            byte[] byteArray = this.Encrypt(Encoding.UTF8.GetBytes(plainStr));
            return Convert.ToBase64String(byteArray);
        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="encryptStr">密文字符串</param>
        /// <returns>明文</returns>
        public override string Decrypt(string encryptStr)
        {
             byte[] byteArray = this.Decrypt(Convert.FromBase64String(encryptStr));
            
            return Encoding.UTF8.GetString(byteArray);
        }

        public override byte[] Decrypt(byte[] encrypt)
        {
            mStream = new MemoryStream();
            cStream = new CryptoStream(mStream, Alorithm.CreateDecryptor(Alorithm.Key, Alorithm.IV),
                CryptoStreamMode.Write);
            cStream.Write(encrypt, 0, encrypt.Length);
            cStream.FlushFinalBlock();
            return mStream.ToArray();
        }

        public override byte[] Encrypt(byte[] plain)
        {

            mStream = new MemoryStream();
            cStream =
               new CryptoStream(mStream, Alorithm.CreateEncryptor(Alorithm.Key, Alorithm.IV),
                                                   CryptoStreamMode.Write);

            cStream.Write(plain, 0, plain.Length);
            cStream.FlushFinalBlock();
            return mStream.ToArray();
        }
    }
}
