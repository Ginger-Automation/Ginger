#region License
/*
Copyright © 2014-2022 European Support Limited

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

using CredentialManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.GeneralLib
{
    public static class WinCredentialUtil
    {
        public static string GetCredential(string target)
        {
            var cm = new Credential { Target = target };
            if (!cm.Load())
            {
                return null;
            }

            // UserPass is just a class with two string properties for user and pass
            return cm.Password;
        }

        public static bool SetCredentials(
             string target, string username, string password, PersistanceType persistenceType = PersistanceType.Enterprise)
        {
            return new Credential
            {
                Target = target,
                Username = username,
                Password = password,
                PersistanceType = persistenceType,
                Type = CredentialType.Generic
            }.Save();
        }

        public static bool RemoveCredentials(string target)
        {
            return new Credential { Target = target }.Delete();
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
