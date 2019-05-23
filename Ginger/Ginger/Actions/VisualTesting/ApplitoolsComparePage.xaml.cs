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
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.VisualTesting;
using System;
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

            InitLayout();

            ApplicationNameUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ApplitoolsParamApplicationName, (Context.GetAsContext(mAct.Context)).BusinessFlow.MainApplication), true, false);
            
            TestNameUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ApplitoolsParamTestName, (Context.GetAsContext(mAct.Context)).BusinessFlow.CurrentActivity.ActivityName), true, false);


            ApplitoolsKeyUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ApplitoolsKey), true, false);
            SetMatchLevelComboBox.Init(mAct.GetOrCreateInputParam(ApplitoolsAnalyzer.ApplitoolsMatchLevel, ApplitoolsAnalyzer.eMatchLevel.Strict.ToString()), typeof(ApplitoolsAnalyzer.eMatchLevel), false, null);
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(DoNotFailActionOnMismatch, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ApplitoolsAnalyzer.FailActionOnMistmach, "False"));

            mAct.PropertyChanged += mAct_PropertyChanged;
        }

        public void InitLayout()
        {
            ApplitoolsAnalyzer.eApplitoolsAction applitoolsAction = (ApplitoolsAnalyzer.eApplitoolsAction)Enum.Parse(typeof(ApplitoolsAnalyzer.eApplitoolsAction), xApplitoolsActionComboBox.ComboBox.SelectedValue.ToString(), true);
            switch (applitoolsAction)
            {
                case ApplitoolsAnalyzer.eApplitoolsAction.OpenEyes:
                    xApplitoolsKey.Visibility = Visibility.Visible;
                    xApplitoolsApplicationName.Visibility = Visibility.Visible;
                    xApplitoolsTestName.Visibility = Visibility.Visible;
                    xApplitoolsMatchLevel.Visibility = Visibility.Collapsed;
                    xApplitoolsResultsButton.Visibility = Visibility.Collapsed;
                    xDoNotFailActionOnMismatchPanel.Visibility = Visibility.Collapsed;
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, Visibility.Visible);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetBaselineSectionVisibility, Visibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetTargetSectionVisibility, Visibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetResultsSectionVisibility, Visibility.Collapsed);
                    break;

                case ApplitoolsAnalyzer.eApplitoolsAction.Checkpoint:
                    xApplitoolsKey.Visibility = Visibility.Collapsed;
                    xApplitoolsApplicationName.Visibility = Visibility.Collapsed;
                    xApplitoolsTestName.Visibility = Visibility.Collapsed;
                    xApplitoolsMatchLevel.Visibility = Visibility.Visible;
                    xApplitoolsResultsButton.Visibility = Visibility.Collapsed;
                    xDoNotFailActionOnMismatchPanel.Visibility = Visibility.Visible;
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, Visibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetBaselineSectionVisibility, Visibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetTargetSectionVisibility, Visibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetResultsSectionVisibility, Visibility.Collapsed);
                    break;

                case ApplitoolsAnalyzer.eApplitoolsAction.CloseEyes:
                    xApplitoolsKey.Visibility = Visibility.Collapsed;
                    xApplitoolsApplicationName.Visibility = Visibility.Collapsed;
                    xApplitoolsTestName.Visibility = Visibility.Collapsed;
                    xApplitoolsMatchLevel.Visibility = Visibility.Collapsed;
                    xApplitoolsResultsButton.Visibility = Visibility.Visible;
                    xDoNotFailActionOnMismatchPanel.Visibility = Visibility.Collapsed;
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, Visibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetBaselineSectionVisibility, Visibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetTargetSectionVisibility, Visibility.Collapsed);
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetResultsSectionVisibility, Visibility.Collapsed);
                    break;
            }
        }

        private void ChangeApplitoolsAction_Changed(object sender, SelectionChangedEventArgs e)
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
                    System.Diagnostics.Process.Start("chrome.exe", url);
                }
                catch (Exception ex)
                {
                    try
                    {
                        //Try open with Firefox
                        System.Diagnostics.Process.Start("firefox.exe", url);
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
    }
}
