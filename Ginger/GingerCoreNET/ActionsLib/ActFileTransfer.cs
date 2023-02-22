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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;

namespace GingerCore.Actions
{
    public class ActFileTransfer : ActWithoutDriver
    {
        public override string ActionDescription { get { return "File Transfer Action"; } }
        public override string ActionUserDescription { get { return "Transfer File from one location to other"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to transfer any file from one location to another.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To transfer any file first select the file transfer action Put file or get file, then select the PC path by clicking Browse button.Then fill all the values to Unix Path,Host,Port,UserName,Password,PrivateKeyFile and KeyPassPhrase");
        }

        public override string ActionEditPage { get { return "ActFileTransferEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

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

        public enum eFileTransferAction
        {
            PutFile = 0,
            GetFile = 1,
        }
        private SftpClient UnixFTPClient;
        private string workdir;
        public eFileTransferAction FileTransferAction 
        {
            get
            {
                return (eFileTransferAction)GetOrCreateInputParam<eFileTransferAction>(nameof(FileTransferAction), eFileTransferAction.GetFile);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FileTransferAction), value.ToString());
            }
        }


        public string Host
        {
            get
            {
                return GetInputParamValue("Host");
            }
            set
            {
                AddOrUpdateInputParamValue("Host", value);
            }
        }
        private string drvHost { get { return GetInputParamCalculatedValue("Host"); } }


        public string Port//public int Port
        {
            get
            {
                //return Convert.ToInt32(GetInputParam("Port"));
                return GetInputParamValue("Port");
            }
            set
            {
                //AddOrUpdateInputParam("Port", Convert.ToString(value));
                AddOrUpdateInputParamValue("Port", value);
            }
        }
        private int drvPort { get { return Convert.ToInt32(GetInputParamCalculatedValue("Port")); } }


        public string UserName
        {
            get
            {
                return GetInputParamValue("UserName");
            }
            set
            {
                AddOrUpdateInputParamValue("UserName", value);
            }
        }
        private string drvUserName { get { return GetInputParamCalculatedValue("UserName"); } }


        public string Password
        {
            get
            {
                return GetInputParamValue("Password");
            }
            set
            {
                AddOrUpdateInputParamValue("Password", value);
            }
        }
        private string drvPassword { get { return GetInputParamCalculatedValue("Password"); } }


        public string PrivateKey
        {
            get
            {
                return GetInputParamValue("PrivateKey");
            }
            set
            {
                AddOrUpdateInputParamValue("PrivateKey", value);
            }
        }
        private string drvPrivateKey { get { return GetInputParamCalculatedValue("PrivateKey"); } }


        public string PrivateKeyPassPhrase
        {
            get
            {
                return GetInputParamValue("PrivateKeyPassPhrase");
            }
            set
            {
                AddOrUpdateInputParamValue("PrivateKeyPassPhrase", value);
            }
        }
        private string drvPrivateKeyPassPhrase { get { return GetInputParamCalculatedValue("PrivateKeyPassPhrase"); } }


        public string PCPath
        {
            get
            {
                return GetInputParamValue("PCPath");
            }
            set
            {
                AddOrUpdateInputParamValue("PCPath", value);
            }
        }

        public bool KeyboardIntractiveAuthentication 
        {
            get 
            {
                return Convert.ToBoolean(GetInputParamValue("KeyboardIntractiveAuthentication"));
            }
            set 
            {
                AddOrUpdateInputParamValue("KeyboardIntractiveAuthentication",Convert.ToString(value));
            }
        }
        private string mPCPathCalculated=string.Empty;

        private string PCPathCalculated
        {
            get
            {
                return WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(GetInputParamCalculatedValue("PCPath")).Replace(Environment.NewLine, "");
            }
        }


        public string UnixPath
        {
            get
            {
                return GetInputParamValue("UnixPath");
            }
            set
            {
                AddOrUpdateInputParamValue("UnixPath", value);
            }
        }

        private string UnixPathCalculated
        {
            get
            {
                return GetInputParamCalculatedValue("UnixPath").Replace("~/", workdir + "/");

            }
        }

        public override String ActionType
        {
            get
            {
                return "File Transfer - " + FileTransferAction.ToString();
            }
        }

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

            public static string KeyboardIntractiveAuthentication = "KeyboardIntractiveAuthentication";
        }

        public override eImageType Image { get { return eImageType.CodeFile; } }

        private bool ConnectFTPClient()
        {
            try
            {
                UnixFTPClient = new SftpClient(GetConnectionInfo());
                UnixFTPClient.Connect();
                workdir = UnixFTPClient.WorkingDirectory;

                return UnixFTPClient.IsConnected;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Connecting FTP Client", ex);
                this.Error = ex.Message;
            }
            return false;
        }

        private ConnectionInfo GetConnectionInfo()
        {
            string passPhrase = null;
            var pw = "";
            if (!string.IsNullOrEmpty(drvPassword))
            {
                pw = drvPassword;
            }
            ConnectionInfo connectionInfo = null;
            if (this.KeyboardIntractiveAuthentication)
            {
                connectionInfo = KeyboardInteractiveAuthConnectionInfo();
            }
            else
            {
                connectionInfo = new ConnectionInfo(drvHost, drvPort, drvUserName,
                            new PasswordAuthenticationMethod(drvUserName, pw)
                        );
            }

            if (!string.IsNullOrEmpty(drvPrivateKeyPassPhrase))
                passPhrase = drvPrivateKeyPassPhrase;


            if (File.Exists(drvPrivateKey))
            {
                connectionInfo = new ConnectionInfo(drvHost, drvPort, drvUserName,
                   new PasswordAuthenticationMethod(drvUserName, pw),
                   new PrivateKeyAuthenticationMethod(drvUserName,
                        new PrivateKeyFile(File.OpenRead(drvPrivateKey), passPhrase)
                        )
               );
            }

            return connectionInfo;
        }

        private ConnectionInfo KeyboardInteractiveAuthConnectionInfo()
        {
            ConnectionInfo connectionInfo;
            KeyboardInteractiveAuthenticationMethod keyboardIntractiveAuth = new KeyboardInteractiveAuthenticationMethod(UserName);
            PasswordAuthenticationMethod pauth = new PasswordAuthenticationMethod(UserName, Password);
            keyboardIntractiveAuth.AuthenticationPrompt += HandleIntractiveKeyBoardEvent;

            connectionInfo = new ConnectionInfo(Host, drvPort, drvUserName,pauth, keyboardIntractiveAuth);
            return connectionInfo;
        }

        void HandleIntractiveKeyBoardEvent(Object sender, Renci.SshNet.Common.AuthenticationPromptEventArgs e)
        {
            foreach (Renci.SshNet.Common.AuthenticationPrompt prompt in e.Prompts)
            {
                if (prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    prompt.Response = drvPassword;
                }
            }
        }

        public override void Execute()
        {
            switch (FileTransferAction)
            {
                case eFileTransferAction.GetFile:
                    try
                    {
                        if (!ConnectFTPClient())
                        {
                            return;
                        }
                        string UnixTargetFilePath = UnixPathCalculated;
                        if (UnixFTPClient.Exists(UnixTargetFilePath) == false)
                        {
                            this.Error = "'" + UnixPathCalculated + "' path does not exists";
                            return;
                        }

                        DownloadFileAndValidate(UnixTargetFilePath, PCPathCalculated);
                    }
                    catch (Exception e)
                    {
                        this.Error = e.Message;
                        Reporter.ToLog(eLogLevel.ERROR, "Get File", e);
                    }
                    finally
                    {
                        UnixFTPClient.Disconnect();
                        UnixFTPClient.Disconnect();
                    }

                    break;

                case eFileTransferAction.PutFile:
                    try
                    {
                        if (!ConnectFTPClient())
                        {
                            return;
                        }
                        string unixTargetFolder = CalculateTargetFolder();

                        if (IsDir(PCPathCalculated) == false)
                        {
                            UploadFileAndValidate(PCPathCalculated, unixTargetFolder);
                        }
                        else
                        {
                            string[] fileEntries = Directory.GetFiles(PCPathCalculated);
                            foreach (string fileName in fileEntries)
                            {
                                UploadFileAndValidate(fileName, unixTargetFolder);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        this.Error = e.Message;
                        Reporter.ToLog(eLogLevel.ERROR, "Put File", e);
                    }
                    finally
                    {
                        UnixFTPClient.Disconnect();
                        UnixFTPClient.Dispose();
                    }
                    break;

                default:
                    break;

            }
        }

        private string CalculateTargetFolder()
        {
            string targetFolder = UnixPathCalculated;
            if (UnixFTPClient.Exists(UnixPathCalculated) == false)
            {
                //if path given by user does not exist upload it to /Ginger/Upload
                targetFolder = Path.Combine(workdir, "Ginger/Uploaded").Replace("\\", "/");

                if (UnixFTPClient.Exists(targetFolder) == false)
                {
                    UnixFTPClient.CreateDirectory(targetFolder);
                }
            }
            return targetFolder;
        }

        private void UploadFileAndValidate(string filePath, string targetFolder)
        {
            using (var fileToUpload = File.OpenRead(filePath))
            {
                string targetFilePath = Path.Combine(targetFolder, Path.GetFileName(filePath)).Replace("\\", "/");
                UnixFTPClient.UploadFile(fileToUpload, targetFilePath);

                if (UnixFTPClient.Exists(targetFilePath))
                {
                    this.ExInfo += "File '" + Path.GetFileName(filePath) + "' successfully uploaded to '" + targetFolder + "' directory\n";
                    this.AddOrUpdateReturnParamActual("Uploaded File Name", Path.GetFileName(filePath));
                    this.AddOrUpdateReturnParamActual("Uploaded File Directory", targetFolder);
                    this.AddOrUpdateReturnParamActual("Uploaded File Path", targetFilePath);
                }
                else
                {
                    this.Error += "Failed to upload '" + Path.GetFileName(filePath) + "' to " + targetFolder + "\n";
                }
            }
        }


        private void DownloadFileAndValidate(string remoteFile, string localPath)
        {
            string filename;
            if (File.Exists(localPath))
            {
                File.Delete(localPath);
                filename = localPath;
            }
            else if (!IsDir(localPath))
            {
                filename = localPath;
            }
            else
            {
                filename = Path.Combine(localPath, Path.GetFileName(remoteFile));
            }

            using (var f = File.OpenWrite(filename))
            {
                UnixFTPClient.DownloadFile(remoteFile, f);
            }


            string targetFileName = Path.GetFileName(remoteFile);
            if (File.Exists(Path.Combine(localPath, targetFileName)))
            {
                this.ExInfo = "File '" + targetFileName + "' successfully  downloaded to '" + localPath + "' directory";
                this.AddOrUpdateReturnParamActual("Downloaded File Name", targetFileName);
                this.AddOrUpdateReturnParamActual("Downloaded File Path", Path.Combine(localPath, targetFileName));
                this.AddOrUpdateReturnParamActual("Downloaded File Directory", localPath);
            }
            else
            {
                this.Error = "Failed to get file '" + targetFileName + "' " + "to " + localPath + "\\" + " directory";
            }
        }

        private bool IsDir(string path)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(path);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
    }
}

