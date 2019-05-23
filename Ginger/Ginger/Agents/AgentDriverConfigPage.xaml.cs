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

using Amdocs.Ginger.Common;
using Ginger.Drivers;
using Ginger.UserControls;
using GingerCore;
using System;
using System.Collections.Generic;
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
        public AgentDriverConfigPage(Agent agent)
        {
            InitializeComponent();
            mAgent = agent;

            SetGridView();

            InitConfigGrid();
            ShowAgentConfig();

            mAgent.PropertyChanged += Agent_PropertyChanged;

        }

        private void Agent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Agent.DriverType))
            {
                mAgent.InitDriverConfigs();
                ShowAgentConfig();
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
            
            DriverConfigurationGrid.AddToolbarTool("@Reset_16x16.png", "Reset Parameters", new RoutedEventHandler(ResetParamsConfig));
        }

        private void ResetParamsConfig(object sender, RoutedEventArgs e)
        {
            mAgent.InitDriverConfigs();
            InitConfigGrid();
        }


        private void InitConfigGrid()
        {
            if (mAgent.DriverConfiguration == null)
            {
                mAgent.InitDriverConfigs();
                if (mAgent.DriverConfiguration == null)
                    Reporter.ToUser(eUserMsgKey.DriverConfigUnknownDriverType, mAgent.DriverType);
            }
            DriverConfigurationGrid.DataSourceList = mAgent.DriverConfiguration;
        }


        private void ParamsGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            DriverConfigParam DCP = (DriverConfigParam)DriverConfigurationGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(DCP, DriverConfigParam.Fields.Value, new Context());
            VEEW.ShowAsWindow();
        }


        private void ShowAgentConfig()
        {
            //TODO: FIXME temp solution to enable config for Selenium Grid Remote Web Driver, or Android driver which have their own edit page
            // Need to make it OO style - 

            //Selenium Remote Web Driver Edit Page
            if (mAgent.DriverType == Agent.eDriverType.SeleniumRemoteWebDriver)
            {
                DriverConfigurationGrid.Visibility = System.Windows.Visibility.Collapsed;
                DriverConfigurationFrame.Visibility = System.Windows.Visibility.Visible;

                Page p = new SeleniumRemoteWebDriverEditPage(mAgent);
                DriverConfigurationFrame.Content = p;
            }
            // Android Edit Page
            else if (mAgent.DriverType == Agent.eDriverType.AndroidADB)
            {
                DriverConfigurationGrid.Visibility = System.Windows.Visibility.Collapsed;
                DriverConfigurationFrame.Visibility = System.Windows.Visibility.Visible;

                Page p = new AndroidADBDriverEditPage(mAgent);
                DriverConfigurationFrame.Content = p;
            }
            //Default Edit Page
            else
            {
                DriverConfigurationGrid.Visibility = System.Windows.Visibility.Visible;
                DriverConfigurationFrame.Visibility = System.Windows.Visibility.Collapsed;                
                InitConfigGrid();
                SetAdvanceConfig();
            }
        }

        public void SetAdvanceConfig()
        {
            switch (mAgent.DriverType)
            {
                case Agent.eDriverType.MobileAppiumAndroid:
                case Agent.eDriverType.MobileAppiumAndroidBrowser:
                case Agent.eDriverType.MobileAppiumIOS:
                case Agent.eDriverType.MobileAppiumIOSBrowser:
                    AdvancedConfigurationTab.Visibility = Visibility.Visible;
                    break;
            }

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Parameter, Header = "Parameter", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Value, Header = "Value", WidthWeight = 150 });

            AdvancedConfigurationGrid.SetAllColumnsDefaultView(view);
            AdvancedConfigurationGrid.InitViewItems();
            AdvancedConfigurationGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddAdvanceconfiguration));
            AdvancedConfigurationGrid.DataSourceList = mAgent.AdvanceAgentConfigurations;
        }


        private void AddAdvanceconfiguration(object sender, RoutedEventArgs e)
        {
            mAgent.AdvanceAgentConfigurations.Add(new DriverConfigParam());
        }




    }
}
