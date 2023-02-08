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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using System;
using System.Drawing;
using System.Collections.Generic;
using GingerCore.Helpers;
using System.IO;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;

namespace GingerCore.Actions
{

    public class ActScreenShot : Act
    {
        public override string ActionDescription { get { return "Screen Shot Action"; } }
        public override string ActionUserDescription { get { return "Takes screen shot"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to automate screen shot of page.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To use this action,select Locate By and locate value and enter folder name where you want to save the screen shot by click on browse buttons");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To use this action, please provide the folder path where do you want to save screenshots.");
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

        

        public override String ActionType
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

        public override void PostExecute()
        {
            if (string.IsNullOrEmpty(SaveToFileName))
            {
                Error = "Missing path to save the screen shot into";
                return;
            }


            string DirectoryPath = SaveToFileName;

            if (DirectoryPath.StartsWith(@"~\"))
            {
                DirectoryPath = Path.Combine(SolutionFolder, DirectoryPath.Replace(@"~\", string.Empty));
            }

            if (!Directory.Exists(DirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(DirectoryPath);
                }
                catch (Exception ex)
                {
                    this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    Error = string.Concat("Invalid Folder Path. :", DirectoryPath);
                    throw ex;
                }

            }

            String FileName = this.Description;
            String timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");

            FileName += timeStamp;


            if (ScreenShots.Count == 0)
            {
                Error = "Failed to capture screen shots to save";
                return;
            }

            //TODO: make it simple file copy + change var to folder to save not file name
            ObservableList<Bitmap> Bitmp = new ObservableList<Bitmap>();
            foreach (String path in ScreenShots)
            {
                Bitmap bmp = Ginger.Utils.BitmapManager.FileToBitmapImage(path);
                Bitmp.Add(bmp);
            }
            
            Dictionary<string, object> outFilePath = new Dictionary<string, object>();

            foreach (Bitmap Bitmap in Bitmp)
            {
                using (Bitmap)
                {
                    string filePath = "";
                    string indexBitmp="";
                    if (Bitmp.IndexOf(Bitmap) == 0)
                    {
                        filePath += DirectoryPath + @"\" + FileName + ".jpg";
                        Bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    else
                    {
                        int i = Bitmp.IndexOf(Bitmap);
                        indexBitmp = i.ToString();
                        filePath += DirectoryPath + @"\" + FileName + "_" + i.ToString() + ".jpg";
                        Bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }

                    outFilePath.Add("ScreenshotFilePath"+indexBitmp, filePath);
                }
            }

            this.AddToOutputValues(outFilePath);

        }
    }
}
