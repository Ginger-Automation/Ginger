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
                PersistanceType = persistenceType
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
