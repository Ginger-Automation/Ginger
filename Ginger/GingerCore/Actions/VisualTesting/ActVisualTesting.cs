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
using GingerCore.Actions.VisualTesting;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GingerCore.Actions
{
    public class ActVisualTesting : Act {
        public override string ActionDescription { get { return "Visual Testing"; } }
        public override string ActionUserDescription { get { return "Add visual testing checkpoint"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH) {
            TBH.AddText("Use this action to add Visual Testing which can compare bitmap or screen shots");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To Perform Visual Testing action, Select a Visual Testing Engine and set a Baseline Image that will be compared against dynamic screenshot or image file.");
        }

        public override System.Drawing.Image Image { get { return Resources.VisualTesting_16x16 ; } }

        public enum eVisualTestingAnalyzer {
            [EnumValueDescription("ImageMagick Analyzer")]
            BitmapPixelsComparison,
            [EnumValueDescription("UI Elements Analyzer")]
            UIElementsComparison,
            [EnumValueDescription("OCR Analyzer - Not Implemented")]
            OCRComparison,
            [EnumValueDescription("Applitools  Analyzer")]
            Applitools,
            [EnumValueDescription("Blink Diff - Not Implemented")]
            BlinkDiff,
            [EnumValueDescription("Spell Check Analyzer - Not Implemented")]
            Spellcheck,
        }

        public enum eChangeAppWindowSize
        {
            [EnumValueDescription("")]
            None,
            [EnumValueDescription("Maximized")]
            Maximized,
            [EnumValueDescription("Custom")]
            Custom,
            [EnumValueDescription("640 x 480")]
            Resolution640x480,
            [EnumValueDescription("800 x 600")]
            Resolution800x600,
            [EnumValueDescription("1024 x 768")]
            Resolution1024x768,
            [EnumValueDescription("1280 x 800")]
            Resolution1280x800,
            [EnumValueDescription("1280 x 1024")]
            Resolution1280x1024,
            [EnumValueDescription("1366 x 768")]
            Resolution1366x768,
            [EnumValueDescription("1920 x 1080")]
            Resolution1920x1080,
        }

        //TODO: is needed here? - add to all? or get from visual driver?
        public enum eApplitoolsParamBrowser {
            [EnumValueDescription("Chrome")]
            Chrome = 0,
            [EnumValueDescription("Firefox")]
            Firefox = 1,
            [EnumValueDescription("Internet Explorer")]
            InternetExplorer = 2
        }

        IVisualTestingDriver mDriver;
        public eVisualTestingAnalyzer VisualTestingAnalyzer
        {
            get
            {
                eVisualTestingAnalyzer eVal = eVisualTestingAnalyzer.BitmapPixelsComparison;
                if (Enum.TryParse<eVisualTestingAnalyzer>(GetInputParamValue(Fields.VisualAnalyzer), out eVal))
                    return eVal;
                else
                    return eVisualTestingAnalyzer.BitmapPixelsComparison; //Default value
            }

            set
            {
                GetOrCreateInputParam(Fields.VisualAnalyzer).Value = value.ToString();
            }
        }

        public eChangeAppWindowSize ChangeAppWindowSize
        {
            get
            {
                eChangeAppWindowSize eVal = eChangeAppWindowSize.None;
                if (Enum.TryParse<eChangeAppWindowSize>(GetInputParamValue(Fields.ChangeAppWindowSize), out eVal))
                    return eVal;
                else
                    return eChangeAppWindowSize.None;
            }

            set
            {
                GetOrCreateInputParam(Fields.ChangeAppWindowSize).Value = value.ToString();
            }
        }

        public string BaseLineFileName
        {
            get
            { return this.GetOrCreateInputParam(Fields.SavedBaseImageFilenameString).Value; }
            set { this.GetOrCreateInputParam(Fields.SavedBaseImageFilenameString).Value = value; }
        }

        public string TargetFileName
        {
            get
            { return this.GetOrCreateInputParam(Fields.SavedTargetImageFilenameString).Value; }
            set { this.GetOrCreateInputParam(Fields.SavedTargetImageFilenameString).Value = value; }
        }

        public bool CreateBaselineAction
        {
            get
            { return this.GetOrCreateInputParam(Fields.CreateBaselineAction).BoolValue; }
            set { this.GetOrCreateInputParam(Fields.CreateBaselineAction).BoolValue = value; }
        }

        public int SetAppWindowWidth
        {
            get
            { return this.GetOrCreateInputParam(ActVisualTesting.Fields.SetAppWindowWidth).IntValue; }
            set { this.GetOrCreateInputParam(ActVisualTesting.Fields.SetAppWindowWidth).IntValue = value; }
        }

        public int SetAppWindowHeight
        {
            get
            { return this.GetOrCreateInputParam(ActVisualTesting.Fields.SetAppWindowHeight).IntValue; }
            set { this.GetOrCreateInputParam(ActVisualTesting.Fields.SetAppWindowHeight).IntValue = value; }
        }

        public string BaselineInfoFile
        {
            get
            { return this.GetOrCreateInputParam(ActVisualTesting.Fields.BaselineInfoFile).Value; }
            set { this.GetOrCreateInputParam(ActVisualTesting.Fields.BaselineInfoFile).Value = value; }
        }

        //TODO: clean unused fields
        public new static partial class Fields {
            //used
            public static string SavedBaseImageFilenameString = "SavedBaseImageFilenameString";
            public static string SavedTargetImageFilenameString = "SavedTargetImageFilenameString";

            //check
            public static string BaselineInfoFile = "BaselineInfoFile";
            public static string SavedApplitoolsBaseFilenameString = "SavedApplitoolsBaseFilenameString";
            
            public static string VisualAnalyzer = "VisualAnalyzer";
            public static string SavedBaselineImageManager = "SavedBaselineImageManager";
            public static string ApplitoolsKey = "ApplitoolsKey";
            public static string CrossEnvironmentTest = "CrossEnvironmentTest";
            public static string IsTargetImageFromScreenShot = "IsTargetImageFromScreenShot";
            public static string IsTargetFromStaticImage = "IsTargetFromStaticImage";
            public static string ApplitoolsParamApplicationName = "ApplitoolsParamApplicationName";
            public static string ApplitoolsParamTestName = "ApplitoolsParamTestName";
            public static string ApplitoolsParamScreenResolution = "ApplitoolsParamScreenResolution";
            public static string ApplitoolsParamBrowser = "ApplitoolsParamBrowser";
            public static string ThresholdPercentageValue = "ThresholdPercentageValue";
            public static string ActDescription = "ActDescription";
            public static string CompareResult = "CompareResult";
            public static string BaseImageFilePath = "BaseImageFilePath";
            public static string ChangeAppWindowSize = "ChangeAppWindowSize";
            public static string SetAppWindowWidth = "SetAppWindowWidth";
            public static string SetAppWindowHeight = "SetAppWindowHeight";
            public static string BaseLineVisualElementsInfoFileName = "BaseLineVisualElementsInfoFileName";            
            public static string CreateBaselineAction = "CreateBaselineAction";
            public static string ErrorMetric = "ErrorMetric";
        }
        
        public override string ActionEditPage { get { return "VisualTesting.ActVisualTestingEditPage"; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms {
            get {
                if (mPlatforms.Count == 0) {
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Mobile);
                    mPlatforms.Add(ePlatformType.Windows);
                    mPlatforms.Add(ePlatformType.AndroidDevice);
                }
                return mPlatforms;
            }
        }

        IVisualAnalyzer mVisualAnalyzer = null;

        public Bitmap baseImage;  //TODO: use screenshots[0]   - create get set
        public Bitmap targetImage;  // TODO: use screenshots[1] - create get set

        //TODO: better convert to enum - maybe support other targets??
        // Screen shot, file, url

        public override String ActionType {
            get {
                return "Visual Testing Element: "; 
            }
        }

        private object mCompareResult;

        // easy wait to know if it's screen shot, if no target file then it is screen shot
        public bool IsTargetSourceFromScreenshot { get { return string.IsNullOrEmpty(TargetFileName); } }

        public object CompareResult
        {
            get
            {
                return mCompareResult;
            }
            set
            {
                if (mCompareResult != value)
                {
                    mCompareResult = value;
                    OnPropertyChanged(Fields.CompareResult);
                }
            }
        }

        public void Execute(IVisualTestingDriver driver)
        {
            mDriver = driver;
            CheckSetVisualAnalyzer();
            CheckSetAppWindowSize();

            if (mVisualAnalyzer.SupportUniqueExecution())
            {
                mVisualAnalyzer.SetAction(mDriver, this);
                mVisualAnalyzer.Execute();
            }
            else
            {
                if (CreateBaselineAction)
                {
                    CreateBaseline(mDriver);
                }
                else
                {
                    Compare();
                }
            }
        }

        public void CheckSetAppWindowSize()
        {
            switch (ChangeAppWindowSize)
            {
                case eChangeAppWindowSize.None:
                    break;
                case eChangeAppWindowSize.Maximized:
                    mDriver.ChangeAppWindowSize(0,0);
                    break;
                case eChangeAppWindowSize.Custom:
                    //TODO:
                    break;
                case eChangeAppWindowSize.Resolution640x480:
                    mDriver.ChangeAppWindowSize(640, 480);
                    break;
                case eChangeAppWindowSize.Resolution800x600:
                    mDriver.ChangeAppWindowSize(800, 800);
                    break;
                case eChangeAppWindowSize.Resolution1024x768:
                    mDriver.ChangeAppWindowSize(1024, 768);
                    break;
                case eChangeAppWindowSize.Resolution1280x800:
                    mDriver.ChangeAppWindowSize(1280, 800);
                    break;
                case eChangeAppWindowSize.Resolution1280x1024:
                    mDriver.ChangeAppWindowSize(1280, 1024);
                    break;
                case eChangeAppWindowSize.Resolution1366x768:
                    mDriver.ChangeAppWindowSize(1366, 768);
                    break;
                case eChangeAppWindowSize.Resolution1920x1080:
                    mDriver.ChangeAppWindowSize(1920, 1080);
                    break;                
            }
        }

        private void Compare()
        {
            // All compares are done on: baseImage vs. targetImage - they are being set before calling engine comapre 
            // compare image or results saved in object this.CompareResult - can be image or text or other

            // ----------------------------------------------------------------
            // set base image
            // ----------------------------------------------------------------

            //TODO: need to work with value for driver
            string baselinefilename = GetInputParamValue(Fields.SavedBaseImageFilenameString);
            string fullBaseFilePath = GetFullFilePath(baselinefilename);

            if (!File.Exists(fullBaseFilePath))
            {
                Error = "Baseline file not found - " + fullBaseFilePath;
                return;
            }

            baseImage = new Bitmap(fullBaseFilePath);

            // ----------------------------------------------------------------
            // set target image
            // ----------------------------------------------------------------

            // if the target file name is empty then we take screen shot, else we take the file
            if (string.IsNullOrEmpty(TargetFileName) )
            {                
                targetImage = mDriver.GetScreenShot();                
            }
            else
            {
                // get it from the file
                string fullTargetFilePath = GetFullFilePath(TargetFileName);
                targetImage = new Bitmap(fullTargetFilePath);
            }

            //TODO: add code to validate we have valid base /target images, if not put err and info in ExInfo

            // TODO: check basic: size diff writye to output param, and other general params

            AddScreenShot(baseImage, "Baseline Image");
            AddScreenShot(targetImage, "Target Image");
            
            CheckSetVisualAnalyzer();

            // Here we call the actual analyzer after everything is prepared
            mVisualAnalyzer.SetAction(mDriver, this);
            mVisualAnalyzer.Compare();

            //Add other info to output params

            AddImageInfo("Baseline image", baseImage);
            AddImageInfo("Target image", targetImage);            
        }

        private void CheckSetVisualAnalyzer()
        {
            //Check what kind of comparison we have - Applitools, simple Bitmap or Elements comparison
            switch (VisualTestingAnalyzer)
            {
                case eVisualTestingAnalyzer.BitmapPixelsComparison:
                    if (mVisualAnalyzer is MagickAnalyzer) return;
                    mVisualAnalyzer = new MagickAnalyzer();
                    break;

                case eVisualTestingAnalyzer.Applitools:
                    if (mVisualAnalyzer is ApplitoolsAnalyzer) return;
                    mVisualAnalyzer = new ApplitoolsAnalyzer();
                    break;

                case eVisualTestingAnalyzer.UIElementsComparison:
                    if (mVisualAnalyzer is UIElementsAnalyzer) return;
                    mVisualAnalyzer = new UIElementsAnalyzer();
                    break;
            }
        }

        private void AddImageInfo(string txt, Bitmap image)
        {
            AddOrUpdateReturnParamActual(txt + " Width", image.Width + "");
            AddOrUpdateReturnParamActual(txt + " Height", image.Height + "");
            AddOrUpdateReturnParamActual(txt + " HorizontalResolution", image.HorizontalResolution + "");
            AddOrUpdateReturnParamActual(txt + " VerticalResolution", image.VerticalResolution + "");
        }

        public void CreateBaseline(IVisualTestingDriver driver)
        {
            mDriver = driver;

            CheckSetAppWindowSize();
            TakeScreenShotforBaseline(driver);
            
            // Call the actual analyzer to take/create the baseline needed
            CheckSetVisualAnalyzer();
            mVisualAnalyzer.SetAction(driver, this);
            mVisualAnalyzer.CreateBaseline();
        }

        public void TakeScreenShotforBaseline(IVisualTestingDriver driver)
        {
            mDriver = driver;
            //TODO: verify we have driver            
            // get updated screen shot
            baseImage = mDriver.GetScreenShot();
            
            //Verify we have screenshots folder
            string SAVING_PATH = System.IO.Path.Combine(SolutionFolder, @"Documents\ScreenShots\");
            if (!Directory.Exists(SAVING_PATH)) Directory.CreateDirectory(SAVING_PATH);

            // Create default file name if not exist
            if (string.IsNullOrEmpty(BaseLineFileName))
            {
                BaseLineFileName = @"~\Documents\ScreenShots\" + Description + " - Baseline.png";
            }

            string FullPath = BaseLineFileName.Replace(@"~\", SolutionFolder);

            // no need to ask user, + it might be at run time
            //TOOD: handle err 
            if (File.Exists(FullPath)) File.Delete(FullPath);

            baseImage.Save(FullPath);
        }

        // TODO: move from here to general or use general
        public string GetFullFilePath(string relativePath)
        {
            if (relativePath.StartsWith(@"~\"))
            {
                return relativePath.Replace(@"~\", SolutionFolder);
            }
            return relativePath;
        }
    }
}