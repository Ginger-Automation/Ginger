#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System;
using System.Collections.Generic;
using System.Linq;
using GingerCore.Helpers;
using System.IO;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
//This class is for dummy act - good for agile, and to be replace later on when real
//  act is available, so tester can write the step to be.
namespace GingerCore.Actions
{
    public class ActFileOperations : ActWithoutDriver
    {
        public override string ActionDescription { get { return "File Operations"; } }
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
            DeleteDirectoryFiles
        }

        private eFileoperations mFileOperation = eFileoperations.CheckFileExists;
        [IsSerializedForLocalRepository]
        public eFileoperations FileOperationMode
        {
            get
            {
                return mFileOperation;
            }
            set
            {
                mFileOperation = value;
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
                //calculatedSourceFilePath = calculatedSourceFilePath.Replace(@"~\", SolutionFolder);
                calculatedSourceFilePath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(calculatedSourceFilePath);
                IsSorcePathRelative = true;
            }

            if (calculatedSourceFilePath.EndsWith("*"))
            {
                string[] FileNameList = System.IO.Directory.GetFiles(Path.GetDirectoryName(calculatedSourceFilePath), Path.GetFileName(calculatedSourceFilePath));

                if (FileNameList.Count()>0)
                    calculatedSourceFilePath = System.IO.Directory.GetFiles(Path.GetDirectoryName(calculatedSourceFilePath), Path.GetFileName(calculatedSourceFilePath))[0];
            }
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
                        base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                        base.ExInfo = "Folder doesn't exists";
                        return;
                    }
                    foreach (string file in System.IO.Directory.GetFiles(calculatedSourceFilePath))
                    {
                        System.IO.File.Delete(file);
                    }
                    break;
                case eFileoperations.Copy:
                    SetupDestinationfolders();
                    if (System.IO.File.Exists(calculatedSourceFilePath))
                    {
                        if (System.IO.Directory.Exists(DestinationFolder))
                        {
                            if (DestinationFile.Length > 0) 
                            {
                                System.IO.File.Copy(calculatedSourceFilePath, Path.Combine(DestinationFolder, DestinationFile));
                            }
                            else                            {
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
                    SetupDestinationfolders();
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
                    SetupDestinationfolders();
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
                        System.IO.File.Move(calculatedSourceFilePath, Path.Combine( DestinationFolder ,Path.GetFileName(calculatedSourceFilePath)));                      
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
                            if (spaceIndex != -1 && System.IO.File.Exists(calculatedSourceFilePath.Substring(0, spaceIndex + 1)))
                                ProcessStart(calculatedSourceFilePath.Substring(0, spaceIndex + 1), calculatedSourceFilePath.Substring(spaceIndex));
                        else
                            ProcessStart(calculatedSourceFilePath);
                    }
                    break;
                case eFileoperations.UnZip:
                    SetupDestinationfolders();
                    if(!calculatedSourceFilePath.ToLower().EndsWith(".zip"))
                    {
                        base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                        base.ExInfo = "Not a valid Zip File";
                        base.Error = "Not a valid Zip File";
                        return;
                    }
                    if (System.IO.File.Exists(calculatedSourceFilePath))
                    {
                        if (!System.IO.Directory.Exists(DestinationFolder))
                        {
                            System.IO.Directory.CreateDirectory(DestinationFolder);
                        }
                        System.IO.Compression.ZipFile.ExtractToDirectory(calculatedSourceFilePath,DestinationFolder);                        
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

        private void ProcessStart(string fileName, string arguments = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(arguments))
                    System.Diagnostics.Process.Start(fileName, Arguments);
                else
                    System.Diagnostics.Process.Start(fileName);

            }catch(Exception e)
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
                    base.Error = "Failed to run File Operation on file " + fileName;
                }
            }
        }

        private void SetupDestinationfolders()
        {
            string calculatedDestinationPath = GetInputParamCalculatedValue(Fields.DestinationFolder);

            //if (calculatedDestinationPath.StartsWith(@"~\"))
            //    calculatedDestinationPath = calculatedDestinationPath.Replace(@"~\", SolutionFolder);
            calculatedDestinationPath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(calculatedDestinationPath);

            DestinationFolder = System.IO.Path.GetDirectoryName(calculatedDestinationPath);
            if (String.IsNullOrEmpty(DestinationFolder))
            {
                if(System.IO.Directory.Exists(calculatedDestinationPath))
                {
                    DestinationFolder = calculatedDestinationPath;
                    if(!DestinationFolder.EndsWith(@"\"))
                    {
                        DestinationFolder = DestinationFolder + @"\";
                    }                    
                }
            }
            DestinationFile = System.IO.Path.GetFileName(calculatedDestinationPath);

        }
        public override bool SerializationError(SerializationErrorType errorType, string name, string value)
        {
            if(errorType==SerializationErrorType.PropertyNotFound && (name==Fields.SourceFilePath || name == Fields.DestinationFolder))
            {
                return true;
            }
            return false;
        }
    }
}
