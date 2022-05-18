using Amdocs.Ginger.Common;
using GingerCore.Actions;
using GingerCore.Actions.VisualTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using VisualRegressionTracker;

namespace GingerCore.Actions.VisualTesting
{
    public class VRTAnalyzer : IVisualAnalyzer
    {
        public static string VRTAction = "VRTAction";

        ActVisualTesting mAct;
        IVisualTestingDriver mDriver;
        VisualRegressionTracker.VisualRegressionTracker vrt;
        VisualRegressionTracker.Config config;

        public VRTAnalyzer()
        {
            VisualRegressionTracker.Config config = new VisualRegressionTracker.Config
            {
                CiBuildId = "commit_sha",
                BranchName = "matser",
                Project = "Default project",
                ApiUrl = "http://10.234.170.81:4200",
                ApiKey = "RYYWC5PFE9MSGSQ2D1A9ZCPVMTP2"
            };
            vrt = new VisualRegressionTracker.VisualRegressionTracker(config);
        }
        public enum eVRTAction
        {
            [EnumValueDescription("Start")]
            Start,
            [EnumValueDescription("Track")]
            Track,
            [EnumValueDescription("Stop")]
            Stop
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
                mAct.CheckSetAppWindowSize();
                mResolution = mAct.GetWindowResolution();
                //start
                //vrt = new VisualRegressionTracker.VisualRegressionTracker(config);
                vrt.Start();
            }
            catch (Exception ex)
            {
                if (mResolution != null && mResolution.Any() && mResolution[0] < 500)
                {
                    mAct.Error += "Start VRT Failed. Set Resolution having width more than 500px, Error: " + ex.Message;
                }
                else
                {
                    mAct.Error += "Start VRT Failed, Error: " + ex.Message;
                }
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
                //track
                vrt.Track();
            }
            catch (Exception ex)
            {
                mAct.Error += ex.Message;
            }
            finally
            {
                mAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
            }
        }
        private void StopVRT()
        {
            try
            {
                if (vrt.IsStarted)
                {
                    vrt.Stop();
                }
                else
                {
                }
                
            }
            catch (Exception ex)
            {
                mAct.Error += "Eyes Close operation failed, Error: " + ex.Message;
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
