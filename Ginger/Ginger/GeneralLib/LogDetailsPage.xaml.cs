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

using Amdocs.Ginger.Common;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using OpenQA.Selenium.DevTools.V101.SystemInfo;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.GeneralLib
{
    /// <summary>
    /// Interaction logic for LogDetailsPage.xaml
    /// </summary>
    public partial class LogDetailsPage : Page
    {
        public const int NoOfLinesToShow = 5000;
        public enum eLogShowLevel
        {
            ALL, DEBUG, INFO, WARN, ERROR, FATAL
        }

        eLogShowLevel mLogLevel { get; set; }
        string mLogText;
        TextBlockHelper mTextBlockHelper;
        GenericWindow mPageGenericWin;

        /// <summary>
        /// Log Details Page
        /// </summary>
        /// <param name="logLevelToShow">Selecte Log level to show</param> 
        public LogDetailsPage(eLogShowLevel logLevelToShow = eLogShowLevel.ERROR)
        {
            InitializeComponent();

            mLogLevel = logLevelToShow;
            GingerCore.General.FillComboFromEnumType(xLogTypeCombo, typeof(eLogShowLevel));
            xLogTypeCombo.SelectedValue = eLogLevel.ERROR;
            xLogTypeCombo.SelectedValue = mLogLevel;
            xLogTypeCombo.SelectionChanged += XLogTypeCombo_SelectionChanged;

            FillLogData().ConfigureAwait(false);
            xScrollViewer.ScrollToBottom();
        }

        public async void Refresh()
        {
            await FillLogData();
            xScrollViewer.ScrollToBottom();
        }

        private async void XLogTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mLogLevel = (eLogShowLevel)xLogTypeCombo.SelectedValue;
            await FillLogData();
        }
        
        private async Task FillLogData()
        {
            //get the log file text            
            using (StreamReader sr = new StreamReader(Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile))
            {
                mLogText = sr.ReadToEnd();
            }

            //cut all log not relevant to last application launch
            int indexOfStart = mLogText.LastIndexOf("######################## Application version");

            if(indexOfStart==-1)
            {
                indexOfStart = 0;
            }

            mLogText = mLogText.Substring(indexOfStart);

            //split the log per log info
            string[] logs = mLogText.Split(new[] { Environment.NewLine },StringSplitOptions.None);

            xLogDetailsTextBlock.Text = string.Empty;
            mTextBlockHelper = new TextBlockHelper(xLogDetailsTextBlock);
            bool allowLogDetailsWrite = true;
            int start = logs.Length > NoOfLinesToShow ? logs.Length - NoOfLinesToShow : 0;
            xProcessingIcon.Visibility = Visibility.Visible;
            xLogDetailsBorder.Visibility = Visibility.Collapsed;
            await Task.Run(() =>
            {
                for (int i = start; i < logs.Length; i++)
                {
                    Dispatcher.Invoke(() => { 
                    if (logs[i] == string.Empty)
                    {
                        if (allowLogDetailsWrite)
                        {
                            mTextBlockHelper.AddLineBreak();
                        }
                    }
                    else if (logs[i].Contains("#### Application version"))
                    {
                        mTextBlockHelper.AddFormattedText(logs[i], Brushes.Black, true);
                    }
                    else if (IsLogHeader(logs[i]))
                    {
                        if (mLogLevel == eLogShowLevel.ALL || logs[i].Contains("| " + mLogLevel.ToString()))
                        {
                            
                            mTextBlockHelper.AddFormattedText(logs[i], GetProperLogTypeBrush(logs[i]), isBold: true);
                            mTextBlockHelper.AddLineBreak();
                            allowLogDetailsWrite = true;
                        }
                        else
                        {
                            allowLogDetailsWrite = false;
                        }
                    }
                    else
                    {
                        if (allowLogDetailsWrite)
                        {
                            mTextBlockHelper.AddText(logs[i]);
                            mTextBlockHelper.AddLineBreak();
                        }
                    }
                    });
                }
            });
            xProcessingIcon.Visibility = Visibility.Collapsed;
            xLogDetailsBorder.Visibility = Visibility.Visible;
        }

        private bool IsLogHeader(string log)
        {
            if (log.Contains("| " + eLogLevel.DEBUG.ToString()) || log.Contains("| " + eLogLevel.ERROR.ToString()) ||
                log.Contains("| " + eLogLevel.FATAL.ToString()) || log.Contains("| " + eLogLevel.INFO.ToString()) ||
                log.Contains("| " + eLogLevel.WARN.ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Brush GetProperLogTypeBrush(string log)
        {
            if (log.Contains("| " + eLogLevel.INFO.ToString()))
            {
                return Brushes.Blue;
            }
            else if (log.Contains("| " + eLogLevel.DEBUG.ToString()))
            {
                return Brushes.Black;
            }
            else if (log.Contains("| " + eLogLevel.WARN.ToString()))
            {
                return Brushes.Orange;
            }
            else if (log.Contains("| " + eLogLevel.ERROR.ToString()))
            {
                return Brushes.Red;
            }
            else if (log.Contains("| " + eLogLevel.FATAL.ToString()))
            {
                return Brushes.Red;
            }
            else
            {
                return Brushes.Black;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            
            
            Button CopyToClipboradBtn = new Button();
            CopyToClipboradBtn.Content = "Copy to Clipboard";
            CopyToClipboradBtn.Click += new RoutedEventHandler(CopyToClipboradBtn_Click);

            Button ViewLogBtn = new Button();
            ViewLogBtn.Content = "Open Full Log File";
            ViewLogBtn.Click += new RoutedEventHandler(ViewLogBtn_Click);

            Button SendMailBtn = new Button();
            SendMailBtn.Content = "Send Details in Mail";
            SendMailBtn.Click += new RoutedEventHandler(SendMailBtn_Click);

            Amdocs.Ginger.Common.ObservableList<Button> winButtons = new Amdocs.Ginger.Common.ObservableList<Button>();
            winButtons.Add(CopyToClipboradBtn);
            winButtons.Add(ViewLogBtn);
            winButtons.Add(SendMailBtn);

            this.Width = 800;
            this.Height = 600;
            GingerCore.General.LoadGenericWindow(ref mPageGenericWin, App.MainWindow, windowStyle, "Log Details", this, winButtons);
        }

        private void CopyToClipboradBtn_Click(object sender, RoutedEventArgs e)
        {            
            Clipboard.SetText(mTextBlockHelper.GetText());
        }

        private void ViewLogBtn_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile, UseShellExecute = true });
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Ginger log file was not found.");
            }
        }

        private void SendMailBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Email mail = new Email();
                EmailOperations emailOperations = new EmailOperations(mail);
                mail.EmailOperations = emailOperations;

                mail.EmailMethod = Email.eEmailMethod.OUTLOOK;

                mail.Subject = "Ginger Log Details";
                mail.Body = BuildMailContentAsHTML();
                mail.MailTo = "GingerCoreTeam@int.amdocs.com";

                //add Full log
                if (System.IO.File.Exists(Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile))
                {
                    mail.Attachments.Add(Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile);
                }               

                mail.EmailOperations.DisplayAsOutlookMail();

                if (mail.Event != null && mail.Event.IndexOf("Failed") >= 0)
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Failed to send the Log Details mail." + System.Environment.NewLine + System.Environment.NewLine + "Details: " + mail.Event);
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Failed to send the Log Details mail." + System.Environment.NewLine + System.Environment.NewLine + "Details: " + ex.Message);
            }
        }

        private string BuildMailContentAsHTML()
        {
            StringBuilder html = new StringBuilder();
            html.Append(@"<html xmlns=""http://www.w3.org/1999/xhtml"">").AppendLine();
            html.Append(@"<body>").AppendLine();
            html.Append(@"<table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""font-family:arial;"">").AppendLine();
            html.Append(@"<tr><td><p>" + "Hi," + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "Please find my Log details below:" + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "<b>Version:</b> " + Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationVersionWithInfo + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "<b>Steps to Reproduce:</b> " + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "1. " + "<br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "2. " + "<br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "3. " + "<br><br><br></p></td></tr>").AppendLine();          
            html.Append(@"<tr><td><p>" + "<b>Log Details:</b> " + "<br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + mTextBlockHelper.GetText().Replace("\n", "<br>") + "<br></p></td></tr>").AppendLine();
            html.Append(@"</table></body></html>").AppendLine();

            return html.ToString();
        }


    }
}
