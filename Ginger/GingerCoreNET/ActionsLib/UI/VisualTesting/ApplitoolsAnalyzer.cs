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
using amdocs.ginger.GingerCoreNET;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading;
using System.IO;
using Amdocs.Ginger.Common.UIElement;
using Ginger.Run;
using OpenQA.Selenium.Appium.Windows;

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
        public const string ActionBy = "ActionBy";
        public const string LocateBy = "LocateBy";

        private const int RETRY_REQUEST_INTERVAL = 500; //ms
        private const int LONG_REQUEST_DELAY_MS = 2000; // ms
        private const int MAX_LONG_REQUEST_DELAY_MS = 10000; // ms
        private const double LONG_REQUEST_DELAY_MULTIPLICATIVE_INCREASE_FACTOR = 1.5;

        private string ServerURL;
        private string batchID;
        private string sessionID;

        // We keep one static eyes so we can reuse across action and close when done, to support applitools behaviour
        static Applitools.Images.Eyes mEyes = null;
        static Eyes WebEyes = null;
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

        public enum eActionBy
        {
            Window,
            Region
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
                    WebEyes.MatchLevel = MatchLevel.Content;
                    break;
                case eMatchLevel.Exact:
                    WebEyes.MatchLevel = MatchLevel.Exact;
                    break;
                case eMatchLevel.Layout:
                    WebEyes.MatchLevel = MatchLevel.Layout;
                    break;
                case eMatchLevel.Strict:
                    WebEyes.MatchLevel = MatchLevel.Strict;
                    break;
            }
        }

        void IVisualAnalyzer.Execute()
        {
            //TODO: Remove hardcoded string and use typeof
            if (mDriver.GetType().Name == "SeleniumDriver")
            {
                switch (GetSelectedApplitoolsActionEnum())
                {
                    case eApplitoolsAction.OpenEyes:
                        WebEyesOpen();
                        break;

                    case eApplitoolsAction.Checkpoint:
                        WebCheckpoint();
                        break;

                    case eApplitoolsAction.CloseEyes:
                        WebCloseEyes();
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
            List<int> mResolution = new List<int>();
            try
            {
                mEyes = new Applitools.Images.Eyes();

                //TODO: set the proxy
                mAppName = mAct.GetInputParamCalculatedValue(ActVisualTesting.Fields.ApplitoolsParamApplicationName);
                mTestName = mAct.GetInputParamCalculatedValue(ActVisualTesting.Fields.ApplitoolsParamTestName);
                mAct.CheckSetAppWindowSize();
                mEyes.ApiKey = mDriver.GetApplitoolKey();
                mEyes.ServerUrl = string.IsNullOrEmpty(mDriver.GetApplitoolServerURL()) ? mEyes.ServerUrl : mDriver.GetApplitoolServerURL();
                OperatingSystem Os_info = System.Environment.OSVersion;
                mEyes.HostOS = Os_info.VersionString;
                mEyes.HostApp = mDriver is SeleniumDriver ? ((SeleniumDriver)mDriver).GetBrowserType().ToString() : mDriver.GetPlatform().ToString();
                mEyes.AddProperty("Environment ID", mDriver.GetEnvironment());
                mResolution = mAct.GetWindowResolution();
                mEyes.Open(mAppName, mTestName, new System.Drawing.Size(mResolution[0], mResolution[1]));
            }
            catch (Exception ex)
            {
                if (mResolution != null && mResolution.Any() && mResolution[0] < 500)
                {
                    mAct.Error += "Eyes Open Failed. Set Resolution having width more than 500px, Error: " + ex.Message;
                }
                else
                {
                    mAct.Error += "Eyes Open Failed, Error: " + ex.Message;
                }
            }

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
            mEyes.Check(mAct.ItemName, Applitools.Images.Target.Image(mDriver.GetScreenShot()).Fully());
            mAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;

        }

        private void CloseEyes()
        {

            try
            {
                TestResults TR = mEyes.Close();
                //Update results info into outputs
                //SaveApplitoolsImages(TR);
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
            
        }

        void WebEyesOpen()
        {
            List<int> mResolution = new List<int>();
            try
            {
                runner = new ClassicRunner();
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig.GingerRunners.Any(x=>x.Executor.IsRunning == true))
                {
                    runner.DontCloseBatches = true;
                }
                WebEyes = new Eyes(runner);
                mAppName = mAct.GetInputParamCalculatedValue(ActVisualTesting.Fields.ApplitoolsParamApplicationName);
                mTestName = mAct.GetInputParamCalculatedValue(ActVisualTesting.Fields.ApplitoolsParamTestName);
                
                SetUp(WebEyes, mDriver.GetApplitoolServerURL(), mDriver.GetApplitoolKey(), ((SeleniumDriver)mDriver).GetBrowserType(), mDriver.GetEnvironment());
                mAct.CheckSetAppWindowSize();
                mResolution = mAct.GetWindowResolution();
                WebEyes.Open(mDriver.GetWebDriver(), mAppName, mTestName, new System.Drawing.Size(mResolution[0], mResolution[1]));
            }
            catch (Exception ex)
            {
                if (mResolution != null && mResolution.Any() && mResolution[0] < 500)
                {
                    mAct.Error += "Eyes Open Failed. Set Resolution having width more than 500px, Error: " + ex.Message;
                }
                else
                {
                    mAct.Error += "Eyes Open Failed, Error: " + ex.Message;
                }
            }

            
        }

        private void WebCheckpoint()
        {
            if (!WebEyes.IsOpen)
            {
                mAct.Error = "Applitools Eyes is not opened";
                mAct.ExInfo = "You require to add Eyes.Open Action on step before.";
                return;
            }

            NewSetEyesMatchLevel();
            string ActionTakenBy = GetActionBy();
            try
            {
                if (ActionTakenBy == "Window")
                {
                    WebEyes.Check(Target.Window().Fully().WithName(mAct.ItemName));
                } 
                else
                {
                    ElementLocator locator = new ElementLocator();
                    locator.LocateBy = GetLocateBy();
                    locator.LocateValue = GetLocateValue();
                    IWebElement webElement = ((SeleniumDriver)mDriver).LocateElement(mAct, false,null,null);
                    WebEyes.Check(Target.Region(webElement).Fully().WithName(mAct.ItemName));
                }
                    
                
            }
            catch (Exception ex) 
            {
                if (ActionTakenBy == "Region") 
                {
                    mAct.Error += "Not Able to locate XPath, Error: " + ex.Message;
                }
                else
                {
                    mAct.Error += ex.Message;
                }
                    
            }
            finally
            {
                mAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
            }
             

            
            
        }
        private void WebCloseEyes()
        {
            try
            {
                TestResults TR;
                if (WebEyes.IsOpen)
                {
                    TR = WebEyes.Close(false);
                }
                else
                {
                    TestResultsSummary allTestResults = runner.GetAllTestResults(false);
                    TestResultContainer[] resultContainer = allTestResults.GetAllResults();
                    TR = resultContainer[0].TestResults;
                }
                SaveApplitoolsImages(TR);
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
        }
        private void SetUp(Eyes eyes,string AppilToolServerUrl,string AppilToolsApiKey, eBrowserType BrowserType,string Environment)
        {
            Applitools.Selenium.Configuration config = new Applitools.Selenium.Configuration();
            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig.GingerRunners.Any(x => x.Executor.IsRunning == true))
            {
                BatchInfo batchInfo = new BatchInfo(WorkSpace.Instance.RunsetExecutor.RunSetConfig.ItemName);

                batchInfo.Id = WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID.ToString();
                config.SetBatch(batchInfo);
            }

            config.SetApiKey(AppilToolsApiKey);
            config.SetServerUrl(AppilToolServerUrl);
            OperatingSystem Os_info = System.Environment.OSVersion;
            config.SetHostOS(Os_info.VersionString);
            config.SetHostApp(BrowserType.ToString());
            eyes.AddProperty("Environment ID", !String.IsNullOrEmpty(Environment) ? Environment : "Default");
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
            setServerURL(testResult);
            setbatchID(testResult);
            setsessionID(testResult);
            String sessionURL = testResult.Url;
            int numOfSteps = testResult.Steps;
            DownloadImages(numOfSteps, testResult);
        }
        private String BakeURL(String sessionURL)
        {
            //Edit URL and prepare it to download images from report
            String bakedURL = sessionURL.Replace("/app/", "/api/");
            bakedURL = bakedURL.Substring(0, bakedURL.IndexOf("?accountId"));
            bakedURL += "/steps/";
            return bakedURL;
        }

        private void DownloadImages(int numOfImages, TestResults testResults)
        {
            for (int i = 1; i <= numOfImages; i++)
            {
                String currImagePath = Act.GetScreenShotRandomFileName();
                String currImageURL = this.ServerURL + "/api/sessions/batches/" + this.batchID + "/" + this.sessionID + "/steps/" + i.ToString() + "/images/diff?ApiKey=" + mDriver.GetApplitoolKey();// ((SeleniumDriver)mDriver).ApplitoolsViewKey;
                try
                {
                    HttpResponseMessage response = runLongRequest(currImageURL);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var fs = new FileStream(currImagePath, FileMode.Create, FileAccess.Write, FileShare.None);
                        response.Content.CopyToAsync(fs).ContinueWith(
                        (discard) =>
                        {
                            fs.Close();
                        });
                        mAct.ScreenShotsNames.Add(Path.GetFileName(currImagePath));
                        mAct.ScreenShots.Add(currImagePath);
                        
                    }
                }
                catch(Exception ex)
                {
                    mAct.Error = mAct.Error + ex.Message;
                }
                
                
            }
        }

        private string GetServerUrl()
        {
            return mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ServerUrl).ValueForDriver;
        }
        private string GetActionBy()
        {
            return mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ActionBy).Value;
        }

        private eLocateBy GetLocateBy()
        {
            eLocateBy eVal = eLocateBy.ByXPath;
            if (Enum.TryParse<eLocateBy>(mAct.GetOrCreateInputParam(ActVisualTesting.Fields.LocateBy).Value, out eVal))
                return eVal;
            else
                return eLocateBy.ByXPath;
        }

        private string GetLocateValue()
        {
            return mAct.GetOrCreateInputParam(ActVisualTesting.Fields.LocateValue).Value;
        }

        private void setServerURL(TestResults testresult)
        {
            int endOfServerUrlLocation = testresult.Url.IndexOf("/app/session");
            if (endOfServerUrlLocation < 1)
            {
                endOfServerUrlLocation = testresult.Url.IndexOf("/app/batches");
            }
            this.ServerURL = testresult.Url.Substring(0, endOfServerUrlLocation);

        }

        private void setbatchID(TestResults testresult)
        {
            string URL = testresult.Url;
            string temp = "^" + this.ServerURL + @"/app/sessions/(?<batchId>\d+).*$";
            Match match = Regex.Match(URL, "^" + this.ServerURL + @"/app/batches/(?<batchId>\d+).*$");
            this.batchID = match.Groups[1].Value;
        }
      
        private void setsessionID(TestResults testresult)
        {
            string URL = testresult.Url;
            Match match = Regex.Match(URL, "^" + this.ServerURL + @"/app/batches/\d+/(?<sessionId>\d+).*$");
            this.sessionID = match.Groups[1].Value;
        }

        private HttpResponseMessage runLongRequest(string URL)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, URL);

            HttpResponseMessage response = sendRequest(request, 1, false);

            return longRequestCheckStatus(response);
        }

        private HttpResponseMessage sendRequest(HttpRequestMessage request, int retry, Boolean delayBeforeRetry)
        {
            HttpClient client = new HttpClient();

            try
            {
                HttpResponseMessage response = client.SendAsync(request).Result;
                return response;
            }
            catch (Exception e)
            {
                String errorMessage = "error message: " + e.Message;
                
                if (retry > 0)
                {
                    if (delayBeforeRetry)
                    {
                        Thread.Sleep(RETRY_REQUEST_INTERVAL);
                    }
                    return sendRequest(request, retry - 1, delayBeforeRetry);
                }
                throw new ThreadInterruptedException(errorMessage);
            }
        }

        public HttpResponseMessage longRequestCheckStatus(HttpResponseMessage responseReceived)
        {
            HttpStatusCode status = responseReceived.StatusCode;


            HttpRequestMessage request = null;
            String URI;

            switch (status)
            {
                case HttpStatusCode.OK:
                    return responseReceived;

                case HttpStatusCode.Accepted:
                    var location = responseReceived.Headers.GetValues("Location");
                    URI = location.First() + "?apiKey=" + WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey;

                    request = new HttpRequestMessage(HttpMethod.Get, URI);
                    HttpResponseMessage response = longRequestLoop(request, LONG_REQUEST_DELAY_MS);
                    return longRequestCheckStatus(response);

                case HttpStatusCode.Created:
                    var location2 = responseReceived.Headers.GetValues("Location");
                    URI = location2.First() + "?apiKey=" + WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey;
                    request = new HttpRequestMessage(HttpMethod.Delete, URI);
                    return sendRequest(request, 1, false);

                case HttpStatusCode.NotFound:
                    return responseReceived;

                case HttpStatusCode.Gone:
                    throw new ThreadInterruptedException("The server task is gone");

                default:
                    throw new ThreadInterruptedException("Unknown error during long request: " + status);
                    
            }
        }

        public HttpResponseMessage longRequestLoop(HttpRequestMessage request, int delay)
        {
            delay = (int)Math.Min(MAX_LONG_REQUEST_DELAY_MS, Math.Floor(delay * LONG_REQUEST_DELAY_MULTIPLICATIVE_INCREASE_FACTOR));
            Thread.Sleep(delay);

            HttpResponseMessage response = sendRequest(request, 1, false);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return longRequestLoop(request, delay);
            }
            return response;
        }
    }
}