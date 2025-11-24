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


extern alias UIAComWrapperNetstandard;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
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
using System.Threading.Tasks;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCore.Actions
{
    public class ActLaunchJavaWSApplication : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Launch Java Application"; } }
        public override eImageType Image { get { return eImageType.Java; } }
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
            public static string AttachAgentProcessSyncTime = "AttachAgentProcessSyncTime";    //the max time in seconds to wait for the process holding the mutex
            public static string BlockingJavaWindow = "BlockingJavaWindow"; //search for blocking java window that popup before the application(Security, Update...) 
            public static string PortConfigParam = "PortConfigParam";
            public static string DynamicPortPlaceHolder = "DynamicPortPlaceHolder";
            public static string IsCustomApplicationProcessName = "IsCustomApplicationProcessName";
            public static string ApplicationProcessName = "ApplicationProcessName";


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
                    if (InputValues.FirstOrDefault(x => x.Param == "URL" && x.Value != string.Empty) != null)
                    {
                        mURL = GetInputParamValue("URL");
                    }
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
                    if (InputValues.FirstOrDefault(x => x.Param == "Port" && x.Value != string.Empty) != null)
                    {
                        mPort = GetInputParamValue("Port");
                    }
                    else
                    {
                        mPort = "8888";
                    }
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

        int mWaitForWindowTitleMaxTime_Calc_int = 60;
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

        private int mAttachAgentProcessSyncTime_Calc_int = 120;
        private string mAttachAgentProcessSyncTime = "120";
        private string mAttachAgentProcessSyncTime_Calc = string.Empty;
        [IsSerializedForLocalRepository]
        public string AttachAgentProcessSyncTime //the max time in seconds to wait for the window to load
        {
            get
            {
                return mAttachAgentProcessSyncTime;
            }
            set
            {
                mAttachAgentProcessSyncTime = value;
                OnPropertyChanged(Fields.AttachAgentProcessSyncTime);
            }
        }

        //------------------- application process name to attach
        string mApplicationProcessName = string.Empty;
        string mJApplicationProcessName_calc = string.Empty;
        [IsSerializedForLocalRepository]
        public string ApplicationProcessName
        {
            get
            {
                return mApplicationProcessName;
            }
            set
            {
                mApplicationProcessName = value;
                OnPropertyChanged(Fields.ApplicationProcessName);
            }
        }     


        private bool mIsCustomApplicationProcessName = false;
        [IsSerializedForLocalRepository]
        public bool IsCustomApplicationProcessName
        {
            get
            {
                return mIsCustomApplicationProcessName;
            }
            set
            {
                mIsCustomApplicationProcessName = value;
                OnPropertyChanged(Fields.IsCustomApplicationProcessName);
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

        private static Mutex mutex = new Mutex(false, "JavaAgentAttachMutex");

        private AutoResetEvent mPortValueAutoResetEvent;

        private CancellationTokenSource mAttachAgentCancellationToken = null;
        private Task mAttachAgentTask = null;
        private bool mWaitForWindowTimeOut = false;
        private readonly List<string> processNames = [];
        public override void Execute()
        {
            processNames.Clear();
            processNames.Add("java");
            processNames.Add("jp2");

            mJavaApplicationProcessID = -1;

            //calculate the arguments
            if (!CalculateArguments())
            {
                return;
            }

            //validate the arguments
            if (!ValidateArguments())
            {
                return;
            }

            if (mLaunchJavaApplication)
            {
                if (!PerformLaunchJavaApplication())
                {
                    return;
                }
            }

            if (mLaunchWithAgent)
            {
                try
                {
                    //For Parallel execution (also between diffrent Ginger processes), we want attach to be synchronized.
                    //So for windows with same title, correct process id will be calculated
                    try
                    {
                        // acquire the mutex (or timeout), will return false if it timed out
                        Reporter.ToLog(eLogLevel.INFO, "Attach Java Agent- Waiting for Mutex Release");
                        if (!mutex.WaitOne(mAttachAgentProcessSyncTime_Calc_int * 1000))
                        {
                            Reporter.ToLog(eLogLevel.WARN, "Attach Java Agent- Mutex Wait Timeout Reached");
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.INFO, "Attach Java Agent- Mutex was Released");
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.WARN, "Attach Java Agent- Mutex Wait Threw Exception", ex); ;
                    }

                    mAttachAgentCancellationToken = new CancellationTokenSource();
                    mAttachAgentTask = Task.Run(() =>
                    {
                        mProcessIDForAttach = -1;
                        if (!PerformAttachGingerAgent())
                        {
                            return;
                        }

                        if (mPort_Calc.Equals(Fields.DynamicPortPlaceHolder) &&
                            mPortValueAutoResetEvent != null)
                        {
                            mPortValueAutoResetEvent.WaitOne(TimeSpan.FromSeconds(10));
                        }
                        AddOrUpdateReturnParamActual("Port", mPort_Calc);
                    }, mAttachAgentCancellationToken.Token);

                    mAttachAgentTask.Wait();
                    //Wait Max 30 secs for Attach agent to attach the jar or process to exit
                    Stopwatch st = new Stopwatch();
                    st.Start();
                    while (st.ElapsedMilliseconds < 30 * 1000 && !mWaitForWindowTimeOut)
                    {
                        if (IsInstrumentationModuleLoaded(mProcessIDForAttach))
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
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

        public override void PostExecute()
        {

            if (mAttachAgentTask != null && mAttachAgentTask.Status != TaskStatus.RanToCompletion && !mAttachAgentTask.IsCanceled && !mAttachAgentTask.IsFaulted)
            {
                mAttachAgentCancellationToken.Cancel();
            }

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
                mPortValueAutoResetEvent = new AutoResetEvent(false);
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
                {
                    mJavaAgentPath_Calc = GetGingerAgentsDefaultFolder();
                }

                CalculatePortValue();

                mWaitForWindowTitle_Calc = CalculateValue(mWaitForWindowTitle);
                mWaitForWindowTitleMaxTime_Calc = CalculateValue(mWaitForWindowTitleMaxTime);
                mAttachAgentProcessSyncTime_Calc = CalculateValue(mAttachAgentProcessSyncTime);
                mJApplicationProcessName_calc = CalculateValue(mApplicationProcessName);
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
                if (!mLaunchJavaApplication && !mLaunchWithAgent)
                {
                    Error = "No action to perform was selected.";
                    return false;
                }

                if (!Directory.Exists(mJavaWSEXEPath_Calc) || !File.Exists(Path.Combine(mJavaWSEXEPath_Calc, "java.exe")))
                {
                    Error = $"The Java path folder '{mJavaWSEXEPath_Calc}' is not valid, please select the Java Bin folder.";
                    return false;
                }

                if (mLaunchJavaApplication)
                {
                    if (!SetURLExtensionType(mURL_Calc))
                    {
                        Error = $"The provided Java application path '{mURL_Calc}' is not valid, please select .Jar or .Jnlp file or .Exe file";
                        return false;
                    }
                }

                if ((mLaunchJavaApplication && WaitForWindowWhenDoingLaunch) || mLaunchWithAgent)
                {
                    if (string.IsNullOrEmpty(mWaitForWindowTitle_Calc))
                    {
                        Error = "Missing valid Java application Window title to search for.";
                        return false;
                    }

                    if (string.IsNullOrEmpty(mWaitForWindowTitleMaxTime_Calc) || !int.TryParse(mWaitForWindowTitleMaxTime_Calc, out mWaitForWindowTitleMaxTime_Calc_int))
                    {
                        Error = "Max waiting time for java application window is not valid.";
                        return false;
                    }
                }

                if (mLaunchWithAgent)
                {
                    if (string.IsNullOrEmpty(mAttachAgentProcessSyncTime_Calc) || !int.TryParse(mAttachAgentProcessSyncTime_Calc, out mAttachAgentProcessSyncTime_Calc_int))
                    {
                        Error = "Process sync time for attach agent is not valid.";
                        return false;
                    }

                    string versionString = GetJavaVersion(Path.Combine(mJavaWSEXEPath_Calc, "java.exe"));
                    int majorVersion = ExtractMajorVersion(versionString);

                    if (!Directory.Exists(mJavaAgentPath_Calc))
                    {
                        Error = $"The Ginger Agent path folder '{mJavaAgentPath_Calc}' is not valid";
                        return false;
                    }

                    // pick required jar based on version
                    string requiredJar = majorVersion <= 8 ? "GingerAgent.jar" : "JavaAgent_V25.jar";

                    JarFilePath = Path.Combine(mJavaAgentPath_Calc, requiredJar);

                    if (!File.Exists(JarFilePath))
                    {
                        Error = $"The '{requiredJar}' file was not found in the location '{mJavaAgentPath_Calc}'";
                        return false;
                    }


                    if ((ePortConfigType)GetInputParamValue<ePortConfigType>(Fields.PortConfigParam) == ePortConfigType.Manual)
                    {
                        return ValidatePort();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Error = $"Error occurred while validating the action arguments. Error={ex.Message}";
                return false;
            }
        }

        private string JarFilePath;

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
            catch (Exception ex)
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
                List<string> commandParams = [];
                foreach (ActInputValue AIV in this.InputValues)
                {
                    if (!string.IsNullOrEmpty(AIV.Param) && !string.IsNullOrEmpty(AIV.ValueForDriver) && string.Compare(AIV.Param, "URL", true) != 0 && string.Compare(AIV.Param, "Port", true) != 0 && string.Compare(AIV.Param, "PortConfigParam", true) != 0)
                    {
                        commandParams.Add(AIV.Param + "=" + AIV.ValueForDriver);
                    }
                }

                string commandParams_OneLine = string.Empty;
                if (commandParams.Count > 0)
                {
                    foreach (string param in commandParams)
                    {
                        if (commandParams_OneLine == string.Empty)
                        {
                            if (isJNLP)
                            {
                                commandParams_OneLine += "?" + param;
                            }
                            else
                            {
                                commandParams_OneLine += " " + "\"" + param + "\"";
                            }
                        }
                        else
                        {
                            if (isJNLP)
                            {
                                commandParams_OneLine += "&" + param;
                            }
                            else
                            {
                                commandParams_OneLine += " " + "\"" + param + "\"";
                            }
                        }
                    }
                }

                //command
                if (URLExtensionType.JNLP == mURLExtension)
                {
                    command = "\"" + mURL_Calc + "\"";

                    if (!string.IsNullOrEmpty(commandParams_OneLine))
                    {
                        // Remove the last quote and add the arguments, then close with a quote
                        if (command.EndsWith("\""))
                        {
                            command = command.Substring(0, command.Length - 1);
                        }
                        command += commandParams_OneLine + "\"";
                    }
                }
                else if (URLExtensionType.JAR == mURLExtension)
                {
                    command = "-jar \"" + mURL_Calc + "\"";
                    if (!string.IsNullOrEmpty(commandParams_OneLine))
                    {
                        command += commandParams_OneLine;
                    }
                }

                // If it java exe then directly use the exe path as java executor
                else
                {
                    javaExecuter = mURL_Calc;
                }

                //run commnad
                ExecuteCommandAsync([javaExecuter, command]);

                if (String.IsNullOrEmpty(Error) != true)
                {
                    return false;
                }

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
              
                commandParams_OneLine += "\"" + "AgentJarPath=" + JarFilePath + "\"";
                commandParams_OneLine += " \"" + "PID=" + mProcessIDForAttach.ToString() + "\"";
                commandParams_OneLine += " \"" + "ShowGingerAgentConsole=" + ShowAgent.ToString() + "\"";
                commandParams_OneLine += " \"" + "Port=" + mPort_Calc + "\"";

                if (JarFilePath.Contains("GingerAgent.jar"))
                {
                    JarFilePath= Path.Combine(mJavaAgentPath_Calc, "GingerAgentStarter.jar");
                }

                //command
                command = "-jar " + "\"" + JarFilePath + "\" " + commandParams_OneLine;

                //run commnad
                ExecuteCommandAsync([javaExecuter, command]);

                if (String.IsNullOrEmpty(Error) != true)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Error = "Failed to attach the Ginger Agent with the command '" + javaExecuter + " " + command + "'";
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
                return false;
            }
        }

        public static string GetJavaVersion(string javaExePath)
        {
            if (!File.Exists(javaExePath))
            {
                throw new FileNotFoundException("java.exe not found at the specified path.", javaExePath);
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = javaExePath,
                Arguments = "-version",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                string output = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // First line usually: java version "17.0.2"
                string versionLine = output.Split('\n')[0];
                return ExtractVersion(versionLine);
            }
        }

        private static string ExtractVersion(string versionLine)
        {
            int start = versionLine.IndexOf('"') + 1;
            int end = versionLine.LastIndexOf('"');
            return (start > 0 && end > start) ? versionLine.Substring(start, end - start) : "Unknown";
        }
    

        private static int ExtractMajorVersion(string versionString)
        {
            if (string.IsNullOrWhiteSpace(versionString))
            {
                return 0; // fallback
            }

            string[] parts = versionString.Split('.');
            int majorVersion = 0;

            if (parts.Length > 1 && parts[0] == "1")
            {
                // For Java 1.x (like 1.8.0_171)
                int.TryParse(parts[1], out majorVersion);
            }
            else
            {
                // For Java 9+ (like 11.0.2, 17.0.2)
                int.TryParse(parts[0], out majorVersion);
            }

            return majorVersion;
        }

        private bool WaitForAppWindowTitle()
        {
            bool bFound = false;
            try
            {
                if (IsCustomApplicationProcessName && !string.IsNullOrEmpty(mJApplicationProcessName_calc))
                {
                    processNames.Add(mJApplicationProcessName_calc.Trim().ToLower());
                }
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (!bFound)
                {
                    // If Application Launch is done by Ginger then we already know the process id. No need to iterate.
                    if (mJavaApplicationProcessID != -1 && ProcessExists(mJavaApplicationProcessID) && !IsInstrumentationModuleLoaded(mJavaApplicationProcessID))
                    {

                        mAttachAgentCancellationToken?.Token.ThrowIfCancellationRequested();

                        bFound = CheckWindowTitleByProcessId(mJavaApplicationProcessID);

                    }
                    // If Application is not launched from Ginger then we go over the process to find the target Process ID
                    else
                    {

                        var processlist = Process.GetProcesses()
                            .Where(process => processNames.Any(name => process.ProcessName.Contains(name, StringComparison.OrdinalIgnoreCase)));

                        List<Process> matchingProcessList = [];

                        foreach (Process process in processlist)
                        {
                            mAttachAgentCancellationToken?.Token.ThrowIfCancellationRequested();
                            var userWithDomain = Environment.UserDomainName + "\\" + Environment.UserName;
                            try
                            {
                                var currentProcessUser = process.WindowsIdentity().Name;

                                if (currentProcessUser != userWithDomain)
                                {
                                    continue;
                                }
                            }
                            catch
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
                                mAttachAgentCancellationToken?.Token.ThrowIfCancellationRequested();
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
                    }
                    // Go out after max seconds
                    if (sw.ElapsedMilliseconds > mWaitForWindowTitleMaxTime_Calc_int * 1000)
                    {
                        mWaitForWindowTimeOut = true;
                        break;
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (OperationCanceledException ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Task cancellation was requested during WaitForAppWindowTitle", ex);
            }

            return bFound;

        }

        private bool CheckWindowTitleByProcessId(int processId)
        {
            bool bFound = false;
            try
            {
                Process process = Process.GetProcessById(processId);

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
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, string.Concat("Error when checking window with processId: ", processId), ex);
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
            UIAuto.TreeWalker walker = UIAuto.TreeWalker.ControlViewWalker;
            UIAuto.AutomationElement window = walker.GetFirstChild(UIAuto.AutomationElement.RootElement);
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
                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync))
                {
                    //Make the thread as background thread.
                    IsBackground = true,
                    //Set the Priority of the thread.
                    Priority = ThreadPriority.AboveNormal
                };
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
                if (commnadConfigs[1].Contains(Fields.DynamicPortPlaceHolder))
                {
                    mPort_Calc = SocketHelper.GetOpenPort().ToString();
                    mPortValueAutoResetEvent.Set();
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
            try
            {
                Process p = Process.GetProcessById(processId);
                if (p != null && !p.HasExited)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error occurred while fetching process by id.", ex);
            }
            return false;
        }
        private static string handleExePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "StaticDrivers", "handle.exe");
        private Boolean IsInstrumentationModuleLoaded(int id)
        {
            //This method will check if process modules contains instrument dll
            //if yes that means java agent is already attached to this process
            //Process processHandle = null;
            try
            {
                if (id == -1)
                {
                    return false;
                }

                var processHandle = Process.Start(new ProcessStartInfo() { FileName = handleExePath, Arguments = String.Format(" -accepteula -p {0} GingerAgent.jar", id), UseShellExecute = false, RedirectStandardOutput = true });
                string cliOut = processHandle.StandardOutput.ReadToEnd();
                processHandle.WaitForExit();
                processHandle.Close();

                if (cliOut.Contains("GingerAgent.jar"))
                {
                    return true;
                }
                //Process process = Process.GetProcessById(id);
                //ProcessModuleCollection processModules = process.Modules;
                //ProcessModule module;

                //int modulesCount = processModules.Count - 1;
                //// we start from end, because instrument dll is always loaded after rest of the modules are already loaded
                ////So we look for only last 5 modules

                //int maxProcessToCheck = modulesCount - 5 > 0 ? modulesCount - 5 : 0;
                //for (int i = modulesCount; i > maxProcessToCheck - 5; i--)
                //{
                //    module = processModules[i];

                //    if (module.ModuleName.Equals("instrument.dll"))
                //    {
                //        return true;
                //    }
                //}
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.WARN, "Exception when checking IsInstrumentationModuleLoaded for process id:" + id, e);
            }
            finally
            {
                //if (processHandle != null && !processHandle.HasExited)
                //{
                //    processHandle.Kill();
                //}
            }

            return false;
        }
    }
}

