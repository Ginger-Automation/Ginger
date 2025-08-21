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
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Renci.SshNet;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ActConsoleCommand = GingerCore.Actions.ActConsoleCommandCore;

namespace GingerCore.Drivers.ConsoleDriverLib
{
    /// <summary>
    /// Headless Unix shell driver (inherits ConsoleDriverBaseCore, but writes outputs to console/log instead of UI window).
    /// </summary>
    public class HeadlessUnixShellDriverCore : ConsoleDriverBaseCore
    {
        #region User Configurable
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
        [UserConfiguredDefault(@"C:\\Keys\\rsakey.txt")]
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
        #endregion

        public SshClient UnixClient;
        public SftpClient UnixFTPClient;
        ShellStream _shellStream;
        string workdir = "/tmp"; // fallback
        bool IsDriverConnected;

        protected int? mWait;
        protected string mExpString;

        public HeadlessUnixShellDriverCore() { }
        public HeadlessUnixShellDriverCore(BusinessFlow BF) { BusinessFlow = BF; }
        public HeadlessUnixShellDriverCore(BusinessFlow BF, Environments.ProjEnvironment env) { BusinessFlow = BF; Environment = env; }

        public override bool IsSTAThread() => false; // no WPF window needed

        public override ePlatformType Platform => ePlatformType.Unix;

        public override bool Connect()
        {
            try
            {
                ConnectionInfo connectionInfo = KeyboardIntractiveAuthentication ? BuildKeyboardInteractiveConnectionInfo() : new ConnectionInfo(Host, Port, UserName, new PasswordAuthenticationMethod(UserName, Password));
                if (File.Exists(PrivateKey))
                {
                    // augment with private key auth
                    var pkFile = string.IsNullOrEmpty(PrivateKeyPassPhrase) ? new PrivateKeyFile(PrivateKey) : new PrivateKeyFile(PrivateKey, PrivateKeyPassPhrase);
                    connectionInfo = new ConnectionInfo(Host, Port, UserName,
                        new PasswordAuthenticationMethod(UserName, Password),
                        new PrivateKeyAuthenticationMethod(UserName, pkFile));
                }
                UnixClient = new SshClient(connectionInfo);
                UnixClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(SSHConnectionTimeout <= 0 ? 30 : SSHConnectionTimeout);
                UnixClient.Connect();
                if (!UnixClient.IsConnected)
                {
                    ErrorMessageFromDriver = "SSH connection failed";
                    return false;
                }
                // Shell stream
                _shellStream = UnixClient.CreateShellStream("ginger-headless", 200, 50, 800, 600, 4096);
                IsDriverConnected = true;
                Reporter.ToLog(eLogLevel.INFO, "Headless Unix shell connected to " + Host);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to connect headless Unix shell: " + ex.Message, ex);
                ErrorMessageFromDriver = ex.Message;
                return false;
            }
        }

        public override void Disconnect()
        {
            try
            {
                _shellStream?.Dispose();
                if (UnixFTPClient?.IsConnected == true) UnixFTPClient.Disconnect();
                UnixFTPClient?.Dispose();
                if (UnixClient?.IsConnected == true) UnixClient.Disconnect();
                UnixClient?.Dispose();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error during disconnect: " + ex.Message, ex);
            }
            IsDriverConnected = false;
        }

        public override bool IsBusy() => false;
        public override string ConsoleWindowTitle() => "Headless Unix Shell Driver";

        public override void SendCommand(string command)
        {
            if (_shellStream == null) throw new InvalidOperationException("Shell stream not initialized");
            _shellStream.Write(command);
        }

        public override string GetCommandText(ActConsoleCommand act)
        {
            if (act == null) return string.Empty;
            switch (act.ConsoleCommand.ToString())
            {
                case "FreeCommand":
                case "ParametrizedCommand":
                case "Script":
                    return GetParameterizedCommand(act);
                default:
                    Reporter.ToLog(eLogLevel.WARN, "Unknown console command type: " + act.ConsoleCommand);
                    return GetParameterizedCommand(act);
            }
        }

        private string GetParameterizedCommand(ActConsoleCommand act)
        {
            string command = act.Command;
            foreach (var aiv in act.InputValues)
            {
                var val = aiv.ValueForDriver;
                command = string.IsNullOrEmpty(command) ? val : command + " " + val;
            }
            return command;
        }

        public override void RunAction(Act act)
        {
            if (act is not ActConsoleCommand consoleAct)
            {
                base.RunAction(act);
                return;
            }
            mWait = consoleAct.WaitTime;
            ValueExpression VE = new ValueExpression(Environment, BusinessFlow);
            VE.Value = consoleAct.ExpString;
            mExpString = VE.ValueCalculated;
            string command = GetCommandText(consoleAct);
            act.ExInfo = command;
            string output = ExecuteAndCollect(command);
            if (!string.IsNullOrEmpty(mExpString) && !output.Contains(mExpString))
            {
                act.Error = "Expected String \"" + mExpString + "\" not found in command output";
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                return;
            }
            act.AddOrUpdateReturnParamActual("Result", output);
        }

        private string ExecuteAndCollect(string command)
        {
            if (_shellStream == null) throw new InvalidOperationException("Shell stream not initialized");
            _shellStream.WriteLine(command);
            if (mWait is 0 or null) mWait = ImplicitWait <= 0 ? 30 : ImplicitWait;
            DateTime start = DateTime.Now;
            StringBuilder reply = new StringBuilder();
            while ((DateTime.Now - start).TotalSeconds <= mWait)
            {
                if (_shellStream.DataAvailable)
                {
                    string chunk = _shellStream.Read();
                    reply.Append(chunk);
                    Console.Write(chunk);
                    Reporter.ToLog(eLogLevel.DEBUG, chunk);
                }
                Thread.Sleep(200);
            }
            return reply.ToString();
        }

        private ConnectionInfo BuildKeyboardInteractiveConnectionInfo()
        {
            var kAuth = new KeyboardInteractiveAuthenticationMethod(UserName);
            kAuth.AuthenticationPrompt += (sender, e) =>
            {
                foreach (var p in e.Prompts)
                {
                    if (p.Request.Contains("Password", StringComparison.CurrentCultureIgnoreCase))
                    {
                        p.Response = Password;
                    }
                }
            };
            return new ConnectionInfo(Host, Port, UserName, kAuth, new PasswordAuthenticationMethod(UserName, Password));
        }
    }
}
