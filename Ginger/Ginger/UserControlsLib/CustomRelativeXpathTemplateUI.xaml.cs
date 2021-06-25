using Amdocs.Ginger.Common;
#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.CoreNET.Application_Models.Common;
using Ginger.UserControls;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib.POMLearnig
{
    /// <summary>
    /// Interaction logic for CustomRelativeXpathTemplate.xaml
    /// </summary>
    public partial class CustomRelativeXpathTemplateUI : UserControl
    {
        public ObservableList<CustomRelativeXpathTemplate> RelativeXpathTemplateList { get; set; }
        public CustomRelativeXpathTemplateUI()
        {
            InitializeComponent();
            ShowsRelativePathCofigGridView();
            xRelXpathSettingsGrid.Grid.RowValidationRules.Add(new ValidateCustomRelativeXpath());
        }

        private void ShowsRelativePathCofigGridView()
        {
            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            var statusList = Enum.GetValues(typeof(CustomRelativeXpathTemplate.SyntaxValidationStatus));

            view.GridColsView.Add(new GridColView() { Field = nameof(CustomRelativeXpathTemplate.Value), Header = "Xpath Template", WidthWeight = 100, ReadOnly = false });
            view.GridColsView.Add(new GridColView() { Field = nameof(CustomRelativeXpathTemplate.Status), Header = "Syntax Validation", CellValuesList= statusList, WidthWeight = 100, ReadOnly = true });

            xRelXpathSettingsGrid.SetAllColumnsDefaultView(view);
            xRelXpathSettingsGrid.InitViewItems();

            AddDefaultTemplate();

            xRelXpathSettingsGrid.DataSourceList = RelativeXpathTemplateList;
            xRelXpathSettingsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddRelativexPathTemplateRow));
        }

      
        private void AddDefaultTemplate()
        {
            if (RelativeXpathTemplateList == null)
            {
                RelativeXpathTemplateList = new ObservableList<CustomRelativeXpathTemplate>();
            }
            RelativeXpathTemplateList.Add(new CustomRelativeXpathTemplate() { Value = "//*[@placeholder=\'\' and @type=\'\']",Status = CustomRelativeXpathTemplate.SyntaxValidationStatus.Passed});
            RelativeXpathTemplateList.Add(new CustomRelativeXpathTemplate() { Value = "//*[@AutomationID=\'\' or @type=\'\']",Status = CustomRelativeXpathTemplate.SyntaxValidationStatus.Passed });
        }

        private void AddRelativexPathTemplateRow(object sender, RoutedEventArgs e)
        {
            RelativeXpathTemplateList.Add(new CustomRelativeXpathTemplate() { Value = string.Empty, Status = CustomRelativeXpathTemplate.SyntaxValidationStatus.Failed });
        }

        private void xCustomRelativeXpathCofigChkBox_Click(object sender, RoutedEventArgs e)
        {
            if (xCustomRelativeXpathCofigChkBox.IsChecked.Equals(true))
            {
                xRelXpathSettingsGrid.Visibility = Visibility.Visible;
            }
            else
            {
                xRelXpathSettingsGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}
