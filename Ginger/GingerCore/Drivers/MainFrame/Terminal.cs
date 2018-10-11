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
using Open3270;
using Open3270.TN3270;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace GingerCore.Drivers.MainFrame
{
    public class Terminal : INotifyPropertyChanged
    {
        public  Open3270.TNEmulator emu = null;
        public  string screenText;
        bool isConnected;
        bool isConnecting;
        MainFrameDriver MFDriver;

        public Terminal(string Host, int Port, string TermType, bool RequireSSl, int RowsCount, int ColumnsCount ,MainFrameDriver mdriver)
        {
            this.emu = new TNEmulator();
            this.emu.Disconnected += emu_Disconnected;
            this.emu.Config.HostName = Host;
            this.emu.Config.HostPort = Port;
            this.emu.Config.TermType = TermType;
            this.emu.Config.UseSSL = RequireSSl;
            emu.Config.RowsCount = RowsCount;
            emu.Config.ColumnsCount = ColumnsCount;
            MFDriver = mdriver;
        }

        void emu_Disconnected(TNEmulator where, string Reason)
        {
            this.IsConnected = false;
            this.IsConnecting = false;
            this.ScreenText = Reason;
        }

        public XMLScreen GetScreenAsXML()
        {
            Refresh();
            XMLScreen XmlS = (XMLScreen)emu.GetScreenAsXML ();
            XmlS.Render (emu.Config.ColumnsCount,emu.Config.RowsCount);
            return XmlS;
        }   

        public bool IsConnecting
        {
            get
            {
                return this.isConnecting;
            }
            set
            {
                this.isConnecting = value;
                this.OnPropertyChanged("IsConnecting");
            }
        }

        /// <summary>
        /// Indicates when the terminal is connected to the host.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.emu.IsConnected;
            }
            set
            {
                this.isConnected = value;
                this.OnPropertyChanged("IsConnected");
            }
        }

        /// <summary>
        /// This is the text buffer to display.
        /// </summary>
        public string ScreenText
        {
            get
            {
                return this.screenText;
            }
            set
            {
                this.screenText = value;
                this.OnPropertyChanged("ScreenText");
            }
        }

        int caretIndex;
        public int CaretIndex
        {
            get
            {
                return this.caretIndex;
            }
            set
            {
                this.caretIndex = value;
                this.OnPropertyChanged("CaretIndex");
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged
        
        /// <summary>
        /// Sends text to the terminal.
        /// This is used for typical alphanumeric text entry.
        /// </summary>
        /// <param name="text">The text to send</param>
        internal void SendText(string text)
        {
             text = Regex.Replace(text, @"\r\n?|\n", String.Empty);
            try
            {
                this.emu.SendText (text);
                this.ScreenText = this.emu.CurrentScreenXML.Dump ();
                this.UpdateCaretIndex ();
            }
            catch
            {
            }
        }


        /// <summary>
        /// Sends a character to the terminal.
        /// This is used for special characters like F1, Tab, et cetera.
        /// </summary>
        /// <param name="key">The key to send.</param>
        public void SendKey(TnKey key)
        {
            try
            {
                this.emu.SendKey (true, key, 2000);
                this.UpdateCaretIndex ();
                if (key != TnKey.Tab && key != TnKey.BackTab)
                {
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        /// <summary>
        /// Forces a refresh and updates the screen display
        /// </summary>
        public void Refresh()
        {
            this.Refresh(100);
        }

        /// <summary>
        /// Forces a refresh and updates the screen display
        /// </summary>
        /// <param name="screenCheckInterval">This is the speed in milliseconds at which the library will poll 
        /// to determine if we have a valid screen of data to display.</param>
        public void Refresh(int screenCheckInterval)
        {
            //This line keeps checking to see when we've received a valid screen of data from the mainframe.
            //It loops until the TNEmulator.Refresh() method indicates that waiting for the screen did not time out.
            //This helps prevent blank screens, etc.
            while (!this.emu.Refresh(true, screenCheckInterval)) { }

            this.ScreenText = this.emu.CurrentScreenXML.Dump();
            this.UpdateCaretIndex();
        }

        public void UpdateCaretIndex()
        {
            this.CaretIndex = this.emu.CursorY * 81 + this.emu.CursorX;
        }

        public void  SetCaretIndex(int Caret)
        {
            try
            {
                int PosY = Caret / 81;
                int PosX = Caret - (81 * PosY);

                this.emu.SetCursor (PosX, PosY);
            }
            catch
            {
            }
        }
        public void SetCaretIndex(int locX,int locY)
        {
            try
            {
                this.emu.SetCursor (locX, locY);
            }
            catch
            {
            }
        }
        public List<int>  GetXYfromCaretIndex(int Caret)
        {
            int PosY = Caret / 81;
            int PosX = Caret - (81 * PosY);
            List<int> XY = new List<int>();
            XY.Add(PosX);
            XY.Add(PosY);
            return XY;
        }

        /// <summary>
        /// Sends field information to the debug console.
        /// This can be used to define well-known field positions in your application.
        /// </summary>
        internal void DumpFillableFields()
        {
            string output = string.Empty;

            XMLScreenField field;

            for (int i = 0; i < this.emu.CurrentScreenXML.Fields.Length; i++)
            {
                field = this.emu.CurrentScreenXML.Fields[i];
                if (!field.Attributes.Protected)
                {
                    Debug.WriteLine(string.Format("public static int fieldName = {0};   //{1},{2}  Length:{3}   {4}", i, field.Location.top + 1, field.Location.left + 1, field.Location.length, field.Text));
                }
            }
        }

        public bool  Connect()
        {
            emu.Config.FastScreenMode = true;          
            return Check();         
        }

        public void Disconnect()
        {   //Dispose Function takes care of the disconnect, for more go to Open3270 Project  TNEmulator.cs line no 125 
            emu.Dispose();
        }
        private bool ConnectToHost()
        {            
            try
            {
                emu.Connect();
                emu.Refresh(true, 1000);
                return  true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog (eAppReporterLogLevel.ERROR, "Failed to connect to Mainframe source : Terminal.cs->ConnectToHost() ",ex);
                return false;
            }           
        }

        bool Check()
        {
            bool status = false; 
            try
            {
                status=ConnectToHost();
                IsConnecting = true;

                Stopwatch SW = new Stopwatch();
                SW.Start();
                if (!emu.IsConnected && SW.ElapsedMilliseconds < 20000)
                {
                    Thread.Sleep(100);
                }
                IsConnected = emu.IsConnected;

                IsConnecting = false;              
            }
            catch (Exception e)
            {
                status = false;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}");
            }
            return status;
        }
    
        public string GetTextatPosition(int CaretPosition,int length =10)
        {
            List<int> xy= GetXYfromCaretIndex(CaretPosition);
            string a=  this.emu.GetText(xy.ElementAt(0), xy.ElementAt(1),length);
            Console.WriteLine(a);
            return a;
        }
        public string GetTextatPosition(int x, int y, int length = 10)
        {           
            string a = this.emu.GetText(x,y, length);
            return a;
        }
    }
}
