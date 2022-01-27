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

namespace GingerCore.Actions
{
    public class ActSikuli : ActImageCaptureSupport
    {
        public override string ActionDescription { get { return "Sikuli Operation"; } }
        public override string ActionUserDescription { get { return string.Empty; } }

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
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Mobile);
                    mPlatforms.Add(ePlatformType.Windows);
                    mPlatforms.Add(ePlatformType.Java);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                    mPlatforms.Add(ePlatformType.MainFrame);
                }
                return mPlatforms;
            }
        }

        public enum eActSikuliOperation
        {
            Click,
            DoubleClick,
            SetValue,
            GetValue,
            Exist,
            MouseRightClick
        }

        [IsSerializedForLocalRepository]
        public string WindowTitle { get; set; }
        [IsSerializedForLocalRepository]
        public eActSikuliOperation ActSikuliOperation { get; set; }

        public override String ActionType
        {
            get
            {
                return "Sikuli based operations execution Action";
            }
        }

        [IsSerializedForLocalRepository]
        public string PatternPath { get; set; }
        [IsSerializedForLocalRepository]
        public bool ShowSikuliConsole { get; set; }
        [IsSerializedForLocalRepository]
        public string SetTextValue { get; set; }
        public int ProcessIDForSikuliOperation
        {
            get
            {
                int procId = -1;
                if (!string.IsNullOrEmpty(ProcessNameForSikuliOperation) && ActiveProcessWindowsDict.ContainsValue(ProcessNameForSikuliOperation))
                {
                    procId = ActiveProcessWindowsDict.Where(d => d.Value == ProcessNameForSikuliOperation).FirstOrDefault().Key;
                }

                return procId;
            }
        }

        [IsSerializedForLocalRepository]
        public string ProcessNameForSikuliOperation { get; set; }

        public override eImageType Image { get { return eImageType.BullsEye; } }

        public void SetFocusToSelectedApplicationInstance()
        {
            if (ProcessIDForSikuliOperation != -1)
            {
                WinAPIAutomation.SetForeGroundWindow(ProcessIDForSikuliOperation);
            }
        }

        public override void Execute()
        {
            string logMessage = string.Empty;
            APILauncher sikuliLauncher = new APILauncher(out logMessage, ShowSikuliConsole);
            sikuliLauncher.Start();

            try
            {
                Screen sekuliScreen = new Screen();

                Pattern sikuliPattern = new Pattern(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(PatternPath));

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

        Dictionary<int, string> ActiveProcessWindowsDict = new Dictionary<int, string>();

        public List<string> ActiveProcessWindows
        {
            get
            {
                RefreshActiveProcessesTitles();
                return ActiveProcessWindowsDict.Values.ToList();
            }
        }

        void RefreshActiveProcessesTitles()
        {
            Process[] processList = Process.GetProcesses();

            ActiveProcessWindowsDict.Clear();
            foreach (Process process in processList)
            {
                // If the process appears on the Taskbar (if has a title)
                // print the information of the process
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    ActiveProcessWindowsDict.Add(process.Id, process.MainWindowTitle);
                    //Console.WriteLine("Process:   {0}", process.ProcessName);
                    //Console.WriteLine("    ID   : {0}", process.Id);
                    //Console.WriteLine("    Title: {0} \n", process.MainWindowTitle);
                }
            }
        }

        [IsSerializedForLocalRepository]
        public override int ClickX { get; set; }
        [IsSerializedForLocalRepository]
        public override int ClickY { get; set; }
        [IsSerializedForLocalRepository]
        public override int StartX { get; set; }
        [IsSerializedForLocalRepository]
        public override int StartY { get; set; }
        [IsSerializedForLocalRepository]
        public override int EndX { get; set; }
        [IsSerializedForLocalRepository]
        public override int EndY { get; set; }
        [IsSerializedForLocalRepository]
        public override string LocatorImgFile { get; set; }
        public override string ImagePath
        {
            get
            {
                return @"Documents\SikuliImages\";
            }
        }
    }
}
