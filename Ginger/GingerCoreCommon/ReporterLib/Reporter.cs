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
using System.Threading;
using GingerCoreNET.ReporterLib;

namespace Amdocs.Ginger.Common
{
    public class Reporter 
    {
        public static IWorkSpaceReporter workSpaceReporter { get; set; }

        public static eAppReporterLoggingLevel CurrentAppLogLevel;

        #region ReportToLog
        
        public static void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded = true, bool writeOnlyInDebugMode = false)
        {            
            workSpaceReporter.ToLog(logLevel, messageToLog, exceptionToLog, writeAlsoToConsoleIfNeeded, writeOnlyInDebugMode);
        }

        public static void ToLogAndConsole(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null)
        {
            ToLog(logLevel, messageToLog, exceptionToLog, false);
            ToConsole(messageToLog);
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


                // FIXME improve if as alreayd found !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                if ((UserMessagesPool != null) && UserMessagesPool.Keys.Contains(messageKey))
                {
                    messageToShow = UserMessagesPool[messageKey];
                }
                if (messageToShow == null)
                {
                    // We do want to pop the error message so below is just in case...
                    string mess = "";
                    foreach (object o in messageArgs)
                    {
                        mess += o.ToString() + " ";
                    }


                    //FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    // RepositoryItemHelper.RepositoryItemFactory.MessageBoxShow(messageKey.ToString() + " - " + mess);
                    workSpaceReporter.ShowMessageToUser(messageKey.ToString() + " - " + mess);

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

                //show the messege and return user selection
                //adding owner window to the message so it will appear on top of any other window including splash screen

                if (CurrentAppLogLevel == eAppReporterLoggingLevel.Debug)
                {
                    ToLog(eLogLevel.INFO, "Showing User Message (Pop-Up): '" + messageText + "'");
                }
                else if (AddAllReportingToConsole)
                {
                    ToConsole("Showing User Message (Pop-Up): '" + messageText + "'");
                }

                MessageBoxResult userSelection = MessageBoxResult.None; //????

                workSpaceReporter.ShowMessageToUser(messageToShow);

                //TODO: find a better option than loop... FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //while (userSelection == null)
                //{
                //    Thread.Sleep(100);
                //}

                if (CurrentAppLogLevel == eAppReporterLoggingLevel.Debug)
                {
                    ToLog(eLogLevel.INFO, "User Selection for Pop-Up Message: '" + userSelection.ToString() + "'");
                }
                else if (AddAllReportingToConsole)
                {
                    ToConsole("User Selection for Pop-Up Message: '" + userSelection.ToString() + "'");
                }

                return userSelection;

            }
            catch (Exception ex)
            {
                ToLog(eLogLevel.ERROR, "Failed to show the user message with the key: " + messageKey, ex);
                
                workSpaceReporter.ShowMessageToUser("Failed to show the user message with the key: " + messageKey);
                return MessageBoxResult.None;
            }
        }
        #endregion Report to User

        #region Report to Ginger Helper
        public static Dictionary<eGingerHelperMsgKey, GingerHelperMsg> GingerHelperMsgsPool { get; set; }
        private static Thread GingerHelperThread = null;
        
        private static bool GingerHelperWinIsLoading = false;
        
        public static void ToGingerHelper(eGingerHelperMsgKey messageKey, object btnHandler = null, params object[] messageArgs)
         {
         }
        
        public static void CloseGingerHelper()
        {
        }
        #endregion Report to Ginger Helper

        #region Report to Console
        public static bool AddAllReportingToConsole = false;
        public static void ToConsole(string messageToConsole, Exception exceptionToConsole = null)
        {
            try
            {
                string msg = messageToConsole;
                if (exceptionToConsole != null)
                {
                    string excFullInfo = "Error:" + exceptionToConsole.Message + Environment.NewLine;
                    excFullInfo += "Source:" + exceptionToConsole.Source + Environment.NewLine;
                    excFullInfo += "Stack Trace: " + exceptionToConsole.StackTrace;
                    msg += System.Environment.NewLine + "Exception Details:" + System.Environment.NewLine + excFullInfo;
                }

                // Console.WriteLine(msg + System.Environment.NewLine);
                workSpaceReporter.ConsoleWriteLine(msg);
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
