#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Ginger.ValidationRules;
using GingerCore.Actions;
using System.Windows;
using System.Windows.Controls;


namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActCLIOrchestrationEditPage.xaml
    /// </summary>
    public partial class ActCLIOrchestrationEditPage : Page
    {
        private ActCLIOrchestration mAct;
        string SHFilesPath = $"{WorkSpace.Instance.Solution.Folder}{System.IO.Path.DirectorySeparatorChar}Documents{System.IO.Path.DirectorySeparatorChar}Scripts{System.IO.Path.DirectorySeparatorChar}";
        public ActCLIOrchestrationEditPage(ActCLIOrchestration act)
        {
            InitializeComponent();
            mAct = act;
            Context mContext = new Context();
            FilePath.FileExtensions.Add(".*");
            FilePath.Init(act, nameof(mAct.FilePath), true);
            FilePath.FilePathTextBox.TextChanged += FilePathTextBox_TextChanged;
            mAct.ScriptPath = SHFilesPath;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(WaitForProcessToFinish, System.Windows.Controls.CheckBox.IsCheckedProperty, act, nameof(mAct.WaitForProcessToFinish));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ParseResult, System.Windows.Controls.CheckBox.IsCheckedProperty, act, nameof(mAct.ParseResult));
            xDelimiterTextBox.Init(mContext, act, nameof(mAct.Delimiter));
            if (mAct.WaitForProcessToFinish)
            {
                xPanelParseResult.Visibility = Visibility.Visible;
                xPanelDelimiter.Visibility = Visibility.Visible;
                if (mAct.ParseResult)
                {
                    xPanelDelimiter.Visibility = Visibility.Visible;
                    xDelimiterTextBox.ValueTextBox.TextChanged += DelimiterTextBox_TextChanged;
                }
                else
                {
                    xPanelDelimiter.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                xPanelParseResult.Visibility = Visibility.Collapsed;
                xPanelDelimiter.Visibility = Visibility.Collapsed;
            }
            ApplyValidationRules();

        }

        private void ApplyValidationRules()
        {
            // check if fields have been populated (font-end validation)
            FilePath.FilePathTextBox.AddValidationRule(new ValidateEmptyValue("Application/File path cannot be empty"));
            xDelimiterTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Delimiter cannot be empty"));

            CallPropertyChange();
        }

        private void CallPropertyChange()
        {
            mAct.OnPropertyChanged(nameof(mAct.FilePath));
            mAct.OnPropertyChanged(nameof(mAct.Delimiter));
        }

        private void DelimiterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(mAct.Delimiter))
            {
                return;
            }
            mAct.Delimiter = xDelimiterTextBox.ValueTextBox.Text;
            mAct.InvokPropertyChanngedForAllFields();
        }

        private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(FilePath.FilePathTextBox.Text))
            {
                return;
            }
            mAct.FilePath = FilePath.FilePathTextBox.Text;
            mAct.InvokPropertyChanngedForAllFields();
        }

        private void WaitForProcessToFinishChecked(object sender, RoutedEventArgs e)
        {
            mAct.WaitForProcessToFinish = true;
            xPanelParseResult.Visibility = Visibility.Visible;
            if (mAct.ParseResult)
            {
                xPanelDelimiter.Visibility = Visibility.Visible;
            }
            else
            {
                xPanelDelimiter.Visibility = Visibility.Collapsed;
            }
            mAct.InvokPropertyChanngedForAllFields();
        }

        private void WaitForProcessToFinishUnChecked(object sender, RoutedEventArgs e)
        {
            mAct.WaitForProcessToFinish = false;
            xPanelParseResult.Visibility = Visibility.Collapsed;
            xPanelDelimiter.Visibility = Visibility.Collapsed;
            mAct.InvokPropertyChanngedForAllFields();

        }

        private void ParseResultChecked(object sender, RoutedEventArgs e)
        {
            mAct.ParseResult = true;
            xPanelDelimiter.Visibility = Visibility.Visible;
            mAct.InvokPropertyChanngedForAllFields();
        }

        private void ParseResultUnChecked(object sender, RoutedEventArgs e)
        {
            mAct.ParseResult = false;
            xPanelDelimiter.Visibility = Visibility.Collapsed;
            mAct.InvokPropertyChanngedForAllFields();
        }
    }
}
