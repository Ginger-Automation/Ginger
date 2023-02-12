#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Amdocs.Ginger.Common;
using GingerExternal;

namespace GingerCore
{
    public class EncryptionHandler
    {
        //Configuring the details to create the Encrypt key
        private static string PASS_PHRASE = Encoding.UTF8.GetString(System.Convert.FromBase64String(ExtraInfo.getInfo().ElementAt(0)));
        private static string SALT_VALUE = Encoding.UTF8.GetString(System.Convert.FromBase64String(ExtraInfo.getInfo().ElementAt(1)));
        private static string INIT_VECTOR = Encoding.UTF8.GetString(System.Convert.FromBase64String(ExtraInfo.getInfo().ElementAt(2)));
        private static string HASH_ALGORITHM = Encoding.UTF8.GetString(System.Convert.FromBase64String(ExtraInfo.getInfo().ElementAt(3)));
        private static int PASSWORD_ITERATIONS = 3; // can be any number
        private static int KEY_SIZE = 128; // can be 192 or 256
        private static string CUSTOM_KEY = string.Empty;
        private static readonly string ENCRYPTION_KEY = ExtraInfo.getInfo().ElementAt(4);

        public static string GetDefaultKey()
        {
            return PASS_PHRASE;
        }

        public static void SetCustomKey(string customKey)
        {
            CUSTOM_KEY = customKey;
        }

        private static string EncryptString(string strToEncrypt, ref bool result)
        {
            try
            {
                //Convert strings defining encryption key characteristics into byte arrays
                byte[] _initVectorBytes = Encoding.UTF8.GetBytes(INIT_VECTOR);
                byte[] _saltValueBytes = Encoding.UTF8.GetBytes(SALT_VALUE);

                // Create a password, from which the key will be derived
                PasswordDeriveBytes _password;
                if (!string.IsNullOrEmpty(CUSTOM_KEY))
                {
                    _password = new PasswordDeriveBytes(CUSTOM_KEY, _saltValueBytes, HASH_ALGORITHM, PASSWORD_ITERATIONS);
                }
                else
                {
                    _password = new PasswordDeriveBytes(PASS_PHRASE, _saltValueBytes, HASH_ALGORITHM, PASSWORD_ITERATIONS);
                }
                // Use the password to generate pseudo-random bytes for the encryption key
                byte[] _keyBytes = _password.GetBytes(KEY_SIZE / 8);

                // Create uninitialized Rijndael Object
                RijndaelManaged _rijndaelObject = new RijndaelManaged();

                //check strToEncrypt is not empty
                if ((strToEncrypt == null) || (strToEncrypt == string.Empty))
                    return strToEncrypt;
                else
                {

                    // Convert strToEncrypt into a byte array.
                    byte[] strToEncryptBytes = Encoding.UTF8.GetBytes(strToEncrypt);

                    // Set Rijndael Object mode to Cipher Block Chaining(CBC)
                    _rijndaelObject.Mode = CipherMode.CBC;

                    // Generate encryptor from the existing key bytes and initializationvector
                    ICryptoTransform encryptor = _rijndaelObject.CreateEncryptor(_keyBytes, _initVectorBytes);

                    // Define memory stream which will be used to hold encrypted data.
                    MemoryStream memoryStream = new MemoryStream();

                    // Define cryptographic stream
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

                    // Start encrypting
                    cryptoStream.Write(strToEncryptBytes, 0, strToEncryptBytes.Length);

                    // Finish encrypting
                    cryptoStream.FlushFinalBlock();

                    // Convert our encrypted data from a memory stream into a byte array
                    byte[] encryptedStrBytes = memoryStream.ToArray();

                    // Close both streams
                    memoryStream.Close();
                    cryptoStream.Close();

                    // Convert encrypted data into a base64-encoded string.
                    string encryptedStr = Convert.ToBase64String(encryptedStrBytes);

                    // Return encrypted string
                    result = true;
                    return encryptedStr;

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Failed to Encrypt the value", ex);

                result = false;
                return string.Empty;
            }
        }

        private static string DecryptString(string strToDecrypt, ref bool result, bool WriteErrorsToLog = true, string key = null)
        {
            try
            {
                //Convert strings defining encryption key characteristics into byte arrays
                byte[] _initVectorBytes = Encoding.UTF8.GetBytes(INIT_VECTOR);
                byte[] _saltValueBytes = Encoding.UTF8.GetBytes(SALT_VALUE);

                // Create a password, from which the key will be derived
                PasswordDeriveBytes _password;
                if (!string.IsNullOrEmpty(key))
                {
                    _password = new PasswordDeriveBytes(key, _saltValueBytes, HASH_ALGORITHM, PASSWORD_ITERATIONS);
                }
                else if (!string.IsNullOrEmpty(CUSTOM_KEY))
                {
                    _password = new PasswordDeriveBytes(CUSTOM_KEY, _saltValueBytes, HASH_ALGORITHM, PASSWORD_ITERATIONS);
                }
                else
                {
                    _password = new PasswordDeriveBytes(PASS_PHRASE, _saltValueBytes, HASH_ALGORITHM, PASSWORD_ITERATIONS);
                }
                // Use the password to generate pseudo-random bytes for the encryption key
                byte[] _keyBytes = _password.GetBytes(KEY_SIZE / 8);

                // Create uninitialized Rijndael Object
                RijndaelManaged _rijndaelObject = new RijndaelManaged();

                //check strToEncrypt is not empty
                if ((strToDecrypt == null) || (strToDecrypt == string.Empty))
                    return strToDecrypt;
                else
                {
                    // Convert strToEncrypt into a byte array.
                    byte[] strToDecryptBytes = Convert.FromBase64String(strToDecrypt);

                    // Set encryption mode to Cipher Block Chaining(CBC)
                    _rijndaelObject.Mode = CipherMode.CBC;

                    // Generate encryptor from the existing key bytes and initializationvector
                    ICryptoTransform decryptor = _rijndaelObject.CreateDecryptor(_keyBytes, _initVectorBytes);

                    // Define memory stream which will be used to hold encrypted data.
                    MemoryStream memoryStream = new MemoryStream(strToDecryptBytes);

                    // Define cryptographic stream
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

                    // allocate the buffer long enough to hold ciphertext;
                    byte[] plainTextBytes = new byte[strToDecryptBytes.Length];

                    // Start decrypting
                    using (MemoryStream ms = new MemoryStream())
                    {
                        cryptoStream.CopyTo(ms, plainTextBytes.Length);
                        plainTextBytes = ms.ToArray();
                    }

                    // Close both streams
                    memoryStream.Close();
                    cryptoStream.Close();

                    // Convert decrypted data into a string. 
                    string decryptedStr = Encoding.UTF8.GetString(plainTextBytes);

                    // Return decrypted string.   
                    result = true;
                    return decryptedStr;
                }
            }
            catch (Exception ex)
            {
                if (WriteErrorsToLog)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Failed to Decrypt the value: '{0}'", strToDecrypt), ex);
                }
                result = false;
                return string.Empty;
            }
        }

        public static bool IsStringEncrypted(string strToCheck)
        {
            bool checkValueDecrypt = false;
            DecryptString(strToCheck, ref checkValueDecrypt, false);
            return checkValueDecrypt;
        }

        private static RijndaelManaged GetRijndaelManaged(String secretKey)
        {
            var keyBytes = new byte[16];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));
            return new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128,
                Key = keyBytes,
                IV = keyBytes
            };
        }

        private static byte[] Encrypt(byte[] plainBytes, RijndaelManaged rijndaelManaged)
        {
            return rijndaelManaged.CreateEncryptor()
                .TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        private static byte[] Decrypt(byte[] encryptedData, RijndaelManaged rijndaelManaged)
        {
            return rijndaelManaged.CreateDecryptor()
                .TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }

        /// <summary>
        /// Encrypts plaintext using AES 128bit key and a Chain Block Cipher and returns a base64 encoded string
        /// </summary>
        /// <param name="plainText">Plain text to encrypt</param>
        /// <param name="key">Secret key</param>
        /// <returns>Base64 encoded string</returns>
        public static string EncryptwithKey(String plainText)
        {
            bool res = false;
            return EncryptString(plainText, ref res);
        }

        /// <summary>
        /// Decrypts a base64 encoded string using the given key (AES 128bit key and a Chain Block Cipher)
        /// </summary>
        /// <param name="encryptedText">Base64 Encoded String</param>
        /// <param name="key">Secret Key</param>
        /// <returns>Decrypted String</returns>
        public static string DecryptwithKey(string encryptedText, string key = null)
        {
            try
            {
                if (String.IsNullOrEmpty(encryptedText))
                {
                    return string.Empty;
                }
                bool res = false;
                string decryptVal = DecryptString(encryptedText, ref res, true, key);
                if (res)
                {
                    return decryptVal;
                }
                var encryptedBytes = Convert.FromBase64String(encryptedText);

                if (!string.IsNullOrEmpty(key))
                {
                    return Encoding.UTF8.GetString(Decrypt(encryptedBytes, GetRijndaelManaged(key)));
                }
                else
                {
                    return Encoding.UTF8.GetString(Decrypt(encryptedBytes, GetRijndaelManaged(ENCRYPTION_KEY)));
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("Failed to Decrypt the value: '{0}'", encryptedText), ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// ReEncryptString is to support existing solution vairables which were encrypted using default key.
        /// </summary>
        /// <param name="strToReEncrypt"></param>
        /// <param name="oldKey"></param>
        /// <returns></returns>
        public static string ReEncryptString(string strToReEncrypt, string oldKey = null)
        {
            bool res = false;
            if (!string.IsNullOrEmpty(oldKey))
            {
                return EncryptwithKey(EncryptionHandler.DecryptString(strToReEncrypt, ref res, false, oldKey));
            }
            else
            {
                return EncryptwithKey(EncryptionHandler.DecryptString(strToReEncrypt, ref res, false, PASS_PHRASE));
            }
        }
    }
}
