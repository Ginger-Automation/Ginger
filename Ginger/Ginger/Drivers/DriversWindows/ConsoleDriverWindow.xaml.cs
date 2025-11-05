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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.ActionsLib;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Console;
using Ginger.Drivers.DriversWindows; // ensure same namespace if needed
using GingerCore; // for BusinessFlow and eLocateBy
using GingerCore.Drivers;
using System; 
using System.Text; 
using System.Windows; 
using System.Windows.Documents; 
using System.Windows.Input; 
using System.Windows.Media; 

namespace Ginger.Drivers.DriversWindows
{
    /// <summary>
    /// Interaction logic for ConsoleDriverWindow.xaml
    /// </summary>
    public partial class ConsoleDriverWindow : Window
    {
        bool mRecording = false;
        StringBuilder mConsoleBuffer = new StringBuilder();

        // default brush style 
        Brush ConsoleBackgroundBrush = Brushes.White;
        Brush ConsoleTextBrush = Brushes.Green;
        Brush ConsoleCommandBrush = Brushes.Blue;
        Brush ConsoleErrorBrush = Brushes.Red;
        BusinessFlow mBusinessFlow;

        ConsoleDriverBase mConsoleDriver;
        Agent mAgent;

        public ConsoleDriverWindow(BusinessFlow BF)
        {
            InitializeComponent();
            mBusinessFlow = BF;
        }

        public ConsoleDriverWindow(DriverBase driver, Agent agent)
        {
            InitializeComponent();
            //mBusinessFlow = BF;
            mConsoleDriver = (ConsoleDriverBase)driver;
            mAgent = agent;

            ((DriverBase)mConsoleDriver).DriverMessageEvent += ConsoleDriverWindow_DriverMessageEvent;
        }

        #region Events
        private async void ConsoleDriverWindow_DriverMessageEvent(object sender, DriverMessageEventArgs e)
        {
            switch (e.DriverMessageType)
            {
                case DriverBase.eDriverMessageType.DriverStatusChanged:
                    await this.Dispatcher.InvokeAsync(async () =>
                    {
                        bool connected = mConsoleDriver.IsRunning();
                        Title = mConsoleDriver.ConsoleWindowTitle() + (connected ? " (Connected)" : " (Disconnected)");
                        CommandTextBox.IsEnabled = connected;
                        RecordButton.IsEnabled = connected;
                        // Optionally show a status line if a label exists (ignore if not)
                        try
                        {
                            var statusLbl = this.FindName("xStatusLabel") as System.Windows.Controls.ContentControl;
                            if (statusLbl != null)
                            {
                                statusLbl.Content = connected ? "Connected" : "Disconnected";
                                statusLbl.Foreground = connected ? Brushes.Green : Brushes.Red;
                            }
                        }
                        catch { }
                    });
                    break;

                case DriverBase.eDriverMessageType.ConsoleBufferUpdate:
                    // After a command/action finished, we can auto-scroll or update UI if needed
                    await Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            if(sender is string)
                                ConsoleWriteCommand((string)sender);
                        }
                        catch { }
                    });
                    break;

                case DriverBase.eDriverMessageType.RecordingEvent:
                    // sender carries bool (recording on/off)
                    if (sender is bool recFlag)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            mRecording = recFlag;
                            RecordButton.Foreground = mRecording ? Brushes.Red : Brushes.Black;
                        });
                    }
                    break;
                
                default:
                    break;
            }
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            mConsoleDriver.CleanExpectedString();
            RunCommand();
        }

        private void RunCommand()
        {
            if (string.IsNullOrEmpty(CommandTextBox.Text))
            {
                CommandTextBox.Text = string.Empty;
            }
            if (mRecording)
            {
                ActConsoleCommand ACC = new ActConsoleCommand
                {
                    Description = "Command: " + CommandTextBox.Text,

                    LocateBy = eLocateBy.NA
                };
                ACC.AddOrUpdateInputParamValue("Free Command", CommandTextBox.Text);
                ACC.ConsoleCommand = ActConsoleCommand.eConsoleCommand.FreeCommand;
                mBusinessFlow.AddAct(ACC);
            }

            mConsoleDriver.RunConsoleCommand(CommandTextBox.Text);

            //Checking Console Driver Platform
            if (mConsoleDriver.Platform.ToString() == "Unix")
            {
                //Unix then only \n is required
                mConsoleDriver.taskFinished = false;
                mConsoleDriver.SendCommand(CommandTextBox.Text);
            }
            else
            {
                //Dos then \r\n is required
                mConsoleDriver.SendCommand(CommandTextBox.Text);
            }
            CommandTextBox.Text = "";
        }

        public void ConsoleWriteCommand(string command)
        {
            Paragraph p = new Paragraph();
            p.Inlines.Add(new System.Windows.Documents.Bold(new System.Windows.Documents.Run(command))
            {
                Foreground = ConsoleCommandBrush
            });

            ConsoleTextBox.Document.Blocks.Add(p);
            ConsoleTextBox.ScrollToEnd();
        }
        public void ConsoleWriteText(string txt, bool applyFormat = false)
        {
            mConsoleBuffer.Append(txt + Environment.NewLine);
            Paragraph p = new Paragraph
            {
                LineHeight = 10
            };
            if (applyFormat == true)
            {
                ApplyStyleToText(txt, ref p);
            }
            else
            {
                p.Inlines.Add(new System.Windows.Documents.Bold(new System.Windows.Documents.Run(txt))
                {
                    Foreground = ConsoleTextBrush
                });
            }
            ConsoleTextBox.Document.Blocks.Add(p);
            ConsoleTextBox.ScrollToEnd();
        }

        public void ConsoleWriteError(string txt)
        {
            mConsoleBuffer.Append("ERROR:" + txt);

            Paragraph p = new Paragraph();
            p.Inlines.Add(new System.Windows.Documents.Bold(new System.Windows.Documents.Run("ERROR: " + txt))
            {
                Foreground = Foreground = ConsoleErrorBrush
            });

            ConsoleTextBox.Document.Blocks.Add(p);
            ConsoleTextBox.ScrollToEnd();
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            mRecording = !mRecording;

            if (mRecording)
            {
                RecordButton.Foreground = Brushes.Red;
            }
            else
            {
                RecordButton.Foreground = Brushes.Black;
            }
        }

        private void CommandTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                mConsoleDriver.CleanExpectedString();
                RunCommand();
            }
        }

        internal string RunConsoleCommand(string Command, string WaitForText = null)
        {
            mConsoleBuffer.Clear();
            CommandTextBox.Text = Command;
            RunCommand();

            string rc = mConsoleBuffer.ToString();
            string GingerRCStart = "~~~GINGER_RC_START~~~";
            string GingerRCEnd = "~~~GINGER_RC_END~~~";

            int i = rc.IndexOf(GingerRCStart);
            if (i > 0)
            {
                int i2 = rc.IndexOf(GingerRCEnd, i);
                if (i2 > 0)
                {
                    rc = rc.Substring(i + GingerRCStart.Length + 1, i2 - i - GingerRCEnd.Length - 4);
                }
            }
            mConsoleBuffer.Clear();
            return rc;
        }

        private void GreenBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleTextBox.Background = Brushes.Green;
            ConsoleTextBrush = Brushes.Black;
            ConsoleCommandBrush = Brushes.Blue;
            ConsoleErrorBrush = Brushes.Orange;
        }

        private void BlackBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleTextBox.Background = Brushes.Black;
            ConsoleTextBrush = Brushes.Yellow;
            ConsoleCommandBrush = Brushes.Orange;
            ConsoleErrorBrush = Brushes.Red;
        }

        private void WhiteBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleTextBox.Background = Brushes.White;
            ConsoleTextBrush = Brushes.Black;
            ConsoleCommandBrush = Brushes.Orange;
            ConsoleErrorBrush = Brushes.Red;
        }

        private void TopButton_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        private void NewActionButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement ConsoleNewActionPage code-behind; currently placeholder removed to prevent build error
            //ConsoleNewActionPage CNAP = new ConsoleNewActionPage(mConsoleDriver, mBusinessFlow);
            //CNAP.ShowAsWindow(this);
        }

        private void ConsoleDriverWindow_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            mRecording = false;
            try
            {
                mConsoleDriver.Disconnect();
                //mConsoleDriver.mConsoleDriverWindow = null;
                mConsoleDriver.CloseDriver();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error when try to close Console Driver - " + ex.Message);
            }
        }
        private void ApplyStyleToText(string result, ref Paragraph p)
        {
            try
            {
                if (result == null)
                {
                    result = string.Empty;
                }

                string[] splitResult = result.Split(new string[] { "\u001b[" }, StringSplitOptions.None);
                foreach (string splitLine in splitResult)
                {
                    if (splitLine.IndexOf("m") != -1)
                    {
                        string format = splitLine[..splitLine.IndexOf('m')];
                        string text = splitLine[(splitLine.IndexOf('m') + 1)..];
                        if (ApplyASCIICodeFormat(text, format, ref p) == false)
                        {
                            p.Inlines.Add(new System.Windows.Documents.Bold(new System.Windows.Documents.Run(splitLine)));
                        }
                    }
                    else
                    {
                        p.Inlines.Add(new System.Windows.Documents.Bold(new System.Windows.Documents.Run(splitLine)));
                    }
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in ApplyStyleToResultAsync", e);
            }
        }
        private bool ApplyASCIICodeFormat(string TargetString, string format, ref Paragraph p)
        {
            try
            {
                string[] arrFormat = format.Split(';');
                System.Windows.Documents.Inline txtElem = new System.Windows.Documents.Run(TargetString);
                foreach (string strformat in arrFormat)
                {
                    switch (Convert.ToInt32(strformat))
                    {
                        case 0://Reset                                                
                            break;
                        case 1://Bold
                            txtElem = new System.Windows.Documents.Bold(txtElem);
                            break;
                        case 3://Italic
                            txtElem.FontStyle = FontStyles.Italic;
                            break;
                        case 4://Underline
                            p.TextDecorations = System.Windows.TextDecorations.Underline;
                            break;
                        case 7://Reverse foreground & background
                            break;
                        case 9://Strike Through                               
                            p.TextDecorations = System.Windows.TextDecorations.Strikethrough;
                            break;
                        case 22://Bold Off
                        case 23://Italic Off
                        case 24://Underline off
                        case 27://Inverse off                                
                            p.FontStyle = FontStyles.Normal;
                            break;
                        case 29://StrikeThrough off                                
                            p.TextDecorations = null;
                            break;
                        case 30://Foreground Color to Black                                
                            txtElem.Foreground = System.Windows.Media.Brushes.Black;
                            break;
                        case 31://Foreground Color to Red                                
                            txtElem.Foreground = System.Windows.Media.Brushes.Red;
                            break;
                        case 32://Foreground Color to Green                                
                            txtElem.Foreground = System.Windows.Media.Brushes.LightGreen;
                            break;
                        case 33://Foreground Color to Yellow                                
                            txtElem.Foreground = System.Windows.Media.Brushes.Yellow;
                            break;
                        case 34://Foreground Color to blue                                
                            txtElem.Foreground = System.Windows.Media.Brushes.LightBlue;
                            break;
                        case 35://Foreground Color to Magenta                                
                            txtElem.Foreground = System.Windows.Media.Brushes.Magenta;
                            break;
                        case 36://Foreground Color to Cyan                                
                            txtElem.Foreground = System.Windows.Media.Brushes.Cyan;
                            break;
                        case 37://Foreground Color to White                                
                            txtElem.Foreground = System.Windows.Media.Brushes.White;
                            break;
                        case 39://Foreground Color to Default (White)                               
                            txtElem.Foreground = System.Windows.Media.Brushes.White;
                            break;
                        case 40://Foreground Color to Black                               
                            txtElem.Background = System.Windows.Media.Brushes.Black;
                            break;
                        case 41://Background Color to Red                                
                            txtElem.Background = System.Windows.Media.Brushes.Red;
                            break;
                        case 42://Background Color to Green                                
                            txtElem.Background = System.Windows.Media.Brushes.LightGreen;
                            break;
                        case 43://Background Color to Yellow                               
                            txtElem.Background = System.Windows.Media.Brushes.Yellow;
                            break;
                        case 44://Background Color to Blue                               
                            txtElem.Background = System.Windows.Media.Brushes.Blue;
                            break;
                        case 45://Background Color to Magenta                               
                            txtElem.Background = System.Windows.Media.Brushes.Magenta;
                            break;
                        case 46://Background Color to Cyan                               
                            txtElem.Background = System.Windows.Media.Brushes.Cyan;
                            break;
                        case 47://Background Color to white                                
                            txtElem.Background = System.Windows.Media.Brushes.White;
                            break;
                        case 49://Background Color to Default (Black)                                
                            txtElem.Background = System.Windows.Media.Brushes.Black;
                            break;
                        default:
                            break;
                    }
                }
                p.Inlines.Add(txtElem);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
#endregion