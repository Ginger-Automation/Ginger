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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Applitools;
using System;
using System.Net;

namespace GingerCore.Actions.VisualTesting
{
    public class ApplitoolsAnalyzer : IVisualAnalyzer
    {
        ActVisualTesting mAct;
        IVisualTestingDriver mDriver;

        public static string ApplitoolsAction = "ApplitoolsAction";
        public static string ApplitoolsEyesClose = "ApplitoolsEyesClose";
        public static string ApplitoolsEyesOpen = "ApplitoolsEyesOpen";
        public static string ApplitoolsMatchLevel = "ApplitoolsMatchLevel";
        public static string FailActionOnMistmach = "FailActionOnMistmach";

        // We keep one static eyes so we can reuse across action and close when done, to support applitools behaviour
        static Applitools.Images.Eyes mEyes = null;
        string mAppName;
        string mTestName;

        bool IVisualAnalyzer.SupportUniqueExecution()
        {
            return true;
        }

        void IVisualAnalyzer.SetAction(IVisualTestingDriver driver, ActVisualTesting act)
        {
            mDriver = driver;
            mAct = act;
        }

        public enum eApplitoolsAction
        {
            [EnumValueDescription("Open Eyes")]
            OpenEyes,
            [EnumValueDescription("Checkpoint")]
            Checkpoint,
            [EnumValueDescription("Close Eyes")]
            CloseEyes
        }

        public enum eMatchLevel
        {
            Exact,
            Strict,
            Content,
            Layout
        }

        private void SetEyesMatchLevel()
        {
            eMatchLevel matchLevel = (eMatchLevel)mAct.GetInputParamCalculatedValue<eMatchLevel>(ApplitoolsMatchLevel);
            switch (matchLevel)
            {
                case eMatchLevel.Content:
                    mEyes.MatchLevel = MatchLevel.Content;
                    break;
                case eMatchLevel.Exact:
                    mEyes.MatchLevel = MatchLevel.Exact;
                    break;
                case eMatchLevel.Layout:
                    mEyes.MatchLevel = MatchLevel.Layout;
                    break;
                case eMatchLevel.Strict:
                    mEyes.MatchLevel = MatchLevel.Strict;
                    break;
            }
        }

        void IVisualAnalyzer.Execute()
        {
            switch (GetSelectedApplitoolsActionEnum())
            {
                case eApplitoolsAction.OpenEyes:
                    EyesOpen();
                    break;

                case eApplitoolsAction.Checkpoint:
                    Checkpoint();
                    break;

                case eApplitoolsAction.CloseEyes:
                    CloseEyes();
                    break;
            }
        }

        void EyesOpen()
        {
            mEyes = new Applitools.Images.Eyes();
            //TODO: set the proxy
            // IWebProxy p = WebRequest.DefaultWebProxy; // .GetSystemWebProxy();
            
            mAppName = mAct.GetInputParamValue(ActVisualTesting.Fields.ApplitoolsParamApplicationName);
            mTestName = mAct.GetInputParamValue(ActVisualTesting.Fields.ApplitoolsParamTestName);
            mAct.CheckSetAppWindowSize();
            mEyes.ApiKey = GetApplitoolsAPIKey();
            mEyes.Open(mAppName, mTestName);
        }

        private void Checkpoint()
        {
            if (mEyes == null)
            {
                mAct.Error = "Applitools Eyes is not opened";
                mAct.ExInfo = "You require to add Eyes.Open Action on step before.";
                return;
            }

            SetEyesMatchLevel();
            AppImage response = mEyes.CheckImage(mDriver.GetScreenShot());
            mAct.AddOrUpdateReturnParamActual("ExactMatches", response.IsMatch.ToString());

            bool FailActionOnMistmach = Boolean.Parse(mAct.GetInputParamValue(ApplitoolsAnalyzer.FailActionOnMistmach));
            if (response.IsMatch == true || !FailActionOnMistmach)
            {
                mAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                if (!FailActionOnMistmach)
                    mAct.ExInfo = "Created new baseline in Applitools or Mismatch between saved baseline and terget checkpoint image";
            }
            else if (response.IsMatch == false)
            {
                mAct.Error = "Created new baseline in Applitools or Mismatch between saved baseline and terget checkpoint image";
            }
        }

        private void CloseEyes()
        {
            try
            {
                TestResults TR = mEyes.Close(false);
                // Update results info into outputs
                mAct.ExInfo = "URL to view results: " + TR.Url;
                mAct.AddOrUpdateReturnParamActual("ResultsURL", TR.Url + "");
                mAct.AddOrUpdateReturnParamActual("Steps", TR.Steps + "");
                mAct.AddOrUpdateReturnParamActual("Mismatches", TR.Mismatches + "");
                mAct.AddOrUpdateReturnParamActual("ExactMatches", TR.ExactMatches + "");
                mAct.AddOrUpdateReturnParamActual("StrictMatches", TR.StrictMatches + "");
                mAct.AddOrUpdateReturnParamActual("ContentMatches", TR.ContentMatches + "");
                mAct.AddOrUpdateReturnParamActual("LayoutMatches", TR.LayoutMatches + "");
                mAct.AddOrUpdateReturnParamActual("ExactMatches", TR.ExactMatches + "");
                mAct.AddOrUpdateReturnParamActual("IsNew", TR.IsNew + "");
            }
            catch (Exception ex)
            {
                mAct.Error += "Eyes Close operation failed, Error: " + ex.Message;
            }
            finally
            {
                mEyes.AbortIfNotClosed();
            }
        }

        private eApplitoolsAction GetSelectedApplitoolsActionEnum()
        {
            eApplitoolsAction applitoolsAction = eApplitoolsAction.Checkpoint;
            Enum.TryParse<eApplitoolsAction>(mAct.GetInputParamValue(ApplitoolsAnalyzer.ApplitoolsAction), out applitoolsAction);
            return applitoolsAction;
        }

        private string GetApplitoolsAPIKey()
        {
            return mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ApplitoolsKey).ValueForDriver;
        }
        
        //TODO: mEyes.Proxy - so it will work in dox

        //--------------------------------------Old Methods--------------------------------------------------------
        void IVisualAnalyzer.Compare()
        {
            throw new NotImplementedException();
        }

        public void CreateBaseline()
        {
            throw new NotImplementedException();
        }

        private void SaveApplitoolsImages(TestResults testResult)
        {
            String sessionURL = testResult.Url;
            String URLforDownloading = BakeURL(sessionURL);
            int numOfSteps = testResult.Steps;
            DownloadImages(URLforDownloading, numOfSteps);
        }

        private String BakeURL(String sessionURL)
        {
            //Edit URL and prepare it to download images from report
            String bakedURL = sessionURL.Replace("/app/", "/api/");
            bakedURL = bakedURL.Insert(bakedURL.IndexOf("sessions") + 9, "batches/");
            bakedURL = bakedURL.Substring(0, bakedURL.IndexOf("?accountId"));
            bakedURL += "/steps/";
            return bakedURL;
        }

        private void DownloadImages(String BaseURLForDownloading, int numOfImages)
        {
            String imagePath = @"C:\test\Applitools\image";  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!! FIXME - add to the action
            for (int i = 1; i <= numOfImages; i++)
            {
                String currImagePath = imagePath + i.ToString() + ".jpg";
                String currImageURL = BaseURLForDownloading + i.ToString() + "/diff?ApiKey=" + GetApplitoolsAPIKey();
                Console.WriteLine(currImageURL);
                WebClient webClient = new WebClient();
                webClient.DownloadFile(currImageURL, currImagePath);
            }
        }
    }
}