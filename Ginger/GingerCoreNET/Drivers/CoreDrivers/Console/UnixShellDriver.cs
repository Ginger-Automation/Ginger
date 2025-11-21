#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.CoreNET.ActionsLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Renci.SshNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Console
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

        [UserConfigured]
        [UserConfiguredDefault("30")]
        [UserConfiguredDescription("Maximum time(in seconds) to wait for SSH Client connection")]
        public int SSHConnectionTimeout { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Enable Keyboard Intractive Authentication for Login")]
        public bool KeyboardIntractiveAuthentication { get; set; }

        public SshClient UnixClient;
        public SftpClient UnixFTPClient;
        private TcpClient tcpClient;
        string workdir;

        ShellStream ss;

        string mScriptsFolder;
        public UnixShellDriver(BusinessFlow BF)
        {
            BusinessFlow = BF;
        }
        public UnixShellDriver(BusinessFlow BF, ProjEnvironment env)
        {
            BusinessFlow = BF;
            Environment = env;
        }

        public override void InitDriver(Agent agent)
        {
            SetScriptsFolder(Path.Combine(agent.SolutionFolder, @"Documents\sh\"));
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
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Disconnecting Unix Shell Driver");

                // Close shell stream first
                if (ss != null)
                {
                    try
                    {
                        ss.Close();
                        ss.Dispose();
                        ss = null;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Error closing shell stream", ex);
                    }
                }

                // Close FTP client if connected
                if (UnixFTPClient != null && UnixFTPClient.IsConnected)
                {
                    try
                    {
                        UnixFTPClient.Disconnect();
                        UnixFTPClient.Dispose();
                        UnixFTPClient = null;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Error closing FTP client", ex);
                    }
                }

                // Close SSH client
                if (UnixClient != null)
                {
                    try
                    {
                        if (UnixClient.IsConnected)
                        {
                            UnixClient.Disconnect();
                        }
                        UnixClient.Dispose();
                        UnixClient = null;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Error closing SSH client", ex);
                    }
                }

                // Close TCP client
                if (tcpClient != null)
                {
                    try
                    {
                        tcpClient.Close();
                        tcpClient.Dispose();
                        tcpClient = null;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Error closing TCP client", ex);
                    }
                }

                Reporter.ToLog(eLogLevel.INFO, "Unix Shell Driver disconnected successfully");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Unix Agent disconnect failed.", ex);
            }
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
            // Removed OnDriverMessage to avoid duplicate command echo
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
                    Reporter.ToLog(eLogLevel.WARN, "One of Settings of Agent is Empty ");
                    throw new Exception("One of Settings of Agent is Empty ");
                }
                if (Password == null)
                {
                    Password = "";
                }

                try
                {
                    tcpClient = new System.Net.Sockets.TcpClient(Host, Port);
                }
                catch (Exception)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Error Connecting to Host: " + Host + " with Port: " + Port);
                    throw new Exception("Error Connecting to Host: " + Host + " with Port: " + Port);
                }

                ConnectionInfo connectionInfo = null;
                if (KeyboardIntractiveAuthentication)
                {
                    connectionInfo = KeyboardInteractiveAuthConnectionInfo();
                }
                else
                {
                    connectionInfo = new ConnectionInfo(Host, Port, UserName,
                               new PasswordAuthenticationMethod(UserName, Password)
                           );
                }


                if (!string.IsNullOrEmpty(PrivateKeyPassPhrase))
                {
                    passPhrase = PrivateKeyPassPhrase;
                }

                if (File.Exists(PrivateKey))
                {
                    connectionInfo = new ConnectionInfo(Host, Port, UserName,
                       new PasswordAuthenticationMethod(UserName, Password),
                       new PrivateKeyAuthenticationMethod(UserName,
                            new PrivateKeyFile(File.OpenRead(PrivateKey), passPhrase)
                            )
                   );
                }
                connectionInfo.Timeout = new TimeSpan(0, 0, SSHConnectionTimeout);
                UnixClient = new SshClient(connectionInfo);

                var task = Task.Factory.StartNew(() =>
                 {
                     try
                     {
                         UnixClient.Connect();

                         if (UnixClient.IsConnected)
                         {
                             UnixClient.SendKeepAlive();
                             ss = UnixClient.CreateShellStream("dumb", 240, 24, 800, 600, 1024);
                         }
                     }
                     catch (Exception ex)
                     {
                         Reporter.ToLog(eLogLevel.ERROR, "Error while connecting to Unix Client.", ex);
                     }
                 });

                var st = Stopwatch.StartNew();

                while (!task.IsCompleted && st.ElapsedMilliseconds < SSHConnectionTimeout * 1000)
                {
                    task.Wait(500);  // Give user feedback every 500ms
                }

                if (UnixClient.IsConnected)
                {
                    // Use the base class method to write to console buffer instead of directly to window
                    System.Console.WriteLine(result);
                    WriteToConsoleBuffer("Connected!");

                    s = ss.ReadLine(new TimeSpan(0, 0, 2));
                    while (!string.IsNullOrEmpty(s))
                    {
                        result = result + "\n" + s;

                        s = ss.ReadLine(new TimeSpan(0, 0, 2));

                    }
                    System.Console.WriteLine(result);
                    WriteToConsoleBuffer(result);

                    return true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, "Error connecting to UnixServer - " + Host);
                    throw new Exception("Error connecting to UnixServer - " + Host);
                }
            }
            catch (Exception e)
            {
                ErrorMessageFromDriver = e.Message;
                return false;
            }
        }

        private ConnectionInfo KeyboardInteractiveAuthConnectionInfo()
        {
            ConnectionInfo connectionInfo;
            var keyboardIntractiveAuth = new KeyboardInteractiveAuthenticationMethod(UserName);
            var pauth = new PasswordAuthenticationMethod(UserName, Password);

            keyboardIntractiveAuth.AuthenticationPrompt += HandleIntractiveKeyBoardEvent;

            connectionInfo = new ConnectionInfo(Host, Port, UserName, pauth, keyboardIntractiveAuth);
            return connectionInfo;
        }

        void HandleIntractiveKeyBoardEvent(object sender, Renci.SshNet.Common.AuthenticationPromptEventArgs e)
        {
            foreach (var prompt in e.Prompts)
            {
                if (prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    prompt.Response = Password;
                }
            }
        }

        public void SSHRunCommand(string command)
        {
            var reply = new StringBuilder();
            ss.Flush();
            var sreader = new StreamReader(ss);
            sreader.ReadToEnd();
            if (command != "\u0003\n")
            {
                while (!sreader.EndOfStream)
                {
                    if (!IsDriverRunning)
                    {
                        break;
                    }

                    reply.AppendLine(sreader.ReadLine());
                    Thread.Sleep(1000);
                }
            }
            if (command.StartsWith("CHAR_"))
            {
                command = command.Replace("CHAR_", "").Replace("\n", "");
                try
                {
                    ss.WriteByte(Convert.ToByte(Convert.ToInt32(command)));
                }
                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
            }
            else
            {
                ss.Write(command);
            }
            try
            {
                if (mWait is 0 or null)
                {
                    mWait = ImplicitWait;
                }

                if (mWait == 0)
                {
                    mWait = 30;
                }

                var startingTime = DateTime.Now;
                var expOut = false;
                Regex regExp;
                mExpString = string.IsNullOrEmpty(mExpString) ? "" : mExpString;
                if (mExpString != "" && command != "\u0003\n")
                {
                    regExp = new Regex(@"~~~GINGER_RC_END~~~|" + mExpString);
                }
                else
                {
                    regExp = new Regex(@"\> |\$ |\% |assword: |\. |~~~GINGER_RC_END~~~");
                }

                while ((DateTime.Now - startingTime).TotalSeconds <= mWait && taskFinished != true)
                {
                    if (!IsDriverRunning)
                    {
                        break;
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
                        {
                            break;
                        }
                    }

                    if (sreader.EndOfStream)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    if ((DateTime.Now - startingTime).TotalSeconds >= mWait)
                    {
                        taskFinished = true;
                        break;
                    }
                }
                if (command != "\u0003\n")
                {
                    // Use the base class method to write to console buffer instead of directly to window
                    System.Console.WriteLine(reply.ToString());
                    WriteToConsoleBuffer(reply.ToString());
                    reply.Clear();
                }
                taskFinished = true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message);
            }
        }

        public override string GetCommandText(ActConsoleCommand act)
        {
            var cmd = "";
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

                    var SHFilesPath = mScriptsFolder;
                    var UnixScriptFilePath = workdir + @"/Ginger/" + act.ScriptName;
                    using (var f = File.OpenRead(SHFilesPath + act.ScriptName))
                    {
                        UnixFTPClient.UploadFile(f, UnixScriptFilePath, null);
                    }

                    UnixFTPClient.ChangePermissions(UnixScriptFilePath, 777);
                    foreach (var p in act.InputValues)
                    {
                        if (!string.IsNullOrEmpty(p.Value))
                        {
                            cmd += " " + p.ValueForDriver;
                        }
                    }

                    if (UnixScriptFilePath.Trim().EndsWith(".sh", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return "dos2unix " + UnixScriptFilePath + ";sh " + UnixScriptFilePath + cmd;
                    }
                    else
                    {
                        return "dos2unix " + UnixScriptFilePath + "; " + UnixScriptFilePath + cmd;
                    }

                default:
                    Reporter.ToLog(eLogLevel.WARN, "Error - unknown command");
                    ErrorMessageFromDriver += "Error - unknown command";
                    return "Error - unknown command";
            }
        }

        private string GetParameterizedCommand(ActConsoleCommand act)
        {
            var command = act.Command;
            foreach (var AIV in act.InputValues)
            {
                string calcValue;
                if (string.IsNullOrEmpty(AIV.Value) == false && AIV.Value.StartsWith("~"))//workaround for keeping the ~ to work on linux
                {
                    string prefix;
                    if (AIV.Value.StartsWith("~/") || AIV.Value.StartsWith("~\\"))
                    {
                        prefix = AIV.Value[..2];
                    }
                    else
                    {
                        prefix = "~";
                    }
                    calcValue = AIV.ValueForDriver.Replace(WorkSpace.Instance.Solution.Folder.TrimEnd(new char[] { '/', '\\' }) + Path.DirectorySeparatorChar, prefix);
                }
                else
                {
                    calcValue = AIV.ValueForDriver;
                }

                if (command != null)
                {
                    command = command + " " + calcValue;
                }
                else
                {
                    command = calcValue;
                }
            }
            return command;
        }

        private void VerifyFTPConnected()
        {
            string passPhrase = null;

            ConnectionInfo connectionInfo = null;
            if (KeyboardIntractiveAuthentication)
            {
                connectionInfo = KeyboardInteractiveAuthConnectionInfo();
            }
            else
            {
                connectionInfo = new ConnectionInfo(Host, Port, UserName,
                            new PasswordAuthenticationMethod(UserName, Password)
                        );
            }


            if (!string.IsNullOrEmpty(PrivateKeyPassPhrase))
            {
                passPhrase = PrivateKeyPassPhrase;
            }

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
