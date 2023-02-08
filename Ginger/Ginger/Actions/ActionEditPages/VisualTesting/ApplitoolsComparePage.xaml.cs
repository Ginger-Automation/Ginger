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
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.VisualTesting;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.VisualTesting
{
    /// <summary>
    /// Interaction logic for ApplitoolsComparePage.xaml
    /// </summary>
    public partial class ApplitoolsComparePage : Page
    {
        private GingerCore.Actions.ActVisualTesting mAct;
        public VisualCompareAnalyzerIntegration visualCompareAnalyzerIntegration = new VisualCompareAnalyzerIntegration();

        
        public ApplitoolsComparePage(GingerCore.Actions.ActVisualTesting mAct)
        {
            InitializeComponent();
            // TODO: Complete member initialization
            this.mAct = mAct;
            xApplitoolsActionComboBox.Init(mAct.GetOrCreateInputParam(ApplitoolsAnalyzer.ApplitoolsAction, ApplitoolsAnalyzer.eApplitoolsAction.Checkpoint.ToString()), typeof(ApplitoolsAnalyzer.eApplitoolsAction), false);
            xApplitoolsActionComboBox.ComboBox.SelectionChanged += ChangeApplitoolsAction_Changed;

            xActionByComboBox.Init(mAct.GetOrCreateInputParam(ApplitoolsAnalyzer.ActionBy, ApplitoolsAnalyzer.eActionBy.Window.ToString()), typeof(ApplitoolsAnalyzer.eActionBy), false);
            xActionByComboBox.ComboBox.SelectionChanged += ChangeActionBy_Changed;
            InitLayout();

            ApplicationNameUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ApplitoolsParamApplicationName, (Context.GetAsContext(mAct.Context)).BusinessFlow.MainApplication), true, false);
            
            TestNameUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ApplitoolsParamTestName, (Context.GetAsContext(mAct.Context)).BusinessFlow.CurrentActivity.ActivityName), true, false);


            SetMatchLevelComboBox.Init(mAct.GetOrCreateInputParam(ApplitoolsAnalyzer.ApplitoolsMatchLevel, ApplitoolsAnalyzer.eMatchLevel.Strict.ToString()), typeof(ApplitoolsAnalyzer.eMatchLevel), false, null);
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(DoNotFailActionOnMismatch, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ApplitoolsAnalyzer.FailActionOnMistmach, "False"));
            //List<eLocateBy> locatorsTypeList = mAct.AvailableLocateBy().Where(e => e != eLocateBy.iOSClassChain && e != eLocateBy.iOSPredicateString).ToList();
            if (mAct.Platform == ePlatformType.NA)
            {
                mAct.Platform = GetActionPlatform();
            }
            PlatformInfoBase mPlatform = PlatformInfoBase.GetPlatformImpl(mAct.Platform);
            List<eLocateBy> LocateByList = mPlatform.GetPlatformUIElementLocatorsList();
            xElementLocateByComboBox.BindControl(mAct,Act.Fields.LocateBy, LocateByList);
            xLocateValueVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(Act.Fields.LocateValue));
            mAct.PropertyChanged += mAct_PropertyChanged;
            SetLocateValueControls();
        }

        public void InitLayout()
        {
            ApplitoolsAnalyzer.eApplitoolsAction applitoolsAction = (ApplitoolsAnalyzer.eApplitoolsAction)Enum.Parse(typeof(ApplitoolsAnalyzer.eApplitoolsAction), xApplitoolsActionComboBox.ComboBox.SelectedValue.ToString(), true);
            ApplitoolsAnalyzer.eActionBy actionBy = (ApplitoolsAnalyzer.eActionBy)Enum.Parse(typeof(ApplitoolsAnalyzer.eActionBy), xActionByComboBox.ComboBox.SelectedValue.ToString(), true);
            
            switch (applitoolsAction)
            {
                case ApplitoolsAnalyzer.eApplitoolsAction.OpenEyes:
                    xApplitoolsApplicationName.Visibility = Visibility.Visible;
                    xApplitoolsTestName.Visibility = Visibility.Visible;
                    xApplitoolsMatchLevel.Visibility = Visibility.Collapsed;
                    xApplitoolsResultsButton.Visibility = Visibility.Collapsed;
                    xDoNotFailActionOnMismatchPanel.Visibility = Visibility.Collapsed;
                    xLocateByAndValuePanel.Visibility = Visibility.Collapsed;
                    xActionByPanel.Visibility = Visibility.Collapsed;
                    if(mAct.Platform == ePlatformType.Web)
                    {
                        visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, eVisualTestingVisibility.Visible);
                    }
                    else
                    {
                        visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, eVisualTestingVisibility.Collapsed);
                        mAct.ChangeAppWindowSize = ActVisualTesting.eChangeAppWindowSize.None;
                    }
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetBaselineSectionVisibility, eVisualTestingVisibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetTargetSectionVisibility, eVisualTestingVisibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetResultsSectionVisibility, eVisualTestingVisibility.Collapsed);
                    break;
                case ApplitoolsAnalyzer.eApplitoolsAction.Checkpoint:
                    xApplitoolsApplicationName.Visibility = Visibility.Collapsed;
                    xApplitoolsTestName.Visibility = Visibility.Collapsed;
                    xApplitoolsMatchLevel.Visibility = Visibility.Visible;
                    xApplitoolsResultsButton.Visibility = Visibility.Collapsed;
                    xDoNotFailActionOnMismatchPanel.Visibility = Visibility.Collapsed;
                    xActionByPanel.Visibility = Visibility.Visible;
                    if (actionBy == ApplitoolsAnalyzer.eActionBy.Region)
                    {
                        xLocateByAndValuePanel.Visibility = Visibility.Visible;
                        SetLocateValueControls();
                    }
                    else
                    {
                        xLocateByAndValuePanel.Visibility = Visibility.Collapsed;
                    }
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, eVisualTestingVisibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetBaselineSectionVisibility, eVisualTestingVisibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetTargetSectionVisibility, eVisualTestingVisibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetResultsSectionVisibility, eVisualTestingVisibility.Collapsed);
                    break;

                case ApplitoolsAnalyzer.eApplitoolsAction.CloseEyes:
                    xApplitoolsApplicationName.Visibility = Visibility.Collapsed;
                    xApplitoolsTestName.Visibility = Visibility.Collapsed;
                    xApplitoolsMatchLevel.Visibility = Visibility.Collapsed;
                    xApplitoolsResultsButton.Visibility = Visibility.Visible;
                    xDoNotFailActionOnMismatchPanel.Visibility = Visibility.Collapsed;
                    xLocateByAndValuePanel.Visibility = Visibility.Collapsed;
                    xActionByPanel.Visibility = Visibility.Collapsed;
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, eVisualTestingVisibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetBaselineSectionVisibility, eVisualTestingVisibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetTargetSectionVisibility, eVisualTestingVisibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetResultsSectionVisibility, eVisualTestingVisibility.Collapsed);
                    break;
            }
        }

        private void ChangeApplitoolsAction_Changed(object sender, SelectionChangedEventArgs e)
        {
            InitLayout();
        }

        private void ChangeActionBy_Changed(object sender, SelectionChangedEventArgs e)
        {
            InitLayout();
        }

        private void ChangeLocateBy_Changed(object sender, SelectionChangedEventArgs e)
        {
            InitLayout();
        }
        private void mAct_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ActVisualTesting.Fields.CompareResult)
            {
                if (mAct.CompareResult is string)
                {
                    Dispatcher.Invoke(() =>
                    {
                        mAct.AddOrUpdateReturnParamActual("ResultsURL", (string)mAct.CompareResult + "");
                    });
                    
                }
            }
        }

        private void OpenResultsURL_Click(object sender, RoutedEventArgs e)
        {
            string url = mAct.GetReturnParam("ResultsURL");
            if (string.IsNullOrEmpty(url))
                Reporter.ToUser(eUserMsgKey.ApplitoolsLastExecutionResultsNotExists);
            else
            {
                try
                {
                    //First try open with Chrome
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = "chrome.exe", Arguments = url , UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    try
                    {
                        //Try open with Firefox
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = "firefox.exe", Arguments = url, UseShellExecute = true });
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    }
                    catch (Exception ee)
                    {
                        //Show message that Applitools can't be open with Explorer Browser
                        Reporter.ToUser(eUserMsgKey.ApplitoolsMissingChromeOrFirefoxBrowser);
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ee.Message}", ee);
                    }
                }
            }
        }

        private void EyesOpenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mAct.GetOrCreateInputParam(ApplitoolsAnalyzer.ApplitoolsEyesOpen).BoolValue = true;
        }

        private void EyesOpenCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mAct.GetOrCreateInputParam(ApplitoolsAnalyzer.ApplitoolsEyesOpen).BoolValue = false;
        }

        private void EyesCloseCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            mAct.GetOrCreateInputParam(ApplitoolsAnalyzer.ApplitoolsEyesClose).BoolValue = true;
        }

        private void EyesCloseCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mAct.GetOrCreateInputParam(ApplitoolsAnalyzer.ApplitoolsEyesClose).BoolValue = false;
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
                    Page p = new LocateByPOMElementPage(Context.GetAsContext(mAct.Context), null, null, mAct, nameof(ActBrowserElement.LocateValue));
                    xLocateValueEditFrame.Content = p;
                    break;
                default:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                    break;
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
    }
}
