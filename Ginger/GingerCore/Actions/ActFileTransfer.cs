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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GingerCore.Properties;
using GingerCore.Repository;
using Renci.SshNet;
using System.IO;
using GingerCore.Platforms;
using GingerCore.Helpers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common;

namespace GingerCore.Actions
{
    public class ActFileTransfer : ActWithoutDriver
    {
        public override string ActionDescription { get { return "File Transfer Action"; } }
        public override string ActionUserDescription { get { return "Transfer File from one location to other"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
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
        [IsSerializedForLocalRepository]
        public eFileTransferAction FileTransferAction { get; set; }

        [IsSerializedForLocalRepository]
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
       
        [IsSerializedForLocalRepository]        
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
        
        [IsSerializedForLocalRepository]
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

        [IsSerializedForLocalRepository]
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

        [IsSerializedForLocalRepository]
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

        [IsSerializedForLocalRepository]
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

        [IsSerializedForLocalRepository]
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
        private string drvPCPath { get { return GetInputParamCalculatedValue("PCPath"); } }

        [IsSerializedForLocalRepository]
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
        private string drvUnixPath { get { return GetInputParamCalculatedValue("UnixPath"); } }

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
            
        }

        public override System.Drawing.Image Image { get { return Resources.console16x16; } }

        private bool VerifyFTPConnected()
        {
            try
            {
                string passPhrase = null;
                string pw = null;
                if (string.IsNullOrEmpty(drvPassword))
                    pw = "";
                else
                    pw = drvPassword;
                var connectionInfo = new ConnectionInfo(drvHost, drvPort, drvUserName,
                                new PasswordAuthenticationMethod(drvUserName, pw)
                            );

                if (!string.IsNullOrEmpty(drvPrivateKeyPassPhrase))
                    passPhrase = drvPrivateKeyPassPhrase;

                //if (File.Exists(PrivateKey))
                if (File.Exists(drvPrivateKey))
                {
                    connectionInfo = new ConnectionInfo(drvHost, drvPort, drvUserName,
                       new PasswordAuthenticationMethod(drvUserName, pw),
                       new PrivateKeyAuthenticationMethod(drvUserName,
                            new PrivateKeyFile(File.OpenRead(drvPrivateKey), passPhrase)
                            )
                   );
                }

                //        var connectionInfo = new PasswordConnectionInfo(drvHost, drvPort, drvUserName, drvPassword);
                
                if (UnixFTPClient == null)
                {
                    //ZZZZ prep for Manuj Send XML to unix folder
                    // OK UnixFTPClient = new SftpClient("10.120.21.172", "infrahf", "unix11");

                    UnixFTPClient = new SftpClient(connectionInfo);
                    UnixFTPClient.Connect();
                    workdir = UnixFTPClient.WorkingDirectory;
                }
                if (UnixFTPClient.IsConnected)
                    return true;

            }
            catch
            {
                //return false;
            }
            return false;
        }

        public override void Execute()
        {
            string sPCPath="";
           // ValueExpression VE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow);


            sPCPath = drvPCPath;
            
            string UnixTargetFilePath = "";
            string targetFileName = string.Empty;
            try
            {               
                switch (FileTransferAction)
                {
                    case eFileTransferAction.GetFile:


                        try
                        {
                            if (!VerifyFTPConnected())
                            {
                                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                this.Error = "Failed to login!";
                                return;
                            }
                            if (UnixFTPClient.Exists(drvUnixPath.Replace("~/", workdir + "/")))
                            {
                                UnixTargetFilePath = drvUnixPath.Replace("~/", workdir + "/");
                            }
                            else
                            {
                                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                this.Error = "'" + drvUnixPath + "' path does not exists";
                                return;
                            }

                            //if (File.Exists(act.GetInputParam("PCDir")))
                            //{
                            //    PCTargetFilePath = act.GetInputParam("PCDir");
                            //}else if (IsFile(act.GetInputParam("PCDir")))
                            //{
                            //    PCTargetFilePath = Path.GetDirectoryName(act.GetInputParam("PCDir"));
                            //}
                            //else
                            //{

                            if (sPCPath.StartsWith(@"~\"))
                            {


                                sPCPath = System.IO.Path.Combine(this.SolutionFolder, sPCPath.Remove(0, 2));
                            }
                            sPCPath=sPCPath.Replace(Environment.NewLine, "");
                            
                            //}

                            DownloadFiles(UnixTargetFilePath, sPCPath);
                            targetFileName = UnixTargetFilePath.Substring(UnixTargetFilePath.LastIndexOf('/') + 1);
                            if(!sPCPath.EndsWith(@"\"))
                            {
                                sPCPath += @"\";
                            }

                            if(!File.Exists(sPCPath+targetFileName))
                            {
                                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                this.Error = "GET File action failed on '"+ targetFileName + "' "+ sPCPath + "\\" + " directory";
                            }
                            this.ExInfo = "GetFile action Success on path "+ sPCPath;
                        }
                        catch (Exception e)
                        {
                            this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            this.Error = e.Message;
                            Reporter.ToLog(eAppReporterLogLevel.ERROR, e.Message);
                        }
                        
                        break;

                    case eFileTransferAction.PutFile:
                          try{
                              if (!VerifyFTPConnected())
                              {
                                  this.Error = "Failed to login!";
                                  return;
                              }
                            if (UnixFTPClient.Exists(drvUnixPath.Replace("~/", workdir + "/")))
                                UnixTargetFilePath = drvUnixPath.Replace("~/", workdir + "/");
                            else if (!UnixFTPClient.Exists(workdir + @"/Ginger/Uploaded"))
                            {
                                UnixFTPClient.CreateDirectory(workdir + @"/Ginger/Uploaded");
                                UnixTargetFilePath = workdir + @"/Ginger/Uploaded";
                            }


                            if (!(UnixTargetFilePath.EndsWith("/")))
                            {
                                UnixTargetFilePath +="/";
                            }

                            if (sPCPath.StartsWith(@"~\"))
                             {


                                 sPCPath = System.IO.Path.Combine(this.SolutionFolder, sPCPath.Remove(0,2));
                             }
                            if (!IsDir(sPCPath))
	                        {
                                using (var f = File.OpenRead(sPCPath))
                                {
                                    UnixFTPClient.UploadFile(f, UnixTargetFilePath + Path.GetFileName(sPCPath));
                                    PutFileOperationStatus(sPCPath, UnixTargetFilePath);
                                }
                            }
	                        else
	                        {
                                string[] fileEntries = Directory.GetFiles(sPCPath);
		                        foreach (string fileName in fileEntries)
		                        { using (var f = File.OpenRead(fileName))
			                        {
                                        UnixFTPClient.UploadFile(f, UnixTargetFilePath + Path.GetFileName(fileName), null);
                                    }
		                        }
                                PutFileOperationStatus(sPCPath, UnixTargetFilePath);
	                        }
                            this.ExInfo = "PutFile action Success on path- "+ UnixTargetFilePath;
                        }
                        catch(Exception e)
                        {
                            this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            this.Error = e.Message;
	                        Reporter.ToLog(eAppReporterLogLevel.ERROR, e.Message);
                        }
                        
                        break;

                    default:

                        break;

                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, e.Message);
            }
        }

        private void PutFileOperationStatus(string sPCPath, string UnixTargetFilePath)
        {
            string targetFileName = sPCPath.Substring(sPCPath.LastIndexOf("\\") + 1);
            if (!UnixFTPClient.Exists(UnixTargetFilePath + targetFileName))
            {
                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                this.Error = "Put File failed on '" + targetFileName + "' to " + UnixTargetFilePath + "\\";
            }
        }

        private void DownloadFiles(string remoteFile, string localPath)
        {
            /// Try catch blocked so as to catch exception inside "Execute()"  
            // var files = UnixFTPClient.ListDirectory(remoteDirectory);
            //try
            {
                var filename = "";

                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                    filename = localPath;
                }
                else if (!IsDir(localPath))
                    filename = localPath;
                else
                    filename = localPath + @"\" + Path.GetFileName(remoteFile);

                using (var f = File.OpenWrite(filename))
                {
                    UnixFTPClient.DownloadFile(remoteFile, f);
                }
            }
            //catch (Exception e)
            //{
            //    Reporter.ToLog(eLogLevel.ERROR, e.Message);
           // }

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

