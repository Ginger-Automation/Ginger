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
    /// Interaction logic for VRTComparePage.xaml
    /// </summary>
    public partial class VRTComparePage : Page
    {
        private GingerCore.Actions.ActVisualTesting mAct;
        public VisualCompareAnalyzerIntegration visualCompareAnalyzerIntegration = new VisualCompareAnalyzerIntegration();


        public VRTComparePage(GingerCore.Actions.ActVisualTesting mAct)
        {
            InitializeComponent();
            this.mAct = mAct;
            xVRTActionComboBox.Init(mAct.GetOrCreateInputParam(VRTAnalyzer.VRTAction, VRTAnalyzer.eVRTAction.Track.ToString()), typeof(VRTAnalyzer.eVRTAction), false);
            xVRTActionComboBox.ComboBox.SelectionChanged += VRTAction_Changed;

            xActionByComboBox.Init(mAct.GetOrCreateInputParam(VRTAnalyzer.ActionBy, VRTAnalyzer.eActionBy.Window.ToString()), typeof(VRTAnalyzer.eActionBy), false);
            xActionByComboBox.ComboBox.SelectionChanged += ActionBy_Changed;

            xVRTImageNameActionComboBox.Init(mAct.GetOrCreateInputParam(VRTAnalyzer.ImageNameBy, VRTAnalyzer.eImageNameBy.ActionName.ToString()), typeof(VRTAnalyzer.eImageNameBy), false);
            xVRTImageNameActionComboBox.ComboBox.SelectionChanged += ImageNameBy_Changed;
            xImageNameUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(VRTAnalyzer.ImageName, mAct.Description));

            InitLayout();
            
            DiffTollerancePercentUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(VRTAnalyzer.VRTParamDiffTollerancePercent, "0.0"), true, false);

            string buildTestName = string.Empty;
            if (Context.GetAsContext(mAct.Context) != null && (Context.GetAsContext(mAct.Context)).Activity != null)
            {
                buildTestName = (Context.GetAsContext(mAct.Context)).Activity.ActivityName;
            }
            xTestNameUCVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(VRTAnalyzer.VRTParamBuildName, buildTestName), true, false);


            List<eLocateBy> locatorsTypeList = mAct.AvailableLocateBy().Where(e => e != eLocateBy.iOSClassChain && e != eLocateBy.iOSPredicateString).ToList();
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
            VRTAnalyzer.eVRTAction vrtAction = (VRTAnalyzer.eVRTAction)Enum.Parse(typeof(VRTAnalyzer.eVRTAction), xVRTActionComboBox.ComboBox.SelectedValue.ToString(), true);
            VRTAnalyzer.eActionBy actionBy = (VRTAnalyzer.eActionBy)Enum.Parse(typeof(VRTAnalyzer.eActionBy), xActionByComboBox.ComboBox.SelectedValue.ToString(), true);
            VRTAnalyzer.eImageNameBy imageNameBy = (VRTAnalyzer.eImageNameBy)Enum.Parse(typeof(VRTAnalyzer.eImageNameBy), xVRTImageNameActionComboBox.ComboBox.SelectedValue.ToString(), true);

            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, eVisualTestingVisibility.Collapsed);
            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetBaselineSectionVisibility, eVisualTestingVisibility.Collapsed);
            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetTargetSectionVisibility, eVisualTestingVisibility.Collapsed);
            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetResultsSectionVisibility, eVisualTestingVisibility.Collapsed);
            switch (vrtAction)
            {
                case VRTAnalyzer.eVRTAction.Start:
                    //diff
                    xDiffTollerancePercentLabel.Visibility = Visibility.Collapsed;
                    DiffTollerancePercentUCVE.Visibility = Visibility.Collapsed;
                    //test/build
                    xTestNameLabel.Visibility = Visibility.Visible;
                    xTestNameUCVE.Visibility = Visibility.Visible;
                    //locateby
                    xLocateByAndValuePanel.Visibility = Visibility.Collapsed;
                    //actionby
                    xVRTActionByLabel.Visibility = Visibility.Collapsed;
                    xActionByComboBox.Visibility = Visibility.Collapsed;
                    //imagenameby
                    xVRTImageNameActionLabel.Visibility = Visibility.Collapsed;
                    xVRTImageNameActionComboBox.Visibility = Visibility.Collapsed;
                    //image name
                    xImageNameLabel.Visibility = Visibility.Collapsed;
                    xImageNameUCVE.Visibility = Visibility.Collapsed;
                    visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, eVisualTestingVisibility.Visible);
                    break;
                case VRTAnalyzer.eVRTAction.Track:
                    xDiffTollerancePercentLabel.Visibility = Visibility.Visible;
                    DiffTollerancePercentUCVE.Visibility = Visibility.Visible;
                    xTestNameLabel.Visibility = Visibility.Collapsed;
                    xTestNameUCVE.Visibility = Visibility.Collapsed;
                    xVRTActionByLabel.Visibility = Visibility.Visible;
                    xActionByComboBox.Visibility = Visibility.Visible;
                    if (actionBy == VRTAnalyzer.eActionBy.Region)
                    {
                        xLocateByAndValuePanel.Visibility = Visibility.Visible;
                        SetLocateValueControls();
                    }
                    else
                    {
                        xLocateByAndValuePanel.Visibility = Visibility.Collapsed;
                    }
                    xVRTImageNameActionLabel.Visibility = Visibility.Visible;
                    xVRTImageNameActionComboBox.Visibility = Visibility.Visible;
                    if (imageNameBy == VRTAnalyzer.eImageNameBy.Custom)
                    {
                        xImageNameLabel.Visibility = Visibility.Visible;
                        xImageNameUCVE.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        xImageNameLabel.Visibility = Visibility.Collapsed;
                        xImageNameUCVE.Visibility = Visibility.Collapsed;
                    }
                    break;

                case VRTAnalyzer.eVRTAction.Stop:
                    //diff
                    xDiffTollerancePercentLabel.Visibility = Visibility.Collapsed;
                    DiffTollerancePercentUCVE.Visibility = Visibility.Collapsed;
                    //test/build
                    xTestNameLabel.Visibility = Visibility.Collapsed;
                    xTestNameUCVE.Visibility = Visibility.Collapsed;
                    //locateby
                    xLocateByAndValuePanel.Visibility = Visibility.Collapsed;
                    //actionby
                    xVRTActionByLabel.Visibility = Visibility.Collapsed;
                    xActionByComboBox.Visibility = Visibility.Collapsed;
                    //imagenameby
                    xVRTImageNameActionLabel.Visibility = Visibility.Collapsed;
                    xVRTImageNameActionComboBox.Visibility = Visibility.Collapsed;
                    //image name
                    xImageNameLabel.Visibility = Visibility.Collapsed;
                    xImageNameUCVE.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void VRTAction_Changed(object sender, SelectionChangedEventArgs e)
        {
            InitLayout();
        }

        private void ActionBy_Changed(object sender, SelectionChangedEventArgs e)
        {
            InitLayout();
        }
        private void ImageNameBy_Changed(object sender, SelectionChangedEventArgs e)
        {
            InitLayout();
        }

        private void LocateBy_Changed(object sender, SelectionChangedEventArgs e)
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