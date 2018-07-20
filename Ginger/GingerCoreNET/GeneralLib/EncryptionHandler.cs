#region License
/*
Copyright © 2014-2018 European Support Limited

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

using GingerCoreNET.ReporterLib;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace GingerCoreNET.GeneralLib
{
    public class EncryptionHandler
    {
        //Configuring the details to create the Encrypt key
        private static string PASS_PHRASE = "D3^hdfr7%ws4Kb56=Qt";  // can be any string
        private static string SALT_VALUE = "EBK@%TTsqw3dv5";  // can be any string
        private static string INIT_VECTOR = "*9OPxcc9r4G2W6H2";  // must be 16 bytes
        private static string HASH_ALGORITHM = "SHA1";   // can be "SHA1" or "MD5"
        private static int PASSWORD_ITERATIONS = 3; // can be any number
        private static int KEY_SIZE = 128; // can be 192 or 256

        public static string EncryptString(string strToEncrypt, ref bool result)
        {
            //Convert strings defining encryption key characteristics into byte arrays
            byte[] _initVectorBytes = Encoding.UTF8.GetBytes(INIT_VECTOR);
            byte[] _saltValueBytes = Encoding.UTF8.GetBytes(SALT_VALUE);

            // Create a password, from which the key will be derived
            PasswordDeriveBytes _password =
                new PasswordDeriveBytes(PASS_PHRASE, _saltValueBytes, HASH_ALGORITHM, PASSWORD_ITERATIONS);

            // Use the password to generate pseudo-random bytes for the encryption key
            byte[] _keyBytes = _password.GetBytes(KEY_SIZE / 8);

            // Create uninitialized Rijndael Object
            RijndaelManaged _rijndaelObject = new RijndaelManaged();

            //check strToEncrypt is not empty
            if ((strToEncrypt == null) || (strToEncrypt == string.Empty))
                return strToEncrypt;
            else
            {
                try
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
                catch (Exception ex)
                {
                    result = false;
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                    return string.Empty;
                }
            }
        }

        public static string DecryptString(string strToDecrypt, ref bool result)
        {
            //Convert strings defining encryption key characteristics into byte arrays
            byte[] _initVectorBytes = Encoding.UTF8.GetBytes(INIT_VECTOR);
            byte[] _saltValueBytes = Encoding.UTF8.GetBytes(SALT_VALUE);

            // Create a password, from which the key will be derived
            PasswordDeriveBytes _password = new PasswordDeriveBytes(PASS_PHRASE, _saltValueBytes, HASH_ALGORITHM, PASSWORD_ITERATIONS);

            // Use the password to generate pseudo-random bytes for the encryption key
            byte[] _keyBytes = _password.GetBytes(KEY_SIZE / 8);

            // Create uninitialized Rijndael Object
            RijndaelManaged _rijndaelObject = new RijndaelManaged();

            //check strToEncrypt is not empty
            if ((strToDecrypt == null) || (strToDecrypt == string.Empty))
                return strToDecrypt;
            else
            {
                try
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
                    int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                    // Close both streams
                    memoryStream.Close();
                    cryptoStream.Close();

                    // Convert decrypted data into a string. 
                    string decryptedStr = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);

                    // Return decrypted string.   
                    result = true;
                    return decryptedStr;
                }
                catch (Exception ex)
                {
                    result = false;
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                    return string.Empty;
                }
            }
        }
    }
}
