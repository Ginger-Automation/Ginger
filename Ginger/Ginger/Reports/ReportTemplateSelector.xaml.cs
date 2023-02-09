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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Reports.Designer;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Reports
{
    /// <summary>
    /// Interaction logic for ReportTemplateSelector.xaml
    /// </summary>
    public partial class ReportTemplateSelector : Page
    {
        GenericWindow _pageGenericWin = null;

        public ReportTemplate SelectedReportTemplate = null;

        ObservableList<ReportTemplate> ReportTemplateList;

        public ReportTemplateSelector()
        {
            InitializeComponent();

            ReportTemplateList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ReportTemplate>();

            AddInternalTemplates();

            ReportsListBox.DisplayMemberPath = ReportTemplate.Fields.Name;            
            ReportsListBox.ItemsSource = ReportTemplateList;

            ReportsListBox.SelectionChanged += ReportsListBox_SelectionChanged;
        }

        private void AddInternalTemplates()
        {
            List <ReportTemplate> lst = ReportTemplate.GetInternalTemplates();

            foreach (ReportTemplate RT in lst)
            {
                ReportTemplateList.Add(RT);
            }
        }

        private void ReportsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowPreview();
        }

        void ShowPreview()
        {
            string Xaml = ((ReportTemplate)ReportsListBox.SelectedItem).Xaml;            
            PreviewFrame.Content = ReportTemplateDesignerPage.GetSampleReportPage(Xaml);
        }

        public void ShowAsWindow()
        {
                Button OKButton = new Button();
                OKButton.Content = "Ok";
                OKButton.Click += new RoutedEventHandler(OKButton_Click);
                
                ObservableList<Button> winButtons = new ObservableList<Button>();
                winButtons.Add(OKButton);

                GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, eWindowShowStyle.Dialog , "Select Report Template", this, winButtons);            
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedReportTemplate = (ReportTemplate)ReportsListBox.SelectedItem;
            _pageGenericWin.Close();
        }
    }
}