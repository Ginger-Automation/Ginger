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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Ginger.Actions._Common.ActUIElementLib;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

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

            //bind controls
            GingerCore.General.FillComboFromEnumObj(xControlActionComboBox, mAct.ControlAction, supportedControlActions.Cast<object>().ToList());
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xControlActionComboBox, ComboBox.SelectedValueProperty, mAct, ActBrowserElement.Fields.ControlAction);

            ValueUC.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam("Value"));
            xLocateValueVE.BindControl(Context.GetAsContext(mAct.Context), mAct, Act.Fields.LocateValue);
            xGotoURLTypeRadioButton.Init(typeof(ActBrowserElement.eGotoURLType), xGotoURLTypeRadioButtonPnl, mAct.GetOrCreateInputParam(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString()));
            xURLSrcRadioButton.Init(typeof(ActBrowserElement.eURLSrc), xURLSrcRadioButtonPnl, mAct.GetOrCreateInputParam(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString()), URLSrcRadioButton_Clicked);
            xElementLocateByComboBox.BindControl(mAct, Act.Fields.LocateBy);
            xImplicitWaitVE.BindControl(Context.GetAsContext(mAct.Context), mAct, ActBrowserElement.Fields.ImplicitWait);

            SetVisibleControlsForAction();
        }

        private void ResetView()
        {
            xLocateByAndValuePanel.Visibility = System.Windows.Visibility.Collapsed;
            xOpenURLInPnl.Visibility = System.Windows.Visibility.Collapsed;
            xURLSrcPnl.Visibility = System.Windows.Visibility.Collapsed;
            xValueGrid.Visibility = System.Windows.Visibility.Collapsed;
            xImplicitWaitPnl.Visibility = System.Windows.Visibility.Collapsed;
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
                    xValueGrid.Visibility = System.Windows.Visibility.Visible;

                    xValueLabel.Content = "URL:";
                }
                else if (mAct.ControlAction == ActBrowserElement.eControlAction.InjectJS || mAct.ControlAction == ActBrowserElement.eControlAction.RunJavaScript)
                {
                    xValueGrid.Visibility = System.Windows.Visibility.Visible;
                    xValueLabel.Content = "Script:";
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
    }
}
