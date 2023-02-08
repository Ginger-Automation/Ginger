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
using Amdocs.Ginger.CoreNET.GeneralLib;
using mshtml;
using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Windows.Controls;

namespace GingerCore.Drivers
{
    /// <summary>
    /// Interaction logic for WebBrowserPage.xaml
    /// </summary>
    public partial class WebBrowserPage : Page
    {
        public WebBrowserPage()
        {
            InitializeComponent();            
            
            SetBrowserFeatures();
        }

        private void SetBrowserFeatures()
        {
            //Get the native browser wrapped by WPF Web Browser
            SHDocVw.IWebBrowser2 axBrowser = typeof(WebBrowser).GetProperty("AxIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(browser, null) as SHDocVw.IWebBrowser2;
            // Hook opening new window event
            ((SHDocVw.DWebBrowserEvents_Event)axBrowser).NewWindow += OnWebBrowserNewWindow;
        }

        private void OnWebBrowserNewWindow(string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed)
        {
            // We want to open the new window inside IB
            Processed = true;
            browser.Navigate(URL);
        }

        public WebBrowser GetBrowser()
        {
            return browser;            
        }

        public string GetElementXPath(mshtml.IHTMLElement element)
        {
            if (element == null)
                return "";
            mshtml.IHTMLElement currentNode = element;
            ArrayList path = new ArrayList();

            while (currentNode != null)
            {
                string pe = getNode(currentNode);
                if (pe != null)
                {
                    path.Add(pe);
                    if (pe.IndexOf("@id") != -1)
                        break;  // Found an ID, no need to go upper, absolute path is OK
                }
                currentNode = currentNode.parentElement;
            }
            path.Reverse();
            return join(path, "/");
        }

        private static string getNode(mshtml.IHTMLElement node)
        {
            string nodeExpr = node.tagName;
            if (nodeExpr == null)  // Eg. node = #text
                return null;
            if (node.id != "" && node.id != null)
            {
                nodeExpr += "[@id='" + node.id + "']";
                // We don't really need to go back up to //HTML, since IDs are supposed
                // to be unique, so they are a good starting point.
                return "/" + nodeExpr;
            }

            // Find rank of node among its type in the parent
            int rank = 1;
            mshtml.IHTMLDOMNode nodeDom = node as mshtml.IHTMLDOMNode;            
            mshtml.IHTMLDOMNode psDom = nodeDom.previousSibling;

            mshtml.IHTMLElement ps = psDom as mshtml.IHTMLElement;
            while (ps != null)
            {
                if (ps.tagName == node.tagName)
                {
                    rank++;
                }
                
                    psDom = psDom.previousSibling;
                ps = psDom as mshtml.IHTMLElement;
            }
            if (rank > 1)
            {
                nodeExpr += "[" + rank + "]";
            }
            else
            { // First node of its kind at this level. Are there any others?
                mshtml.IHTMLDOMNode nsDom = nodeDom.nextSibling;
                mshtml.IHTMLElement ns = nsDom as mshtml.IHTMLElement;
                while (ns != null)
                {
                    if (ns.tagName == node.tagName)
                    { // Yes, mark it as being the first one
                        nodeExpr += "[1]";
                        break;
                    }
                    nsDom = nsDom.nextSibling;
                    ns = nsDom as mshtml.IHTMLElement;
                }
            }
            return nodeExpr;
        }

        private static string join(ArrayList items, string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object item in items)
            {
                if (item == null)
                    continue;

                sb.Append(delimiter);
                sb.Append(item);
            }
            return sb.ToString();
        }
        
        // -----------------------------------------------------------------------------------------------------------------------
        // Get Element by XPath - using JS injection and wgxpath code
        // -----------------------------------------------------------------------------------------------------------------------

        public mshtml.IHTMLElement GetElementByXPath(string Xpath)
        { 
            mshtml.HTMLDocument doc = (mshtml.HTMLDocument)browser.Document;
            
            //Check if it is better to create temp js file instead of pushing the whole code to the html doc
            //TODO: inject only once            
            var v= doc.body.getAttribute("data-GingerXpath");
            if (v is System.DBNull)
            {
                injectScriptCode(doc, JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.wgxpath_install));
                string js = GetXpathJS();
                injectScriptCode(doc, js);
                doc.body.setAttribute("data-GingerXpath", "Done");
            }
            var elem = GetHtmlElement(Xpath);
            return elem;
        }

        public void injectScriptCode(mshtml.HTMLDocument doc, string JSCode)
        {
            var script = (IHTMLScriptElement)doc.createElement("SCRIPT");
            script.type = "text/javascript";

            script.text = JSCode;

            InjectJSScript(doc, script);            
        }

        public void InjectJSScript(mshtml.HTMLDocument doc, IHTMLScriptElement JavaSCript)
        {
            IHTMLElementCollection nodes = doc.getElementsByTagName("head");
            foreach (IHTMLElement elem in nodes)
            {
                var head = (HTMLHeadElement)elem;
                head.appendChild((IHTMLDOMNode)JavaSCript);
            }
        }

        private string GetXpathJS()
        {
            //Make sure we run the inject only once
            string javaScriptText = @"
                function getXPath(xPath)
                {                    
                    wgxpath.install();                                 
                    var xPathRes = document.evaluate(xPath, document.body, null, XPathResult.ORDERED_NODE_ITERATOR_TYPE, null);
                    // Get the first element matching
                    var nextElement = xPathRes.iterateNext();                
                    return nextElement;
                };
                ";
            return javaScriptText;
        }

        /// <summary>
        /// Gets Html element's mshtml.IHTMLElement object instance using XPath query
        /// </summary>
        public mshtml.IHTMLElement GetHtmlElement(string xPathQuery)
        {
            try
            {                
                string code = string.Format("getXPath(\"{0}\")", xPathQuery);                              
                dynamic el = InvokeJS(code); 
                return (mshtml.IHTMLElement)el;
            }
            catch(Exception ex)
            {                
                Reporter.ToUser(eUserMsgKey.JSExecutionFailed, ex.Message);
                return null;
            }
        }

        public dynamic InvokeJS(string JSCode)
        {
            return browser.InvokeScript("eval", new object[] { JSCode });
        }
    }
}
