#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Amdocs.Ginger.Repository;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Selenium;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.ALM.Repository;
using Ginger.Configurations;
using Ginger.UserControls;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTest.WizardLib;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using Windows.Foundation.Collections;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActAccessibilityTestingEditPage.xaml
    /// </summary>
    public partial class ActAccessibilityTestingEditPage : Page
    {
        private ActAccessibilityTesting mAct;
        PlatformInfoBase mPlatform;
        string mExistingPOMAndElementGuidString = null;
        private AccessibilityConfiguration AccessibilityConfiguration = new AccessibilityConfiguration();
        public ActAccessibilityTestingEditPage(ActAccessibilityTesting act)
        {
            InitializeComponent();
            mAct = act;
            if (act.Platform == ePlatformType.NA)
            {
                act.Platform = GetActionPlatform();
            }
            mPlatform = PlatformInfoBase.GetPlatformImpl(act.Platform);
            List<eLocateBy> LocateByList = mPlatform.GetPlatformUIElementLocatorsList();
            xElementLocateByComboBox.BindControl(mAct, Act.Fields.LocateBy, LocateByList);
            xLocateValueVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(Act.Fields.LocateValue));

            ObservableList<OperationValues> StandardTaglist = GetStandardTagslist();
            ObservableList<OperationValues> SeverityList = GetSeverityList();
            mAct.Items = new Dictionary<string, object>();
            xStdStack.Visibility = Visibility.Visible;
            foreach (OperationValues StandardTag in StandardTaglist)
            {
                if (!string.IsNullOrEmpty(StandardTag.Value.ToString()))
                {
                    mAct.Items.Add(StandardTag.Value.ToString(), StandardTag);
                }
            }
            xStdCB.ItemsSource = mAct.Items;
            //Boolean value is to show description of enum value  
            xStdCB.Init(mAct, nameof(mAct.StandardList),ShowEnumDesc: true);

            xSeverityStack.Visibility = Visibility.Visible;
            mAct.SeverityItems = new Dictionary<string, object>();
            foreach (OperationValues severity in SeverityList)
            {
                if (!string.IsNullOrEmpty(severity.Value.ToString()))
                {
                    mAct.SeverityItems.Add(severity.Value.ToString(), severity);
                }
            }
            xSeverityCB.ItemsSource = mAct.SeverityItems;
            xSeverityCB.Init(mAct, nameof(mAct.SeverityList));

            xLocateValueVE.BindControl(Context.GetAsContext(mAct.Context), mAct, Act.Fields.LocateValue);
            xTargetRadioButton.Init(typeof(ActAccessibilityTesting.eTarget), xTargetRadioButtonPnl, mAct.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Target, ActAccessibilityTesting.eTarget.Page.ToString()), TargetRadioButton_Clicked);
            xAnalyzerRadioButton.Init(typeof(ActAccessibilityTesting.eAnalyzer), xAnalyzerRadioButtonPnl, mAct.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Analyzer, ActAccessibilityTesting.eAnalyzer.ByStandard.ToString()), AnalyzerRadioButton_Clicked);
            BindControlForTarget();
            BindControlForAnalyzer();
            SetLocateValueFrame();
        }

        private void BindControlForTarget()
        {
            if ((mAct.GetInputParamValue(ActAccessibilityTesting.Fields.Target) == ActAccessibilityTesting.eTarget.Element.ToString()))
            {
                xLocateByAndValuePanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                xLocateByAndValuePanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        private void BindControlForAnalyzer()
        {
            if ((mAct.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == ActAccessibilityTesting.eAnalyzer.ByStandard.ToString()))
            {
                xStdStack.Visibility = System.Windows.Visibility.Visible;
                xSeveritylbl.Content = "Acceptable Severities :";
            }
            else if ((mAct.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == ActAccessibilityTesting.eAnalyzer.BySeverity.ToString()))
            {
                xStdStack.Visibility = System.Windows.Visibility.Collapsed;
                xSeveritylbl.Content = "Severities :";
            }
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

        private void TargetRadioButton_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            BindControlForTarget();
        }

        private void AnalyzerRadioButton_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            BindControlForAnalyzer();
        }

        private void SetLocateValueFrame()
        {
            if (xElementLocateByComboBox.SelectedItem == null)
            {
                xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            else
            {
                mAct.LocateBy = (eLocateBy)((ComboEnumItem)xElementLocateByComboBox.SelectedItem).Value;
            }

            eLocateBy SelectedLocType = (eLocateBy)((ComboEnumItem)xElementLocateByComboBox.SelectedItem).Value;

            switch (SelectedLocType)
            {
                case eLocateBy.POMElement:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Collapsed;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Visible;
                    Page p = new LocateByPOMElementPage(Context.GetAsContext(mAct.Context), null, null, mAct, nameof(ActBrowserElement.LocateValue));
                    xLocateValueEditFrame.ClearAndSetContent(p);
                    break;
                default:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                    break;
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
            else
            {
                mAct.LocateBy = (eLocateBy)((ComboEnumItem)xElementLocateByComboBox.SelectedItem).Value;
            }

            eLocateBy SelectedLocType = (eLocateBy)((ComboEnumItem)xElementLocateByComboBox.SelectedItem).Value;

            switch (SelectedLocType)
            {
                case eLocateBy.POMElement:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Collapsed;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Visible;
                    Page p = new LocateByPOMElementPage(Context.GetAsContext(mAct.Context), null, null, mAct, nameof(ActAccessibilityTesting.LocateValue));
                    xLocateValueEditFrame.ClearAndSetContent(p);
                    break;
                default:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }

        public ObservableList<OperationValues> GetStandardTagslist()
        {
            ObservableList<OperationValues> StandardTagList = new ObservableList<OperationValues>();
            StandardTagList.Add(new OperationValues() { Value = ActAccessibilityTesting.eTags.wcag2a.ToString(), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), ActAccessibilityTesting.eTags.wcag2a.ToString()) });
            StandardTagList.Add(new OperationValues() { Value = ActAccessibilityTesting.eTags.wcag2aa.ToString(), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), ActAccessibilityTesting.eTags.wcag2aa.ToString()) });
            StandardTagList.Add(new OperationValues() { Value = ActAccessibilityTesting.eTags.wcag21a.ToString(), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), ActAccessibilityTesting.eTags.wcag21a.ToString()) });
            StandardTagList.Add(new OperationValues() { Value = ActAccessibilityTesting.eTags.wcag21aa.ToString(), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), ActAccessibilityTesting.eTags.wcag21aa.ToString()) });
            StandardTagList.Add(new OperationValues() { Value = ActAccessibilityTesting.eTags.wcag22a.ToString(), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), ActAccessibilityTesting.eTags.wcag22a.ToString()) });
            StandardTagList.Add(new OperationValues() { Value = ActAccessibilityTesting.eTags.wcag22aa.ToString(), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), ActAccessibilityTesting.eTags.wcag22aa.ToString()) });
            StandardTagList.Add(new OperationValues() { Value = ActAccessibilityTesting.eTags.bestpractice.ToString(), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), ActAccessibilityTesting.eTags.bestpractice.ToString()) });
            return StandardTagList;
        }

        public ObservableList<OperationValues> GetSeverityList()
        {
            ObservableList<OperationValues> SeverityList = new ObservableList<OperationValues>();

            SeverityList.Add(new OperationValues() { Value = ActAccessibilityTesting.eSeverity.Critical.ToString() });
            SeverityList.Add(new OperationValues() { Value = ActAccessibilityTesting.eSeverity.Serious.ToString() });
            SeverityList.Add(new OperationValues() { Value = ActAccessibilityTesting.eSeverity.Moderate.ToString() });
            SeverityList.Add(new OperationValues() { Value = ActAccessibilityTesting.eSeverity.Minor.ToString() });
            return SeverityList;
        }

    }
}
