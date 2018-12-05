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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Renci.SshNet;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GingerCore.Drivers.ConsoleDriverLib
{
    public class UnixShellDriver : ConsoleDriverBase
    {
        [UserConfigured]
        [UserConfiguredDescription("Host Name or IP")]
        public string Host { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("User Name to use for connection")]
        public string UserName { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Password to use for connection")]
        public string Password { get; set; }

        [UserConfigured]
        [UserConfiguredDefault(@"C:\Keys\rsakey.txt")]
        [UserConfiguredDescription("Path to private key file to use for connection")]
        public string PrivateKey { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Passphrase to private key")]
        public string PrivateKeyPassPhrase { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("22")]
        [UserConfiguredDescription("Port")]
        public int Port { get; set; }

        public SshClient UnixClient;
        public SftpClient UnixFTPClient;
        string workdir;

        ShellStream ss;

        string mScriptsFolder;
        public UnixShellDriver(BusinessFlow BF)
        {
            this.BusinessFlow = BF;
        }
        public UnixShellDriver(BusinessFlow BF, Environments.ProjEnvironment env)
        {
            this.BusinessFlow = BF;
            this.Environment = env;
        }
        public void SetScriptsFolder(string FolderPath)
        {
            mScriptsFolder = FolderPath;
        }

        public override bool Connect()
        {
            return ConnectToUnix();
        }

        public override void Disconnect()
        {
            UnixClient.Disconnect();
        }

        public override string ConsoleWindowTitle()
        {
            return "Unix Shell Console Driver - Host=" + Host + " User=" + UserName;
        }

        public override bool IsBusy()
        {
            // TODO: impl how to check if command is still running!?
            return false;
        }

        public override void SendCommand(string command)
        {
            SSHRunCommand(command);
        }

        private bool ConnectToUnix()
        {
            string result = "", s;
            string passPhrase = null;
            try
            {
                if (string.IsNullOrEmpty(Host) || string.IsNullOrEmpty(Port.ToString()) || string.IsNullOrEmpty(UserName))
                {
                    Reporter.ToLog(eAppReporterLogLevel.INFO, "One of Settings of Agent is Empty ");
                    throw new Exception("One of Settings of Agent is Empty ");    
                }
                if (Password == null)
                    Password = "";
                try
                {
                    System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient(Host, Port);
                }
                catch (Exception)
                {
                    Reporter.ToLog(eAppReporterLogLevel.INFO, "Error Connecting to Host: " + Host + " with Port: " + Port);
                    throw new Exception("Error Connecting to Host: " + Host + " with Port: " + Port);
                }

                var connectionInfo = new ConnectionInfo(Host, Port, UserName,
                            new PasswordAuthenticationMethod(UserName, Password)
                        );

                if (!string.IsNullOrEmpty(PrivateKeyPassPhrase))
                    passPhrase = PrivateKeyPassPhrase;

                if (File.Exists(PrivateKey))
                {
                    connectionInfo = new ConnectionInfo(Host, Port, UserName,
                       new PasswordAuthenticationMethod(UserName, Password),
                       new PrivateKeyAuthenticationMethod(UserName,
                            new PrivateKeyFile(File.OpenRead(PrivateKey), passPhrase)
                            )
                   );
                }

                UnixClient = new SshClient(connectionInfo);

                UnixClient.Connect();

                if (UnixClient.IsConnected)
                {
                    UnixClient.SendKeepAlive();
                    ss = UnixClient.CreateShellStream("dumb", 240, 24, 800, 600, 1024);
                    mConsoleDriverWindow.ConsoleWriteText("Connected!");

                    s = ss.ReadLine(new TimeSpan(0, 0, 2));
                    while (!String.IsNullOrEmpty(s))
                    {
                        result = result + "\n" + s;

                        s = ss.ReadLine(new TimeSpan(0, 0, 2));

                    }
                    mConsoleDriverWindow.ConsoleWriteText(result);

                    return true;
                }
                else
                {
                    Reporter.ToLog(eAppReporterLogLevel.INFO, "Error connecting to UnixServer - " + Host);
                    throw new Exception("Error connecting to UnixServer - " + Host);
                }
            }
            catch (Exception e)
            {
                ErrorMessageFromDriver = e.Message;
                return false;
            }
        }
        
        public void SSHRunCommand(string command)
        {
            StringBuilder reply = new StringBuilder();
            ss.Flush();
            StreamReader sreader = new StreamReader(ss);
            sreader.ReadToEnd();
            if (command != "\u0003\n")
            {
                while (!sreader.EndOfStream)
                {
                    if (!this.IsDriverRunning)
                        break;
                    reply.AppendLine(sreader.ReadLine());
                    Thread.Sleep(1000);
                }
            }
            if (command.StartsWith("CHAR_"))
            {
                command = command.Replace("CHAR_", "").Replace("\n", "");
                try
                {
                    ss.WriteByte(System.Convert.ToByte(Convert.ToInt32(command)));
                }
                catch(Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
            }
            else
            {
                ss.Write(command);
            }
            try
            {
                if (mWait == 0 || mWait == null)
                    mWait = this.ImplicitWait;
                if (mWait == 0)
                    mWait = 30;

                DateTime startingTime = DateTime.Now;
                bool expOut = false;
                Regex regExp;
                mExpString = string.IsNullOrEmpty(mExpString) ? "" : mExpString;
                if (mExpString != "" && command != "\u0003\n")
                    regExp = new Regex(@"~~~GINGER_RC_END~~~|" + mExpString);
                else
                    regExp = new Regex(@"\> |\$ |\% |assword: |\. |~~~GINGER_RC_END~~~");

                while ((DateTime.Now - startingTime).TotalSeconds <= mWait && taskFinished != true)
                {
                    if (!this.IsDriverRunning)
                        break;

                    if (sreader.EndOfStream)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    reply.AppendLine(sreader.ReadToEnd());                    
                    Thread.Sleep(1000);
                    if (regExp.Matches(reply.ToString()).Count > 0)
                    {
                        if (reply.ToString().Contains(command) && expOut == false)
                        {

                            taskFinished = true;
                            expOut = true;
                        }
                        else
                            break;
                    }
                    if ((DateTime.Now - startingTime).TotalSeconds >= mWait)
                    {
                        taskFinished = true;
                        break;
                    }
                }
                if (command != "\u0003\n")
                {                    
                    mConsoleDriverWindow.ConsoleWriteText(reply.ToString(),true);
                    reply.Clear();
                }
                taskFinished = true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.Message);
            }            
            mConsoleDriverWindow.ConsoleWriteText(reply.ToString(),true);
            reply.Clear();
        }

        public override string GetCommandText(ActConsoleCommand act)
        {
            string cmd = "";
            switch (act.ConsoleCommand)
            {
                case ActConsoleCommand.eConsoleCommand.FreeCommand:
                    return GetParameterizedCommand(act);
                case ActConsoleCommand.eConsoleCommand.ParametrizedCommand:
                    return GetParameterizedCommand(act);
               
                case ActConsoleCommand.eConsoleCommand.Script:

                    VerifyFTPConnected();

                    if (!UnixFTPClient.Exists(workdir + @"/Ginger"))
                    {
                        UnixFTPClient.CreateDirectory(workdir + @"/Ginger");
                    }

                    string SHFilesPath = mScriptsFolder;
                    string UnixScriptFilePath = workdir + @"/Ginger/" + act.ScriptName;
                    using (var f = File.OpenRead(SHFilesPath + act.ScriptName))
                    {
                        UnixFTPClient.UploadFile(f, UnixScriptFilePath, null);
                    }

                    UnixFTPClient.ChangePermissions(UnixScriptFilePath, 777);
                    foreach (var p in act.InputValues)
                        if (!string.IsNullOrEmpty(p.Value))
                            cmd += " " + p.ValueForDriver;
                    if (UnixScriptFilePath.Trim().EndsWith(".sh", StringComparison.CurrentCultureIgnoreCase))
                        return "dos2unix " + UnixScriptFilePath + ";sh " + UnixScriptFilePath + cmd;
                    else
                        return "dos2unix " + UnixScriptFilePath + "; " + UnixScriptFilePath + cmd;
               
                default:
                    Reporter.ToLog(eAppReporterLogLevel.INFO, "Error - unknown command");
                    ErrorMessageFromDriver += "Error - unknown command";
                    return "Error - unknown command";
            }
        }

        private string GetParameterizedCommand(ActConsoleCommand act)
        {
            string s = act.Command;
            foreach (ActInputValue AIV in act.InputValues)
            {
                if (s != null)
                    s = s + " " + AIV.ValueForDriver;
                else
                    s = AIV.ValueForDriver;
            }
            return s;
        }

        private void VerifyFTPConnected()
        {
            string passPhrase = null;
            var connectionInfo = new ConnectionInfo(Host, Port, UserName,
                            new PasswordAuthenticationMethod(UserName, Password)
                        );

            if (!string.IsNullOrEmpty(PrivateKeyPassPhrase))
                passPhrase = PrivateKeyPassPhrase;

            if (File.Exists(PrivateKey))
            {
                connectionInfo = new ConnectionInfo(Host, Port, UserName,
                   new PasswordAuthenticationMethod(UserName, Password),
                   new PrivateKeyAuthenticationMethod(UserName,
                        new PrivateKeyFile(File.OpenRead(PrivateKey), passPhrase)
                        )
               );
            }
            
            if (UnixFTPClient == null)
            {
                UnixFTPClient = new SftpClient(connectionInfo);
                UnixFTPClient.Connect();
                workdir = UnixFTPClient.WorkingDirectory;
            }
        }

        public override ePlatformType Platform { get { return ePlatformType.Unix; } }
    }
}
