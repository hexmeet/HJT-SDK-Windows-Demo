using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace EasyVideoWin.Helpers
{
    public static class CryptoUtil
    {
        public static string Sha1(this string str)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            byte[] data = SHA1.Create().ComputeHash(buffer);

            StringBuilder sb = new StringBuilder();
            foreach (byte t in data)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString().ToLower();
        }
        
        const string KEY_64 = "EV123456";//注意了，是8个字符，64位

        const string IV_64 = "EV123456";

        const byte key = 112;

        public static string EncDec(string srcStr)
        {
            if (srcStr == null || srcStr == "")
            {
                return "";
            }

            byte[] bs = Encoding.Default.GetBytes(srcStr);
            for (int i = 0; i < bs.Length; i++)
            {
                bs[i] = (byte)(bs[i] ^ key);
            }
            return Encoding.Default.GetString(bs);
        }

        public static string Encrypt(string srcStr)
        {
            if (srcStr == null || srcStr == "")
            {
                return "";
            }
            
            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);

            string encStr = "";
            using (System.Security.Cryptography.DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider())
            {
                int i = cryptoProvider.KeySize;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cst))
                        {
                            sw.Write(srcStr);
                            sw.Flush();
                            cst.FlushFinalBlock();
                            sw.Flush();
                            encStr = Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
                        }
                    }
                }
            }
                
            return encStr;
        }

        public static string Decrypt(string encStr)
        {
            if (encStr == null || encStr == "")
            {
                return "";
            }
            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);

            byte[] byEnc;
            try
            {
                byEnc = Convert.FromBase64String(encStr);
            }
            catch
            {
                return null;
            }

            string decStr = "";
            using (DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider())
            {
                using (MemoryStream ms = new MemoryStream(byEnc))
                {
                    using (CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cst))
                        {
                            decStr = sr.ReadToEnd();
                        }
                    }
                }
            }
            
            return decStr;
        }


        #region -- AES enrypt/decrypt --

        /// <summary>  
        /// AES encypt  
        /// </summary>  
        /// <param name="plainStr">plain string</param>  
        /// <returns>encrypted string</returns>  
        public static string AESEncrypt(string plainStr, string key, string iv)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(key);
            byte[] bIV = Encoding.UTF8.GetBytes(iv);

            byte[] bEncrypt = EncryptStringToBytes_Aes(plainStr, bKey, bIV);

            string encrypt = null == bEncrypt ? "" : Convert.ToBase64String(bEncrypt);
            return encrypt;

            //byte[] byteArray = Encoding.UTF8.GetBytes(plainStr);

            //string encrypt = null;
            //Rijndael aes = Rijndael.Create();
            //using (MemoryStream mStream = new MemoryStream())
            //{
            //    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write))
            //    {
            //        cStream.Write(byteArray, 0, byteArray.Length);
            //        cStream.FlushFinalBlock();
            //        encrypt = Convert.ToBase64String(mStream.ToArray());
            //    }
            //}
            //aes.Clear();
            //return encrypt;
        }

        public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            //if (plainText == null || plainText.Length <= 0)
            //    throw new ArgumentNullException("plainText");
            if (plainText == null)
                throw new ArgumentNullException("plainText");

            if (plainText.Length <= 0)
            {
                return null;
            }
            
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an AesCryptoServiceProvider object 
            // with the specified key and IV. 
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt =
                            new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream. 
            return encrypted;
        }

        /// <summary>  
        /// AES encypt  
        /// </summary>  
        /// <param name="plainStr">plain string</param>  
        /// <param name="returnNull">if return null when failed，false return String.Empty</param>  
        /// <returns>encrypted string</returns>  
        public static string AESEncrypt(string plainStr, string key, string iv, bool returnNull)
        {
            string encrypt = CryptoUtil.AESEncrypt(plainStr, key, iv);
            return returnNull ? encrypt : (encrypt == null ? String.Empty : encrypt);
        }

        /// <summary>  
        /// AES decrypt  
        /// </summary>  
        /// <param name="encryptStr">encrypted string</param>  
        /// <returns>plain string</returns>  
        public static string AESDecrypt(string encryptStr, string key, string iv)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(key);
            byte[] bIV = Encoding.UTF8.GetBytes(iv);

            byte[] byteArray = null;
            try
            {
                byteArray = Convert.FromBase64String(encryptStr);
            }
            catch(Exception ex)
            {
                return null;
            }

            //return encryptStr;
            return DecryptStringFromBytes_Aes(byteArray, bKey, bIV);

            //string decrypt = null;
            //Rijndael aes = Rijndael.Create();
            //using (MemoryStream mStream = new MemoryStream())
            //{
            //    try
            //    {
            //        using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write))
            //        {
            //            cStream.Write(byteArray, 0, byteArray.Length);
            //            cStream.FlushFinalBlock();
            //            decrypt = Encoding.UTF8.GetString(mStream.ToArray());
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        return null;
            //    }
            //    finally
            //    {
            //        aes.Clear();
            //    }
            //}
            //aes.Clear();
            //return decrypt;
        }

        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            //if (cipherText == null || cipherText.Length <= 0)
            //    throw new ArgumentNullException("cipherText");
            if (cipherText == null)
                throw new ArgumentNullException("cipherText");

            if (cipherText.Length <= 0)
            {
                return "";
            }

            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            // Declare the string used to hold 
            // the decrypted text. 
            string plaintext = null;
            // Create an AesCryptoServiceProvider object 
            // with the specified key and IV. 
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt =
                            new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream 
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }

        /// <summary>  
        /// AES decrypt  
        /// </summary>  
        /// <param name="encryptStr">encrypted string</param>  
        /// <param name="returnNull">if return null when failed，false return String.Empty</param>  
        /// <returns>plain string</returns>  
        public static string AESDecrypt(string encryptStr, string key, string iv, bool returnNull)
        {
            string decrypt = AESDecrypt(encryptStr, key, iv);
            return returnNull ? decrypt : (decrypt == null ? String.Empty : decrypt);
        }

        #endregion

    }
}
