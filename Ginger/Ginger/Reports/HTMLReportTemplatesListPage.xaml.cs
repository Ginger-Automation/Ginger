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
using Ginger.UserControls;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Reports
{
    /// <summary>
    /// Interaction logic for HTMLReportTemplatesListPage.xaml
    /// </summary>
    public partial class HTMLReportTemplatesListPage : Page
    {
        HTMLReportsConfiguration _selectedHTMLReportConfiguration = new HTMLReportsConfiguration();
        public HTMLReportTemplatesListPage()
        {
            InitializeComponent();

            SetGridView();
            SetGridData();
            _selectedHTMLReportConfiguration = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.FirstOrDefault(x => (x.IsSelected == true));
        }

        private void SetGridView()
        {
            grdHTMLReportTemplatesList.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = HTMLReportConfiguration.Fields.Name, WidthWeight = 200, ReadOnly = true },
                new GridColView() { Field = HTMLReportConfiguration.Fields.IsDefault, Header = "Default Template", StyleType = GridColView.eGridColStyleType.CheckBox, ReadOnly = true },
                new GridColView() { Field = HTMLReportConfiguration.Fields.Description, WidthWeight = 400, ReadOnly = true },
            ]
            };

            grdHTMLReportTemplatesList.SetAllColumnsDefaultView(view);
            grdHTMLReportTemplatesList.InitViewItems();


            grdHTMLReportTemplatesList.AddToolbarTool(Amdocs.Ginger.Common.Enums.eImageType.Check, "Set Selected HTML Reports Template as Default", new RoutedEventHandler(SetTemplateAsDefault));

        }

        private void SetGridData()
        {
            ObservableList<HTMLReportConfiguration> templates = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetSolutionHTMLReportConfigurations();
            grdHTMLReportTemplatesList.DataSourceList = templates;
        }



        private void SetTemplateAsDefault(object sender, System.Windows.RoutedEventArgs e)
        {
            if (grdHTMLReportTemplatesList.CurrentItem != null && ((HTMLReportConfiguration)grdHTMLReportTemplatesList.CurrentItem).IsDefault == false)
            {
                Ginger.Reports.GingerExecutionReport.ExtensionMethods.SetTemplateAsDefault((HTMLReportConfiguration)grdHTMLReportTemplatesList.CurrentItem);
            }
        }




    }
}
