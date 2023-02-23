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
using System.Windows.Controls;
using mshtml;

namespace Ginger.Support
{
    /// <summary>
    /// Interaction logic for SupportPage.xaml
    /// </summary>
    public partial class WebSupportPage : Page
    {
        private mshtml.HTMLDocument mDocument;
        public WebSupportPage()
        {
            InitializeComponent();
            try
            {
                ViewWebBrowser.Navigated += browser_Navigated;
                ViewWebBrowser.Navigate("http://ginger");

            }
            catch(Exception )
            {
                //TODO: display message saying support's unavailable & disable buttons
             
            }  
        }

        private const string DisableScriptError =
             @"function noError() {
                return true;
            }
            window.onerror = noError;";

        private void InjectDisableScript()
        {
            mshtml.HTMLDocument doc2 = (HTMLDocument)ViewWebBrowser.Document;
            IHTMLScriptElement scriptErrorSuppressed = (IHTMLScriptElement)doc2.createElement("SCRIPT");
            scriptErrorSuppressed.type = "text/javascript";
            scriptErrorSuppressed.text = DisableScriptError;

            IHTMLElementCollection nodes = mDocument.getElementsByTagName("head");
            foreach (IHTMLElement elem in nodes)
            {
                HTMLHeadElement head = (HTMLHeadElement)elem;
                head.appendChild((IHTMLDOMNode)scriptErrorSuppressed);
            }
        }

        private void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            mDocument = (mshtml.HTMLDocument)ViewWebBrowser.Document;
            InjectDisableScript();
        }
    }
}