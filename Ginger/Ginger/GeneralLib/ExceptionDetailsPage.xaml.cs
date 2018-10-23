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

using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.GeneralLib
{
    /// <summary>
    /// Interaction logic for ExceptionDetailsPage.xaml
    /// </summary>
    public partial class ExceptionDetailsPage : Page
    {        
        GenericWindow _pageGenericWin;
        Exception mException;

        bool _ShowingFull;

        string mLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\\amdocs\\Ginger\\WorkingFolder\\Logs\\Ginger_Log.txt";

        public static void ShowError(Exception ex)
        {
            ExceptionDetailsPage GE = new ExceptionDetailsPage(ex);
            GE.ShowAsWindow();
        }

        public ExceptionDetailsPage(Exception ex)
        {
            InitializeComponent();

            mException = ex;

            ShowFullDetails(false);
            FillExceptionData();
        }

        private void ShowFullDetails(bool toShow)
        {            
            if (toShow)
            {
                FullErrorDetailsRow.Height = new GridLength(80, GridUnitType.Star);                
            }
            else
            {
                FullErrorDetailsRow.Height = new GridLength(0);
            }
            _ShowingFull = toShow;
        }

        private void FillExceptionData()
        {
            //General
            GeneralErrorDetailsTextBlock.Text = "Unexpected error occurred, please report it to Ginger Core Team along with all steps to reproduce.";

            //Full
            FullErrorDetailsTextBlock.Text = string.Empty;
            TextBlockHelper TBH = new TextBlockHelper(FullErrorDetailsTextBlock);
            TBH.AddBoldText("Message: ");
            TBH.AddFormattedText(mException.Message, System.Windows.Media.Brushes.Red, true);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddBoldText("Source: ");
            TBH.AddText(mException.Source);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddBoldText("Stack Trace: ");
            TBH.AddLineBreak();
            TBH.AddText(mException.StackTrace);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button CopyToClipboradBtn = new Button();
            CopyToClipboradBtn.Content = "Copy to Clipboard";
            CopyToClipboradBtn.Click += new RoutedEventHandler(CopyToClipboradBtn_Click);

            Button ViewLogBtn = new Button();
            ViewLogBtn.Content = "View Log";
            ViewLogBtn.Click += new RoutedEventHandler(ViewLogBtn_Click);

            Button SendMailBtn = new Button();
            SendMailBtn.Content = "Send Mail";
            SendMailBtn.Click += new RoutedEventHandler(SendMailBtn_Click);

            ObservableList<Button> winButtons = new ObservableList<Button>();          
            winButtons.Add(CopyToClipboradBtn);
            winButtons.Add(ViewLogBtn);
            winButtons.Add(SendMailBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Error Occurred!", this, winButtons);
        }

        private void MoreLessInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowFullDetails(!_ShowingFull);
            if (_ShowingFull)
                MoreLessInfoBtn.Content = "Less";
            else
                MoreLessInfoBtn.Content = "More";
        }

        private string GetFullErrorText()
        {
            string FullInfo = "Error: " + mException.Message + Environment.NewLine + Environment.NewLine;
            FullInfo += "Source: " + mException.Source + Environment.NewLine + Environment.NewLine;
            FullInfo += "Stack Trace: " + Environment.NewLine + mException.StackTrace;
            return FullInfo;
        }

        private void CopyToClipboradBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GetFullErrorText());
        }

        private void ViewLogBtn_Click(object sender, RoutedEventArgs e)
        {            
            if (System.IO.File.Exists(mLogFilePath))
            {
                Process.Start(mLogFilePath);
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Ginger log file was not found.");
            }
        }

        private void SendMailBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Email mail = new Email();
                mail.EmailMethod = Email.eEmailMethod.OUTLOOK;

                mail.Subject = "Ginger Error Occurred: " + mException.Message;
                mail.Body = BuildErrorAsHTML();
                mail.MailTo = "GingerCoreTeam@int.amdocs.com";

                //add log
                if (System.IO.File.Exists(mLogFilePath))
                {
                    mail.Attachments.Add(mLogFilePath);
                }
                //add screen shot
                string screenShot = TakeScreenShot();
                if (System.IO.File.Exists(screenShot))
                {
                    mail.Attachments.Add(screenShot);
                }                

                mail.DisplayAsOutlookMail();

                if (mail.Event != null && mail.Event.IndexOf("Failed") >= 0)
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Failed to send the error mail." + System.Environment.NewLine + System.Environment.NewLine + "Details: " + mail.Event);
            }
            catch(Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Failed to send the error mail." + System.Environment.NewLine + System.Environment.NewLine + "Details: " + ex.Message);
            }
        }

        private string TakeScreenShot()
        {
            string imagePath=string.Empty;
            try
            {
                System.Drawing.Rectangle bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
                    }
                    imagePath= System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ScreenShot_"+ DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")+".jpg");
                    bitmap.Save(imagePath, ImageFormat.Jpeg);
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }

            return imagePath;
        }

        private string BuildErrorAsHTML()
        {
            StringBuilder html= new StringBuilder();
            html.Append(@"<html xmlns=""http://www.w3.org/1999/xhtml"">").AppendLine();
            html.Append(@"<body>").AppendLine();
            html.Append(@"<table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""font-family:arial;"">").AppendLine();
            html.Append(@"<tr><td><p>" + "Hi," + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "Error occurred on Ginger, please find details below:" + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "<b>Version:</b> " + App.AppVersion + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "<b>Steps to Reproduce:</b> " + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "1. " + "<br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "2. " + "<br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "3. " + "<br><br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "<b>Error:</b>"+@"<font color=""red""> " + mException.Message + "</font><br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "<b>Source:</b> " + mException.Source + "<br><br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + "<b>Stack Trace:</b> " + "<br></p></td></tr>").AppendLine();
            html.Append(@"<tr><td><p>" + mException.StackTrace.Replace("\n", "<br>") + "<br></p></td></tr>").AppendLine();
            html.Append(@"</table></body></html>").AppendLine();

            return html.ToString();
        }
    }

}
