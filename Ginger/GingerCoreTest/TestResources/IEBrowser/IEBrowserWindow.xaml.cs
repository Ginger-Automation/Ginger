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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GingerTestHelper;
using mshtml;
using UnitTests;

namespace GingerUnitTests.Documents.IEBrowser
{
    /// <summary>
    /// Interaction logic for IEBrowserWindow.xaml
    /// </summary>
    public partial class IEBrowserWindow : Window
    {

        string SnippestFolder = TestResources.GetTestResourcesFolder(@"IEBrowser\ScriptSnippests");

        public IEBrowserWindow()
        {
            InitializeComponent();

            FillScriptsCombo();
            URLTextBox.Text = TestResources.GetTestResourcesFile(@"HTML\HTMLControls.html");
        }

        private void FillScriptsCombo()
        {
            
            string[] Files = Directory.GetFiles(SnippestFolder);
            foreach (string ScriptFileName in Files)
            {
                //Show only the file name
                ScriptSnippestComboBox.Items.Add(ScriptFileName.Replace(SnippestFolder,""));
            }
            
        }

        private void ScriptSnippestComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ScriptFileName = System.IO.Path.Combine(SnippestFolder + ScriptSnippestComboBox.SelectedItem.ToString());
            string txt = System.IO.File.ReadAllText(ScriptFileName);
            ScriptTextBlock.Text = txt;

        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            browser.Navigate(URLTextBox.Text);
        }

        public void InvokeScript(string script)
        {
            ScriptTextBlock.Text = script;
            RunScriptonBrowser(script);
        }

        private void InvokeScriptButton_Click(object sender, RoutedEventArgs e)
        {
            RunScriptonBrowser(ScriptTextBlock.Text);
        }

        private void RunScriptonBrowser(string Script)
        {           
            dynamic rc = browser.InvokeScript("eval", Script);
            OutputTextBox.Text = rc + "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InjectGingerHTMLHelperButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: check if Jquery already exist befire injecting
            
            string script = GingerCore.Helpers.HTMLHelper.GetJquery();            
            InvokeScript(script);

            //TODO: check if not already injected, set a flag

            script = GetXPathScript();
            InvokeScript(script);
            
            script = GingerCore.Helpers.HTMLHelper.GetGingerHTMLHelper();            
            InvokeScript(script);
        }

        //TODO: move me to HTMLHelper
        private string GetXPathScript()
        {
            
            string script = GingerCore.Helpers.HTMLHelper.wgxpath_install();            

            // string script = File.ReadAllText(@"c:\temp\xpath.js");
            // script = "function aa(){} alert('aaa');";

            script += "wgxpath.install();";
            script += "function getElementByXPath(xPath){";
            script += "var xPathRes = document.evaluate(xPath, document.body, null, XPathResult.ORDERED_NODE_ITERATOR_TYPE, null);";
            // Get the first element matching
            script += "var nextElement = xPathRes.iterateNext();";
            script += "return nextElement;";
            script += "};\n";
            return script;
        }

        private void ClearOutputButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Text = "";
        }

        



    }
}
