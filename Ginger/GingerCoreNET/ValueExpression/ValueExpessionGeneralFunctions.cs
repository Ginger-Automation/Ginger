#region License
/*
Copyright © 2014-2019 European Support Limited

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
        public string GetUnixTimeStamp()
        {
            DateTimeOffset dtoffset = new DateTimeOffset(DateTime.Now);
            long unixDateTime = dtoffset.ToUnixTimeSeconds();
            return unixDateTime.ToString();
        }


        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Get GUID")]
        [ValueExpressionFunctionExpression("{Function Fun=GetGUID()}")]
        public string GetGUID()
        {
            return Guid.NewGuid().ToString();
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Generate HashCode")]
        [ValueExpressionFunctionExpression("{Function Fun=GenerateHashCode(\"Hello\")}")]
        public string GenerateHashCode(object[] obj)
        {
            SHA1CryptoServiceProvider sha1Hasher = new SHA1CryptoServiceProvider();
            byte[] hashedDataBytes = sha1Hasher.ComputeHash(Encoding.UTF8.GetBytes(obj[0].ToString()));
            return Convert.ToBase64String(hashedDataBytes);
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Current UTC time stamp")]
        [ValueExpressionFunctionExpression("{Function Fun=GetUTCTimeStamp()}")]

        public string GetUTCTimeStamp()
        {
            DateTime todayDate = DateTime.UtcNow;
            return todayDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Get Hashed Data Byte string")]
        [ValueExpressionFunctionExpression("{Function Fun=GetHashedDataByteString(\"Hello\")}")]
        public string GetHashedDataByteString(object[] obj)
        {
            SHA1CryptoServiceProvider sha1Hasher = new SHA1CryptoServiceProvider();
            byte[] hashedDataBytes = sha1Hasher.ComputeHash(Encoding.UTF8.GetBytes(obj[0].ToString().ToCharArray()));
            return ASCIIEncoding.ASCII.GetString( hashedDataBytes);
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Encrypt  to Base 64")]
        [ValueExpressionFunctionExpression("{Function Fun=GetEncryptedBase64String(\"Hello\")}")]
        public string GetEncryptedBase64String(object[] obj)
        {
            byte[] hashedDataBytes = Encoding.ASCII.GetBytes((obj[0].ToString()).ToCharArray()); 
            return Convert.ToBase64String(hashedDataBytes);
        }

        [ValueExpressionFunctionAttribute]
        [ValueExpressionFunctionDescription("Decrypt to Base 64")]
        [ValueExpressionFunctionExpression("{Function Fun=GetDecryptedBase64String(\"Hello\")}")]
        public string GetDecryptedBase64String(object[] obj)
        {
            byte[] hashedDataBytes = Convert.FromBase64String(obj[0].ToString()); 
            return ASCIIEncoding.ASCII.GetString(hashedDataBytes); 
        }

        #endregion PlaceHolders

    }
}
