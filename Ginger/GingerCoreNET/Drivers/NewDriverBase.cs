#region License
/*
Copyright © 2014-2018 European Support Limited

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

using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;

namespace GingerCoreNET.Drivers
{
    public abstract class NewDriverBase
    {
        //Basic configurations
        [UserConfigured]
        [UserConfiguredDefault("30")]
        [UserConfiguredDescription("The amount of maximum time (in seconds) to wait for the driver to load/connect")]
        public int DriverLoadWaitingTime { get; set; }

        //If this is WPF Window like IB, or Device/Mobile, Unix need to run on it's own STA

        public bool PreviousRunStopped { get; set; }

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
        }

        public abstract void StartDriver();
        public abstract void CloseDriver();
        public string DriverClass { get { return this.GetType().ToString(); } }

        // running an action on the UI
        //public abstract void RunAction(Act act);

        //// UI Inspector
        //public abstract void HighlightActElement(Act act);
        //public BusinessFlow BusinessFlow { get; set; }
        //public ProjEnvironment Environment { get; set; }
        //public abstract Platform.ePlatformType Platform { get; }
        public abstract bool IsRunning();

        // Input to this method is call back funtion to handle the events

        //TODO: Later on make this abstract
        // public virtual void StartRecording(Action<object, ActionName> CreateActionHandler){}
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
            //DriverMessageEventHandler handler = driverMessageEventHandler;
            //if (handler != null)
            //{
            //    handler(this, new DriverMessageEventArgs(DriverMessageType));
            //}
        }

        //public delegate void DriverMessageEventHandler(object sender, DriverMessageEventArgs e);
        // public event DriverMessageEventHandler driverMessageEventHandler;
    }
}