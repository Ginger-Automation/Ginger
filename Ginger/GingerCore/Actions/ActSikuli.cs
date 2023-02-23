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

extern alias UIAComWrapperNetstandard;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;
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
using amdocs.ginger.GingerCoreNET;
using GingerCore.DataSource;

namespace GingerCore.Actions
{
    public class ActSikuli : ActImageCaptureSupport
    {
        public override string ActionDescription { get { return "Image Based Operation"; } }
        public override string ActionUserDescription { get { return "Image based locator and operation using Sikuli"; } }

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
            GetValue,
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
                return (eActSikuliOperation)GetOrCreateInputParam<eActSikuliOperation>(nameof(ActSikuliOperation), eActSikuliOperation.Click);
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
                return "Image based locator and operation using Sikuli";
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

        public string PatternSimilarity
        {
            get
            {
                return GetOrCreateInputParam(nameof(PatternSimilarity)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(PatternSimilarity), value);
            }
        }

        public override eImageType Image { get { return eImageType.BullsEye; } }

        private List<UIAuto.AutomationElement> lstWindows = new List<UIAuto.AutomationElement>();

        public static new partial class Fields
        {
            public static readonly string ChangeAppWindowSize = "ChangeAppWindowSize";
        }

        public enum eChangeAppWindowSize
        {
            [EnumValueDescription("")]
            None,
            [EnumValueDescription("640 x 480")]
            Resolution640x480,
            [EnumValueDescription("800 x 600")]
            Resolution800x600,
            [EnumValueDescription("1024 x 768")]
            Resolution1024x768,
            [EnumValueDescription("1280 x 800")]
            Resolution1280x800,
            [EnumValueDescription("1280 x 1024")]
            Resolution1280x1024,
            [EnumValueDescription("1366 x 768")]
            Resolution1366x768,
            [EnumValueDescription("1920 x 1080")]
            Resolution1920x1080,
        }
        public bool SetCustomResolution
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

        public eChangeAppWindowSize ChangeAppWindowSize
        {
            get
            {
                eChangeAppWindowSize eVal = eChangeAppWindowSize.None;
                if (Enum.TryParse(GetInputParamValue(Fields.ChangeAppWindowSize), out eVal))
                {
                    return eVal;
                }
                else
                {
                    return eChangeAppWindowSize.None;
                }
            }

            set
            {
                GetOrCreateInputParam(Fields.ChangeAppWindowSize).Value = value.ToString();
            }
        }

        [IsSerializedForLocalRepository]
        public bool UseCustomJava { get; set; }

        private string mCustomJavaPath = string.Empty;

        public string CustomJavaPath
        {
            get
            {
                return GetOrCreateInputParam(nameof(CustomJavaPath)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(CustomJavaPath), value);
            }
        }

        public void SetFocusToSelectedApplicationInstance()
        {
            string processNameForSikuli = ValueExpression.Calculate(ProcessNameForSikuliOperation);
            if (!string.IsNullOrEmpty(processNameForSikuli))
            {
                if (lstWindows.Count == 0)
                {
                    RefreshActiveProcessesTitles();
                }
                if (lstWindows.Count != 0)
                {
                    WinAPIAutomation.ShowWindow(lstWindows.Where(m => m.Current.Name.Equals(processNameForSikuli)).First());
                    if (SetCustomResolution)
                    {
                        List<int> lstVal = GetCustomResolutionValues();
                        if (lstVal.Count == 2)
                        {
                            WinAPIAutomation.ResizeExternalWindow(lstWindows.Where(m => m.Current.Name.Equals(processNameForSikuli)).First(), lstVal[0], lstVal[1]);
                        }
                    }
                }
            }
        }

        public override async void Execute()
        {
            string veProcessName = ProcessNameForSikuliOperation;
            if (CheckIfImageValidAndIfPercentageValidAndSelectedApplicationValid())
            {
                string logMessage = string.Empty;
                APILauncher sikuliLauncher = new APILauncher(out logMessage, ShowSikuliConsole, UseCustomJava,
                                                             ValueExpression.Calculate(CustomJavaPath));
                if (!ActSikuliOperation.Equals(eActSikuliOperation.GetValue))
                {
                    sikuliLauncher.EvtLogMessage += sikuliLauncher_EvtLogMessage;
                    sikuliLauncher.Start();
                }
                try
                {
                    Screen sekuliScreen = new Screen();

                    Pattern sikuliPattern = new Pattern(WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(
                                                        ValueExpression.Calculate(PatternPath)), new Point(0, 0), PatternSimilarity);

                    if (!ActSikuliOperation.Equals(eActSikuliOperation.GetValue))
                    {
                        System.Threading.Tasks.Task.Run(() => SetFocusToSelectedApplicationInstance());
                    }
                    switch (ActSikuliOperation)
                    {
                        case eActSikuliOperation.Click:
                            sekuliScreen.Click(sikuliPattern);
                            break;
                        case eActSikuliOperation.SetValue:
                            sekuliScreen.Type(sikuliPattern, ValueExpression.Calculate(SetTextValue));
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
                        case eActSikuliOperation.GetValue:
                            string txtOutput = GingerOCR.GingerOcrOperations.ReadTextFromImage(sikuliPattern.ImagePath);
                            if (!string.IsNullOrEmpty(txtOutput))
                            {
                                Dictionary<string, object> dctOutput = new Dictionary<string, object>();
                                dctOutput.Add("output", txtOutput);
                                AddToOutputValues(dctOutput);
                            }
                            else
                            {
                                Error = "Unable to read text from image";
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, ex.Message + Environment.NewLine + ex.Source, ex);
                    Error = string.Format("Error Occured while executing Sikuli Operation {0} : {1}", ActSikuliOperation, ex.Message);
                    if (Error.Contains("No connection could be made "))
                    {
                        Error += " , please try running Ginger as Administrator";
                    }
                }
                finally
                {
                    if (!ActSikuliOperation.Equals(eActSikuliOperation.GetValue))
                    {
                        await sikuliLauncher.Stop();
                    }
                }
            }
            ProcessNameForSikuliOperation = veProcessName;
        }

        private void sikuliLauncher_EvtLogMessage(object sender, EventArgs e)
        {
            SikuliErrorModel ex = (SikuliErrorModel)sender;
            if (ex.Exception == null)
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
            foreach (UIAuto.AutomationElement process in lstAppWindow)
            {
                // If the process appears on the Taskbar (if has a title)
                // print the information of the process
                if (!String.IsNullOrEmpty(process.Current.Name))
                {
                    ActiveProcessWindowsList.Add(process.Current.Name);
                    lstWindows.Add(process);
                }
            }
            if (!string.IsNullOrEmpty(ProcessNameForSikuliOperation) &&
                !ActiveProcessWindowsList.Contains(ProcessNameForSikuliOperation))
            {
                ActiveProcessWindowsList.Add(ProcessNameForSikuliOperation);
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

        private bool CheckIfImageValidAndIfPercentageValidAndSelectedApplicationValid()
        {
            SetProcessAsPerVE();
            if (string.IsNullOrEmpty(ValueExpression.Calculate(PatternPath)))
            {
                Error = "File Path is Empty";
                return false;
            }
            if (!File.Exists(WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(
                ValueExpression.Calculate(PatternPath))))
            {
                Error = "File Path is Invalid";
                return false;
            }
            double result = 0;
            if (!double.TryParse(ValueExpression.Calculate(PatternSimilarity), out result))
            {
                Error = "Please enter a valid percentage similarity";
                return false;
            }
            else
            {
                if (result < 0 || result > 100)
                {
                    Error = "Percentage Similarity should be between 0-100";
                    return false;
                }
            }
            if (!ActSikuliOperation.Equals(eActSikuliOperation.GetValue))
            {
                RefreshActiveProcessesTitles();
                if (lstWindows.Where(m => m.Current.Name.Equals(ProcessNameForSikuliOperation)) == null ||
                    lstWindows.Where(m => m.Current.Name.Equals(ProcessNameForSikuliOperation)).Count() == 0)
                {
                    Error = "Target Application is not running";
                    return false;
                }
            }
            if (UseCustomJava)
            {
                if (string.IsNullOrEmpty(ValueExpression.Calculate(CustomJavaPath)))
                {
                    Error = "Java Version Path cannot be empty";
                    return false;
                }
                else
                {
                    string customJavaPath = ValueExpression.Calculate(CustomJavaPath);
                    string customBinPath = Path.Combine(customJavaPath, @"bin");
                    string customExePath = Path.Combine(customBinPath, @"java.exe");
                    string customWExePath = Path.Combine(customBinPath, @"javaw.exe");
                    if (!File.Exists(customExePath))
                    {
                        Error = "java.exe is missing inside bin folder " + customJavaPath;
                        return false;
                    }
                    if (!File.Exists(customWExePath))
                    {
                        Error = "javaw.exe is missing inside bin folder " + customJavaPath;
                        return false;
                    }
                }
            }
            return true;
        }

        private List<int> GetCustomResolutionValues()
        {
            List<int> lstVal = new List<int>();
            switch (ChangeAppWindowSize)
            {
                case eChangeAppWindowSize.Resolution640x480:
                    lstVal.Add(640);
                    lstVal.Add(480);
                    break;
                case eChangeAppWindowSize.Resolution800x600:
                    lstVal.Add(800);
                    lstVal.Add(600);
                    break;
                case eChangeAppWindowSize.Resolution1024x768:
                    lstVal.Add(1024);
                    lstVal.Add(768);
                    break;
                case eChangeAppWindowSize.Resolution1280x800:
                    lstVal.Add(1280);
                    lstVal.Add(800);
                    break;
                case eChangeAppWindowSize.Resolution1280x1024:
                    lstVal.Add(1280);
                    lstVal.Add(1024);
                    break;
                case eChangeAppWindowSize.Resolution1366x768:
                    lstVal.Add(1366);
                    lstVal.Add(768);
                    break;
                case eChangeAppWindowSize.Resolution1920x1080:
                    lstVal.Add(1920);
                    lstVal.Add(1080);
                    break;
                default:
                    break;
            }

            return lstVal;
        }

        private void SetProcessAsPerVE()
        {
            string calculateValue = GetInputParamCalculatedValue(nameof(ProcessNameForSikuliOperation));
            bool bSimilar = ActiveProcessWindows.Any(p => p.Contains(calculateValue));
            if (bSimilar)
            {
                ProcessNameForSikuliOperation = ActiveProcessWindows.First(p => p.Contains(calculateValue));
            }
            else
            {
                ProcessNameForSikuliOperation = String.Empty;
            }
        }
    }
}
