#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Applitools;
using Applitools.Selenium;
using Applitools.Utils.Geometry;
using Applitools.VisualGrid;
using GingerCore.Drivers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Net;
using static GingerCore.Agent;
using static GingerCore.Drivers.SeleniumDriver;
using System.Linq;

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
        static Eyes newmEyes = null;
        static ClassicRunner runner = null;
        string mAppName;
        string mTestName;
        string mBaseServerUrl;

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

        private void NewSetEyesMatchLevel()
        {
            eMatchLevel matchLevel = (eMatchLevel)mAct.GetInputParamCalculatedValue<eMatchLevel>(ApplitoolsMatchLevel);
            switch (matchLevel)
            {
                case eMatchLevel.Content:
                    newmEyes.MatchLevel = MatchLevel.Content;
                    break;
                case eMatchLevel.Exact:
                    newmEyes.MatchLevel = MatchLevel.Exact;
                    break;
                case eMatchLevel.Layout:
                    newmEyes.MatchLevel = MatchLevel.Layout;
                    break;
                case eMatchLevel.Strict:
                    newmEyes.MatchLevel = MatchLevel.Strict;
                    break;
            }
        }

        void IVisualAnalyzer.Execute()
        {
            if (mDriver.GetType().Name == "SeleniumDriver")
            {
                switch (GetSelectedApplitoolsActionEnum())
                {
                    case eApplitoolsAction.OpenEyes:
                        NewEyesOpen();
                        break;

                    case eApplitoolsAction.Checkpoint:
                        NewCheckpoint();
                        break;

                    case eApplitoolsAction.CloseEyes:
                        NewCloseEyes();
                        break;
                }
            }
            else
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
            
        }


        void EyesOpen()
        {
            mEyes = new Applitools.Images.Eyes();

            //TODO: set the proxy
            // IWebProxy p = WebRequest.DefaultWebProxy; // .GetSystemWebProxy();

            mAppName = mAct.GetInputParamCalculatedValue(ActVisualTesting.Fields.ApplitoolsParamApplicationName);
            mTestName = mAct.GetInputParamCalculatedValue(ActVisualTesting.Fields.ApplitoolsParamTestName); 
            mAct.CheckSetAppWindowSize();
            mEyes.ApiKey = ((SeleniumDriver)mDriver).ApplitoolsViewKey; 
            mEyes.ServerUrl = string.IsNullOrEmpty(((SeleniumDriver)mDriver).ApplitoolsServerUrl) ? mEyes.ServerUrl : ((SeleniumDriver)mDriver).ApplitoolsServerUrl;
            OperatingSystem Os_info = System.Environment.OSVersion;
            mEyes.HostOS = Os_info.VersionString;
            mEyes.HostApp = ((SeleniumDriver)mDriver).GetBrowserType().ToString();
            List<int> mResolution = mAct.GetWindowResolution();
            mEyes.Open(mAppName, mTestName, new System.Drawing.Size(mResolution[0], mResolution[1]));
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
                    mAct.ExInfo = "Created new baseline in Applitools or Mismatch between saved baseline and target checkpoint image";
            }
            else if (response.IsMatch == false)
            {
                mAct.Error = "Created new baseline in Applitools or Mismatch between saved baseline and target checkpoint image";
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

        void NewEyesOpen()
        {
            runner = new ClassicRunner();
            newmEyes = new Eyes(runner);
            mAppName = mAct.GetInputParamCalculatedValue(ActVisualTesting.Fields.ApplitoolsParamApplicationName);
            mTestName = mAct.GetInputParamCalculatedValue(ActVisualTesting.Fields.ApplitoolsParamTestName);
            SetUp(newmEyes,((SeleniumDriver)mDriver).ApplitoolsServerUrl, ((SeleniumDriver)mDriver).ApplitoolsViewKey, ((SeleniumDriver)mDriver).GetBrowserType());
            
            mAct.CheckSetAppWindowSize();
            List<int> mResolution= mAct.GetWindowResolution();
            newmEyes.Open(((SeleniumDriver)mDriver).GetWebDriver(), mAppName, mTestName, new System.Drawing.Size(mResolution[0], mResolution[1]));

        }

        private void NewCheckpoint()
        {
            if (!newmEyes.IsOpen)
            {
                mAct.Error = "Applitools Eyes is not opened";
                mAct.ExInfo = "You require to add Eyes.Open Action on step before.";
                return;
            }

            NewSetEyesMatchLevel();
            
            newmEyes.Check(Target.Window().Fully().WithName(mAct.GetInputParamCalculatedValue(mAct.ItemName)));

            mAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;          

        }
        private void NewCloseEyes()
        {
            try
            {
                TestResults TR;
                if (newmEyes.IsOpen)
                {
                    TR = newmEyes.Close(false);
                }
                else
                {
                    TestResultsSummary allTestResults = runner.GetAllTestResults(false);
                    TestResultContainer[] resultContainer = allTestResults.GetAllResults();
                    TR = resultContainer[0].TestResults;
                }

                // Update results info into outputs
                mAct.ExInfo = "URL to view results: " + TR.Url;
                mAct.AddOrUpdateReturnParamActual("ResultsURL", TR.Url + "");
                mAct.AddOrUpdateReturnParamActual("Steps", TR.Steps + "");
                mAct.AddOrUpdateReturnParamActual("Matches", TR.Matches + "");
                mAct.AddOrUpdateReturnParamActual("Mismatches", TR.Mismatches + "");
                mAct.AddOrUpdateReturnParamActual("ExactMatches", TR.ExactMatches + "");
                mAct.AddOrUpdateReturnParamActual("StrictMatches", TR.StrictMatches + "");
                mAct.AddOrUpdateReturnParamActual("ContentMatches", TR.ContentMatches + "");
                mAct.AddOrUpdateReturnParamActual("LayoutMatches", TR.LayoutMatches + "");
                mAct.AddOrUpdateReturnParamActual("Missing", TR.Missing + "");



                if (!TR.IsNew)
                {
                    foreach (StepInfo step in TR.StepsInfo)
                    {
                        if (!step.HasCurrentImage)
                        {
                            mAct.AddOrUpdateReturnParamActual(step.Name, "Failed with Missing Image" + "");
                        }
                        else
                        {
                            mAct.AddOrUpdateReturnParamActual(step.Name, step.IsDifferent ? "Failed" : "Passed" + "");
                        }
                    }
                }
                mAct.AddOrUpdateReturnParamActual("IsNew", TR.IsNew + "");
                if ((TR.Mismatches == 0 || TR.IsNew) && TR.Missing == 0)
                {
                    mAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                    if (TR.IsNew)
                    {
                        mAct.ExInfo = "Created new baseline in Applitools.";
                    }
                    else
                    {
                        mAct.ExInfo = TR.Matches + " steps Matched with saved baseline in Applitools.";
                    }

                }
                else
                {
                    mAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    if (TR.Mismatches != 0)
                    {
                        mAct.Error = TR.Mismatches + " steps Mismatched with saved baseline image in Applitools. ";
                    }
                    if (TR.Missing != 0)
                    {
                        mAct.Error += TR.Missing + " steps missing current images.";
                    }
                }
            }
            catch (Exception ex)
            {
                mAct.Error += "Eyes Close operation failed, Error: " + ex.Message;
            }
            finally
            {
                newmEyes.AbortIfNotClosed();
            }
        }
        private void SetUp(Eyes eyes,string AppilToolServerUrl,string AppilToolsApiKey, eBrowserType BrowserType)
        {
            Applitools.Selenium.Configuration config = new Applitools.Selenium.Configuration();
            
            config.SetApiKey(AppilToolsApiKey);
            config.SetServerUrl(AppilToolServerUrl);
            OperatingSystem Os_info = System.Environment.OSVersion;
            config.SetHostOS(Os_info.VersionString);
            config.SetHostApp(BrowserType.ToString());
            eyes.SetConfiguration(config);

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

        private string GetServerUrl()
        {
            return mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ServerUrl).ValueForDriver;
        }
    }
}