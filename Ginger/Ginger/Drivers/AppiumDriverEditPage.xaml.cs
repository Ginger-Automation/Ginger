using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET;
using Ginger.UserControls;
using GingerCore;
using GingerCore.GeneralLib;
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

namespace Ginger.Drivers
{
    /// <summary>
    /// Interaction logic for AppiumDriverEditPage.xaml
    /// </summary>
    public partial class AppiumDriverEditPage : Page
    {
        Agent mAgent = null;
        DriverConfigParam mAppiumServer;
        DriverConfigParam mAppiumCapabilities;

        public AppiumDriverEditPage(Agent appiumAgent)
        {
            InitializeComponent();
            
            mAgent = appiumAgent;

            //mAgent.InitDriverConfigs();
            BindConfigurationsFields();
        }

        private void BindConfigurationsFields()
        {
            if (mAgent.DriverConfiguration == null)
            {
                mAgent.DriverConfiguration = new ObservableList<DriverConfigParam>();
            }

            mAppiumServer = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AppiumServer), "http://127.0.0.1:4444");
            BindingHandler.ObjFieldBinding(xServerURLTextBox, TextBox.TextProperty, mAppiumServer, "Value");

            mAppiumCapabilities = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AppiumCapabilities));
            SetCapabilitiesGridView();
        }

        private void SetCapabilitiesGridView()
        {
            xCapabilitiesGrid.SetTitleLightStyle = true;

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Parameter, Header="Capability", BindingMode = BindingMode.OneWay, WidthWeight = 20 });
            view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Value, WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["ParamValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Description, BindingMode = BindingMode.OneWay, WidthWeight = 45 });

            xCapabilitiesGrid.SetAllColumnsDefaultView(view);
            xCapabilitiesGrid.InitViewItems();

            xCapabilitiesGrid.AddToolbarTool(eImageType.Reset, "Reset Capabilities", new RoutedEventHandler(ResetCapabilities));
            xCapabilitiesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddCapability));

            if (mAppiumCapabilities.MultiValues == null)//not need to be here- need more generic
            {
                mAppiumCapabilities.MultiValues = new ObservableList<DriverConfigParam>(); 
            }
            xCapabilitiesGrid.DataSourceList = mAppiumCapabilities.MultiValues;
        }

        private void ResetCapabilities(object sender, RoutedEventArgs e)
        {
            //mAgent.InitDriverConfigs();
            InitCapabilities();
        }

        private void AddCapability(object sender, RoutedEventArgs e)
        {
            mAppiumCapabilities.MultiValues.Add(new DriverConfigParam());
        }

        private void InitCapabilities()
        {
            //if (mAgent.DriverConfiguration == null)
            //{
            //    mAgent.InitDriverConfigs();
            //    if (mAgent.DriverConfiguration == null)
            //        Reporter.ToUser(eUserMsgKey.DriverConfigUnknownDriverType, mAgent.DriverType);
            //}

            xCapabilitiesGrid.DataSourceList = mAgent.DriverConfiguration;
        }

        private void CapabilitiesGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            DriverConfigParam capability = (DriverConfigParam)xCapabilitiesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(capability, DriverConfigParam.Fields.Value, new Context());
            VEEW.ShowAsWindow();
        }

       
    }
}
