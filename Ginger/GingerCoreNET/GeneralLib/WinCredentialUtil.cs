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

using Amdocs.Ginger.Common;
using Meziantou.Framework.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.GeneralLib
{
    public static class WinCredentialUtil
    {
        public static string GetCredential(string target)
        {
            try
            {
                // Get a credential from the credential manager
                var cred = CredentialManager.ReadCredential(applicationName: target);
                return cred?.Password;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return null;
            }
        }
        public static bool SetCredentials(string target, string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return true;
                }
                // Save the credential to the credential manager
                CredentialManager.WriteCredential(
                    applicationName: target,
                    userName: username,
                    secret: password,
                    persistence: CredentialPersistence.LocalMachine);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return false;
            }
        }
        public static bool RemoveCredentials(string target)
        {
            // Delete a credential from the credential manager
            CredentialManager.DeleteCredential(applicationName: target);
            return true;
        }

        public static UserPass ReadCredential(string target)
        {
            try
            {
                var cred = CredentialManager.ReadCredential(applicationName: target);
                return new UserPass(cred?.UserName, cred?.Password);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return null;
            }
        }
    }

    public class UserPass
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public UserPass(string user, string pass)
        {
            this.Username = user;
            this.Password = pass;
        }
    }
}