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
using Ginger.Drivers;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for AgentDriverConfigPage.xaml
    /// </summary>
    public partial class AgentDriverConfigPage : Page
    {
        Agent mAgent;

        enum eConfigsViewType { Grid,Page};
        eConfigsViewType mConfigsViewType;

        public AgentDriverConfigPage(Agent agent)
        {
            InitializeComponent();

            mAgent = agent;
            mAgent.PropertyChanged += Agent_PropertyChanged;

            InitAgentDriverConfigs();
            SetDriverConfigsPageContent();
        }

        private void InitAgentDriverConfigs()
        {
            if (mAgent.DriverConfiguration == null)
            {
                mAgent.AgentOperations.InitDriverConfigs();
                if (mAgent.DriverConfiguration == null)
                {
                    Reporter.ToUser(eUserMsgKey.DriverConfigUnknownDriverType, mAgent.DriverType);
                }
            }
        }

        private void SetDriverConfigsPageContent()
        {
            DriverBase driver = (DriverBase)TargetFrameworkHelper.Helper.GetDriverObject(mAgent);

            if (driver.GetDriverConfigsEditPageName(mAgent.DriverType) != null)
            {
                DriverConfigurationGrid.Visibility = System.Windows.Visibility.Collapsed;
                DriverConfigurationFrame.Visibility = System.Windows.Visibility.Visible;

                //Custome edit page
                string classname = "Ginger.Drivers.DriversConfigsEditPages." + driver.GetDriverConfigsEditPageName(mAgent.DriverType);
                Type t = Assembly.GetExecutingAssembly().GetType(classname);
                if (t == null)
                {
                    throw new Exception(string.Format("The Driver edit page was not found '{0}'", classname));
                }
                Page p = (Page)Activator.CreateInstance(t, mAgent);
                if (p != null)
                {

                    DriverConfigurationFrame.Content = p;
                }
            }
            else
            {
                //Grid 
                DriverConfigurationGrid.Visibility = System.Windows.Visibility.Visible;
                DriverConfigurationFrame.Visibility = System.Windows.Visibility.Collapsed;
                SetGridView();
            }            
        }

        private void Agent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Agent.DriverType))
            {
                mAgent.AgentOperations.InitDriverConfigs();
                SetDriverConfigsPageContent();
            }
        }

        private void SetGridView()
        {
            DriverConfigurationGrid.SetTitleLightStyle = true;

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Parameter, BindingMode = BindingMode.OneWay, WidthWeight = 20 });
            view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Value, WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ParamValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Description, BindingMode = BindingMode.OneWay, WidthWeight = 45 });

            DriverConfigurationGrid.SetAllColumnsDefaultView(view);
            DriverConfigurationGrid.InitViewItems();
            
            DriverConfigurationGrid.AddToolbarTool("@Reset_16x16.png", "Reset Parameters", new RoutedEventHandler(ResetAgentDriverConfigs));

            DriverConfigurationGrid.DataSourceList = mAgent.DriverConfiguration;
        }

        private void ResetAgentDriverConfigs(object sender, RoutedEventArgs e)
        {
            mAgent.AgentOperations.InitDriverConfigs();
            InitAgentDriverConfigs();
        }


        private void ParamsGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            DriverConfigParam DCP = (DriverConfigParam)DriverConfigurationGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(DCP, DriverConfigParam.Fields.Value, new Context());
            VEEW.ShowAsWindow();
        }

        //public void SetAdvanceConfig()
        //{
        //    switch (mAgent.DriverType)
        //    {
        //        case Agent.eDriverType.MobileAppiumAndroid:
        //        case Agent.eDriverType.MobileAppiumAndroidBrowser:
        //        case Agent.eDriverType.MobileAppiumIOS:
        //        case Agent.eDriverType.MobileAppiumIOSBrowser:
        //            AdvancedConfigurationTab.Visibility = Visibility.Visible;
        //            break;
        //    }

        //    GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
        //    view.GridColsView = new ObservableList<GridColView>();

        //    view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Parameter, Header = "Parameter", WidthWeight = 150 });
        //    view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Value, Header = "Value", WidthWeight = 150 });

        //    AdvancedConfigurationGrid.SetAllColumnsDefaultView(view);
        //    AdvancedConfigurationGrid.InitViewItems();
        //    AdvancedConfigurationGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddAdvanceconfiguration));
        //    AdvancedConfigurationGrid.DataSourceList = mAgent.AdvanceAgentConfigurations;
        //}


        //private void AddAdvanceconfiguration(object sender, RoutedEventArgs e)
        //{
        //    mAgent.AdvanceAgentConfigurations.Add(new DriverConfigParam());
        //}
    }
}
