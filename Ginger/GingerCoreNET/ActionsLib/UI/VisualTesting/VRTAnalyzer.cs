#region License
/*
Copyright © 2014-2022 European Support Limited

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
        static VisualRegressionTracker.VisualRegressionTracker vrt;
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
                EnableSoftAssert = WorkSpace.Instance.Solution.VRTConfiguration.EnableSoftAssert == Ginger.Configurations.VRTConfiguration.eEnableSoftAssert.Yes ? true : false
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
            List<int> mResolution = new List<int>();
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
            string diffImage = string.Empty;

            if (!vrt.IsStarted)
            {
                mAct.Error = "VRT is not Started";
                mAct.ExInfo = "You require to add VRT Start Action one step before.";
                return;
            }
            try
            {
                Image image;
                if (mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ActionBy).Value == eActionBy.Window.ToString())
                {
                    image = mDriver.GetScreenShot(null, mAct.IsFullPageScreenshot);
                }
                else
                {
                    image = mDriver.GetElementScreenshot(mAct);
                }
                bool res = Double.TryParse(mAct.GetInputParamCalculatedValue(VRTAnalyzer.VRTParamDiffTollerancePercent), out double diffTollerancePercent);

                string os = "Linux";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    os = "Windows";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    os = "Mac";
                }
                else
                {
                    os = "Linux";
                }
                string browser = ((Drivers.SeleniumDriver)mDriver).mBrowserTpe.ToString();
                //tags
                string tags = null;
                var activityTagsList = Context.GetAsContext(mAct.Context).Activity.Tags.Select(x => x.ToString());
                if (activityTagsList != null)
                {
                    tags = string.Join(",", activityTagsList);
                }

                //Get resolution from driver
                Size size = ((Drivers.SeleniumDriver)mDriver).GetWindowSize();
                string viewport = size.ToString();

                string device = null;

                //imageName
                string imageName = mAct.Description;
                if (mAct.GetOrCreateInputParam(VRTAnalyzer.ImageNameBy).Value == eImageNameBy.ActionGuid.ToString())
                {
                    imageName = mAct.Guid.ToString();
                }
                else if (mAct.GetOrCreateInputParam(VRTAnalyzer.ImageNameBy).Value == eImageNameBy.ActionNameGUID.ToString())
                {
                    imageName = mAct.Description + "_" + mAct.Guid.ToString();
                }
                else if (mAct.GetOrCreateInputParam(VRTAnalyzer.ImageNameBy).Value == eImageNameBy.Custom.ToString())
                {
                    imageName = mAct.GetInputParamCalculatedValue(VRTAnalyzer.ImageName);
                }
                //checkpoint
                TestRunResult result = vrt.Track(imageName, General.ImageToByteArray(image, System.Drawing.Imaging.ImageFormat.Png), null, os, browser, viewport, device, tags, diffTollerancePercent).GetAwaiter().GetResult();
                //results
                mAct.ExInfo = "TestRun Results Status: " + result.Status;
                mAct.AddOrUpdateReturnParamActual("Result Status", result.Status + "");
                mAct.AddOrUpdateReturnParamActual("Results Image URL", result.ImageUrl + "");
                mAct.AddOrUpdateReturnParamActual("Results Baseline URL", result.BaselineUrl + "");
                mAct.AddOrUpdateReturnParamActual("Results Diff URL", result.DiffUrl + "");
                mAct.AddOrUpdateReturnParamActual("Results URL", result.Url + "");
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
                if (ex.Message.Contains("Difference found"))
                {
                    int index = ex.Message.LastIndexOf("/");
                    mAct.Error += ex.Message.Substring(0, index);
                    diffImage = ex.Message.Substring(index + 1);
                }
                else
                {
                    mAct.Error += ex.Message;
                }
            }
            finally
            {
                if(!string.IsNullOrEmpty(diffImage))
                {
                    //Adding only difference image
                    General.DownloadImage(WorkSpace.Instance.Solution.VRTConfiguration.ApiUrl + "/" + diffImage, mAct);
                }
            }
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