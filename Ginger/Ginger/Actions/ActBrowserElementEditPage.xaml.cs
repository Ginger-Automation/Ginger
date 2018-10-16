#region License
/*
Copyright © 2014-2018 European Support Limited

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

using System.Windows.Controls;
using GingerCore.Actions;
using GingerCore.Platforms;
using System.Linq;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.UIElement;
using Ginger.Actions._Common.ActUIElementLib;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActJavaBrowserElementEditPage.xaml
    /// </summary>
    public partial class ActBrowserElementEditPage : Page
    {
        private ActBrowserElement mAct;

        public ActBrowserElementEditPage(ActBrowserElement act)
        {
            InitializeComponent();
            mAct = act;
           
            //TODO: use .Fields
            App.FillComboFromEnumVal(ControlActionComboBox, mAct.ControlAction);
            App.ObjFieldBinding(ControlActionComboBox, ComboBox.SelectedValueProperty, mAct, ActBrowserElement.Fields.ControlAction);
            ValueUC.Init(mAct.GetOrCreateInputParam("Value"));
            LocateValueVE.BindControl(mAct, Act.Fields.LocateValue);
            GotoURLTypeRadioButton.Init(typeof(ActBrowserElement.eGotoURLType), GotoURLRadioButton, mAct.GetOrCreateInputParam(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString()));
            ElementLocateByComboBox.BindControl(mAct, Act.Fields.LocateBy);
            ImplicitWaitVE.BindControl(mAct, ActBrowserElement.Fields.ImplicitWait);
        }

        private void ControlActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVisibleControlsForAction();
        }

        private ePlatformType GetActionPlatform()
        {
            string targetapp = App.BusinessFlow.CurrentActivity.TargetApplication;
            ePlatformType platform = (from x in App.UserProfile.Solution.ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
            return platform;
        }

        private void SetVisibleControlsForAction()
        {
            ePlatformType ActivityPlatform = GetActionPlatform();

            if (mAct.ControlAction == ActBrowserElement.eControlAction.SwitchFrame || mAct.ControlAction == ActBrowserElement.eControlAction.SwitchWindow || mAct.ControlAction == ActBrowserElement.eControlAction.CloseTabExcept)
            {
                LocateBy.Visibility = System.Windows.Visibility.Visible;
                ElementLocateByComboBox.Visibility = System.Windows.Visibility.Visible;
                Value.Visibility = System.Windows.Visibility.Collapsed;
                ValueUC.Visibility = System.Windows.Visibility.Collapsed;
                GotoURLRadioButton.Visibility = System.Windows.Visibility.Collapsed;
                Lable.Visibility = System.Windows.Visibility.Collapsed;
                ImplicitWait.Visibility = System.Windows.Visibility.Collapsed;
                ImplicitWaitVE.Visibility = System.Windows.Visibility.Collapsed;
                LocateValue.Visibility = System.Windows.Visibility.Visible;
                LocateValueVE.Visibility = System.Windows.Visibility.Visible;
                LocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                ElementLocateByComboBox_SelectionChanged(null, null);
            }
            else if (mAct.ControlAction == ActBrowserElement.eControlAction.GotoURL || mAct.ControlAction == ActBrowserElement.eControlAction.OpenURLNewTab ||
                     mAct.ControlAction == ActBrowserElement.eControlAction.InjectJS || mAct.ControlAction == ActBrowserElement.eControlAction.RunJavaScript)
            {
                if (mAct.ControlAction == ActBrowserElement.eControlAction.GotoURL || mAct.ControlAction == ActBrowserElement.eControlAction.OpenURLNewTab)
                {
                    GotoURLRadioButton.Visibility = System.Windows.Visibility.Visible;
                    Lable.Visibility = System.Windows.Visibility.Visible;
                    Value.Content = "URL";
                }
                else if (mAct.ControlAction == ActBrowserElement.eControlAction.InjectJS || mAct.ControlAction == ActBrowserElement.eControlAction.RunJavaScript)
                {
                    Value.Content = "Script";
                }
                LocateBy.Visibility = System.Windows.Visibility.Collapsed;
                ElementLocateByComboBox.Visibility = System.Windows.Visibility.Collapsed;
                LocateValue.Visibility = System.Windows.Visibility.Collapsed;
                LocateValueVE.Visibility = System.Windows.Visibility.Collapsed;
                Value.Visibility = System.Windows.Visibility.Visible;
                ValueUC.Visibility = System.Windows.Visibility.Visible;
                ImplicitWait.Visibility = System.Windows.Visibility.Collapsed;
                ImplicitWaitVE.Visibility = System.Windows.Visibility.Collapsed;
                LocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                if (mAct.ControlAction == ActBrowserElement.eControlAction.InitializeBrowser)
                {
                    if (!(ActivityPlatform == ePlatformType.Web))
                    {
                        LocateBy.Visibility = System.Windows.Visibility.Visible;
                        ElementLocateByComboBox.Visibility = System.Windows.Visibility.Visible;
                        Value.Visibility = System.Windows.Visibility.Collapsed;
                        ValueUC.Visibility = System.Windows.Visibility.Collapsed;
                        GotoURLRadioButton.Visibility = System.Windows.Visibility.Collapsed;
                        Lable.Visibility = System.Windows.Visibility.Collapsed;
                        ImplicitWait.Visibility = System.Windows.Visibility.Visible;
                        ImplicitWaitVE.Visibility = System.Windows.Visibility.Visible;
                        LocateValue.Visibility = System.Windows.Visibility.Visible;
                        LocateValueVE.Visibility = System.Windows.Visibility.Visible;
                        LocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        GotoURLRadioButton.Visibility = System.Windows.Visibility.Collapsed;
                        Value.Visibility = System.Windows.Visibility.Collapsed;
                        ValueUC.Visibility = System.Windows.Visibility.Collapsed;
                        Lable.Visibility = System.Windows.Visibility.Collapsed;
                        ImplicitWait.Visibility = System.Windows.Visibility.Collapsed;
                        ImplicitWaitVE.Visibility = System.Windows.Visibility.Collapsed;
                        LocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                else
                {
                    LocateBy.Visibility = System.Windows.Visibility.Collapsed;
                    ElementLocateByComboBox.Visibility = System.Windows.Visibility.Collapsed;
                    LocateValue.Visibility = System.Windows.Visibility.Collapsed;
                    LocateValueVE.Visibility = System.Windows.Visibility.Collapsed;
                    Value.Visibility = System.Windows.Visibility.Collapsed;
                    ValueUC.Visibility = System.Windows.Visibility.Collapsed;
                    GotoURLRadioButton.Visibility = System.Windows.Visibility.Collapsed;
                    Lable.Visibility = System.Windows.Visibility.Collapsed;
                    ImplicitWait.Visibility = System.Windows.Visibility.Collapsed;
                    ImplicitWaitVE.Visibility = System.Windows.Visibility.Collapsed;
                    LocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void ElementLocateByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mAct.ControlAction == ActBrowserElement.eControlAction.SwitchFrame || mAct.ControlAction == ActBrowserElement.eControlAction.SwitchWindow || mAct.ControlAction == ActBrowserElement.eControlAction.CloseTabExcept)
            {
                LocateValueEditFrame.Content = null;
                if (ElementLocateByComboBox.SelectedItem == null)
                {
                    return;
                }
                eLocateBy SelectedLocType = (eLocateBy)((GingerCore.General.ComboEnumItem)ElementLocateByComboBox.SelectedItem).Value;
                switch (SelectedLocType)
                {
                    case eLocateBy.POMElement:
                        Page p = new LocateByPOMElementPage(mAct);
                        LocateValueEditFrame.Content = p;
                        LocateValueEditFrame.Width = 1035;
                        LocateValueEditFrame.Visibility = System.Windows.Visibility.Visible;
                        LocateValue.Visibility = System.Windows.Visibility.Collapsed;
                        LocateValueVE.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    default:
                        LocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                }
            }
        }
    }
}
