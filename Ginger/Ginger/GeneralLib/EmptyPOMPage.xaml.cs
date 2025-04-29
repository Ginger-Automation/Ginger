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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.GeneralLib
{
    /// <summary>
    /// Interaction logic for EmptyPOMPage.xaml
    /// </summary>
    public partial class EmptyPOMPage : Page
    {
        public string PageTitle { get; set; }

        public ApplicationPOMModel emptyPOM;
        GenericWindow _pageGenericWin = null;
        bool okClicked = false;
        public string pomNameValue;
        public ApplicationPlatform TargetApplicationvalue = null;
        public EmptyPOMPage(ApplicationPOMModel emptyPOM, ObservableList<ApplicationPlatform> targetApps, string title = "")
        {
            InitializeComponent();
            PageTitle = title;
            this.Title = PageTitle;
            DataContext = this;
            this.emptyPOM = emptyPOM;

            xTargetApplicationComboBox.SelectionChanged += xTargetApplicationComboBox_SelectionChanged;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xPOMName, TextBox.TextProperty, emptyPOM, nameof(ApplicationPOMModel.Name));
            xPOMName.AddValidationRule(new POMNameValidationRule());
            xTargetApplicationComboBox.ItemsSource = targetApps;
            xTargetApplicationComboBox.SelectedIndex = 0;
        }

        private void xTargetApplicationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (xTargetApplicationComboBox.SelectedItem != null)
                {
                    TargetApplicationvalue = (ApplicationPlatform)xTargetApplicationComboBox.SelectedValue;
                    Reporter.ToLog(eLogLevel.INFO, $"Target application selected: {TargetApplicationvalue}");
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in selecting TargetApplication " + ex);
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Error in selecting TargetApplication");
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(xPOMName.Text))
            {
                pomNameValue = xPOMName.Text;
                emptyPOM.TargetApplicationKey = TargetApplicationvalue.Key;
                okClicked = true;
                _pageGenericWin.Close();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.RequiredFieldsEmpty);
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button okBtn = new()
            {
                Content = "OK"
            };
            okBtn.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = [];
            winButtons.Add(okBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, true, "Cancel");
        }

    }
}
