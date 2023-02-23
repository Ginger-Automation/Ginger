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
using GingerCoreNET.GeneralLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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


        ActVisualTesting mAct;
        IVisualTestingDriver mDriver;
        VisualRegressionTracker.VisualRegressionTracker vrt;
        VisualRegressionTracker.Config config;

        public VRTAnalyzer()
        {
            if (vrt == null)
            {
                CreateVRTConfig();
            }
        }

        private void CreateVRTConfig()
        {
            config = new VisualRegressionTracker.Config
            {
                BranchName = WorkSpace.Instance.Solution.VRTConfiguration.BranchName,
                Project = WorkSpace.Instance.Solution.VRTConfiguration.Project,
                ApiUrl = WorkSpace.Instance.Solution.VRTConfiguration.ApiUrl,
                ApiKey = WorkSpace.Instance.Solution.VRTConfiguration.ApiKey,
                EnableSoftAssert = WorkSpace.Instance.Solution.VRTConfiguration.FailActionOnCheckpointMismatch == Ginger.Configurations.VRTConfiguration.eFailActionOnCheckpointMismatch.Yes ? false : true
            };
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
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured when StartVRT", ae);
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
            if (!vrt.IsStarted)
            {
                mAct.Error = "VRT is not Started";
                mAct.ExInfo = "You require to add VRT Start Action one step before.";
                return;
            }
            try
            {
                //get Image
                Image image;
                if (mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ActionBy).Value == eActionBy.Window.ToString())
                {
                    image = mDriver.GetScreenShot(null, mAct.IsFullPageScreenshot);
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

                //Operating System
                string os = string.Empty;
                if (WorkSpace.Instance.Solution.VRTConfiguration.OS)
                {
                    os = GingerPluginCore.OperatingSystem.GetCurrentOS();
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
                    browser = mDriver.GetPlatform() == GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Web ? mDriver.GetAgentAppName() : mDriver.GetAgentAppName() + "App";
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


                //Calculate the action status based on the results
                if (WorkSpace.Instance.Solution.VRTConfiguration.FailActionOnCheckpointMismatch == Ginger.Configurations.VRTConfiguration.eFailActionOnCheckpointMismatch.Yes && result.Status != TestRunStatus.Ok)
                {
                    switch (result.Status)
                    {
                        case TestRunStatus.New:
                            mAct.Error += $"No baseline found, Please approve it on dashboard to create baseline." + System.Environment.NewLine + result.Url;
                            break;
                        case TestRunStatus.Unresolved:
                            mAct.Error += $"Differences from baseline was found." + System.Environment.NewLine + result.DiffUrl;

                            //Add difference image to act screenshots
                            int index = result.DiffUrl.LastIndexOf("/");
                            string imageToDownload = result.DiffUrl.Substring(index + 1);
                            General.DownloadImage(WorkSpace.Instance.Solution.VRTConfiguration.ApiUrl + "/" + imageToDownload, mAct);

                            //Add baseline image to act screenshots
                            index = result.BaselineUrl.LastIndexOf("/");
                            imageToDownload = result.BaselineUrl.Substring(index + 1);
                            General.DownloadImage(WorkSpace.Instance.Solution.VRTConfiguration.ApiUrl + "/" + imageToDownload, mAct);

                            //No need to Add current Screenshot to act screenshots, it will be added in the end if the action is failed
                            break;
                        default:
                            mAct.ExInfo = "TestRun Results Status: " + result.Status;
                            break;
                    }
                }
            }
            catch (AggregateException ae)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured when TrackVRT", ae);
                foreach (var e in ae.InnerExceptions)
                {
                    mAct.Error += e.Message;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured when TrackVRT", ex);
                mAct.Error += ex.Message;
            }
        }

        private string GetTags()
        {
            try
            {
                var tagNames = WorkSpace.Instance.Solution.Tags.Where(f => Context.GetAsContext(mAct.Context).Activity.Tags.Contains(f.Guid)).Select(f => f.Name);
                return string.Join(",", tagNames);
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured when getting Tags from activity", ex);
                return null;
            }
        }

        private string GetImageName()
        {
            string imageName;
            eImageNameBy imageNameBy;
            bool imageNameByResult = Enum.TryParse(mAct.GetOrCreateInputParam(VRTAnalyzer.ImageNameBy).Value, out imageNameBy);
            if (!imageNameByResult)
            {
                imageNameBy = eImageNameBy.ActionName;
            }
            switch (imageNameBy)
            {
                case eImageNameBy.ActionName:
                    imageName = mAct.Description;
                    break;
                case eImageNameBy.ActionGuid:
                    imageName = mAct.Guid.ToString();
                    break;
                case eImageNameBy.ActionNameGUID:
                    imageName = mAct.Description + "_" + mAct.Guid.ToString();
                    break;
                case eImageNameBy.Custom:
                    imageName = mAct.GetInputParamCalculatedValue(VRTAnalyzer.ImageName);
                    break;
                default:
                    imageName = mAct.Description;
                    break;
            }
            return imageName;
        }

        private void StopVRT()
        {
            try
            {
                if (vrt.IsStarted)
                {
                    vrt.Stop().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                mAct.Error += "Stop VRT operation failed, Error: " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured when StopVRT", ex);
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

    }
}