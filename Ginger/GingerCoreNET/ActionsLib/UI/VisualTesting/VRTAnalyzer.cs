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
using Amdocs.Ginger.CoreNET;
using GingerCore.Environments;
using GingerCoreNET.GeneralLib;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using VisualRegressionTracker;

namespace GingerCore.Actions.VisualTesting
{
    public class VRTAnalyzer : IVisualAnalyzer
    {
        public static string VRTAction = "VRTAction";
        public const string ActionBy = "ActionBy";
        public const string ImageNameBy = "ImageNameBy";
        public const string LocateBy = "LocateBy";
        public static string VRTParamDiffTollerancePercent = "VRTParamDiffTollerancePercent";
        public static string VRTParamBuildName = "VRTParamBuildName";
        public static string ImageName = "ImageName";
        public static string BaselineImage = "BaselineImage";
        public static string VRTSavedBaseImageFilenameString = "VRTSavedBaseImageFilenameString";


        ActVisualTesting mAct;
        IVisualTestingDriver mDriver;
        VisualRegressionTracker.VisualRegressionTracker vrt;
        VisualRegressionTracker.Config config;


        private void CreateVRTConfig()
        {
            ValueExpression VE = new ValueExpression(GetCurrentProjectEnvironment(), null);

            config = new Config
            {
                BranchName = VE.Calculate(WorkSpace.Instance.Solution.VRTConfiguration.BranchName),
                Project = VE.Calculate(WorkSpace.Instance.Solution.VRTConfiguration.Project),
                ApiUrl = VE.Calculate(WorkSpace.Instance.Solution.VRTConfiguration.ApiUrl),
                ApiKey = ValueExpression.PasswordCalculation(VE.Calculate(WorkSpace.Instance.Solution.VRTConfiguration.ApiKey)),
                EnableSoftAssert = WorkSpace.Instance.Solution.VRTConfiguration.FailActionOnCheckpointMismatch != Ginger.Configurations.VRTConfiguration.eFailActionOnCheckpointMismatch.Yes
            };
        }

        private ProjEnvironment GetCurrentProjectEnvironment()
        {
            foreach (ProjEnvironment env in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>())
            {
                if (env.Name.Equals(mDriver.GetEnvironment()))
                {
                    return env;
                }
            }

            return null;
        }

        public enum eVRTAction
        {
            [EnumValueDescription("Start Test")]
            Start,
            [EnumValueDescription("Checkpoint")]
            Track,
            [EnumValueDescription("Stop Test")]
            Stop
        }
        public enum eActionBy
        {
            Window,
            Region
        }

        public enum eImageNameBy
        {
            [EnumValueDescription("Action Name")]
            ActionName,
            [EnumValueDescription("Action Guid")]
            ActionGuid,
            [EnumValueDescription("Action Name & Guid")]
            ActionNameGUID,
            [EnumValueDescription("Custom Name")]
            Custom
        }

        public enum eBaselineImageBy
        {
            [EnumValueDescription("Create baseline from active window")]
            ActiveWindow,
            [EnumValueDescription("Image File")]
            ImageFile,
        }
        bool IVisualAnalyzer.SupportUniqueExecution()
        {
            return true;
        }

        void IVisualAnalyzer.SetAction(IVisualTestingDriver driver, ActVisualTesting act)
        {
            mDriver = driver;
            mAct = act;
        }
        public enum eMatchLevel
        {
            PixelMatch,
            LookSame,
            ODiff
        }
        public void Compare()
        {


        }

        public void CreateBaseline()
        {
            throw new NotImplementedException();
        }

        public void Execute()
        {
            switch (GetSelectedVRTActionEnum())
            {
                case eVRTAction.Start:
                    StartVRT();
                    break;

                case eVRTAction.Track:
                    TrackVRT();
                    break;

                case eVRTAction.Stop:
                    StopVRT();
                    break;
            }
        }

        private void StartVRT()
        {
            try
            {
                string buildName = mAct.GetInputParamCalculatedValue(VRTAnalyzer.VRTParamBuildName);
                if (!string.IsNullOrEmpty(buildName))
                {
                    CreateVRTConfig();
                    config.CiBuildId = buildName;
                    vrt = new VisualRegressionTracker.VisualRegressionTracker(config);
                    vrt.Start().GetAwaiter().GetResult();
                    if (!vrt.IsStarted)
                    {
                        mAct.Error = "VRT is not Started";
                        mAct.ExInfo = "Please check VRT configuration. From Configurations -> External Integrations -> VRT configurations";
                        return;
                    }
                }
                else
                {
                    mAct.Error = "VRT is not Started, Test/build name not provided.";
                    mAct.ExInfo = "Test/build name not provided.";
                }
            }
            catch (AggregateException ae)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred when StartVRT", ae);
                foreach (var e in ae.InnerExceptions)
                {
                    mAct.Error += e.Message;
                    mAct.Error += " Please check VRT configuration. From Configurations -> External Integrations -> VRT configurations";
                }
            }
            catch (Exception ex)
            {
                mAct.Error += ex.Message;
                mAct.Error += " Please check VRT configuration. From Configurations -> External Integrations -> VRT configurations";
            }
            finally
            {
            }

        }
        private void TrackVRT()
        {

            if (vrt == null || !vrt.IsStarted)
            {
                mAct.Error = "VRT is not Started";
                mAct.ExInfo = "Please include a VRT Start Action one step before the current one, if it has not been done already, and ensure that it runs before the current action.";
                return;
            }
            try
            {
                //get Image
                Image image;
                if (mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ActionBy).Value == eActionBy.Window.ToString())
                {
                    if (mAct.CreateBaselineImage)
                    {
                        if (mAct.GetInputParamValue(VRTAnalyzer.BaselineImage) == eBaselineImageBy.ActiveWindow.ToString())
                        {
                            image = GetScreenshot();
                        }
                        else
                        {
                            string baselinefilename = mAct.GetInputParamCalculatedValue(VRTSavedBaseImageFilenameString);
                            image = GetBaseLineImage(baselinefilename);
                        }
                    }
                    else
                    {
                        image = GetScreenshot();
                    }
                }
                else
                {
                    image = mDriver.GetElementScreenshot(mAct);
                }

                //diffTolerancePercent
                string toleranceValue = mAct.GetInputParamCalculatedValue(VRTAnalyzer.VRTParamDiffTollerancePercent);
                if (!Double.TryParse(toleranceValue, out double diffTolerancePercent))
                {
                    mAct.Error = string.Format("The configured tolerance Precentage value '{0}' is not valid", toleranceValue);
                    return;
                }
                else
                {
                    if (diffTolerancePercent <= 0.0)// Noncompliant indirect equality test
                    {
                        toleranceValue = WorkSpace.Instance.Solution.VRTConfiguration.DifferenceTolerance;
                        if (!Double.TryParse(toleranceValue, out diffTolerancePercent))
                        {
                            mAct.Error = string.Format("The configured tolerance Precentage value '{0}' is not valid on the VRT Configurations", toleranceValue);
                            return;
                        }
                    }
                }
                IWebDriver webDriver = mDriver.GetWebDriver();
                //Operating System
                string os = string.Empty;
                if (WorkSpace.Instance.Solution.VRTConfiguration.OS)
                {
                    if (webDriver is AndroidDriver)
                    {
                        os = $"Andriod";
                    }
                    else if (webDriver is IOSDriver)
                    {
                        os = $"IOS";
                    }
                    else
                    {
                        os = GingerPluginCore.OperatingSystem.GetCurrentOS();
                    }

                }
                //tags
                string tags = string.Empty;
                if (WorkSpace.Instance.Solution.VRTConfiguration.ActivityTags)
                {
                    tags = GetTags();
                    if (!string.IsNullOrEmpty(tags))
                    {
                        tags = "Tags:" + tags;
                    }
                }
                //Environment tag
                if (WorkSpace.Instance.Solution.VRTConfiguration.Environment)
                {
                    if (string.IsNullOrEmpty(tags))
                    {
                        tags = "Environment:" + mDriver.GetEnvironment();
                    }
                    else
                    {
                        tags += ", Environment:" + mDriver.GetEnvironment();
                    }
                }

                //Browser/agent/app name 
                string browser = string.Empty;
                if (WorkSpace.Instance.Solution.VRTConfiguration.Agent)
                {
                    if (mDriver.GetPlatform() == GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Web)
                    {

                        if (webDriver is AndroidDriver)
                        {
                            browser = $"chrome";
                        }
                        else if (webDriver is IOSDriver)
                        {
                            browser = $"safari";
                        }
                        else
                        {
                            browser = mDriver.GetAgentAppName();
                        }
                    }
                    else
                    {
                        browser = mDriver.GetAgentAppName() + "App";
                    }
                }
                //Viewport/resolution from driver
                string viewport = string.Empty;
                if (WorkSpace.Instance.Solution.VRTConfiguration.Viewport)
                {
                    viewport = mDriver.GetViewport();
                }
                //device
                string device = null;
                //imageName
                string imageName = GetImageName();

                //checkpoint
                TestRunResult result = vrt.Track(imageName, General.ImageToByteArray(image, System.Drawing.Imaging.ImageFormat.Png), null, os, browser, viewport, device, tags, diffTolerancePercent).GetAwaiter().GetResult();
                //results
                mAct.ExInfo = "TestRun Results Status: " + result.Status;
                mAct.AddOrUpdateReturnParamActual("Status", result.Status + "");
                mAct.AddOrUpdateReturnParamActual("Image URL", result.ImageUrl + "");
                mAct.AddOrUpdateReturnParamActual("Baseline URL", result.BaselineUrl + "");
                mAct.AddOrUpdateReturnParamActual("Difference URL", result.DiffUrl + "");
                mAct.AddOrUpdateReturnParamActual("URL", result.Url + "");
                if (result.BaselineUrl != null)
                {
                    mAct.previewBaselineImageName = Path.GetFileName(result.BaselineUrl);
                }

                //Calculate the action status based on the results
                if (WorkSpace.Instance.Solution.VRTConfiguration.FailActionOnCheckpointMismatch == Ginger.Configurations.VRTConfiguration.eFailActionOnCheckpointMismatch.Yes && result.Status != TestRunStatus.Ok)
                {
                    switch (result.Status)
                    {
                        case TestRunStatus.New:
                            if (mAct.CreateBaselineImage)
                            {
                                mAct.ExInfo += $"Baseline uploaded, Please approve it on VRT dashboard.{System.Environment.NewLine}{result.Url}";
                            }
                            else
                            {
                                mAct.Error += $"No baseline found or exsiting baseline not approved, Please approve it on VRT dashboard.{System.Environment.NewLine}{result.Url}";
                            }

                            //Add baseline image to act screenshots
                            if (result.ImageUrl != null)
                            {
                                mAct.previewBaselineImageName = Path.GetFileName(result.ImageUrl);
                            }
                            break;
                        case TestRunStatus.Unresolved:
                            mAct.Error += $"Differences from baseline was found.{System.Environment.NewLine}{result.DiffUrl}";

                            //Add difference image to act screenshots
                            if (result.DiffUrl != null)
                            {
                                string DiffrenceImage = General.DownloadImage($"{WorkSpace.Instance.Solution.VRTConfiguration.ApiUrl}/{Path.GetFileName(result.DiffUrl)}", mAct);
                                if (!string.IsNullOrEmpty(DiffrenceImage) && File.Exists(DiffrenceImage))
                                {
                                    Act.AddArtifactToAction("Difference_Image", mAct, DiffrenceImage);
                                }
                            }


                            //Add baseline image to act screenshots
                            if (result.BaselineUrl != null)
                            {
                                mAct.previewBaselineImageName = Path.GetFileName(result.BaselineUrl);
                                string BaseLineImage = General.DownloadImage($"{WorkSpace.Instance.Solution.VRTConfiguration.ApiUrl}/{Path.GetFileName(result.BaselineUrl)}", mAct);
                                if (!string.IsNullOrEmpty(BaseLineImage) && File.Exists(BaseLineImage))
                                {
                                    Act.AddArtifactToAction("Baseline_Image", mAct, BaseLineImage);
                                }
                            }


                            //No need to Add current Screenshot to act screenshots, it will be added in the end if the action is failed
                            break;
                        default:
                            mAct.ExInfo = $"TestRun Results Status: {result.Status}";
                            break;

                    }
                    mAct.CreateBaselineImage = false;//unchecked create Base line image after creation
                }
            }
            catch (AggregateException ae)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred when TrackVRT", ae);
                foreach (var e in ae.InnerExceptions)
                {
                    mAct.Error += e.Message;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred when TrackVRT", ex);
                mAct.Error += ex.Message;
            }
        }

        private Image GetScreenshot()
        {
            Image image;
            image = mDriver is GenericAppiumDriver gDriver
                ? gDriver.CaptureFullPageCroppedScreenshot()
                : mDriver.GetScreenShot(null, mAct.IsFullPageScreenshot);

            if (image == null)
            {
                throw new InvalidOperationException("Screen-shot capture returned null for driver " + mDriver.GetType().Name);
            }
            return image;
        }

        private string GetTags()
        {
            try
            {
                var tagNames = WorkSpace.Instance.Solution.Tags.Where(f => Context.GetAsContext(mAct.Context).Activity.Tags.Contains(f.Guid)).Select(f => f.Name);
                return string.Join(",", tagNames);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred when getting Tags from activity", ex);
                return null;
            }
        }

        private string GetImageName()
        {
            eImageNameBy imageNameBy;
            bool imageNameByResult = Enum.TryParse(mAct.GetOrCreateInputParam(VRTAnalyzer.ImageNameBy).Value, out imageNameBy);
            if (!imageNameByResult)
            {
                imageNameBy = eImageNameBy.ActionName;
            }

            var imageName = imageNameBy switch
            {
                eImageNameBy.ActionName => mAct.Description,
                eImageNameBy.ActionGuid => mAct.Guid.ToString(),
                eImageNameBy.ActionNameGUID => mAct.Description + "_" + mAct.Guid.ToString(),
                eImageNameBy.Custom => mAct.GetInputParamCalculatedValue(VRTAnalyzer.ImageName),
                _ => mAct.Description,
            };
            return imageName;
        }

        private void StopVRT()
        {
            try
            {
                if (vrt == null || !vrt.IsStarted)
                {
                    mAct.Error = "VRT is not Started";
                    mAct.ExInfo = "Please include a VRT Start Action one step before the current one, if it has not been done already, and ensure that it runs before the current action..";
                    return;
                }
                if (vrt.IsStarted)
                {
                    vrt.Stop().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                mAct.Error += "Stop VRT operation failed, Error: " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred when StopVRT", ex);
            }
            finally
            {
            }
        }

        private eVRTAction GetSelectedVRTActionEnum()
        {
            eVRTAction vrtAction = eVRTAction.Track;
            Enum.TryParse<eVRTAction>(mAct.GetInputParamValue(VRTAnalyzer.VRTAction), out vrtAction);
            return vrtAction;
        }

        private Bitmap GetBaseLineImage(string filepath)
        {
            Bitmap bmp = new Bitmap(filepath);
            return bmp;
        }
    }
}