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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GingerCoreNET.ReporterLib;

namespace Amdocs.Ginger.Common
{
    public class Reporter 
    {        
        public static WorkSpaceReporterBase WorkSpaceReporter { get; set; }

        public static ReporterData ReporterData = new ReporterData();

        public static eAppReporterLoggingLevel CurrentAppLogLevel;

        

        #region ReportToLog

        public static void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded = true, bool writeOnlyInDebugMode = false)
        {
            if (writeOnlyInDebugMode && CurrentAppLogLevel != eAppReporterLoggingLevel.Debug)
            {
                return;
            }
            if (logLevel == eLogLevel.ERROR)
            {
                ReporterData.ErrorCounter++;
            }
            WorkSpaceReporter.ToLog(logLevel, messageToLog, exceptionToLog, writeAlsoToConsoleIfNeeded);
        }

        public static void ToLogAndConsole(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null)
        {
            ToLog(logLevel, messageToLog, exceptionToLog, false);
            ToConsole(logLevel, messageToLog);            
        }
        #endregion Report to Log

        #region Report to User
        
        public static Dictionary<eUserMsgKeys, UserMessage> UserMessagesPool { get; set; }

        public static MessageBoxResult ToUser(eUserMsgKeys messageKey, params object[] messageArgs)
        {
            UserMessage messageToShow = null;
            string messageText = string.Empty;
            MessageBoxImage messageImage = MessageBoxImage.None;

            try
            {
                //get the message from pool

                // FIXME improve if as already found !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                if ((UserMessagesPool != null) && UserMessagesPool.Keys.Contains(messageKey))
                {
                    messageToShow = UserMessagesPool[messageKey];
                }
                if (messageToShow == null) // Message not found in message pool
                {
                    // We do want to pop the error message so below is just in case...
                    string mess = "";
                    foreach (object o in messageArgs)
                    {
                        mess += o.ToString() + " ";
                    }

                    string txt = messageKey.ToString() + " - " + mess + "{Error message key not found!}" ;
                    WorkSpaceReporter.MessageBoxShow(txt, "Ginger",  MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);

                    ToLog(eLogLevel.WARN, "The user message with key: '" + messageKey + "' was not found! and won't show to the user!");
                    return MessageBoxResult.None;
                }

                //set the message type
                switch (messageToShow.MessageType)
                {
                    case eMessageType.ERROR:
                        messageImage = MessageBoxImage.Error;
                        break;
                    case eMessageType.INFO:
                        messageImage = MessageBoxImage.Information;
                        break;
                    case eMessageType.QUESTION:
                        messageImage = MessageBoxImage.Question;
                        break;
                    case eMessageType.WARN:
                        messageImage = MessageBoxImage.Warning;
                        break;
                    default:
                        messageImage = MessageBoxImage.Information;
                        break;
                }

                //enter message args if exist
                if (messageArgs.Length > 0)
                {
                    messageText = string.Format(messageToShow.Message, messageArgs);
                }
                else
                {
                    messageText = messageToShow.Message;
                }
                                
                if (CurrentAppLogLevel == eAppReporterLoggingLevel.Debug)
                {
                    ToLog(eLogLevel.INFO, "Showing User Message (Pop-Up): '" + messageText + "'");
                }
                else if (AddAllReportingToConsole)
                {
                    ToConsole(eLogLevel.DEBUG, "Showing User Message (Pop-Up): '" + messageText + "'");
                }

                //show the messege and return user selection
                MessageBoxResult userSelection = WorkSpaceReporter.MessageBoxShow(messageText, messageToShow.Caption, messageToShow.ButtonsType, messageImage, messageToShow.DefualtResualt);                


                if (CurrentAppLogLevel == eAppReporterLoggingLevel.Debug)
                {
                    ToLog(eLogLevel.INFO, "User Selection for Pop-Up Message: '" + userSelection.ToString() + "'");
                }
                else if (AddAllReportingToConsole)
                {
                    ToConsole(eLogLevel.DEBUG, "User Selection for Pop-Up Message: '" + userSelection.ToString() + "'");
                }

                return userSelection;

            }
            catch (Exception ex)
            {
                ToLog(eLogLevel.ERROR, "Failed to show the user message with the key: " + messageKey, ex);

                string txt = "Failed to show the user message with the key: " + messageKey;
                WorkSpaceReporter.MessageBoxShow(txt, "Ginger", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                
                return MessageBoxResult.None;
            }
        }
        #endregion Report to User

        #region Report to Ginger Helper
        public static Dictionary<eGingerHelperMsgKey, GingerHelperMsg> GingerHelperMsgsPool { get; set; }

        static Stopwatch mLastStatusTime = new Stopwatch();
        public static void ToGingerHelper(eGingerHelperMsgKey messageKey, object btnHandler = null, params object[] messageArgs)
         {            
            GingerHelperMsg messageToShow = null;
            string messageContent = string.Empty;
            
            try
            {
                // TODO: use TryGet

                //get the message from pool
                if ((GingerHelperMsgsPool != null) && GingerHelperMsgsPool.Keys.Contains(messageKey))
                {
                    messageToShow = GingerHelperMsgsPool[messageKey];
                }
                if (messageToShow == null)
                {
                    // We do want to pop the error message so below is just in case...
                    string mess = "";
                    foreach (object o in messageArgs)
                    {
                        mess += o.ToString() + " ";
                    }                    
                    ToUser(eUserMsgKeys.StaticErrorMessage, "Cannot find MessageKey: " + messageKey);
                    ToLog(eLogLevel.WARN, "The Status message with key: '" + messageKey + "' was not found! and won't show to the user!");
                }
                messageContent = messageToShow.MsgContent;
                //enter message args if exist
                if (messageArgs.Length > 0)
                {
                    messageContent = string.Format(messageContent, messageArgs);
                }

                if (CurrentAppLogLevel == eAppReporterLoggingLevel.Debug)
                {
                    ToLog(eLogLevel.INFO, "Showing Status Message: " + messageContent);
                }
                else if (AddAllReportingToConsole)
                {
                    ToConsole(eLogLevel.DEBUG, "Showing Status Message: " + messageContent);
                }

                mLastStatusTime.Start();
                WorkSpaceReporter.ToStatus(messageToShow.MessageType, messageContent);
            }
            catch (Exception ex)
            {
                ToLog(eLogLevel.ERROR, "Failed to show the Status message with the key: " + messageKey, ex);
            }

        }

        static bool bClosing = false;
        public static void CloseGingerHelper()
        {
            if (bClosing)
            {
                return;
            }
            bClosing = true;
            // TODO: run on task
            Task t = new Task(() => {
                while (mLastStatusTime.ElapsedMilliseconds < 1000)  // let the message show for at least one second
                {
                    Task.Delay(100);
                }                
                WorkSpaceReporter.ToStatus(eStatusMessageType.INFO, null);
                mLastStatusTime.Reset();
                bClosing = false;
            });
            t.Start();
        }
        #endregion Report to Ginger Helper

        #region Report to Console
        public static bool AddAllReportingToConsole = false;
        public static void ToConsole(eLogLevel logLevel, string messageToConsole, Exception exceptionToConsole = null)
        {
            try
            {
                string msg = messageToConsole;                
                if (exceptionToConsole != null)
                {
                    string excFullInfo = "Error:" + exceptionToConsole.Message + Environment.NewLine;
                    excFullInfo += "Source:" + exceptionToConsole.Source + Environment.NewLine;
                    excFullInfo += "Stack Trace: " + exceptionToConsole.StackTrace;
                    msg += Environment.NewLine + "Exception Details:" + Environment.NewLine + excFullInfo;
                }

                // Console.WriteLine(msg + System.Environment.NewLine);
                WorkSpaceReporter.ConsoleWriteLine(logLevel, msg);
            }
            catch (Exception ex)
            {
                ToLog(eLogLevel.ERROR, "Failed to report to Console", ex);
            }
        }
        
        #endregion Report to Console
        
        static StringBuilder dt()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss - "));
            return sb;
        }


        //???????????????????????????????????????????!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public static void ToTrace(string action, string info)
        {
            StringBuilder sb = dt();
            sb.Append(action);
            sb.Append(info);
            Trace.WriteLine(sb.ToString());
        }


        //???????????????????????????????????????????!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public static Stopwatch ToTraceStart(string action, string info)
        {
            Trace.WriteLine("{");
            Trace.Indent();            
            StringBuilder sb = dt();
            sb.Append(action);
            sb.Append(", ");
            sb.Append(info);
            sb.Append("*** START ***");
            Trace.WriteLine(sb.ToString());         
            Stopwatch st = new Stopwatch();
            st.Start();
            return st;
        }

        //???????????????????????????????????????????!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public static void ToTraceEnd(string action, string info, Stopwatch st)
        {            
            st.Stop();
            
            StringBuilder sb = dt();
            sb.Append(action);
            sb.Append(", ");
            sb.Append(info);
            sb.Append(", Elasped=");
            sb.Append(st.ElapsedMilliseconds);
            sb.Append(" *** END ***");
            Trace.WriteLine(sb.ToString());                        
            Trace.Unindent();
            Trace.WriteLine("}");
        }


        private static bool RunningFromConfigFile = false;

        

        public static void SetRunConfigMode(bool RunConfigMode)
        {
            RunningFromConfigFile = RunConfigMode;
        }


    }


    
}
