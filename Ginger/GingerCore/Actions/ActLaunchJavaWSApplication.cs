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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.Repository;
using GingerCore.Drivers;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
namespace GingerCore.Actions
{
    public class ActLaunchJavaWSApplication : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Launch Java Application"; } }
        public override string ActionUserDescription { get { return "Launch Java Application"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to launch Java Applications jnlps/jars like CRM/OMS or any other java app");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Use this action for attaching Ginger Java Agent to the launched Java application or to an Java application which is already running");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Note: If there are popups window like Java security warning, you can dismiss them using the SendKeys to window action");
            TBH.AddLineBreak();
        }

        public override string ActionEditPage { get { return "ActLaunchJavaWSApplicationEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }


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

        public override string ActionType
        {
            get { return "ActLaunchJavaWSApplication"; }
        }

        public new static partial class Fields
        {
            public static string JavaWSEXEPath = "JavaWSEXEPath"; //contains the Java version path in case user do not want to use JAVA_HOME

            public static string LaunchJavaApplication = "LaunchJavaApplication";    //flag to determine if to launch java application        
            public static string URL = "URL"; //the actual Java application path (jar/jnlp url)                    

            public static string LaunchWithAgent = "LaunchWithAgent"; //flag to determine if to Attach the Ginger Agent
            public static string JavaAgentPath = "JavaAgentPath"; //contains the Ginger Agent jars Folder path in case user do not want to use the defualt folder
            public static string Port = "Port"; //the port to configure the Ginger Agent to listen on
            public static string ShowAgent = "ShowAgent";   //flag to determine if to show Ginger Agent Console         

            public static string WaitForWindowWhenDoingLaunch = "WaitForWindowWhenDoingLaunch";   //flag to determine if to wait for java window with required title when launching java application
            public static string WaitForWindowTitle = "WaitForWindowTitle"; //the title of the Java application to wait for
            public static string WaitForWindowTitleMaxTime = "WaitForWindowTitleMaxTime";    //the max time in seconds to wait for the window to load
            public static string BlockingJavaWindow = "BlockingJavaWindow"; //search for blocking java window that popup before the application(Security, Update...) 
            public static string PortConfigParam = "PortConfigParam";
            public static string DynamicPortPlaceHolder = "DynamicPortPlaceHolder";
        }

        //------------------- Java version to use args
        string mJavaWSEXEPath = string.Empty;
        string mJavaWSEXEPath_Calc = string.Empty;
        [IsSerializedForLocalRepository]
        public string JavaWSEXEPath //contains the Java version path in case user do not want to use JAVA_HOME
        {
            get
            {
                return mJavaWSEXEPath;
            }
            set
            {
                mJavaWSEXEPath = value;
                OnPropertyChanged(Fields.JavaWSEXEPath);
            }
        }

        //------------------- Launch Java Application args 
        private bool mLaunchJavaApplication = true;
        [IsSerializedForLocalRepository(true)]
        public bool LaunchJavaApplication //flag to determine if to launch java application
        {
            get
            {
                return mLaunchJavaApplication;
            }
            set
            {
                mLaunchJavaApplication = value;
                OnPropertyChanged(Fields.LaunchJavaApplication);
            }
        }

        private string mURL = string.Empty;
        string mURL_Calc = string.Empty;
        [IsSerializedForLocalRepository]
        public string URL //the actual Java application path (jar/jnlp url)
        {
            get
            {
                if (mURL == string.Empty)//backward support
                {
                    if (InputValues.Where(x => x.Param == "URL" && x.Value != string.Empty).FirstOrDefault() != null)
                        mURL = GetInputParamValue("URL");
                }
                return mURL;
            }
            set
            {
                mURL = value;
                OnPropertyChanged(Fields.URL);
            }
        }

        //------------------- Attach Ginger Agent args 
        private bool mLaunchWithAgent = false;
        [IsSerializedForLocalRepository]
        public bool LaunchWithAgent //flag to determine if to Attach the Ginger Agent
        {
            get
            {
                return mLaunchWithAgent;
            }
            set
            {
                mLaunchWithAgent = value;
                OnPropertyChanged(Fields.LaunchWithAgent);
            }
        }
        private bool mBlockingJavaWindow = false;
        [IsSerializedForLocalRepository]
        public bool BlockingJavaWindow
        {
            get
            {
                return mBlockingJavaWindow;
            }
            set
            {
                mBlockingJavaWindow = value;
                OnPropertyChanged(Fields.BlockingJavaWindow);
            }
        }
        string mJavaAgentPath = string.Empty;
        string mJavaAgentPath_Calc = string.Empty;
        [IsSerializedForLocalRepository]
        public string JavaAgentPath//contains the Ginger Agent jars Folder path in case user do not want to use the defualt folder
        {
            get
            {
                return mJavaAgentPath;
            }
            set
            {
                mJavaAgentPath = value;
                OnPropertyChanged(Fields.JavaAgentPath);
            }
        }

        string mPort = string.Empty;
        string mPort_Calc = string.Empty;
        [IsSerializedForLocalRepository]
        public string Port //the port to configure the Ginger Agent to listen on
        {
            get
            {
                if (mPort == string.Empty)//backward support
                {
                    if (InputValues.Where(x => x.Param == "Port" && x.Value != string.Empty).FirstOrDefault() != null)
                        mPort = GetInputParamValue("Port");
                    else
                        mPort = "8888";
                }
                return mPort;
            }
            set
            {
                mPort = value;
                OnPropertyChanged(Fields.Port);
            }
        }

        private bool mShowAgent = true;
        [IsSerializedForLocalRepository(true)]
        public bool ShowAgent //flag to determine if to show Ginger Agent Console
        {
            get
            {
                return mShowAgent;
            }
            set
            {
                mShowAgent = value;
                OnPropertyChanged(nameof(ShowAgent));
            }
        }

        //------------------- Wait for Java application window args 
        private bool mWaitForWindowWhenDoingLaunch = false;
        [IsSerializedForLocalRepository]
        public bool WaitForWindowWhenDoingLaunch //flag to determine if to wait for java window application
        {
            get
            {
                return mWaitForWindowWhenDoingLaunch;
            }
            set
            {
                mWaitForWindowWhenDoingLaunch = value;
                OnPropertyChanged(Fields.WaitForWindowWhenDoingLaunch);
            }
        }

        private string mWaitForWindowTitle = "Login";
        string mWaitForWindowTitle_Calc = string.Empty;
        int mWaitForWindowTitleMaxTime_Calc_int = 0;
        [IsSerializedForLocalRepository]
        public string WaitForWindowTitle //the title of the Java application to wait for
        {
            get
            {
                return mWaitForWindowTitle;
            }
            set
            {
                mWaitForWindowTitle = value;
                OnPropertyChanged(Fields.WaitForWindowTitle);
            }
        }

        string mWaitForWindowTitleMaxTime = "60";
        string mWaitForWindowTitleMaxTime_Calc = string.Empty;
        [IsSerializedForLocalRepository]
        public string WaitForWindowTitleMaxTime //the max time in seconds to wait for the window to load
        {
            get
            {
                return mWaitForWindowTitleMaxTime;
            }
            set
            {
                mWaitForWindowTitleMaxTime = value;
                OnPropertyChanged(Fields.WaitForWindowTitleMaxTime);
            }
        }


        // ValueExpression mVE = null;

        enum URLExtensionType
        {
            JNLP,
            JAR,
            EXE
        }

        public enum ePortConfigType
        {
            [EnumValueDescription("Use explicitly specified port")]
            Manual,
            [EnumValueDescription("Auto detect available port")]
            AutoDetect
        }

        URLExtensionType mURLExtension = URLExtensionType.JNLP;
        private int mJavaApplicationProcessID = -1;

        private int mProcessIDForAttach = -1;

        private static readonly object syncLock = new object();

        AutoResetEvent portValueAutoResetEvent;

        public override void Execute()
        {
            mJavaApplicationProcessID = -1;
           
            //calculate the arguments
            if (!CalculateArguments()) return;

            //validate the arguments
            if (!ValidateArguments()) return;

            if (mLaunchJavaApplication)
            {
                if (!PerformLaunchJavaApplication()) return;
            }

            if (mLaunchWithAgent)
            {
                //For Parallel execution, we want attach to be synchronized.
                //So for windows with same title, correct process id will be calculated
                lock (syncLock)
                {
                    mProcessIDForAttach = -1;
                    if (!PerformAttachGingerAgent()) return;

                    if(mPort_Calc.Equals(Fields.DynamicPortPlaceHolder) &&
                        portValueAutoResetEvent!=null)
                    {
                        portValueAutoResetEvent.WaitOne(TimeSpan.FromSeconds(10));
                    }                 
                    AddOrUpdateReturnParamActual("Port", mPort_Calc);
                }
            }


            //Changing the existing param namefrom "Actual" to "Process ID", it Param "Actual" exist, to support Return params configured on old version
            ActReturnValue ARC = (from arc in ReturnValues where arc.Param == "Actual" select arc).FirstOrDefault();
            if (ARC != null)
            {
                ARC.Param = "Process ID";
                ARC.ParamCalculated = "Process ID";
            }

            AddOrUpdateReturnParamActual("Process ID", "" + mProcessIDForAttach);


        }


        private void CalculatePortValue()
        {
            //Action created with old version do not have this parameter. 
            //If running the action without opening the edit page, the param will be null
            //So we add the default value to param
            if (GetInputParamValue(Fields.PortConfigParam) == null)
            {
                AddOrUpdateInputParamValue(Fields.PortConfigParam, ePortConfigType.Manual.ToString());
            }

            ePortConfigType portConfigType = (ePortConfigType)GetInputParamValue<ePortConfigType>(Fields.PortConfigParam);
            if (portConfigType == ePortConfigType.Manual)
            {
                mPort_Calc = CalculateValue(mPort);
            }          
            else
            {
                //If port calculation is auto detect then we initialize autoreset event for it
                portValueAutoResetEvent = new AutoResetEvent(false);
            }
        }

        private string CalculateValue(string valueTocalc)
        {
            return ValueExpression.Calculate(valueTocalc).Trim();
        }

        private bool CalculateArguments()
        {
            try
            {
                mJavaWSEXEPath_Calc = CalculateValue(mJavaWSEXEPath);
                if (string.IsNullOrEmpty(mJavaWSEXEPath_Calc))
                {
                    mJavaWSEXEPath_Calc = CommonLib.GetJavaHome();
                }
                if (mJavaWSEXEPath_Calc.ToLower().Contains("bin") == false)
                {
                    mJavaWSEXEPath_Calc = Path.Combine(mJavaWSEXEPath_Calc, @"bin");
                }

                mURL_Calc = CalculateValue(mURL);

                mJavaAgentPath_Calc = CalculateValue(mJavaAgentPath);
                if (string.IsNullOrEmpty(mJavaAgentPath_Calc))
                    mJavaAgentPath_Calc = GetGingerAgentsDefaultFolder();

                CalculatePortValue();

                mWaitForWindowTitle_Calc = CalculateValue(mWaitForWindowTitle);
                mWaitForWindowTitleMaxTime_Calc = CalculateValue(mWaitForWindowTitleMaxTime);

                return true;
            }
            catch (Exception ex)
            {
                Error = "Error occurred while calculating the action arguments. Error=" + ex.Message;
                return false;
            }
        }

        private bool ValidateArguments()
        {
            try
            {
                if (mLaunchJavaApplication == false && mLaunchWithAgent == false)
                {
                    Error = "No action to perform was selected.";
                    return false;
                }

                if (Directory.Exists(mJavaWSEXEPath_Calc) == false || File.Exists(Path.Combine(mJavaWSEXEPath_Calc, "java.exe")) == false || File.Exists(Path.Combine(mJavaWSEXEPath_Calc, "javaws.exe")) == false)
                {
                    Error = "The Java path folder '" + mJavaWSEXEPath_Calc + "' is not valid, please select the Java Bin folder.";
                    return false;
                }

                if (mLaunchJavaApplication == true)
                {
                    bool isValidURL = SetURLExtensionType(mURL_Calc);
                    if (!isValidURL)
                    {
                        Error = "The provided Java application path '" + mURL_Calc + "' is not valid, please select .Jar or .Jnlp file or .Exe file";
                        return false;
                    }
                }

                if ((mLaunchJavaApplication == true && WaitForWindowWhenDoingLaunch == true) || mLaunchWithAgent == true)
                {
                    if (string.IsNullOrEmpty(mWaitForWindowTitle_Calc))
                    {
                        Error = "Missing valid Java application Window title to search for.";
                        return false;
                    }

                    if (string.IsNullOrEmpty(mWaitForWindowTitleMaxTime_Calc) || int.TryParse(mWaitForWindowTitleMaxTime_Calc, out mWaitForWindowTitleMaxTime_Calc_int) == false)
                    {
                        Error = "Max waiting time for java application window is not valid.";
                        return false;
                    }
                }

                if (mLaunchWithAgent == true)
                {
                    if (Directory.Exists(mJavaAgentPath_Calc) == false || File.Exists(Path.Combine(mJavaAgentPath_Calc, "GingerAgent.jar")) == false || File.Exists(Path.Combine(mJavaAgentPath_Calc, "GingerAgentStarter.jar")) == false)
                    {
                        Error = "The Ginger Agent path folder '" + mJavaAgentPath_Calc + "' is not valid, please select the folder which contains the 'GingerAgent.jar' &  'GingerAgentStarter.jar' files.";
                        return false;
                    }

                    ePortConfigType portConfigType = (ePortConfigType)GetInputParamValue<ePortConfigType>(Fields.PortConfigParam);
                    if (portConfigType== ePortConfigType.Manual)
                    {
                        return ValidatePort();
                    }                    
                }

                return true;
            }
            catch (Exception ex)
            {
                Error = "Error occurred while validating the action arguments. Error=" + ex.Message;
                return false;
            }
        }

        private bool ValidatePort()
        {
            try
            {
                int mPort_Calc_int = 0;
                if (string.IsNullOrEmpty(mPort_Calc) || int.TryParse(mPort_Calc, out mPort_Calc_int) == false)
                {
                    Error = "The Port number '" + mPort_Calc + "' is not valid.";
                    return false;
                }

            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception during parsing Port Value", ex);
                return false;
            }
            return true;

        }
        private bool SetURLExtensionType(string url)
        {

            if (string.IsNullOrEmpty(mURL_Calc))
            {
                return false;
            }

            string extension = Path.GetExtension(url).ToLower();

            switch (extension)
            {
                case ".jnlp":
                    mURLExtension = URLExtensionType.JNLP;
                    return true;

                case ".jar":
                    mURLExtension = URLExtensionType.JAR;
                    return true;

                case ".exe":
                    mURLExtension = URLExtensionType.EXE;
                    return true;

                default:
                    return false;
            }

        }

        private bool PerformLaunchJavaApplication()
        {
            string command = string.Empty;
            string javaExecuter = string.Empty;
            try
            {
                //choosing executer
                bool isJNLP = false;
                if (URLExtensionType.JNLP == mURLExtension)
                {
                    javaExecuter = Path.Combine(mJavaWSEXEPath_Calc, "javaws.exe");
                    isJNLP = true;
                }
                else if (URLExtensionType.JAR == mURLExtension)
                {
                    javaExecuter = Path.Combine(mJavaWSEXEPath_Calc, "java.exe");
                }
                // If extesion is exe then we keep java executer empty

                //arrange java application command params
                List<string> commandParams = new List<string>();
                foreach (ActInputValue AIV in this.InputValues)
                    if (!string.IsNullOrEmpty(AIV.Param) && !string.IsNullOrEmpty(AIV.ValueForDriver) && string.Compare(AIV.Param, "URL", true) != 0 && string.Compare(AIV.Param, "Port", true) != 0 && string.Compare(AIV.Param, "PortConfigParam", true) != 0)
                        commandParams.Add(AIV.Param + "=" + AIV.ValueForDriver);

                string commandParams_OneLine = string.Empty;
                if (commandParams.Count > 0)
                {
                    foreach (string param in commandParams)
                    {
                        if (commandParams_OneLine == string.Empty)
                        {
                            if (isJNLP)
                                commandParams_OneLine += "?" + param;
                            else
                                commandParams_OneLine += " " + "\"" + param + "\"";
                        }
                        else
                        {
                            if (isJNLP)
                                commandParams_OneLine += "&" + param;
                            else
                                commandParams_OneLine += " " + "\"" + param + "\"";
                        }
                    }
                }

                //command
                if (URLExtensionType.JNLP == mURLExtension)
                    command = "\"" + mURL_Calc + "\"";

                else if (URLExtensionType.JAR == mURLExtension)
                    command = "-jar \"" + mURL_Calc + "\"";

                // If it java exe then directly use the exe path as java executor
                else
                    javaExecuter = mURL_Calc;

                if (commandParams_OneLine != string.Empty)
                    command += commandParams_OneLine;

                //run commnad
                ExecuteCommandAsync(new List<string> { javaExecuter, command });

                if (String.IsNullOrEmpty(Error) != true)
                    return false;

                if (mWaitForWindowWhenDoingLaunch && !mLaunchWithAgent) // If wait for window is true and attach agent is false we wait for window to load. Else wait will be done when doing attach
                {
                    if (WaitForAppWindowTitle() == false)
                    {
                        Error = "Failed to find the Java application with title '" + mWaitForWindowTitle_Calc + "' after " + mWaitForWindowTitleMaxTime_Calc + " seconds.";
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Error = "Failed to launch the java application with the command '" + javaExecuter + " " + command + "'";
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
                return false;
            }
        }

        private bool PerformAttachGingerAgent()
        {
            string command = string.Empty;
            string javaExecuter = string.Empty;
            try
            {
                if (WaitForAppWindowTitle() == false)
                {
                    Error = "Failed to find the Java application with title '" + mWaitForWindowTitle_Calc + "' after " + mWaitForWindowTitleMaxTime_Calc + " seconds.";
                    return false;
                }

                //If Auto detect port then we get available port just before doing attach
                //So for parallel execution 2 runners won't get same Port                
                ePortConfigType portConfigType = (ePortConfigType)GetInputParamValue<ePortConfigType>(Fields.PortConfigParam);
                if (portConfigType == ePortConfigType.AutoDetect)
                {
                    mPort_Calc = Fields.DynamicPortPlaceHolder;                  
                }

                //choosing executer
                javaExecuter = Path.Combine(mJavaWSEXEPath_Calc, "java.exe");

                //arrange java application command params
                string commandParams_OneLine = string.Empty;
                commandParams_OneLine += "\"" + "AgentJarPath=" + Path.Combine(mJavaAgentPath_Calc, "GingerAgent.jar") + "\"";
                commandParams_OneLine += " \"" + "PID=" + mProcessIDForAttach.ToString() + "\"";
                commandParams_OneLine += " \"" + "ShowGingerAgentConsole=" + ShowAgent.ToString() + "\"";
                commandParams_OneLine += " \"" + "Port=" + mPort_Calc + "\"";

                //command
                command = "-jar " + "\"" + Path.Combine(mJavaAgentPath_Calc, "GingerAgentStarter.jar") + "\" " + commandParams_OneLine;

                //run commnad
                ExecuteCommandAsync(new List<string> { javaExecuter, command });

                if (String.IsNullOrEmpty(Error) != true)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Error = "Failed to attach the Ginger Agent with the command '" + javaExecuter + " " + command + "'";
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
                return false;
            }
        }

        private bool WaitForAppWindowTitle()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            bool bFound = false;

            // If Application Launch is done by Ginger then we already know the process id. No need to iterate.
            if (mJavaApplicationProcessID != -1 && ProcessExists(mJavaApplicationProcessID) && !IsInstrumentationModuleLoaded(mJavaApplicationProcessID))
            {
                while (!bFound)
                {
                    Process process = Process.GetProcessById(mJavaApplicationProcessID);

                    if (!IsInstrumentationModuleLoaded(process.Id))
                    {
                        if (BlockingJavaWindow)
                        {
                            bFound = CheckForBlockWindow(process);
                        }
                        else
                        {
                            bFound = MatchProcessTitle(process.MainWindowTitle.ToLower());
                        }
                        if (bFound)
                        {
                            mProcessIDForAttach = process.Id;
                        }
                    }

                    // Go out after max seconds
                    if (sw.ElapsedMilliseconds > mWaitForWindowTitleMaxTime_Calc_int * 1000)
                        break;

                    Thread.Sleep(1000);
                }
            }
            // If Application is not launched from Ginger then we go over the process to find the target Process ID
            else
            {
                while (!bFound)
                {
                    Process[] processlist = Process.GetProcesses();

                    List<Process> matchingProcessList = new List<Process>();

                    foreach (Process process in processlist)
                    {
                        if (process.StartInfo.Environment["USERNAME"] != Environment.UserName)
                        {
                            continue;
                        }
                        if (BlockingJavaWindow)
                        {
                            if (CheckForBlockWindow(process))
                            {
                                matchingProcessList.Add(process);
                            }
                        }

                        else if (MatchProcessTitle(process.MainWindowTitle.ToLower()))
                        {
                            matchingProcessList.Add(process);
                        }
                    }


                    if (matchingProcessList.Count == 1)
                    {
                        if (!IsInstrumentationModuleLoaded(matchingProcessList.ElementAt(0).Id))
                        {
                            bFound = true;
                            mProcessIDForAttach = matchingProcessList.ElementAt(0).Id;
                        }
                    }
                    else if (matchingProcessList.Count > 1)
                    {
                        foreach (Process process in matchingProcessList)
                        {
                            if ((process.ProcessName.StartsWith("java", StringComparison.CurrentCultureIgnoreCase) || process.ProcessName.StartsWith("jp2", StringComparison.CurrentCultureIgnoreCase)))
                            {
                                if (!IsInstrumentationModuleLoaded(process.Id))
                                {
                                    bFound = true;
                                    mProcessIDForAttach = process.Id;
                                    break;
                                }
                            }
                        }
                    }

                    // Go out after max seconds
                    if (sw.ElapsedMilliseconds > mWaitForWindowTitleMaxTime_Calc_int * 1000)
                        break;

                    Thread.Sleep(1000);
                }
            }

            return bFound;

        }
        private bool CheckForBlockWindow(Process p)
        {
            bool bFound = false;
            if ((p.ProcessName.StartsWith("java", StringComparison.CurrentCultureIgnoreCase) || p.ProcessName.StartsWith("jp2", StringComparison.CurrentCultureIgnoreCase)))
            {
                bFound = FindProcessWindowTitle(p, mWaitForWindowTitle_Calc);
            }
            return bFound;
        }

        private bool FindProcessWindowTitle(Process p, string waitForWindowTilte)//no need to get all windows.
        {
            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement window = walker.GetFirstChild(AutomationElement.RootElement);
            UIAComWrapperHelper uiHelper = new UIAComWrapperHelper();
            while (window != null)
            {
                if (window.Current.ProcessId == p.Id)
                {
                    String WindowTitle = "";
                    WindowTitle = uiHelper.GetWindowInfo(window);
                    if (!string.IsNullOrEmpty(WindowTitle))
                    {
                        if (WindowTitle.ToLower().Contains(waitForWindowTilte.ToLower()))
                        {
                            return true;
                        }
                    }
                }
                try
                {
                    window = walker.GetNextSibling(window);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception in FindProcessWindowTitle", ex);
                }
            }
            return false;
        }
        private bool MatchProcessTitle(string title)
        {
            string NoTitleWindow = "no title window";// this was added earlier but not in sync with other drivers.
            string NoTitleWindow_2 = "NoTitleWindow";// In PB we use NoTitleWindow for blank title windows. So adding this to have uniformity across the drivers.
            if ((mWaitForWindowTitle_Calc.ToLower().Contains(NoTitleWindow) || mWaitForWindowTitle_Calc.ToLower().Contains(NoTitleWindow_2.ToLower())) && title.Equals(String.Empty))
            {
                return true;
            }

            else if (title.Contains(mWaitForWindowTitle_Calc.ToLower()))
            {
                return true;
            }

            return false;
        }

        public void ExecuteCommandAsync(List<string> command)
        {
            try
            {
                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
                //Make the thread as background thread.
                objThread.IsBackground = true;
                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                //Start the thread.
                objThread.Start(command);
            }
            catch (Exception ex)
            {
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                Error = ex.Message;
            }
        }

        public void ExecuteCommandSync(object command)
        {
            try
            {
                List<string> commnadConfigs = (List<string>)command;
                if(commnadConfigs[1].Contains(Fields.DynamicPortPlaceHolder))
                {
                    mPort_Calc = SocketHelper.GetOpenPort().ToString();
                    portValueAutoResetEvent.Set();
                    commnadConfigs[1] = commnadConfigs[1].Replace("DynamicPortPlaceHolder", mPort_Calc);                   
                }
                
                ExInfo += "Executing Command: " + commnadConfigs[0] + " " + commnadConfigs[1] + Environment.NewLine;

                Process p = new Process();
                p.StartInfo.FileName = commnadConfigs[0];
                p.StartInfo.WorkingDirectory = mJavaWSEXEPath_Calc;
                p.StartInfo.Arguments = commnadConfigs[1];
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.Start();
                mJavaApplicationProcessID = p.Id;
                string result = p.StandardOutput.ReadToEnd();
                ExInfo += result + Environment.NewLine;
                string error = p.StandardError.ReadToEnd();
                if (error != "")
                {
                    //Status = eStatus.Failed;
                    if (Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
                    {
                        Error = "Failed to execute the command, Error: " + error;
                    }
                    else
                    {
                        ExInfo += error;
                    }

                }
            }
            catch (Exception ex)
            {
                //Status = eStatus.Fail;
                Error = "Failed to execute the command, Error: " + ex.Message;
            }
        }


        public String GetGingerAgentsDefaultFolder()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\StaticDrivers\";
        }


        private bool ProcessExists(int processId)
        {
            Process p = Process.GetProcesses().FirstOrDefault(x => x.Id == processId);

            if (p != null && !p.HasExited)
            {
                return true;
            }

            return false;
        }

        private Boolean IsInstrumentationModuleLoaded(int id)
        {
            //This method will check if process modules contains instrument dll
            //if yes that means java agent is already attached to this process
            try
            {
                Process process = Process.GetProcessById(id);
                ProcessModuleCollection processModules = process.Modules;
                ProcessModule module;

                int modulesCount = processModules.Count - 1;
                // we start from end, because instrument dll is always loaded after rest of the modules are already loaded
                //So we look for only last 5 modules
                for (int i = modulesCount; i > modulesCount - 5; i--)
                {
                    module = processModules[i];

                    if (module.ModuleName.Equals("instrument.dll"))
                        return true;
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception when checking IsInstrumentationModuleLoaded", e);
            }

            return false;
        }


    }
}
