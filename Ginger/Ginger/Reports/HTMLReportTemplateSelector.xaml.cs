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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
namespace Ginger.Reports
{
    /// <summary>
    /// Interaction logic for ReportTemplateSelector.xaml
    /// </summary>
    public partial class HTMLReportTemplateSelector : Page
    {
        GenericWindow _pageGenericWin = null;
        public HTMLReportTemplate SelectedReportTemplate = null;
        ObservableList<HTMLReportTemplate> HTMLReportTemplateList;

        public HTMLReportTemplateSelector()
        {
            InitializeComponent();

            HTMLReportTemplateList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportTemplate>();
            AddInternalTemplates();
            ReportsListBox.DisplayMemberPath = ReportTemplate.Fields.Name;
            ReportsListBox.ItemsSource = HTMLReportTemplateList;
            ReportsListBox.SelectionChanged += ReportsListBox_SelectionChanged;
        }

        private void AddInternalTemplates()
        {
            List<HTMLReportTemplate> lst = HTMLReportTemplate.GetInternalTemplates();
            foreach (HTMLReportTemplate RT in lst)
            {
                HTMLReportTemplateList.Add(RT);
            }
        }

        private void ReportsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowPreview();
        }

        void ShowPreview()
        {
            string HTML = ((HTMLReportTemplate)ReportsListBox.SelectedItem).HTML;
            BodyWebBrowser.NavigateToString(HTML);
        }

        public void ShowAsWindow()
        {
            Button OKButton = new Button
            {
                Content = "Ok"
            };
            OKButton.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = [OKButton];
            this.Title = "Select HTMl Report Template";
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, eWindowShowStyle.Dialog, "Select HTMl Report Template", this, winButtons);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedReportTemplate = (HTMLReportTemplate)ReportsListBox.SelectedItem;
            _pageGenericWin.Close();
        }
    }
}