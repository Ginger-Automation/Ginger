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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.VisualTesting;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using static GingerCore.Actions.ActVisualTesting;

namespace GingerCore.Drivers
{
    public abstract class DriverBase
    {
        //Basic configurations
        [UserConfigured]
        [UserConfiguredDefault("30")]
        [UserConfiguredDescription("The amount of maximum time (in seconds) to wait for the driver to load/connect")]
        public int DriverLoadWaitingTime { get; set; }

        public virtual int ActionTimeout
        {
            get
            {
                return -1;
            }
            set
            { }
        }

        public string ErrorMessageFromDriver { get; set; }

        //If this is WPF Window like IB, or Device/Mobile, Unix need to run on it's own STA
        public Thread STAThread { get; set; }

        // Used for Driver with WPF window
        public IDispatcher Dispatcher { get; set; }

        private IVisualAnalyzer VisualAnalyzer = null;
        public IVisualAnalyzer GetVisualAnalyzer(eVisualTestingAnalyzer VisualTestingAnalyzer)
        {
            //Check what kind of comparison we have - Applitools, simple Bitmap or Elements comparison
            switch (VisualTestingAnalyzer)
            {
                case eVisualTestingAnalyzer.BitmapPixelsComparison:
                    if (VisualAnalyzer is not MagickAnalyzer)
                    {
                        VisualAnalyzer = new MagickAnalyzer();
                    }
                    break;

                case eVisualTestingAnalyzer.Applitools:
                    if (VisualAnalyzer is not ApplitoolsAnalyzer)
                    {
                        VisualAnalyzer = new ApplitoolsAnalyzer();
                    }
                    break;

                case eVisualTestingAnalyzer.UIElementsComparison:
                    if (VisualAnalyzer is not UIElementsAnalyzer)
                    {
                        VisualAnalyzer = new UIElementsAnalyzer();
                    }
                    break;

                case eVisualTestingAnalyzer.VRT:
                    if (VisualAnalyzer is not VRTAnalyzer)
                    {
                        VisualAnalyzer = new VRTAnalyzer();
                    }
                    break;
            }
            return VisualAnalyzer;
        }

        public virtual bool StopProcess { get; set; }

        public bool PreviousRunStopped { get; set; }
        public bool IsDriverRunning { get; set; }
        // Driver like IB will override and return true
        public virtual bool IsSTAThread()
        {
            return false;
        }

        //TODO:Remove this once on the drivers implementing IWindowExplorer are ready
        public virtual bool IsWindowExplorerSupportReady()
        {
            return false;
        }

        public virtual bool IsShowWindowExplorerOnStart()
        {
            return false;
        }

        public void CreateSTA(Action ThreadStartingPoint)
        {
            STAThread = new Thread(new ThreadStart(ThreadStartingPoint));
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.IsBackground = true;
            STAThread.Start();


        }

        public ObservableList<DriverConfigParam> AdvanceDriverConfigurations { get; set; }
        public abstract void StartDriver();
        public bool cancelAgentLoading = false;
        public abstract void CloseDriver();
        public string DriverClass { get { return this.GetType().ToString(); } }

        // running an action on the UI
        public abstract void RunAction(Act act);


        // UI Inspector

        //TODO: to be removed - not used!?
        public abstract Act GetCurrentElement();
        public abstract string GetURL();

        public abstract void HighlightActElement(Act act);
        public BusinessFlow BusinessFlow { get; set; }
        public Environments.ProjEnvironment Environment { get; set; }
        public abstract ePlatformType Platform { get; }
        public abstract bool IsRunning();

        protected bool mIsDriverBusy;



        public bool IsDriverBusy
        {
            get
            {
                return mIsDriverBusy;
            }
        }

        // Input to this method is call back function to handle the events

        //TODO: Later on make this abstract
        public virtual void StartRecording()
        {
            throw new NotImplementedException();
        }

        //TODO: Need to be part of IWindowExplore
        public virtual void StopRecording()
        {
            throw new NotImplementedException();
        }

        // What is this? cleanup?
        // Availbe Recording Actions, to be sent to the processing window
        public enum ActionName
        {
            Click,
            ClickTab,
            ExpandMenu,
            AddToSelection,
            RemoveFromSelection,
            SwitchWindow,
            SelectListItem,
            SetText,
            SelectRadioButton,
            CloseWindow,
            UnCheckCheckBox,
            CheckCheckBox
        }

        public enum eDriverMessageType
        {
            DriverStatusChanged,
            ActionPerformed,
            RecordingEvent,
            HighlightElement,
            UnHighlightElement,
            RotateEvent,
            ConsoleBufferUpdate,
            CloseDriverWindow
        }

        public void OnDriverMessage(eDriverMessageType DriverMessageType, object CustomSenderObj = null)
        {
            if (DriverMessageEvent != null)
            {
                DriverMessageEvent(CustomSenderObj ?? this, new DriverMessageEventArgs(DriverMessageType));
            }
        }

        public delegate void DriverMessageEventHandler(object sender, DriverMessageEventArgs e);
        public event DriverMessageEventHandler DriverMessageEvent;

        public event SpyingElementEventHandler SpyingElementEvent;
        public delegate object SpyingElementEventHandler();

        public object OnSpyingElementEvent()
        {
            if (SpyingElementEvent != null)
            {
                return SpyingElementEvent();
            }
            else
            {
                return null;
            }
        }

        public virtual void ActionCompleted(Act act)
        {
            // Do nothing, can be implemented in sub class like: ConsoleDriverBase, WindowsDriver, PBDriver
        }

        public virtual void UpdateContext(Context context)
        {
            BusinessFlow = context.BusinessFlow;
        }

        public virtual ePomElementCategory? PomCategory { get; set; }

        #region VirtualDrivers

        //TODO: .net STandard 2.1 Use C# 8 default interface implmentation  once we upgrade to .net standard2.1
        /// <summary>
        /// WIll give list of active virtual drivers
        /// </summary>
        public static readonly List<KeyValuePair<string, DriverBase>> VirtualDrivers = [];


        public void DriverStarted(string AgentGuid)
        {
            DriverBase.VirtualDrivers.Add(new KeyValuePair<string, DriverBase>(AgentGuid, this));
        }

        public void DriverClosed()
        {
            foreach (var drvr in DriverBase.VirtualDrivers.Where(x => x.Value == this))
            {
                DriverBase.VirtualDrivers.Remove(drvr);
            }
        }

        #endregion

        public virtual void InitDriver(Agent agent)
        {
        }

        /// <summary>
        /// Name of customized edit page for the driver to load on Agent edit page
        /// </summary>
        public virtual string GetDriverConfigsEditPageName(Agent.eDriverType driverSubType = Agent.eDriverType.NA, IEnumerable<DriverConfigParam> driverConfigParams = null)
        {
            return null;
        }

        public virtual Point GetPointOnAppWindow(Point clickedPoint, double SrcWidth, double SrcHeight, double ActWidth, double ActHeight)
        {
            Point pointOnAppScreen = new Point();
            double ratio_X, ratio_Y;

            ratio_X = SrcWidth / ActWidth;
            ratio_Y = SrcHeight / ActHeight;

            pointOnAppScreen.X = (int)(clickedPoint.X * ratio_X);
            pointOnAppScreen.Y = (int)(clickedPoint.Y * ratio_Y);

            return pointOnAppScreen;
        }

        public virtual bool SetRectangleProperties(ref Point ElementStartPoints, ref Point ElementMaxPoints, double SrcWidth, double SrcHeight, double ActWidth, double ActHeight, Amdocs.Ginger.Common.UIElement.ElementInfo clickedElementInfo)
        {
            return false;
        }

        public virtual double ScreenShotInitialZoom()
        {
            return 0.5;
        }

        Regex AttRegexWeb = new("@[a-zA-Z]*", RegexOptions.Compiled);
        Regex AttRegexMobile = new("{[a-zA-Z]*}", RegexOptions.Compiled);
        public ElementLocator GetUserDefinedCustomLocatorFromTemplates(string locatorTemplate, eLocateBy locateBy, List<ControlProperty> elementProperties)
        {
            try
            {
                var locateValue = string.Empty;

                var attributeCount = 0;
                List<string> strList = [];

                MatchCollection attList;
                if (locateBy == eLocateBy.iOSPredicateString)
                {
                    attList = AttRegexMobile.Matches(locatorTemplate);

                    foreach (var item in attList)
                    {
                        strList.Add(item.ToString());
                    }

                    foreach (var item in elementProperties)
                    {
                        if (strList.Contains("{" + item.Name + "}"))
                        {
                            if (item.Value == "true")
                            {
                                item.Value = "1";
                            }
                            else if (item.Value == "false")
                            {
                                item.Value = "0";
                            }

                            locateValue = locatorTemplate.Replace("{" + item.Name + "}", item.Value);

                            locatorTemplate = locateValue;
                            attributeCount++;
                        }
                    }
                }
                else
                {
                    attList = AttRegexWeb.Matches(locatorTemplate);

                    foreach (var item in attList)
                    {
                        strList.Add(item.ToString().Remove(0, 1));
                    }

                    foreach (var item in elementProperties)
                    {
                        if (strList.Contains(item.Name))
                        {
                            locateValue = locatorTemplate.Replace(item.Name + "=\'\'", item.Name + "=\'" + item.Value + "\'");

                            locatorTemplate = locateValue;
                            attributeCount++;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(locateValue) && attributeCount == attList.Count) // && CheckElementLocateStatus(xPathTemplate))
                {
                    var elementLocator = new ElementLocator() { LocateBy = locateBy, LocateValue = locateValue, IsAutoLearned = true, Help = "Custom Locator evaluated from user template" };
                    return elementLocator;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error occurred during POM learining while learning Custom Locators", ex);
                return null;
            }
        }

        protected ProjEnvironment GetCurrentProjectEnvironment()
        {
            if (Environment == null)
            {
                foreach (ProjEnvironment env in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>())
                {
                    if (env.Name.Equals(BusinessFlow.Environment))
                    {
                        return env;
                    }
                }
            }

            return Environment;
        }

        public static int GetMaxTimeout(ActSmartSync act)
        {
            try
            {
                if (act.WaitTime.HasValue)
                {
                    return act.WaitTime.GetValueOrDefault();
                }
                else if (string.IsNullOrEmpty(act.GetInputParamValue("Value")))
                {
                    return 5;
                }
                else
                {
                    return Convert.ToInt32(act.GetInputParamCalculatedValue("Value"));
                }
            }
            catch (Exception)
            {
                return 5;
            }
        }

    }
}

