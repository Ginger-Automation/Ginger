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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Ginger.GingerCoreNETTestLib;
using GingerWPF;
using GingerWPF.BusinessFlowsLib;
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

        
    }
}
