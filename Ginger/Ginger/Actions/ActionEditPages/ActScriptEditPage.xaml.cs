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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCore.Actions;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActTextBoxEditPage.xaml
    /// </summary>
    public partial class ActScriptEditPage : Page
    {
        public ActionEditPage actp;
        private ActScript actScript;

        string SHFilesPath = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"Documents\Scripts\");


        public ActScriptEditPage(GingerCore.Actions.ActScript Act)
        {
            InitializeComponent();
            this.actScript = Act;
            GingerCore.General.FillComboFromEnumObj(ScriptActComboBox, Act.ScriptCommand);
            GingerCore.General.FillComboFromEnumObj(ScriptInterpreterComboBox, Act.ScriptInterpreterType);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScriptInterpreterComboBox, ComboBox.SelectedValueProperty, Act, nameof(ActScript.ScriptInterpreterType));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScriptActComboBox, ComboBox.SelectedValueProperty, Act, nameof(ActScript.ScriptCommand));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScriptNameComboBox, ComboBox.SelectedValueProperty, actScript, nameof(ActScript.ScriptName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(chkIgnoreScriptErrors, CheckBox.IsCheckedProperty, actScript, nameof(ActScript.IgnoreStdOutErrors));

            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: ScriptNameComboBox, eventName: nameof(Selector.SelectionChanged), handler: ScriptNameComboBox_SelectionChanged);

            xVScriptInterPreter.Init(Context.GetAsContext(actScript.Context), actScript, nameof(ActScript.ScriptInterpreter), true, true, UCValueExpression.eBrowserType.File, "*.*");

            actScript.ScriptPath = SHFilesPath;

            var comboEnumItem = ScriptInterpreterComboBox.Items.Cast<GingerCore.GeneralLib.ComboEnumItem>().FirstOrDefault(x => x.text == ActScript.eScriptInterpreterType.JS.ToString());
            ScriptInterpreterComboBox.Items.Remove(comboEnumItem);//Removed JS from UI
        }

        private void ScriptActComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Ugly code but working, find way to make it simple use the enum val from combo            
            ActScript.eScriptAct comm = (ActScript.eScriptAct)Enum.Parse(typeof(ActScript.eScriptAct), ScriptActComboBox.SelectedItem.ToString());
            switch (comm)
            {
                case ActScript.eScriptAct.FreeCommand:
                    ScriptStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    ScriptDescriptionPanel.Visibility = Visibility.Collapsed;
                    actScript.RemoveAllButOneInputParam("Free Command");
                    actScript.AddInputValueParam("Free Command");
                    ScriptNameComboBox.SelectedItem = null;
                    break;

                case ActScript.eScriptAct.Script:
                    ScriptStackPanel.Visibility = System.Windows.Visibility.Visible;
                    actScript.RemoveInputParam("Free Command");
                    break;
            }
        }

        private void ScriptNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ScriptNameComboBox.SelectedItem != null)
            {
                string ScriptFile = SHFilesPath + ScriptNameComboBox.SelectedItem;
                if (!Directory.Exists(SHFilesPath))
                {
                    Directory.CreateDirectory(SHFilesPath);
                }
                actScript.ReturnValues.Clear();
                actScript.InputValues.Clear();

                string[] script = File.ReadAllLines(ScriptFile);
                ScriptDescriptionContent.Content = "";
                parseScriptHeader(script);
            }

            if (actScript.InputValues.Count == 0 && ScriptInterpreterComboBox.SelectedValue.ToString() != ActScript.eScriptInterpreterType.VBS.ToString())
            {
                actScript.AddInputValueParam("Value");
            }
        }

        private void parseScriptHeader(string[] script)
        {
            foreach (string line in script)
            {
                if (line.Contains("GINGER_Description"))
                {
                    ScriptDescriptionContent.Content = replaceStartWithInput(line);
                }
                if (line.Contains("GINGER_$"))
                {
                    actScript.AddOrUpdateInputParamValue(replaceStartWithInput(line), "");
                }
            }
            if (String.IsNullOrEmpty(ScriptDescriptionContent.Content.ToString()))
            {
                ScriptDescriptionPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ScriptDescriptionPanel.Visibility = Visibility.Visible;
            }
        }
        private string replaceStartWithInput(string input)
        {
            return Regex.Replace(input, @"^(('|//|#|REM )(GINGER_Description|GINGER_\$))", "").Trim();
        }
        private void ScriptInterpreterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScriptInterpreterComboBox.SelectedValue == null)
            {
                return;
            }
            ActScript.eScriptInterpreterType interpreterType;
            Enum.TryParse(ScriptInterpreterComboBox.SelectedValue.ToString(), out interpreterType);

            string[] fileEntries = null;

            if (!Directory.Exists(SHFilesPath))
            {
                Directory.CreateDirectory(SHFilesPath);
            }

            if (interpreterType == ActScript.eScriptInterpreterType.Other)
            {
                InterpreterPathPanel.Visibility = Visibility.Visible;
                fileEntries = Directory.EnumerateFiles(SHFilesPath, "*.*", SearchOption.AllDirectories)
               .Where(s => s.ToLower().EndsWith(".vbs") || s.ToLower().EndsWith(".js") || s.ToLower().EndsWith(".pl") || s.ToLower().EndsWith(".bat") || s.ToLower().EndsWith(".cmd")
               || s.ToLower().EndsWith(".py") || s.ToLower().EndsWith(".ps1") || s.ToLower().EndsWith(".sh")).ToArray();
            }
            else
            {
                InterpreterPathPanel.Visibility = Visibility.Collapsed;
                fileEntries = GingerCore.General.ReturnFilesWithDesiredExtension(SHFilesPath, "." + interpreterType.ToString().ToLower());
            }
            if (fileEntries != null)
            {
                fileEntries = fileEntries.Select(q => q.Replace(SHFilesPath, "")).ToArray();
                ScriptNameComboBox.ItemsSource = fileEntries;

                if (actScript.ScriptName == null)
                {
                    ScriptNameComboBox.SelectedValue = fileEntries.FirstOrDefault();
                }
            }
        }
        private void chkIgnoreScriptErrorsChecked(object sender, RoutedEventArgs e)
        {
            actScript.IgnoreStdOutErrors = true;
            actScript.InvokPropertyChanngedForAllFields();
        }

        private void chkIgnoreScriptErrorsUnChecked(object sender, RoutedEventArgs e)
        {
            actScript.IgnoreStdOutErrors = false;
            actScript.InvokPropertyChanngedForAllFields();
        }
    }
}


