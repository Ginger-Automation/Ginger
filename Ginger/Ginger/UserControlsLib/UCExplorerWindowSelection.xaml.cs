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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
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

namespace Amdocs.Ginger.UserControls
{
    /// <summary>
    /// Interaction logic for ucExplorerWindowSelection.xaml
    /// </summary>
    public partial class UCExplorerWindowSelection : UserControl
    {
        public Context context;
        public LocateByPOMElementPage locateByPOMElementPage;
        SingleItemTreeViewSelectionPage mApplicationPOMSelectionPage = null;
        public ApplicationPOMModel SelectedPOM;

        public UCExplorerWindowSelection()
        {
            InitializeComponent();
        }

        //private void AddSelectedPOM(List<object> selectedPOMs)
        //{
        //    if (selectedPOMs != null && selectedPOMs.Count > 0)
        //    {
        //        foreach (ApplicationPOMModel pom in selectedPOMs)
        //        {
        //            if (mPomModels.Contains(pom) == false)
        //            {
        //                mPomModels.Add(pom);
        //            }
        //        }
        //    }
        //}

        //private void MApplicationPOMSelectionPage_SelectionDone(object sender, SelectionTreeEventArgs e)
        //{
        //    AddSelectedPOM(e.SelectedItems);
        //}

        //private void POMsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{

        //}

        private void xIntegratePOMChkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (locateByPOMElementPage == null)
            {
                ActUIElement act = new ActUIElement();

                locateByPOMElementPage = new LocateByPOMElementPage(context, act, nameof(ActUIElement.ElementType), act, nameof(ActUIElement.ElementLocateValue), true);
                locateByPOMElementPage.SelectPOM_Click(sender, e);

                locateByPOMElementPage.POMChangedPageEvent += LocateByPOMElementPage_POMChangedPageEvent;

                SelectedPOM = locateByPOMElementPage.SelectedPOM;
                //xPOMSelectionFrame.Content = locateByPOMElementPage;
            }

            //xPOMSelectionFrame.Visibility = Visibility.Visible;
        }

        private void LocateByPOMElementPage_POMChangedPageEvent()
        {
            SelectedPOM = locateByPOMElementPage.SelectedPOM;
        }

        private void SelectedPOM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Reporter.ToLog(eLogLevel.ERROR, "Selected POM Change TRIGGERED");
            SelectedPOM = locateByPOMElementPage.SelectedPOM;
        }

        private void xIntegratePOMChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //xPOMSelectionFrame.Visibility = Visibility.Collapsed;
        }
    }
}
