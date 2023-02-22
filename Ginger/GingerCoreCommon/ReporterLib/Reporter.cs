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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace Amdocs.Ginger.Common
{
    public class Reporter
    {
        public static WorkSpaceReporterBase WorkSpaceReporter { get; set; }

        public static ReporterData ReporterData = new ReporterData();

        public static bool ReportAllAlsoToConsole { get; set; }


        #region ToLog
        public static eAppReporterLoggingLevel AppLoggingLevel { get; set; }
        public static void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null)
        {
            if (WorkSpaceReporter == null || (logLevel == eLogLevel.DEBUG && AppLoggingLevel != eAppReporterLoggingLevel.Debug))
            {
                return;
            }

            if (logLevel == eLogLevel.ERROR || logLevel == eLogLevel.FATAL)
            {
                ReporterData.ErrorCounter++;
            }

            if (ReportAllAlsoToConsole)
            {
                ToConsole(logLevel, messageToLog, exceptionToLog);
            }

            WorkSpaceReporter.ToLog(logLevel, messageToLog, exceptionToLog);            
        }
        #endregion ToLog



        #region ToUser        
        public static Dictionary<eUserMsgKey, UserMsg> UserMsgsPool { get; set; }

        public static eUserMsgSelection ToUser(eUserMsgKey messageKey, params object[] messageArgs)
        {
            UserMsg messageToShow = null;
            string messageText = string.Empty;
            eUserMsgIcon messageImage = eUserMsgIcon.None;

            try
            {                
                if ((UserMsgsPool != null) && UserMsgsPool.Keys.Contains(messageKey))
                {             
                    messageToShow = UserMsgsPool[messageKey];
                }

                if (messageToShow == null) // Message not found in message pool
                {
                    // We do want to pop the error message so below is just in case...
                    string mess = "";
                    foreach (object o in messageArgs)
                    {
                        mess += o.ToString() + " ";
                    }

                    string txt = messageKey.ToString() + " - " + mess + "{Error message key not found!}";
                    WorkSpaceReporter.ToUser(txt, "Ginger", eUserMsgOption.OK, eUserMsgIcon.Information, eUserMsgSelection.OK);

                    ToLog(eLogLevel.WARN, "The user message with key: '" + messageKey + "' was not found! and won't show to the user!");
                    return eUserMsgSelection.None;
                }

                //set the message type
                switch (messageToShow.MessageType)
                {
                    case eUserMsgType.ERROR:
                        messageImage = eUserMsgIcon.Error;
                        break;
                    case eUserMsgType.INFO:
                        messageImage = eUserMsgIcon.Information;
                        break;
                    case eUserMsgType.QUESTION:
                        messageImage = eUserMsgIcon.Question;
                        break;
                    case eUserMsgType.WARN:
                        messageImage = eUserMsgIcon.Warning;
                        break;
                    default:
                        messageImage = eUserMsgIcon.Information;
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

                if (AppLoggingLevel == eAppReporterLoggingLevel.Debug)
                {
                    ToLog(eLogLevel.INFO, "Showing User Message: '" + messageText + "'");
                }
                else if (ReportAllAlsoToConsole)
                {
                    ToConsole(eLogLevel.INFO, "Showing User Message: '" + messageText + "'");
                }

                //show the messege and return user selection
                eUserMsgSelection userSelection = WorkSpaceReporter.ToUser(messageText, messageToShow.Caption, messageToShow.SelectionOptions, messageImage, messageToShow.DefualtSelectionOption);

                if (AppLoggingLevel == eAppReporterLoggingLevel.Debug)
                {
                    ToLog(eLogLevel.INFO, "User Message Option Selection: '" + userSelection.ToString() + "'");
                }
                else if (ReportAllAlsoToConsole)
                {
                    ToConsole(eLogLevel.INFO, "User Message Option Selection: '" + userSelection.ToString() + "'");
                }

                return userSelection;
            }
            catch (Exception ex)
            {
                string txt = "Failed to show the user message with the key: " + messageKey;

                ToLog(eLogLevel.ERROR, txt, ex);

                if (ReportAllAlsoToConsole)
                {
                    ToConsole(eLogLevel.ERROR, txt, ex);
                }

                WorkSpaceReporter.ToUser(txt, "Ginger", eUserMsgOption.OK, eUserMsgIcon.Information, eUserMsgSelection.OK);

                return eUserMsgSelection.None;
            }
        }
        #endregion ToUser



        #region ToStatus
        public static Dictionary<eStatusMsgKey, StatusMsg> StatusMsgsPool { get; set; }
        static Stopwatch mLastStatusTime = new Stopwatch();

        public static void ToStatus(eStatusMsgKey messageKey, object btnHandler = null, params object[] messageArgs)
        {
            StatusMsg messageToShow = null;
            string messageContent = string.Empty;

            try
            {
                // TODO: use TryGet
                //get the message from pool
                if ((StatusMsgsPool != null) && StatusMsgsPool.Keys.Contains(messageKey))
                {
                    messageToShow = StatusMsgsPool[messageKey];
                }
                if (messageToShow == null)
                {
                    // We do want to pop the error message so below is just in case...
                    string mess = "";
                    foreach (object o in messageArgs)
                    {
                        mess += o.ToString() + " ";
                    }
                    ToUser(eUserMsgKey.StaticErrorMessage, "Cannot find Status message key: " + messageKey);
                    ToLog(eLogLevel.ERROR, "The Status message with key: '" + messageKey + "' was not found! and won't show to the user!");
                }
                messageContent = messageToShow.MsgContent;

                //enter message args if exist
                if (messageArgs.Length > 0)
                {
                    messageContent = string.Format(messageContent, messageArgs);
                }

                if (AppLoggingLevel == eAppReporterLoggingLevel.Debug)
                {
                    ToLog(eLogLevel.INFO, "Showing Status Message: " + messageContent);
                }
                else if (ReportAllAlsoToConsole)
                {
                    ToConsole(eLogLevel.INFO, "Showing Status Message: " + messageContent);
                }

                WorkSpaceReporter.ToStatus(messageToShow.MessageType, messageContent);
                mLastStatusTime.Start();
            }
            catch (Exception ex)
            {
                ToLog(eLogLevel.ERROR, "Failed to show the Status message with the key: " + messageKey, ex);
            }

        }

        static bool bClosing = false;
        public static void HideStatusMessage()
        {
            if (bClosing)
            {
                return;
            }

            if (mLastStatusTime.IsRunning)
            {
                bClosing = true;
                // let the message show for at least one second
                var timer = new Timer();
                timer.Interval = 1000; // In milliseconds
                timer.AutoReset = false; // Stops it from repeating
                timer.Elapsed += new ElapsedEventHandler(HideMessage_Event);
                timer.Start();
            }

        }

        private static void HideMessage_Event(object sender, ElapsedEventArgs e)
        {
            WorkSpaceReporter.ToStatus(eStatusMsgType.INFO, null);
            mLastStatusTime.Reset();
            bClosing = false;
        }
        #endregion ToStatus



        #region ToConsole        
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

                WorkSpaceReporter.ToConsole(logLevel, msg);

                // if we have log to console event listener send the message 
                logToConsoleEvent?.Invoke(logLevel, msg);
            }
            catch (Exception ex)
            {
                ToLog(eLogLevel.ERROR, "Failed to report to Console", ex);
            }
        }
        #endregion ToConsole

        // in case we want to listen to console write events - used for unit tests
        public static event LogToConsoleEventHandler logToConsoleEvent;
        public delegate void LogToConsoleEventHandler(eLogLevel logLevel, string messageToConsole);

    }
}
