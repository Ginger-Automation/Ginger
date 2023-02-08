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
using Amdocs.Ginger.Common.UIElement;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.UserControls;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Repository;
using System.Windows.Data;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActJavaBrowserElementEditPage.xaml
    /// </summary>
    public partial class ActBrowserElementEditPage : Page
    {
        private ActBrowserElement mAct;
        PlatformInfoBase mPlatform;

        public ActBrowserElementEditPage(ActBrowserElement act)
        {
            InitializeComponent();
            mAct = act;

            if (act.Platform == ePlatformType.NA)
            {
                act.Platform = GetActionPlatform();
            }
            mPlatform = PlatformInfoBase.GetPlatformImpl(act.Platform);

            List<ActBrowserElement.eControlAction> supportedControlActions = mPlatform.GetPlatformBrowserControlOperations();
            //validate operation is valid
            if (supportedControlActions.Contains(mAct.ControlAction) == false)
            {
                mAct.ControlAction = supportedControlActions[0];
            }

            //bind controls
            GingerCore.General.FillComboFromEnumObj(xControlActionComboBox, mAct.ControlAction, supportedControlActions.Cast<object>().ToList());
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xControlActionComboBox, ComboBox.SelectedValueProperty, mAct, ActBrowserElement.Fields.ControlAction);

            ValueUC.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam("Value"));
            xLocateValueVE.BindControl(Context.GetAsContext(mAct.Context), mAct, Act.Fields.LocateValue);
            xGotoURLTypeRadioButton.Init(typeof(ActBrowserElement.eGotoURLType), xGotoURLTypeRadioButtonPnl, mAct.GetOrCreateInputParam(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString()));
            xURLSrcRadioButton.Init(typeof(ActBrowserElement.eURLSrc), xURLSrcRadioButtonPnl, mAct.GetOrCreateInputParam(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString()), URLSrcRadioButton_Clicked);
            xMonitorURLRadioButton.Init(typeof(ActBrowserElement.eMonitorUrl), xMonitorURLRadioButtonPnl, mAct.GetOrCreateInputParam(nameof(ActBrowserElement.eMonitorUrl), ActBrowserElement.eMonitorUrl.AllUrl.ToString()), MonitorURLRadioButton_Clicked);
            xRequestTypeRadioButton.Init(typeof(ActBrowserElement.eRequestTypes), xRequestTypeRadioButtonPnl, mAct.GetOrCreateInputParam(nameof(ActBrowserElement.eRequestTypes), ActBrowserElement.eRequestTypes.All.ToString()), RequestTypeRadioButton_Clicked);
            xElementLocateByComboBox.BindControl(mAct, Act.Fields.LocateBy);
            xImplicitWaitVE.BindControl(Context.GetAsContext(mAct.Context), mAct, ActBrowserElement.Fields.ImplicitWait);
            SetGridView();
            SetVisibleControlsForAction();
        }

        private void ResetView()
        {
            xLocateByAndValuePanel.Visibility = System.Windows.Visibility.Collapsed;
            xOpenURLInPnl.Visibility = System.Windows.Visibility.Collapsed;
            xURLSrcPnl.Visibility = System.Windows.Visibility.Collapsed;
            xValueGrid.Visibility = System.Windows.Visibility.Collapsed;
            xImplicitWaitPnl.Visibility = System.Windows.Visibility.Collapsed;
            xMonitorURLPnl.Visibility = System.Windows.Visibility.Collapsed;
            xRequestTypePnl.Visibility = System.Windows.Visibility.Collapsed;
            xUpdateNetworkUrlGridPnl.Visibility = System.Windows.Visibility.Collapsed;

        }

        private void ResetPOMView()
        {
            xPOMUrlFrame.Visibility = System.Windows.Visibility.Collapsed;
            ValueUC.Visibility = System.Windows.Visibility.Visible;
        }

        private void ControlActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVisibleControlsForAction();
        }

        private ePlatformType GetActionPlatform()
        {
            ePlatformType platform;
            if (mAct.Context != null && (Context.GetAsContext(mAct.Context)).BusinessFlow != null)
            {
                string targetapp = (Context.GetAsContext(mAct.Context)).BusinessFlow.CurrentActivity.TargetApplication;
                platform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
            }
            else
            {
                platform = WorkSpace.Instance.Solution.ApplicationPlatforms[0].Platform;
            }
            return platform;
        }

        private void SetVisibleControlsForAction()
        {
            ResetView();
            ePlatformType ActivityPlatform = mAct.Platform;

            if (mAct.ControlAction == ActBrowserElement.eControlAction.SwitchFrame || mAct.ControlAction == ActBrowserElement.eControlAction.SwitchWindow || mAct.ControlAction == ActBrowserElement.eControlAction.CloseTabExcept)
            {
                xLocateByAndValuePanel.Visibility = System.Windows.Visibility.Visible;
                SetLocateValueControls();
            }
            else if (mAct.ControlAction == ActBrowserElement.eControlAction.GotoURL || mAct.ControlAction == ActBrowserElement.eControlAction.OpenURLNewTab ||
                     mAct.ControlAction == ActBrowserElement.eControlAction.InjectJS || mAct.ControlAction == ActBrowserElement.eControlAction.RunJavaScript)
            {
                if (mAct.ControlAction == ActBrowserElement.eControlAction.GotoURL || mAct.ControlAction == ActBrowserElement.eControlAction.OpenURLNewTab)
                {
                    if (mAct.ControlAction == ActBrowserElement.eControlAction.GotoURL)
                    {
                        xOpenURLInPnl.Visibility = System.Windows.Visibility.Visible;
                        xURLSrcPnl.Visibility = System.Windows.Visibility.Visible;
                        if (mAct.GetInputParamValue(ActBrowserElement.Fields.URLSrc) == ActBrowserElement.eURLSrc.UrlPOM.ToString())
                        {
                            ValueUC.Visibility = System.Windows.Visibility.Collapsed;
                            xPOMUrlFrame.Visibility = System.Windows.Visibility.Visible;

                            xValueLabel.Content = "Page Objects Model:";
                            SetLocateValueFrame();
                        }

                    }
                    else
                    {
                        ResetPOMView();
                    }
                    xValueGrid.Visibility = System.Windows.Visibility.Visible;

                    xValueLabel.Content = "URL:";
                }
                else if (mAct.ControlAction == ActBrowserElement.eControlAction.InjectJS || mAct.ControlAction == ActBrowserElement.eControlAction.RunJavaScript)
                {
                    ResetPOMView();
                    if (ActivityPlatform == ePlatformType.Java)
                    {
                        xLocateByAndValuePanel.Visibility = System.Windows.Visibility.Visible;
                        SetLocateValueControls();
                    }
                    xValueGrid.Visibility = System.Windows.Visibility.Visible;
                    xValueLabel.Content = "Script:";
                }
            }
            else if (mAct.ControlAction == ActBrowserElement.eControlAction.StartMonitoringNetworkLog)
            {
                xRequestTypePnl.Visibility = System.Windows.Visibility.Visible;
                xMonitorURLPnl.Visibility = System.Windows.Visibility.Visible;
                if (mAct.GetOrCreateInputParam(nameof(ActBrowserElement.eMonitorUrl)).Value == ActBrowserElement.eMonitorUrl.SelectedUrl.ToString())
                {
                    xUpdateNetworkUrlGridPnl.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                if (mAct.ControlAction == ActBrowserElement.eControlAction.InitializeBrowser)
                {
                    if (!(ActivityPlatform == ePlatformType.Web))
                    {
                        xLocateByAndValuePanel.Visibility = System.Windows.Visibility.Visible;
                        SetLocateValueControls();

                        xImplicitWaitPnl.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
        }

        private void ElementLocateByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetLocateValueControls();
        }

        private void SetLocateValueControls()
        {
            if (xElementLocateByComboBox.SelectedItem == null)
            {
                xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            eLocateBy SelectedLocType = (eLocateBy)((ComboEnumItem)xElementLocateByComboBox.SelectedItem).Value;
            switch (SelectedLocType)
            {
                case eLocateBy.POMElement:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Collapsed;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Visible;
                    Page p = new LocateByPOMElementPage(Context.GetAsContext(mAct.Context), null, null, mAct, nameof(ActBrowserElement.LocateValue));
                    xLocateValueEditFrame.Content = p;
                    break;
                default:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }

        private void URLSrcRadioButton_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            RadioButton rbSender = sender as RadioButton;

            if(rbSender.Content.ToString() == ActBrowserElement.eURLSrc.Static.ToString())
            {
                ValueUC.Visibility = System.Windows.Visibility.Visible;
                xPOMUrlFrame.Visibility = System.Windows.Visibility.Collapsed;
                xValueLabel.Content = "URL:";
            }
            else
            {
                ValueUC.Visibility = System.Windows.Visibility.Collapsed;
                xPOMUrlFrame.Visibility = System.Windows.Visibility.Visible;

                xValueLabel.Content = "Page Objects Model:";

                SetLocateValueFrame();
            }
        }

        private void SetLocateValueFrame()
        {
            LocateByPOMElementPage locateByPOMElementPage = new LocateByPOMElementPage(Context.GetAsContext(mAct.Context), mAct, null, mAct, nameof(ActBrowserElement.Fields.PomGUID), true);
            xPOMUrlFrame.Content = locateByPOMElementPage;
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header = "URL", WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = "...", Header = "...", WidthWeight = 5, MaxWidth = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xUpdateNetworkUrlGridPnl.Resources["UpdateNetworkParametersPathValueExpressionButton"] });
            UpdateNetworkUrlGrid.SetAllColumnsDefaultView(view);
            UpdateNetworkUrlGrid.InitViewItems();
            UpdateNetworkUrlGrid.SetTitleLightStyle = true;
            UpdateNetworkUrlGrid.btnAdd.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(AddPatchOperationForNetwork));
            UpdateNetworkUrlGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddPatchOperationForNetwork));
            UpdateNetworkUrlGrid.btnDown.Visibility = Visibility.Collapsed;
            UpdateNetworkUrlGrid.btnUp.Visibility = Visibility.Collapsed;
            UpdateNetworkUrlGrid.btnClearAll.Visibility = Visibility.Collapsed;
            UpdateNetworkUrlGrid.btnRefresh.Visibility = Visibility.Collapsed;

            UpdateNetworkUrlGrid.DataSourceList = mAct.UpdateOperationInputValues;
        }

        private void MonitorURLRadioButton_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (mAct.GetOrCreateInputParam(nameof(ActBrowserElement.eMonitorUrl)).Value == ActBrowserElement.eMonitorUrl.AllUrl.ToString())
            {
                xUpdateNetworkUrlGridPnl.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                xUpdateNetworkUrlGridPnl.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void RequestTypeRadioButton_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void AddPatchOperationForNetwork(object sender, RoutedEventArgs e)
        {
            ActInputValue NetworkPatchInput = new ActInputValue();
            mAct.UpdateOperationInputValues.Add(NetworkPatchInput);
        }

        private void UpdateNetworkParametersGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue cosmosPatchInput = (ActInputValue)UpdateNetworkUrlGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(cosmosPatchInput, nameof(ActInputValue.Value), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }
        private void UpdateNetworkParametersGridPathVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue cosmosPatchInput = (ActInputValue)UpdateNetworkUrlGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(cosmosPatchInput, nameof(ActInputValue.Param), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }
    }
}
