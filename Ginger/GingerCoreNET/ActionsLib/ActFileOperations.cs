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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Applitools.Utils;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//This class is for dummy act - good for agile, and to be replace later on when real
//  act is available, so tester can write the step to be.
namespace GingerCore.Actions
{
    public class ActFileOperations : ActWithoutDriver
    {
        public override string ActionDescription { get { return "File Operations"; } }
        public override eImageType Image { get { return eImageType.File; } }
        public override string ActionUserDescription { get { return "Perform File operations like Check if File Exists, Execute a file "; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to perform File operations ");

        }
        public new static partial class Fields
        {
            public static string SourceFilePath = "SourceFilePath";
            public static string FileOperationMode = "FileOperationMode";
            public static string DestinationFolder = "DestinationFolder";
        }

        public override string ActionEditPage { get { return "ActFileOperationEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public enum eFileoperations
        {
            CheckFileExists,
            CheckFolderExists,
            Execute,
            Copy,
            Move,
            ForceCopy,
            RunCommand,
            UnZip,
            DeleteDirectoryFiles,
            DeleteDirectory
        }

        public eFileoperations FileOperationMode
        {
            get
            {
                return GetOrCreateInputParam<eFileoperations>(nameof(FileOperationMode), eFileoperations.CheckFileExists);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FileOperationMode), value.ToString());
            }
        }

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

        public override String ActionType
        {
            get
            {
                return "File Operations";
            }
        }

        public string Arguments
        {
            get
            {
                return GetInputParamCalculatedValue(nameof(Arguments));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(Arguments), value);
            }
        }

        string calculatedSourceFilePath;
        string DestinationFolder;
        string DestinationFile;

        public override void Execute()
        {
            string FileText = String.Empty;


            calculatedSourceFilePath = GetInputParamCalculatedValue(Fields.SourceFilePath);

            bool IsSorcePathRelative = false;
            if (calculatedSourceFilePath.StartsWith(@"~"))
            {
                calculatedSourceFilePath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(calculatedSourceFilePath);
                IsSorcePathRelative = true;
            }

            if (calculatedSourceFilePath.EndsWith("*"))
            {
                string[] FileNameList = System.IO.Directory.GetFiles(Path.GetDirectoryName(calculatedSourceFilePath), Path.GetFileName(calculatedSourceFilePath));

                if (FileNameList.Any())
                {
                    calculatedSourceFilePath = System.IO.Directory.GetFiles(Path.GetDirectoryName(calculatedSourceFilePath), Path.GetFileName(calculatedSourceFilePath))[0];
                }
            }
            try
            {
                switch (FileOperationMode)
                {
                    case eFileoperations.CheckFileExists:
                        if (!System.IO.File.Exists(calculatedSourceFilePath))
                        {
                            base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            base.ExInfo = "File doesn't exists";
                            return;
                        }
                        break;

                    case eFileoperations.CheckFolderExists:
                        if (!System.IO.Directory.Exists(calculatedSourceFilePath))
                        {
                            base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            base.ExInfo = "Folder doesn't exists";
                            return;
                        }

                        break;
                    case eFileoperations.DeleteDirectoryFiles:
                        if (!System.IO.Directory.Exists(calculatedSourceFilePath))
                        {
                            base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            base.ExInfo = "Folder doesn't exists";
                            return;
                        }
                        foreach (string file in System.IO.Directory.GetFiles(calculatedSourceFilePath))
                        {
                            System.IO.File.Delete(file);
                        }
                        break;
                    case eFileoperations.DeleteDirectory:
                        if (!IsLinuxPath(calculatedSourceFilePath))
                        {
                            base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            base.ExInfo = "Path is not valid";
                            return;
                        }
                        if (!System.IO.Directory.Exists(calculatedSourceFilePath))
                        {
                            base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            base.ExInfo = "Directory doesn't exist";
                            return;
                        }
                        System.IO.Directory.Delete(calculatedSourceFilePath, recursive: true);
                        break;
                    case eFileoperations.Copy:
                        SetupDestinationFolder();
                        if (System.IO.File.Exists(calculatedSourceFilePath))
                        {
                            if (System.IO.Directory.Exists(DestinationFolder))
                            {
                                if (DestinationFile.Length > 0)
                                {
                                    System.IO.File.Copy(calculatedSourceFilePath, Path.Combine(DestinationFolder, DestinationFile));
                                }
                                else
                                {
                                    System.IO.File.Copy(calculatedSourceFilePath, Path.Combine(DestinationFolder, Path.GetFileName(calculatedSourceFilePath)));
                                }
                            }
                            else
                            {
                                base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                base.Error = "Destination folder not found";
                                base.ExInfo = "Destination folder not found";
                                return;
                            }
                        }
                        else
                        {
                            base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            base.ExInfo = "File doesn't exists";
                            base.Error = "File doesn't exists";

                            return;
                        }
                        break;
                    case eFileoperations.ForceCopy:
                        SetupDestinationFolder();
                        if (System.IO.File.Exists(calculatedSourceFilePath))
                        {
                            if (!System.IO.Directory.Exists(DestinationFolder))
                            {

                                System.IO.Directory.CreateDirectory(DestinationFolder);
                            }
                            System.IO.File.Copy(calculatedSourceFilePath, Path.Combine(DestinationFolder, Path.GetFileName(calculatedSourceFilePath)), true);
                        }
                        else
                        {
                            base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            base.ExInfo = "File doesn't exists";
                            base.Error = "File doesn't exists";
                            return;
                        }
                        break;
                    case eFileoperations.Move:
                        SetupDestinationFolder();
                        if (IsSorcePathRelative)
                        {
                            base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            base.ExInfo = "You cannot move a file from Solution";
                            base.Error = "You cannot move a file from Solution";
                            return;
                        }
                        else if (System.IO.File.Exists(calculatedSourceFilePath))
                        {
                            if (!System.IO.Directory.Exists(DestinationFolder))
                            {

                                base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                base.ExInfo = "Destination Folder doesn't exists ";
                                base.Error = "Destination Folder doesn't exists";
                                return;
                            }
                            string fileName = string.Empty;
                            if (!string.IsNullOrEmpty(DestinationFile))
                            {
                                fileName = DestinationFile;
                            }
                            else
                            {
                                fileName = Path.GetFileName(calculatedSourceFilePath);
                            }
                            System.IO.File.Move(calculatedSourceFilePath, Path.Combine(DestinationFolder, fileName));
                        }
                        else
                        {
                            base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            base.ExInfo = "File doesn't exists";
                            base.Error = "File doesn't exists";
                            return;
                        }
                        break;
                    case eFileoperations.RunCommand:
                    case eFileoperations.Execute:
                        if (!string.IsNullOrEmpty(Arguments))
                        {
                            ProcessStart(calculatedSourceFilePath, Arguments);
                        }
                        else
                        {
                            int spaceIndex = calculatedSourceFilePath.IndexOf(' ');
                            //For Backward Support - Providing structure like: file Path + ' ' + arguments
                            if (spaceIndex != -1 && System.IO.File.Exists(calculatedSourceFilePath[..(spaceIndex + 1)]))
                            {
                                ProcessStart(calculatedSourceFilePath[..(spaceIndex + 1)], calculatedSourceFilePath[spaceIndex..]);
                            }
                            else
                            {
                                ProcessStart(calculatedSourceFilePath);
                            }
                        }
                        break;
                    case eFileoperations.UnZip:
                        SetupDestinationFolder();
                        if (!calculatedSourceFilePath.ToLower().EndsWith(".zip"))
                        {
                            base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            base.ExInfo = "Not a valid Zip File";
                            base.Error = "Not a valid Zip File";
                            return;
                        }

                        if (System.IO.File.Exists(calculatedSourceFilePath))
                        {
                            if (!DestinationFolder.EndsWithOrdinal(Path.GetFileNameWithoutExtension(calculatedSourceFilePath)))
                            {
                                DestinationFolder = Path.Combine(DestinationFolder, Path.GetFileNameWithoutExtension(calculatedSourceFilePath));
                            }
                            if (!System.IO.Directory.Exists(DestinationFolder))
                            {
                                System.IO.Directory.CreateDirectory(DestinationFolder);
                            }
                            System.IO.Compression.ZipFile.ExtractToDirectory(calculatedSourceFilePath, DestinationFolder);
                        }

                        else
                        {
                            base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            base.ExInfo = "File doesn't exists";
                            base.Error = "File doesn't exists";
                            return;
                        }
                        break;

                    default:
                        break;

                }
            }
            catch (Exception ex)
            {
                base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                base.Error = $"{ActionType} File Operation failed due to {ex.Message}";
                Reporter.ToLog(eLogLevel.ERROR, $"{ActionType} File Operation failed", ex);
            }
        }

        private void ProcessStart(string fileName, string arguments = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(arguments))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = fileName, Arguments = Arguments, UseShellExecute = true });
                }
                else
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = fileName, UseShellExecute = true });
                }

            }
            catch (Exception e)
            {
                base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                if (e.Message == "The system cannot find the file specified")
                {
                    base.ExInfo = "File at path " + fileName + " doesn't exists";
                    base.Error = "File doesn't exists";
                }
                else
                {
                    base.ExInfo = e.Message;
                    base.Error = $"Failed to run File Operation on file {fileName}";
                }
            }
        }

        private void SetupDestinationFolder()
        {
            string calculatedDestinationPath = GetInputParamCalculatedValue(Fields.DestinationFolder);
            calculatedDestinationPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(calculatedDestinationPath);
            //DestinationFolder = Path.GetDirectoryName(calculatedDestinationPath);
            if (!string.IsNullOrEmpty(calculatedDestinationPath))
            {
                if (Directory.Exists(calculatedDestinationPath))
                {
                    DestinationFolder = calculatedDestinationPath;
                    if (!Path.EndsInDirectorySeparator(DestinationFolder))
                    {
                        DestinationFolder += Path.DirectorySeparatorChar;
                    }
                    DestinationFile = Path.GetFileName(DestinationFolder);
                }
                else
                {
                    DestinationFolder = Path.GetDirectoryName(calculatedDestinationPath);
                    DestinationFile = Path.GetFileName(calculatedDestinationPath);
                }

            }
            else
            {
                base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                base.ExInfo = "Provide a valid Destination Path";
                base.Error = "Provide a valid Destination Path";
                return;
            }
        }
        public override bool SerializationError(SerializationErrorType errorType, string name, string value)
        {
            if (errorType == SerializationErrorType.PropertyNotFound && (name == Fields.SourceFilePath || name == Fields.DestinationFolder))
            {
                return true;
            }
            return false;
        }
        public bool IsLinuxPath(string path) //Method checking valid linux path
        {
            char[] invalidPathChars = Path.GetInvalidPathChars();

            if (Environment.OSVersion.Platform is PlatformID.Unix or PlatformID.MacOSX)
            {
                // Linux or macOS platform, check for valid Linux path characters
                foreach (char c in path)
                {
                    if (invalidPathChars.Contains(c))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                // Non-Linux OS, assuming path is valid
                return true;
            }
        }
    }
}
