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
using Amdocs.Ginger.Common.UIElement;
using Applitools.Selenium;
using GingerCore.Actions.VisualTesting;
using GingerCore.Drivers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace GingerCore.Actions
{
    public class ActVisualTesting : Act {
        public override string ActionDescription { get { return "Visual Testing"; } }
        public override string ActionUserDescription { get { return "Add visual testing checkpoint"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH) {
            TBH.AddText("Use this action to add Visual Testing which can compare bitmap or screen shots");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To Perform Visual Testing action, Select a Visual Testing Engine and set a Baseline Image that will be compared against dynamic screenshot or image file.");
        }

        public override eImageType Image { get { return eImageType.Visible; } }         //eImageType.Visible => FontAwesomeIcon.Eye

        public enum eVisualTestingAnalyzer {
            [EnumValueDescription("ImageMagick Analyzer")]
            BitmapPixelsComparison,
            [EnumValueDescription("UI Elements Analyzer")]
            UIElementsComparison,
            //[EnumValueDescription("OCR Analyzer - Not Implemented")]
            //OCRComparison,
            [EnumValueDescription("Applitools  Analyzer")]
            Applitools,
            //[EnumValueDescription("Blink Diff - Not Implemented")]
            //BlinkDiff,
            //[EnumValueDescription("Spell Check Analyzer - Not Implemented")]
            //Spellcheck,
            [EnumValueDescription("Visual Regression Tracker")]
            VRT
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

        //TODO: For ActComparepage ObjectLocatrosConfigNeeded is false 
        //But still for Switch frame , intialize browser etc the locate by and locate value is binded with Act.cs LocateBy and LocateValue fields
        //We override this field to ignore ObjectConfigNeeded check only for this action
        //Need to remove all of this once restructring Act.cs
        public override eLocateBy LocateBy
        {
            get
            {
                return GetOrCreateInputParam<eLocateBy>(Act.Fields.LocateBy, eLocateBy.NA);
            }
            set
            {
                AddOrUpdateInputParamValue(Act.Fields.LocateBy, value.ToString());
                OnPropertyChanged(Act.Fields.LocateBy);
                OnPropertyChanged(Act.Fields.Details);
            }
        }

        public override string LocateValue
        {
            get
            {
                return GetOrCreateInputParam(Act.Fields.LocateValue).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(Act.Fields.LocateValue, value);
                OnPropertyChanged(Act.Fields.LocateValue);
                OnPropertyChanged(Act.Fields.Details);
            }
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

        public bool IsFullPageScreenshot
        {
            get
            {
                bool value = true;
                bool.TryParse(GetOrCreateInputParam(nameof(IsFullPageScreenshot), value.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(IsFullPageScreenshot), value.ToString());
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
            public static string ServerUrl = "ServerUrl";
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
            public static readonly string ActionBy = "ActionBy";
            public static readonly string LocateBy = "LocateBy";
            public static readonly string LocateValue ="LocateValue";
        }
        
        public override string ActionEditPage { get { return "VisualTesting.ActVisualTestingEditPage"; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms {
            get {
                if (mPlatforms.Count == 0) {
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Mobile);
                    mPlatforms.Add(ePlatformType.Java);
                    mPlatforms.Add(ePlatformType.Windows);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                    //mPlatforms.Add(ePlatformType.AndroidDevice);
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
            mVisualAnalyzer = ((DriverBase)mDriver).GetVisualAnalyzer(VisualTestingAnalyzer);

            if (mDriver is SeleniumDriver)
            {
                if(!CheckSetAppWindowSize())
                {
                    return;
                }
            }
            if (Amdocs.Ginger.Common.Context.GetAsContext(Context).Activity == null)
            {
                Amdocs.Ginger.Common.Context.GetAsContext(Context).Activity = ((Drivers.DriverBase)mDriver).BusinessFlow.CurrentActivity;
                Amdocs.Ginger.Common.Context.GetAsContext(Context).BusinessFlow = ((Drivers.DriverBase)mDriver).BusinessFlow;
            }
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

        public bool CheckSetAppWindowSize()
        {
            switch (ChangeAppWindowSize)
            {
                case eChangeAppWindowSize.None:
                    break;
                case eChangeAppWindowSize.Maximized:
                    mDriver.ChangeAppWindowSize(0,0);
                    break;
                case eChangeAppWindowSize.Custom:
                    mDriver.ChangeAppWindowSize(Convert.ToInt32(GetInputParamCalculatedValue(nameof(SetAppWindowWidth))), Convert.ToInt32(GetInputParamCalculatedValue(nameof(SetAppWindowHeight))));
                    Size size = mDriver.GetWebDriver().Manage().Window.Size;
                    if (Convert.ToInt32(GetInputParamCalculatedValue(nameof(SetAppWindowWidth))) + 5  < size.Width)//+5 added to check with actual viewport/size of the browser which can be different by 2 0r 3 points
                    {
                        this.Error = string.Format("Unable to set custom width of web page to {0}, min supported width is {1}.", GetInputParamCalculatedValue(nameof(SetAppWindowWidth)), size.Width.ToString());
                        mDriver.ChangeAppWindowSize(0, 0);
                        return false;
                    }
                    else if (Convert.ToInt32(GetInputParamCalculatedValue(nameof(SetAppWindowWidth))) > size.Width)
                    {
                        this.Error = string.Format("Unable to set custom width of web page to {0}, max supported width is {1}.", GetInputParamCalculatedValue(nameof(SetAppWindowWidth)), size.Width.ToString());
                        mDriver.ChangeAppWindowSize(0, 0);
                        return false;
                    }
                    else if (Convert.ToInt32(GetInputParamCalculatedValue(nameof(SetAppWindowHeight))) + 5 < size.Height)//+5 added to check with actual viewport/size of the browser which can be different by 2 0r 3 points
                    {
                        this.Error = string.Format("Unable to set custom width of web page to {0}, min supported height is {1}.", GetInputParamCalculatedValue(nameof(SetAppWindowHeight)), size.Height.ToString());
                        mDriver.ChangeAppWindowSize(0, 0);
                        return false;
                    }
                    else if (Convert.ToInt32(GetInputParamCalculatedValue(nameof(SetAppWindowHeight))) > size.Height)
                    {
                        this.Error = string.Format("Unable to set custom width of web page to {0}, max supported height is {1}.", GetInputParamCalculatedValue(nameof(SetAppWindowHeight)), size.Height.ToString());
                        mDriver.ChangeAppWindowSize(0, 0);
                        return false;
                    }
                    break;
                case eChangeAppWindowSize.Resolution640x480:
                    mDriver.ChangeAppWindowSize(640, 480);
                    break;
                case eChangeAppWindowSize.Resolution800x600:
                    mDriver.ChangeAppWindowSize(800, 600);
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
            return true;
        }

        public List<int> GetWindowResolution()
        {
            List<int> Resolution = new List<int>();
            switch (ChangeAppWindowSize)
            {
                case eChangeAppWindowSize.None:
                    Resolution.Add(0);
                    Resolution.Add(0);
                    return Resolution;
                case eChangeAppWindowSize.Maximized:
                    Resolution.Add(0);
                    Resolution.Add(0);
                    return Resolution;
                case eChangeAppWindowSize.Custom:
                    //TODO:
                    Resolution.Add(Convert.ToInt32(GetInputParamCalculatedValue(ActVisualTesting.Fields.SetAppWindowWidth)));
                    Resolution.Add(Convert.ToInt32(GetInputParamCalculatedValue(ActVisualTesting.Fields.SetAppWindowHeight)));
                    return Resolution;
                case eChangeAppWindowSize.Resolution640x480:
                    Resolution.Add(640);
                    Resolution.Add(480);
                    return Resolution;
                case eChangeAppWindowSize.Resolution800x600:
                    Resolution.Add(800);
                    Resolution.Add(600);
                    return Resolution;
                case eChangeAppWindowSize.Resolution1024x768:
                    Resolution.Add(1024);
                    Resolution.Add(768);
                    return Resolution;
                case eChangeAppWindowSize.Resolution1280x800:
                    Resolution.Add(1280);
                    Resolution.Add(800);
                    return Resolution;
                case eChangeAppWindowSize.Resolution1280x1024:
                    Resolution.Add(1280);
                    Resolution.Add(1024);
                    return Resolution;
                case eChangeAppWindowSize.Resolution1366x768:
                    Resolution.Add(1366);
                    Resolution.Add(768);
                    return Resolution;
                case eChangeAppWindowSize.Resolution1920x1080:
                    Resolution.Add(1920);
                    Resolution.Add(1080);
                    return Resolution;
                default:
                    Resolution.Add(0);
                    Resolution.Add(0);
                    return Resolution;
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
                targetImage = mDriver.GetScreenShot(null, IsFullPageScreenshot);                
            }
            else
            {
                // get it from the file
                string fullTargetFilePath = GetFullFilePath(TargetFileName);
                targetImage = new Bitmap(fullTargetFilePath);
            }

            //TODO: add code to validate we have valid base /target images, if not put err and info in ExInfo

            // TODO: check basic: size diff writye to output param, and other general params

            AddScreenShot((Bitmap)baseImage.Clone(), "Baseline Image");
            AddScreenShot((Bitmap)targetImage.Clone(), "Target Image");

            // Here we call the actual analyzer after everything is prepared
            mVisualAnalyzer.SetAction(mDriver, this);
            mVisualAnalyzer.Compare();

            //Add other info to output params
            AddImageInfo("Baseline image", baseImage);
            AddImageInfo("Target image", targetImage);
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
            mVisualAnalyzer = ((DriverBase)mDriver).GetVisualAnalyzer(VisualTestingAnalyzer);
            mVisualAnalyzer.SetAction(driver, this);
            mVisualAnalyzer.CreateBaseline();
        }

        public void TakeScreenShotforBaseline(IVisualTestingDriver driver)
        {
            mDriver = driver;
            //TODO: verify we have driver            
            // get updated screen shot
            baseImage = mDriver.GetScreenShot(null, IsFullPageScreenshot);
            
            //Verify we have screenshots folder
            string SAVING_PATH = System.IO.Path.Combine(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.Folder, @"Documents\ScreenShots\");
            if (!Directory.Exists(SAVING_PATH)) 
            {
                Directory.CreateDirectory(SAVING_PATH); 
            }

            // Create default file name if not exist
            if (string.IsNullOrEmpty(BaseLineFileName))
            {
                BaseLineFileName = @"~\Documents\ScreenShots\" + Description + " - Baseline.png";
            }

            string FullPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(BaseLineFileName);

            // no need to ask user, + it might be at run time
            if (File.Exists(FullPath))
            {
                try
                {
                    File.Delete(FullPath);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                }
            }
            baseImage.Save(FullPath);
        }

        // TODO: move from here to general or use general
        public string GetFullFilePath(string relativePath)
        {
            relativePath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(relativePath);

            return relativePath;
        }
    }
}