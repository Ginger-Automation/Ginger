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
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace GingerCore.Actions.Tuxedo
{
    public class ActTuxedo : ActWithoutDriver
    {
        public new static partial class Fields
        {
            public static string Host = "Host";
            public static string Port = "Port";
            public static string UserName = "UserName";
            public static string Password = "Password";
            public static string PrivateKey = "PrivateKey";
            public static string PrivateKeyPassPhrase = "PrivateKeyPassPhrase";
            public static string UnixPath = "UnixPath";
            public static string PCPath = "PCPath";
            public static string PreCommand = "PreCommand";                        
        }

        public override string ActionDescription { get { return "Tuxedo UD File Action"; } }
        public override string ActionUserDescription { get { return "Performs Tuxedo UD File action"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you want to perform any Tuxedo UD File.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a Tuxedo UD File action, enter Local UD File by clicking browse button and then enter all Unix Config.");
        }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public override string ActionEditPage { get { return "Tuxedo.ActTuxedoEditPage"; } }
        
        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }
        
        private SftpClient UnixFTPClient;
        private string workdir;
        
        public ActInputValue Host { get { return GetOrCreateInputParam(Fields.Host); } }
        
        public ActInputValue Port { get { return GetOrCreateInputParam(Fields.Port); } }
        
        public ActInputValue UserName { get { return GetOrCreateInputParam(Fields.UserName); } }
       
        public ActInputValue Password { get { return GetOrCreateInputParam(Fields.Password); } }
       
        public ActInputValue PrivateKey { get { return GetOrCreateInputParam(Fields.PrivateKey); } }
        
        public ActInputValue PrivateKeyPassPhrase { get { return GetOrCreateInputParam(Fields.PrivateKeyPassPhrase); } }
       
        public ActInputValue PCPath { get { return GetOrCreateInputParam(Fields.PCPath); } }
        
        public ActInputValue UnixPath { get { return GetOrCreateInputParam(Fields.UnixPath); } }

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> DynamicUDElements = new ObservableList<ActInputValue>();
     
        public ActInputValue PreCommand { get { return GetOrCreateInputParam(Fields.PreCommand); } }

        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = new List<ObservableList<ActInputValue>>();
            list.Add(DynamicUDElements);
            return list;
        }
        
        public override String ActionType
        {
            get
            {
                return "Tuxedo UD File";
            }
        }
        
        
        public override System.Drawing.Image Image { get { return Resources.console16x16; } }

        private bool FTPConnect()
        {
            try
            {
                //TODO: add option to use passkey

                 var connectionInfo = new PasswordConnectionInfo(Host.ValueForDriver, UserName.ValueForDriver, Password.ValueForDriver);
                    UnixFTPClient = new SftpClient(connectionInfo);
                    UnixFTPClient.Connect();
                    workdir = UnixFTPClient.WorkingDirectory;
                if (UnixFTPClient.IsConnected)
                    return true;
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Cannot Login to Unix" , ex);
                throw (ex);
            }
            return false;
        }

        public override void Execute()
        {
            // Steps
            // 1. Connect to Unix
            // 2. Prepare local UD file after replacing place holders = take ValueForDriver
            // 3. Upload the UD file to unix
            // 4. Run Unix command: ud32 –n < file_name.ud   --> output to file
            // 5. Parse result to Act.ReturnValues 

            // TODO: add optional save result to local file
             string drvUnixPath = GetInputParamCalculatedValue("UnixPath");
        
            string UnixTargetFilePath = "";
            try
            {
                if (!FTPConnect())
                    {
                        Error = "Failed to login!";
                        return;
                    }
                    if (String.IsNullOrEmpty(drvUnixPath))
                    {
                        UnixTargetFilePath = workdir + "/";
                    }
                    else if (UnixFTPClient.Exists(drvUnixPath.Replace("~/", workdir + "/")))
                    {
                        UnixTargetFilePath = drvUnixPath.Replace("~/", workdir + "/");
                    }
                    string LocalUDFileName = PrepareUDFile();

                    string UnixUDfileName = "GingerUD_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".ud";

                    if (!UnixTargetFilePath.EndsWith("/"))
                            UnixTargetFilePath += "/";

                    UploadUDFile(UnixTargetFilePath + UnixUDfileName, LocalUDFileName);                    
                    
                    string Result = ExecuteUDCommand(PreCommand.ValueForDriver, "ud32 –n < " + UnixTargetFilePath + UnixUDfileName);

                    //TODO: temp for testing
                    if(!String.IsNullOrEmpty(Result))
                        ParseResult(Result);     
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, e.Message);
                Error = e.Message;
            }
        }

        private void ParseResult(string Result)
        {
            Result = Result.Replace("\r", "");
            Result = Result.Replace("\t", "");
            
            AddOrUpdateReturnParamActual("Actual", Result);            
        }

        private void UploadUDFile(string UnixUDfileName, string LocalTempUDFileName)
        {
            //  check Unix folder exist
            
            using (var f = File.OpenRead(LocalTempUDFileName))
            {
                UnixFTPClient.UploadFile(f,UnixUDfileName, null);
            }
        }

        private string PrepareUDFile()
        {
            string txt = "";
            
            foreach(ActInputValue AIV in DynamicUDElements)
            {
                txt += AIV.Param + "\t"+ AIV.ValueForDriver+"\n";  // Add \t for tab and \r for new line
            
            }
            txt += "\n";
            string TempFileName = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(TempFileName, txt);
            return TempFileName;

            //TODO: later delete the tempfile
        }

        private string ExecuteUDCommand(string PreCommand, string Command)
        {
            SshClient UnixClient;
            string cmdResult = string.Empty;
            int iPort;
            ConnectionInfo connectionInfo;
            if (int.TryParse(Port.ValueForDriver,out iPort))            
                connectionInfo = new ConnectionInfo(Host.ValueForDriver,iPort, UserName.ValueForDriver,
                            new PasswordAuthenticationMethod(UserName.ValueForDriver, Password.ValueForDriver));
            
            else
                connectionInfo = new ConnectionInfo(Host.ValueForDriver, UserName.ValueForDriver,
                            new PasswordAuthenticationMethod(UserName.ValueForDriver, Password.ValueForDriver));

            UnixClient = new SshClient(connectionInfo);           
            UnixClient.Connect();
            if (UnixClient.IsConnected)
            {                
                ExInfo += "Connected to: " + Host.ValueForDriver;                
            }
            else
            {
                ExInfo += "Cannot connect to: " + Host.ValueForDriver;
                Error += "Cannot connect to: " + Host.ValueForDriver;
                return null;
            }
            using (ShellStream shell = UnixClient.CreateShellStream("tmp", 80, 24, 800, 600, 1024))
            {                
                //StreamWriter writer = new StreamWriter(shell);

                if (PreCommand != "")
                {
                    ExInfo += ", Sending PreCommand: " + PreCommand;                   
                    foreach (string sCmd in PreCommand.Split(';'))
                    {
                        cmdResult += ExecuteCommandUsingShellStream(shell, sCmd);
                    }                    
                    ExInfo += ", PreCommandResult: " + cmdResult;
                }
                cmdResult = ExecuteCommandUsingShellStream(shell, Command);
                ExInfo += ", Result: " + cmdResult;
                shell.Close();
            }
            UnixClient.Disconnect();

            return cmdResult;
        }

        private string ExecuteCommandUsingShellStream(ShellStream shell, string Command)
        {
            string cmdResult=string.Empty;           
            StreamReader reader = new StreamReader(shell);            
            shell.Flush();
            cmdResult = string.Empty;
            ExInfo += ", Sending Command: " + Command;
            shell.Write(Command + "\n");
            cmdResult = reader.ReadToEnd();
            int iCount = 1;
            Regex regExp = new Regex(@"ENTER ...|continue ...|option :|\> |\$ |\%");
            while (regExp.Matches(cmdResult.ToString()).Count == 0 && iCount<20)
            {
                cmdResult += reader.ReadToEnd();
               
                cmdResult=Regex.Replace(cmdResult, @"[\u001b]\[[0-9]+m", "");
                Thread.Sleep(1000);
                iCount++;
            }
            return cmdResult;
        }
    }
}