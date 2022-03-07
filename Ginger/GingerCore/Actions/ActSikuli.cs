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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.Repository;
using GingerCore.Properties;
using GingerCore.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using GingerCore.Platforms;
using System.Runtime.InteropServices;
using GingerCore.Helpers;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Automation;
using GingerCore.Drivers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;
using SikuliStandard.sikuli_UTIL;
using SikuliStandard.sikuli_REST;
using Amdocs.Ginger.Common;
using System.Diagnostics;
using System.Linq;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCore.Actions
{
    public class ActSikuli : ActImageCaptureSupport
    {
        public override string ActionDescription { get { return "Sikuli Operation"; } }
        public override string ActionUserDescription { get { return "Sikuli Operation"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you are unable to perform interactive operations using platform driver.");
            TBH.AddLineBreak();
            TBH.AddText("This action provides capability to interact with an active GUI based application window and perform basic interactive operations.");
        }

        public override string ActionEditPage { get { return "ActSikuliEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        public enum eActSikuliOperation
        {
            Click,
            DoubleClick,
            SetValue,
            //GetValue,
            Exist,
            MouseRightClick
        }

        public string WindowTitle
        {
            get
            {
                return GetOrCreateInputParam(nameof(WindowTitle)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(WindowTitle), value);
            }
        }
        public eActSikuliOperation ActSikuliOperation
        {
            get
            {
                return (eActSikuliOperation)GetOrCreateInputParam<eActSikuliOperation>(nameof(ActSikuliOperation), eActSikuliOperation.SetValue);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ActSikuliOperation), value.ToString());
            }
        }

        public override String ActionType
        {
            get
            {
                return "Sikuli based operations execution Action";
            }
        }

        public string PatternPath
        {
            get
            {
                return GetOrCreateInputParam(nameof(PatternPath)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(PatternPath), value);
            }
        }
        public bool ShowSikuliConsole
        {
            get
            {
                bool value = false;
                bool.TryParse(GetOrCreateInputParam(nameof(ShowSikuliConsole)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ShowSikuliConsole), value.ToString());
            }
        }
        public string SetTextValue
        {
            get
            {
                return GetOrCreateInputParam(nameof(SetTextValue)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetTextValue), value);
            }
        }
        public int ProcessIDForSikuliOperation
        {
            get
            {
                int procId = -1;
                //if (!string.IsNullOrEmpty(ProcessNameForSikuliOperation) && ActiveProcessWindowsDict.ContainsValue(ProcessNameForSikuliOperation))
                //{
                //    procId = ActiveProcessWindowsDict.Where(d => d.Value == ProcessNameForSikuliOperation).FirstOrDefault().Key;
                //}

                return procId;
            }
        }

        public string ProcessNameForSikuliOperation
        {
            get
            {
                return GetOrCreateInputParam(nameof(ProcessNameForSikuliOperation)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ProcessNameForSikuliOperation), value);
            }
        }

        public override eImageType Image { get { return eImageType.BullsEye; } }

        private List<AutomationElement> lstWindows = new List<AutomationElement>();

        public void SetFocusToSelectedApplicationInstance()
        {
            if (!string.IsNullOrEmpty(ProcessNameForSikuliOperation))
            {
                WinAPIAutomation.ShowWindow(lstWindows.Where(m => m.Current.Name.Equals(ProcessNameForSikuliOperation)).First());
            }
        }

        public override void Execute()
        {
            if (CheckIfImageValid())
            {
                string logMessage = string.Empty;
                APILauncher sikuliLauncher = new APILauncher(out logMessage, ShowSikuliConsole);
                sikuliLauncher.EvtLogMessage += sikuliLauncher_EvtLogMessage;
                sikuliLauncher.Start();

                try
                {
                    Screen sekuliScreen = new Screen();

                    Pattern sikuliPattern = new Pattern(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(PatternPath));

                    System.Threading.Tasks.Task.Run(() => SetFocusToSelectedApplicationInstance());

                    switch (ActSikuliOperation)
                    {
                        case eActSikuliOperation.Click:
                            sekuliScreen.Click(sikuliPattern);
                            break;
                        case eActSikuliOperation.SetValue:
                            sekuliScreen.Type(sikuliPattern, SetTextValue);
                            break;
                        case eActSikuliOperation.DoubleClick:
                            sekuliScreen.DoubleClick(sikuliPattern);
                            break;
                        case eActSikuliOperation.MouseRightClick:
                            sekuliScreen.RightClick(sikuliPattern);
                            break;
                        case eActSikuliOperation.Exist:
                            sekuliScreen.Exists(sikuliPattern);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, ex.Message + Environment.NewLine + ex.Source, ex);
                    Error = string.Format("Error Occured while executing Sikuli Operation {0} : {1}", ActSikuliOperation, ex.Message);
                }
                finally
                {
                    sikuliLauncher.Stop();
                }
            }
        }

        private void sikuliLauncher_EvtLogMessage(object sender, EventArgs e)
        {
            SikuliErrorModel ex = (SikuliErrorModel)sender;
            if (ex.Exception != null)
            {
                Reporter.ToLog(eLogLevel.INFO, ex.Message);
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex.Exception);
            }
        }

        List<string> ActiveProcessWindowsList = new List<string>();

        public List<string> ActiveProcessWindows
        {
            get
            {
                RefreshActiveProcessesTitles();
                return ActiveProcessWindowsList;
            }
        }

        void RefreshActiveProcessesTitles()
        {
            UIAComWrapperHelper uiHelper = new UIAComWrapperHelper();
            uiHelper.mPlatform = GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Windows;
            List<object> lstAppWindow = uiHelper.GetListOfWindows();
            ActiveProcessWindowsList.Clear();
            lstWindows.Clear();
            foreach (AutomationElement process in lstAppWindow)
            {
                // If the process appears on the Taskbar (if has a title)
                // print the information of the process
                if (!String.IsNullOrEmpty(process.Current.Name))
                {
                    ActiveProcessWindowsList.Add(process.Current.Name);
                    lstWindows.Add(process);
                }
            }
        }

        public override int ClickX
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(ClickX)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ClickX), value.ToString());
            }
        }
        public override int ClickY
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(ClickY)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ClickY), value.ToString());
            }
        }
        public override int StartX
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(StartX)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(StartX), value.ToString());
            }
        }
        public override int StartY
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(StartY)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(StartY), value.ToString());
            }
        }
        public override int EndX
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(EndX)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(EndX), value.ToString());
            }
        }
        public override int EndY
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(EndY)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(EndY), value.ToString());
            }
        }
        public override string LocatorImgFile { get; set; }
        public override string ImagePath
        {
            get
            {
                return @"Documents\SikuliImages\";
            }
        }

        private bool CheckIfImageValid()
        {
            if (string.IsNullOrEmpty(PatternPath))
            {
                Error = "File Path is Empty";
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "File Path is Empty");
                return false;
            }
            if (!File.Exists(PatternPath))
            {
                Error = "File Path is Invalid";
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "File Path is Invalid");
                return false;
            }
            return true;
        }
    }
}
