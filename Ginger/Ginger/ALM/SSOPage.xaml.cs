#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using GingerCoreNET.ALMLib;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ALM
{
    /// <summary>
    /// Interaction logic for SSOPage.xaml
    /// </summary>
    public partial class SSOPage : Page
    {
        private string ssoURL = string.Empty;
        private ALMIntegrationEnums.eALMType almType;
        GenericWindow _pageGenericWin;
        public SSOPage(string loginURL, ALMIntegrationEnums.eALMType aLMType)
        {
            InitializeComponent();
            this.ssoURL = loginURL;
            this.almType = aLMType;
            xBrowser.LoadCompleted += XBrowser_LoadCompleted;
            xBrowser.Navigated += XBrowser_Navigated;

        }

        private void XBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (almType == ALMIntegrationEnums.eALMType.Octane)
            {
                // For Suppressing the error pop-ups coming during Octane SSO Authentications
                SetSilent(sender as WebBrowser, true);
            }
        }

        private static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
            {
                throw new ArgumentNullException(nameof(browser));
            }

            if (browser.Document is IOleServiceProvider sp)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }
        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        private void XBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // For Auto Closing the Browser once the SSO Authentication is successful.
            if (almType == ALMIntegrationEnums.eALMType.Octane && e.Uri.AbsoluteUri.Contains("close-browser.html"))
            {
                CloseWindow(sender, e);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            xBrowser.Navigate(new Uri(this.ssoURL));
        }


        public void ShowAsWindow()
        {
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, eWindowShowStyle.Dialog, this.Title, this, closeEventHandler: CloseWindow);
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            xBrowser.LoadCompleted -= XBrowser_LoadCompleted;
            xBrowser.Navigated -= XBrowser_Navigated;
            xBrowser.Dispose();
            _pageGenericWin.Close();
        }

    }
}
