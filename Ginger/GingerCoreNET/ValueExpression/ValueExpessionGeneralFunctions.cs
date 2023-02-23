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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Amdocs.Ginger.CoreNET.ValueExpression
{
    public  class ValueExpessionGeneralFunctions
    {
        #region PlaceHolders

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Current Unix time stamp")]
        [ValueExpressionFunctionExpression("{Function Fun=GetUnixTimeStamp()}")]
        [ValueExpressionFunctionCategory("Data")]
        [ValueExpressionFunctionSubCategory("Date Time")]
        public string GetUnixTimeStamp()
        {
            DateTimeOffset dtoffset = new DateTimeOffset(DateTime.Now);
            long unixDateTime = dtoffset.ToUnixTimeSeconds();
            return unixDateTime.ToString();
        }


        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Get GUID")]
        [ValueExpressionFunctionExpression("{Function Fun=GetGUID()}")]
        [ValueExpressionFunctionCategory("Data")]
        [ValueExpressionFunctionSubCategory("Functions")]
        public string GetGUID()
        {
            return Guid.NewGuid().ToString();
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Get Clipboard Text")]
        [ValueExpressionFunctionExpression("{Function Fun=GetClipboardText()}")]
        [ValueExpressionFunctionCategory("Data")]
        [ValueExpressionFunctionSubCategory("Functions")]
        public string GetClipboardText()
        {
            return WorkSpace.Instance.EventHandler.GetClipboardText();
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Generate HashCode")]
        [ValueExpressionFunctionExpression("{Function Fun=GenerateHashCode(\"Hello\")}")]
        [ValueExpressionFunctionCategory("Data Operations")]
        [ValueExpressionFunctionSubCategory("Functions")]
        public string GenerateHashCode(object[] obj)
        {
            SHA1CryptoServiceProvider sha1Hasher = new SHA1CryptoServiceProvider();
            byte[] hashedDataBytes = sha1Hasher.ComputeHash(Encoding.UTF8.GetBytes(obj[0].ToString()));
            return Convert.ToBase64String(hashedDataBytes);
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Current UTC time stamp")]
        [ValueExpressionFunctionExpression("{Function Fun=GetUTCTimeStamp()}")]
        [ValueExpressionFunctionCategory("Data")]
        [ValueExpressionFunctionSubCategory("Date Time")]

        public string GetUTCTimeStamp()
        {
            DateTime todayDate = DateTime.UtcNow;
            return todayDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Get Hashed Data Byte string")]
        [ValueExpressionFunctionExpression("{Function Fun=GetHashedDataByteString(\"Hello\")}")]
        [ValueExpressionFunctionCategory("Data Operations")]
        [ValueExpressionFunctionSubCategory("Functions")]
        public string GetHashedDataByteString(object[] obj)
        {
            Encoding asciiEncoding = System.Text.Encoding.GetEncoding("ISO646-US", new EncoderReplacementFallback(""), new DecoderReplacementFallback());
            Encoding utf8Encoding = System.Text.Encoding.GetEncoding("UTF-8");
            SHA1CryptoServiceProvider sha1Hasher = new SHA1CryptoServiceProvider();
            byte[] hashedDataBytes = sha1Hasher.ComputeHash(Encoding.UTF8.GetBytes((obj[0].ToString()).ToCharArray()));
            byte[] asciiBytes = Encoding.Convert(utf8Encoding, asciiEncoding, hashedDataBytes);
            return asciiEncoding.GetString(asciiBytes);
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Encrypt  to Base 64")]
        [ValueExpressionFunctionExpression("{Function Fun=GetEncryptedBase64String(\"Hello\")}")]
        [ValueExpressionFunctionCategory("Data Operations")]
        [ValueExpressionFunctionSubCategory("Functions")]
        public string GetEncryptedBase64String(object[] obj)
        {
            byte[] hashedDataBytes = Encoding.ASCII.GetBytes((obj[0].ToString()).ToCharArray());
            return Convert.ToBase64String(hashedDataBytes);
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Decrypt to Base 64")]
        [ValueExpressionFunctionExpression("{Function Fun=GetDecryptedBase64String(\"SGVsbG8=\")}")]
        [ValueExpressionFunctionCategory("Data Operations")]
        [ValueExpressionFunctionSubCategory("Functions")]
        public string GetDecryptedBase64String(object[] obj)
        {
            try
            {
                byte[] hashedDataBytes = Convert.FromBase64String(obj[0].ToString());
                return ASCIIEncoding.ASCII.GetString(hashedDataBytes);
            }
            catch
            {
                Reporter.ToLog(eLogLevel.WARN, "User provided invalid base 64 string for decrypt");
                return "Invalid Base64 String";

            }
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Replace special chars by another")]
        [ValueExpressionFunctionExpression("{Function Fun=ReplaceSpecialChars(\"Hello\",\",_)}")]
        [ValueExpressionFunctionCategory("Data Operations")]
        [ValueExpressionFunctionSubCategory("Functions")]
        public string ReplaceSpecialChars(object[] obj)
        {
            try
            {
                return obj[0].ToString().Replace(obj[1].ToString(), obj[2].ToString());
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "User provided invalid number of string arguments");
                return "Invalid string with arguments. "+ex.Message;

            }
        }


        #endregion PlaceHolders

    }
}
