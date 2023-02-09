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

using Amdocs.Ginger.Common.InterfacesLib;
using System.Windows;

namespace GingerCore.Drivers.WebServicesDriverLib
{
    /// <summary>
    /// Interaction logic for WebServicesDriverWindow.xaml
    /// </summary>
    public partial class WebServicesDriverWindow : Window, IWebserviceDriverWindow
    {
        public WebServicesDriverWindow(BusinessFlow BF)
        {
            InitializeComponent();

            //TODO: below is Amdocs Proxy - need to externalize
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        public IDispatcher GingerDispatcher { get; set; }

        public void ShowDriverWindow()
        {
            Show();
  
            GingerDispatcher = new DriverWindowDispatcher(Dispatcher);
        }

        public void UpdateRequestParams(string uRL, string value, string mRequest)
        {
            URLTextBox.Text = uRL;
            SOAPActionTextBox.Text = value;
            ReqBox.Text = mRequest;
            General.DoEvents();
        }

        public void UpdateResponseTextBox(string responseCode)
        {
            ResponseTextBox.Text = responseCode;
        }

        public void updateResponseXMLText(string Response)
        {
          RespXML.Text = Response;
        }

        public void UpdateStatusLabel(string status)
        {
            StatusLabel.Content = status;
            General.DoEvents();
        }
    }
}
