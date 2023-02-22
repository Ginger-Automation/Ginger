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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Ginger.UserControlsLib.TextEditor.Office
{
    /// <summary>
    /// Interaction logic for OfficeDocumentPage.xaml
    /// </summary>
    public partial class OfficeDocumentPage : System.Windows.Controls.Page, ITextEditorPage 
    {        
        string mFileName;

        public OfficeDocumentPage()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hwc, IntPtr hwp);

        public bool Load(string FileName)
        {
            mFileName = FileName;
            FileNameLabel.Content = FileName;

            return true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ////TODO:for speed do the cleanup only when Ginger is closing, so keep word app ready, report generation will be faster 4-5 secs to start word
        }

        public void HostWindowInFrame(System.Windows.Controls.Frame fraContainer, System.Windows.Window win)
        {
            object tmp = win.Content;
            
            win.Content = null;
            
            fraContainer.Content = new System.Windows.Controls.ContentControl() { Content = tmp };
        }

        IntPtr GetHWND()
        {
            HwndSource hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            return hwndSource.Handle;
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        static void Cleanup()
        {
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }


        void OpenFile()
        {            
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo();
            procStartInfo.FileName = mFileName;
            procStartInfo.CreateNoWindow = true;
            procStartInfo.UseShellExecute = true;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
        }
    }
}
