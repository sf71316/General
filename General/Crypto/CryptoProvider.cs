using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace General.Crypto
{
   public abstract class CryptoProvider:IDisposable
    {
      
       protected SymmetricAlgorithm Alorithm;
       public CryptoProvider()
       {

       }
       #region Property

       public abstract byte[] IV { get; set; }
       public abstract byte[] Key { get; set; }
       public int BlockSize
       {
           get
           {
               return this.Alorithm.BlockSize;
           }
       }
       public CipherMode Mode
       {
           get
           {
              return this.Alorithm.Mode;
           }
           set
           {
               this.Alorithm.Mode = value;
           }
       }
       #endregion

       #region Abstract Metod
       public abstract string Decrypt(string encrypt);
       public abstract string Encrypt(string plain);
       public abstract byte[] Decrypt(byte[] encrypt);
       public abstract byte[] Encrypt(byte[] plain);
       #endregion
       #region Method
       public void GenerateIV()
       {
           this.Alorithm.GenerateIV();
       }
       public void GenerateKey()
       {
           this.Alorithm.GenerateKey();
       }
       #endregion
       public  void Dispose()
       {
           Dispose(true);
           GC.SuppressFinalize(this);
       }
       protected virtual void Dispose(bool disposing)
       {
           if (disposing)
           {
               Alorithm.Clear();
               
           }

       }
       public static CryptoProvider GetModule(CryptoEum type)
       {
           switch (type)
           {
               case CryptoEum.Aes:
                   return new AESCrypto();
               case CryptoEum.Rijndael:
                   return new RijndaelCrypto();
               default:
                   return null;
           }
           
       }
    }
}
