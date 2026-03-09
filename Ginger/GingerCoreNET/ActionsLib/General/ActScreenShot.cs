#region License
/*
Copyright © 2014-2026 European Support Limited

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

using Amdocs.Ginger.Common.InterfacesLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GingerCore.Actions
{

    public class ActScreenShot : Act
    {
        public override string ActionDescription { get { return "Screen Shot Action"; } }
        public override string ActionUserDescription { get { return "Takes a screenshot"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Used to automate the capturing of a screenshot for a web page.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Enter the file path where you'd like to save the screenshot. Alternatively, click the browse button to choose a save location. If no filename is included, a default name will be given.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("If including a filename in your path, make sure to provide the file with the .jpeg extension.");
        }

        public override string ActionEditPage { get { return "ActScreenShotEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Windows);
                    mPlatforms.Add(ePlatformType.Mobile);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                    //mPlatforms.Add(ePlatformType.AndroidDevice);
                    mPlatforms.Add(ePlatformType.Java);
                    mPlatforms.Add(ePlatformType.ASCF);
                }
                return mPlatforms;
            }
        }

        public override string ActionType
        {
            get
            {
                return "Take Screen Shot";
            }
        }

        public new static partial class Fields
        {
            public static string SaveToFileName = "SaveToFileName";

        }

        public string SaveToFileName
        {
            get
            {
                return GetInputParamCalculatedValue("SaveToFileName");
            }
            set
            {
                AddOrUpdateInputParamValue(Fields.SaveToFileName, value);
            }
        }

        private const string DEFAULT_IMAGE_EXTENSION = ".jpeg";

        private const string TILDE_PATH_PREFIX = @"\";

        private string ConstructFilePath(string directory, string fileName, string extension, int? index = null)
        {
            string baseName = index.HasValue ? $"{fileName}_{index}" : fileName;
            return Path.Combine(directory, baseName + extension);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(SaveToFileName))
            {
                Error = "File pathname for saving the screenshot was not provided.";
                return false;
            }

            string fileExtension = Path.GetExtension(SaveToFileName);
            if (!string.IsNullOrEmpty(fileExtension) && fileExtension != DEFAULT_IMAGE_EXTENSION)
            {
                Error = $"Unsupported file extension '{fileExtension}'. Only {DEFAULT_IMAGE_EXTENSION} is allowed.";
                return false;
            }

            if (!ScreenShots.Any())
            {
                Error = "No screenshots were captured for saving.";
                return false;
            }
            return true;
        }

        private string ManageFilePath()
        {
            string directoryPath;

            if (!SaveToFileName.EndsWith(DEFAULT_IMAGE_EXTENSION) && !Path.EndsInDirectorySeparator(SaveToFileName))
            {
                SaveToFileName += Path.DirectorySeparatorChar;
                directoryPath = Path.GetDirectoryName(SaveToFileName + Path.DirectorySeparatorChar);
            }
            else
            {
                directoryPath = Path.GetDirectoryName(SaveToFileName);
            }

            if (directoryPath.StartsWith(TILDE_PATH_PREFIX))
            {
                directoryPath = Path.Combine(SolutionFolder, directoryPath.Replace(TILDE_PATH_PREFIX, string.Empty));
            }

            if (!Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                }
                catch
                {
                    Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    Error = $"The provided folder path is invalid: {directoryPath}";
                    throw;
                }
            }

            return directoryPath;
        }
        private void SaveImages(string directoryPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(SaveToFileName);
            string fileExtension = Path.GetExtension(SaveToFileName);

            if (string.IsNullOrEmpty(fileExtension))
            {
                fileName = $"{Description}{DateTime.Now:ddMMyyyyHHmmss}";
                fileExtension = DEFAULT_IMAGE_EXTENSION;
            }

            Dictionary<string, object> outputFilePaths = [];
            for (int i = 0; i < ScreenShots.Count; i++)
            {
                string sourceFileName = ScreenShots[i];
                string destFileName = ConstructFilePath(directoryPath, fileName, fileExtension, i == 0 ? null : i);

                // Copy files from existing screenshot directory to path provided by user 
                File.Copy(sourceFileName, destFileName, overwrite: true);

                string key = i == 0 ? "ScreenshotFilePath" : $"ScreenshotFilePath{i}";
                Act.AddArtifactToAction(Path.GetFileName(destFileName), this, destFileName);
                outputFilePaths.Add(key, destFileName);
            }

            AddToOutputValues(outputFilePaths);
        }

        public override void PostExecute()
        {
            try
            {
                Artifacts = [];
                if (ValidateInput())
                {
                    string directoryPath = ManageFilePath();
                    SaveImages(directoryPath);
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return;
            }
        }
    }
}
