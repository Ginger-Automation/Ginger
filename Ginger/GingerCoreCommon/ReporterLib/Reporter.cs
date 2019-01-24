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
            if (ReportAllAlsoToConsole) 
            {
                ToConsole(logLevel, messageToLog, exceptionToLog);
            }

            if (logLevel == eLogLevel.DEBUG && AppLoggingLevel != eAppReporterLoggingLevel.Debug)
            {
                return;
            }

            if (logLevel == eLogLevel.ERROR || logLevel == eLogLevel.FATAL)
            {
                ReporterData.ErrorCounter++;
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
                //get the message from pool
                // FIXME improve if as already found !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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

                    string txt = messageKey.ToString() + " - " + mess + "{Error message key not found!}" ;
                    WorkSpaceReporter.ToUser(txt, "Ginger",  eUserMsgOption.OK, eUserMsgIcon.Information, eUserMsgSelection.OK);

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
                    ToLog(eLogLevel.DEBUG, "Showing User Message: '" + messageText + "'");
                }
                else if (ReportAllAlsoToConsole)
                {
                    ToConsole(eLogLevel.DEBUG, "Showing User Message: '" + messageText + "'");
                }

                //show the messege and return user selection
                eUserMsgSelection userSelection = WorkSpaceReporter.ToUser(messageText, messageToShow.Caption, messageToShow.SelectionOptions, messageImage, messageToShow.DefualtSelectionOption);                

                if (AppLoggingLevel == eAppReporterLoggingLevel.Debug)
                {
                    ToLog(eLogLevel.DEBUG, "User Message Option Selection: '" + userSelection.ToString() + "'");
                }
                else if (ReportAllAlsoToConsole)
                {
                    ToConsole(eLogLevel.DEBUG, "User Message Option Selection: '" + userSelection.ToString() + "'");
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
                    ToLog(eLogLevel.DEBUG, "Showing Status Message: " + messageContent);
                }
                else if (ReportAllAlsoToConsole)
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
        public static void HideStatusMessage()
        {
            if (bClosing)
            {
                return;
            }
            bClosing = true;
            
            Task t = new Task(() => {
                while (mLastStatusTime.ElapsedMilliseconds < 1000)  // let the message show for at least one second
                {
                    Task.Delay(100);
                }                
                WorkSpaceReporter.ToStatus(eStatusMsgType.INFO, null);
                mLastStatusTime.Reset();
                bClosing = false;
            });
            t.Start();
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
            }
            catch (Exception ex)
            {
                ToLog(eLogLevel.ERROR, "Failed to report to Console", ex);
            }
        }        
        #endregion ToConsole


        
        //static StringBuilder dt()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss - "));
        //    return sb;
        //}


        ////???????????????????????????????????????????!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //public static void ToTrace(string action, string info)
        //{
        //    StringBuilder sb = dt();
        //    sb.Append(action);
        //    sb.Append(info);
        //    Trace.WriteLine(sb.ToString());
        //}


        ////???????????????????????????????????????????!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //public static Stopwatch ToTraceStart(string action, string info)
        //{
        //    Trace.WriteLine("{");
        //    Trace.Indent();            
        //    StringBuilder sb = dt();
        //    sb.Append(action);
        //    sb.Append(", ");
        //    sb.Append(info);
        //    sb.Append("*** START ***");
        //    Trace.WriteLine(sb.ToString());         
        //    Stopwatch st = new Stopwatch();
        //    st.Start();
        //    return st;
        //}

        ////???????????????????????????????????????????!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //public static void ToTraceEnd(string action, string info, Stopwatch st)
        //{            
        //    st.Stop();
            
        //    StringBuilder sb = dt();
        //    sb.Append(action);
        //    sb.Append(", ");
        //    sb.Append(info);
        //    sb.Append(", Elasped=");
        //    sb.Append(st.ElapsedMilliseconds);
        //    sb.Append(" *** END ***");
        //    Trace.WriteLine(sb.ToString());                        
        //    Trace.Unindent();
        //    Trace.WriteLine("}");
        //}
    }    
}
