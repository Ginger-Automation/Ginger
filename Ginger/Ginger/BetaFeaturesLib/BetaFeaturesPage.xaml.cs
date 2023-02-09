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
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Repository;
using Ginger.GingerCoreNETTestLib;
using Ginger.ReporterLib;
using GingerCore;
using GingerCore.Actions;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for BetaFeaturesPage.xaml
    /// </summary>
    public partial class BetaFeaturesPage : Page
    {
        GenericWindow genWin = null;

        public BetaFeaturesPage()
        {
            InitializeComponent();
            FeatureListView.ItemsSource = WorkSpace.Instance.BetaFeatures.Features;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(FeatureListView.ItemsSource);
            // Add grouping only once
            if (view.GroupDescriptions.Count == 0) 
            {
                PropertyGroupDescription groupDescription = new PropertyGroupDescription(nameof(BetaFeature.Group));
                view.GroupDescriptions.Add(groupDescription);
            }

            // General
            ShowDebugConsoleCheckBox.BindControl(WorkSpace.Instance.BetaFeatures, nameof(BetaFeatures.ShowDebugConsole));
            ShowTimingsCheckBox.BindControl(WorkSpace.Instance.BetaFeatures, nameof(BetaFeatures.ShowTimings));
            ShowSocketMonitorCheckBox.BindControl(WorkSpace.Instance.BetaFeatures, nameof(BetaFeatures.ShowSocketMonitor));
        }

        public void ShowAsWindow()
        {
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, Ginger.eWindowShowStyle.Dialog, this.Title, this);
        }

        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            WorkSpace.Instance.BetaFeatures.SaveUserPref();
        }

        private void FunctionTesterButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GingerCoreNETWindow w = new GingerCoreNETWindow();
            w.Show();
        }

        private void xCompressSolutionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> BFs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            
            foreach (BusinessFlow BF in BFs)
            {
                foreach(Activity activity in BF.Activities)
                {
                    foreach(Act act in activity.Acts)
                    {
                        if(act is ActGenElement)
                        {
                            ActInputValue AIV1 = (from x in act.InputValues where x.Param == "Xoffset" select x).SingleOrDefault();
                            if (AIV1 != null && AIV1.Value == "0")
                            {
                                act.InputValues.Remove(AIV1);                                
                            }
                            ActInputValue AIV2 = (from x in act.InputValues where x.Param == "Yoffset" select x).SingleOrDefault();
                            if (AIV2 != null && AIV2.Value == "0")
                            {
                                act.InputValues.Remove(AIV2);
                            }

                            ActInputValue AIV3 = (from x in act.InputValues where x.Param == "Value" select x).SingleOrDefault();
                            if (AIV3 != null && AIV3.Value == "")
                            {
                                act.InputValues.Remove(AIV3);
                            }
                        }
                    }
                }
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(BF);
            }

            // TODO: save all other items

           Reporter.ToUser(eUserMsgKey.StaticInfoMessage,"Done");
        }

        private void XMessageTestWindow_Click(object sender, RoutedEventArgs e)
        {
            ReporterTestWindow reporterTestWindow = new ReporterTestWindow();
            reporterTestWindow.Show();

        }
    }
}
