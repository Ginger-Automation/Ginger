#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using GingerCore.Actions.Common;
using GingerCore.XPathParser;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for LocateByXPathEditPage.xaml
    /// </summary>
    public partial class LocateByXPathEditPage : Page
    {
        ActUIElement mAction;

        public LocateByXPathEditPage(ActUIElement Action)
        {
            InitializeComponent();

            mAction = Action;

            XPathTextBox.BindControl(mAction, ActUIElement.Fields.ElementLocateValue);

            VerifyXPath();
        }

        private void VerifyXPath()
        {
            string Xpath = mAction.ElementLocateValue;
            try
            {                
                XPathParser<XElement> xpp = new XPathParser<XElement>();
                XElement xe1 = xpp.Parse(Xpath, new XPathTreeBuilder());
                xpp.GetOKPath();
                                              
                XElement xe = new XPathParser<XElement>().Parse(Xpath, new XPathTreeBuilder());

                XmlWriterSettings ws = new XmlWriterSettings();
                {
                    ws.Indent = true;
                    ws.OmitXmlDeclaration = true;
                }
                StringBuilder sb = new StringBuilder();
                using (XmlWriter w = XmlWriter.Create(sb, ws))
                {
                    xe.WriteTo(w);
                }  

                ResultLabel.Content = sb.ToString();
            }
            catch (Exception e)
            {
                
                ResultLabel.Content = e.Message;
            }       
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            VerifyXPath();            
        }        
    }
}
