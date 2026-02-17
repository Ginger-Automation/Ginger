#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.Configurations;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            ObservableList<OperationValues>? StandardTaglist;
            if (act.Platform == ePlatformType.Mobile)
            {
                StandardTaglist = GetStandardTagslistMobile();
                mAct.CurrentRuleType = ePlatformType.Mobile.ToString();
            }
            else
            {
                StandardTaglist = GetStandardTagslist();
                mAct.CurrentRuleType = ePlatformType.Web.ToString();
            }
            ObservableList<OperationValues> SeverityList = GetSeverityList();
            mAct.Items = [];
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
            xStdCB.Init(mAct, nameof(mAct.StandardList), ShowEnumDesc: true);

            xSeverityStack.Visibility = Visibility.Visible;
            mAct.SeverityItems = [];
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
            xTargetRadioButton.Init(typeof(ActAccessibilityTesting.eTarget), xTargetRadioButtonPnl, mAct.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Target, nameof(ActAccessibilityTesting.eTarget.Page)), TargetRadioButton_Clicked);
            xAnalyzerRadioButton.Init(typeof(ActAccessibilityTesting.eAnalyzer), xAnalyzerRadioButtonPnl, mAct.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Analyzer, nameof(ActAccessibilityTesting.eAnalyzer.ByStandard)), AnalyzerRadioButton_Clicked);
            BindControlForTarget();
            BindControlForAnalyzer();
            SetLocateValueFrame();
        }

        private void BindControlForTarget()
        {
            if ((mAct.GetInputParamValue(ActAccessibilityTesting.Fields.Target) == nameof(ActAccessibilityTesting.eTarget.Element)))
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
            if ((mAct.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.ByStandard)))
            {
                xStdStack.Visibility = System.Windows.Visibility.Visible;
                xSeveritylbl.Content = "Acceptable Severities :";
            }
            else if ((mAct.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.BySeverity)))
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
                platform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == targetapp).Platform;
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
            ObservableList<OperationValues> StandardTagList =
            [
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag2a), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), nameof(ActAccessibilityTesting.eTags.wcag2a)) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag2aa), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), nameof(ActAccessibilityTesting.eTags.wcag2aa)) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag21a), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), nameof(ActAccessibilityTesting.eTags.wcag21a)) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag21aa), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), nameof(ActAccessibilityTesting.eTags.wcag21aa)) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag22a), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), nameof(ActAccessibilityTesting.eTags.wcag22a)) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag22aa), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), nameof(ActAccessibilityTesting.eTags.wcag22aa)) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.bestpractice), DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), nameof(ActAccessibilityTesting.eTags.bestpractice)) },
            ];
            return StandardTagList;
        }

        public ObservableList<OperationValues> GetStandardTagslistMobile()
        {
            ObservableList<OperationValues> StandardTagListMobile = new ObservableList<OperationValues>
        {
            // WCAG Principles/Levels
            new OperationValues() {
                Value = nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.WCAG21A),
                DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eMobileAccessibilityStandards), nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.WCAG21A))
            },
            new OperationValues() {
                Value = nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.WCAG21AA),
                DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eMobileAccessibilityStandards), nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.WCAG21AA))
            },
            new OperationValues() {
                Value = nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.WCAG21AAA),
                DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eMobileAccessibilityStandards), nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.WCAG21AAA))
            },

            // European Standard EN 301 549
            new OperationValues() {
                Value = nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.EN_301_549),
                DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eMobileAccessibilityStandards), nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.EN_301_549))
            },
            new OperationValues() {
                Value = nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.EN_9_4_1_2),
                DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eMobileAccessibilityStandards), nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.EN_9_4_1_2))
            },
            new OperationValues() {
                Value = nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.EN_9_4_1_3),
                DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eMobileAccessibilityStandards), nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.EN_9_4_1_3))
            },

            // General Categories / Best Practices
            
            new OperationValues() {
                Value = nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.BestPractice),
                DisplayName = GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eMobileAccessibilityStandards), nameof(ActAccessibilityTesting.eMobileAccessibilityStandards.BestPractice))
            }
        };

            return StandardTagListMobile;
        }

        public ObservableList<OperationValues> GetSeverityList()
        {
            ObservableList<OperationValues> SeverityList =
            [
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Critical) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Serious) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Moderate) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Minor) },
            ];
            return SeverityList;
        }

    }
}
