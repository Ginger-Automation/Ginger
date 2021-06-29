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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

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

        public bool mStopProcess { get; set; }

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
        }

        public void OnDriverMessage(eDriverMessageType DriverMessageType)
        {
            DriverMessageEventHandler handler = DriverMessageEvent;
            if (handler != null)
            {
                handler(this, new DriverMessageEventArgs(DriverMessageType));
            }
        }

        public delegate void DriverMessageEventHandler(object sender, DriverMessageEventArgs e);
        public event DriverMessageEventHandler DriverMessageEvent;

        public virtual void ActionCompleted(Act act)
        {
            // Do nothing, can be implemented in sub class like: ConsoleDriverBase, WindowsDriver, PBDriver
        }

        public virtual void UpdateContext(Context context)
        {
            BusinessFlow = context.BusinessFlow;
        }


        #region VirtualDrivers

        //TODO: .net STandard 2.1 Use C# 8 default interface implmentation  once we upgrade to .net standard2.1
        /// <summary>
        /// WIll give list of active virtual drivers
        /// </summary>
        public static readonly List<KeyValuePair<string, DriverBase>> VirtualDrivers = new List<KeyValuePair<string, DriverBase>>();


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
        public virtual string GetDriverConfigsEditPageName(Agent.eDriverType driverSubType = Agent.eDriverType.NA)
        {
            return null;
        }

        /// <summary>
        /// Used for handling serialization adjustments for legacy properties or values
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="errorType"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool SerializationError(Agent agent, SerializationErrorType errorType, string name, string value)
        {
            return false;
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
    }
}

