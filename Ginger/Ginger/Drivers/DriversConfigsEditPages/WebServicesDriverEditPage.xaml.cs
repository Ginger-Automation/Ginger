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

using GingerCore;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.GeneralLib;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Drivers.DriversConfigsEditPages
{
    /// <summary>
    /// Interaction logic for WebServicesDriverEditPage.xaml
    /// </summary>
    public partial class WebServicesDriverEditPage : Page
    {
        Agent mAgent = null;
        public WebServicesDriverEditPage(Agent webServicesAgent)
        {
            InitializeComponent();

            mAgent = webServicesAgent;
            BindConfigurations();
        }

        private void BindConfigurations()
        {
            #region Connection Configurations Settings Binding
            #region TCP Details Binding
            DriverConfigParam useTCP = mAgent.GetOrCreateParam(nameof(WebServicesDriver.UseTcp));
            BindingHandler.ObjFieldBinding(xUseTcpCheckBox, CheckBox.IsCheckedProperty, useTCP, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xUseTcpCheckBox, CheckBox.ToolTipProperty, useTCP, nameof(DriverConfigParam.Description));

            DriverConfigParam tcpHostname = mAgent.GetOrCreateParam(nameof(WebServicesDriver.TcpHostname));
            xTcpHostnameTextBox.Init(null, tcpHostname, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xTcpHostnameTextBox, TextBox.ToolTipProperty, tcpHostname, nameof(DriverConfigParam.Description));

            DriverConfigParam tcpPort = mAgent.GetOrCreateParam(nameof(WebServicesDriver.TcpPort));
            xTcpPortTextBox.Init(null, tcpPort, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xTcpPortTextBox, TextBox.ToolTipProperty, tcpPort, nameof(DriverConfigParam.Description));
            #endregion
            #region Advanced Connection Settings Binding
            // Proxy Settings Binding
            DriverConfigParam useProxy = mAgent.GetOrCreateParam(nameof(WebServicesDriver.UseServerProxySettings));
            BindingHandler.ObjFieldBinding(xUseProxyCheckBox, CheckBox.IsCheckedProperty, useProxy, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xUseProxyCheckBox, CheckBox.ToolTipProperty, useProxy, nameof(DriverConfigParam.Description));

            DriverConfigParam proxy = mAgent.GetOrCreateParam(nameof(WebServicesDriver.WebServicesProxy));
            xProxyTextBox.Init(null, proxy, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xProxyTextBox, TextBox.ToolTipProperty, proxy, nameof(DriverConfigParam.Description));

            //Driver loading time TO
            DriverConfigParam driverTimeout = mAgent.GetOrCreateParam(nameof(WebServicesDriver.DriverLoadWaitingTime));
            xTimeoutTextBox.Init(null, driverTimeout, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xTimeoutTextBox, TextBox.ToolTipProperty, driverTimeout, nameof(DriverConfigParam.Description));

            //Driver's Security Type
            GingerCore.General.FillComboFromEnumType(xSecurityTypeComboBox, typeof(WebServicesDriver.eSecurityType));
            DriverConfigParam securityType = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SecurityType));
            BindingHandler.ObjFieldBinding(xSecurityTypeComboBox, ComboBox.TextProperty, securityType, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSecurityTypeComboBox, ComboBox.ToolTipProperty, securityType, nameof(DriverConfigParam.Description));
            #endregion
            #endregion
            #region Save Request/Response Binding
            DriverConfigParam saveRequest = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SaveRequestXML));
            BindingHandler.ObjFieldBinding(xSaveRequestCheckBox, CheckBox.IsCheckedProperty, saveRequest, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSaveRequestCheckBox, CheckBox.ToolTipProperty, saveRequest, nameof(DriverConfigParam.Description));

            DriverConfigParam saveResponse = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SaveResponseXML));
            BindingHandler.ObjFieldBinding(xSaveResponseCheckBox, CheckBox.IsCheckedProperty, saveResponse, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSaveResponseCheckBox, CheckBox.ToolTipProperty, saveResponse, nameof(DriverConfigParam.Description));

            DriverConfigParam savePath = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SavedXMLDirectoryPath));
            xSaveRequestResponsePathTextBox.Init(null, savePath, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSaveRequestResponsePathTextBox, TextBox.ToolTipProperty, savePath, nameof(DriverConfigParam.Description));
            #endregion
            #region SoapUI Orchestration Settings Binding
            DriverConfigParam soapUIDirectoryPath = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SoapUIDirectoryPath));
            xSoapUIDirectoryPathTextBox.Init(null, soapUIDirectoryPath, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSoapUIDirectoryPathTextBox, TextBox.ToolTipProperty, soapUIDirectoryPath, nameof(DriverConfigParam.Description));

            DriverConfigParam soapUIExecutionDirectoryPath = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SoapUIExecutionOutputsDirectoryPath));
            xSoapUIOutputsDirectoryPathTextBox.Init(null, soapUIExecutionDirectoryPath, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSoapUIOutputsDirectoryPathTextBox, TextBox.ToolTipProperty, soapUIExecutionDirectoryPath, nameof(DriverConfigParam.Description));

            DriverConfigParam soapUISettingsFile = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SoapUISettingFile));
            xSoapUISettingsFile.Init(null, soapUISettingsFile, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSoapUISettingsFile, TextBox.ToolTipProperty, soapUISettingsFile, nameof(DriverConfigParam.Description));

            DriverConfigParam soapUISettingsFilePassword = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SoapUISettingFilePassword));
            xSoapUISettingsFilePassword.Init(null, soapUISettingsFilePassword, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSoapUISettingsFilePassword, TextBox.ToolTipProperty, soapUISettingsFilePassword, nameof(DriverConfigParam.Description));

            DriverConfigParam soapUIProjectPassword = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SoapUIProjectPassword));
            xSoapUIProjectPassword.Init(null, soapUIProjectPassword, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSoapUIProjectPassword, TextBox.ToolTipProperty, soapUIProjectPassword, nameof(DriverConfigParam.Description));

            DriverConfigParam soapUIRunAsAdmin = mAgent.GetOrCreateParam(nameof(WebServicesDriver.RunSoapUIProcessAsAdmin));
            BindingHandler.ObjFieldBinding(xSoapUIRunAsAdminCheckBox, CheckBox.IsCheckedProperty, soapUIRunAsAdmin, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSoapUIRunAsAdminCheckBox, CheckBox.ToolTipProperty, soapUIRunAsAdmin, nameof(DriverConfigParam.Description));

            DriverConfigParam soapUIRedirectError = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SoapUIProcessRedirectStandardError));
            BindingHandler.ObjFieldBinding(xSoapUIRedirectErrorCheckBox, CheckBox.IsCheckedProperty, soapUIRedirectError, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSoapUIRedirectErrorCheckBox, CheckBox.ToolTipProperty, soapUIRedirectError, nameof(DriverConfigParam.Description));

            DriverConfigParam soapUIRedirectOutput = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SoapUIProcessRedirectStandardOutput));
            BindingHandler.ObjFieldBinding(xSoapUIRedirectOutputCheckBox, CheckBox.IsCheckedProperty, soapUIRedirectOutput, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSoapUIRedirectOutputCheckBox, CheckBox.ToolTipProperty, soapUIRedirectOutput, nameof(DriverConfigParam.Description));

            DriverConfigParam soapUIUseShell = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SoapUIProcessUseShellExecute));
            BindingHandler.ObjFieldBinding(xSoapUIUseShellCheckBox, CheckBox.IsCheckedProperty, soapUIUseShell, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSoapUIUseShellCheckBox, CheckBox.ToolTipProperty, soapUIUseShell, nameof(DriverConfigParam.Description));

            DriverConfigParam soapUIWindowStyle = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SoapUIProcessWindowStyle));
            BindingHandler.ObjFieldBinding(xSoapUIWindowStyleCheckBox, CheckBox.IsCheckedProperty, soapUIWindowStyle, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSoapUIWindowStyleCheckBox, CheckBox.ToolTipProperty, soapUIWindowStyle, nameof(DriverConfigParam.Description));

            DriverConfigParam soapUINoWindow = mAgent.GetOrCreateParam(nameof(WebServicesDriver.SoapUIProcessCreateNoWindow));
            BindingHandler.ObjFieldBinding(xSoapUINoWindowCheckBox, CheckBox.IsCheckedProperty, soapUINoWindow, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSoapUINoWindowCheckBox, CheckBox.ToolTipProperty, soapUINoWindow, nameof(DriverConfigParam.Description));
            #endregion
        }

        private void xUseProxyChxBox_Checked(object sender, RoutedEventArgs e)
        {
            if (xProxyTextBox != null)
            {
                if (xProxyTextBox.IsEnabled)
                {
                    xProxyTextBox.IsEnabled = false;
                }
            }
        }
        private void xUseProxyChxBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (xProxyTextBox != null)
            {
                if (!xProxyTextBox.IsEnabled)
                {
                    xProxyTextBox.IsEnabled = true;
                }
            }
        }
    }
}
