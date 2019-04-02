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

using System.Linq;
using System.Windows.Controls;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionAutomatedALMDefectsEditPage.xaml
    /// </summary>
    public partial class RunSetActionAutomatedALMDefectsEditPage : Page
    {
        private RunSetActionAutomatedALMDefects runSetActionAutomatedALMDefects;

        public RunSetActionAutomatedALMDefectsEditPage(RunSetActionAutomatedALMDefects runSetActionAutomatedALMDefects)
        {
            InitializeComponent();
            this.runSetActionAutomatedALMDefects = runSetActionAutomatedALMDefects;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RadioDefectsOpeningModeForAll, RadioButton.IsCheckedProperty, runSetActionAutomatedALMDefects, RunSetActionAutomatedALMDefects.Fields.DefectsOpeningModeForAll);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RadioDefectsOpeningModeForMarked, RadioButton.IsCheckedProperty, runSetActionAutomatedALMDefects, RunSetActionAutomatedALMDefects.Fields.DefectsOpeningModeForMarked);

            if ((!(bool)RadioDefectsOpeningModeForAll.IsChecked) && (!(bool)RadioDefectsOpeningModeForMarked.IsChecked))
            {
                RadioDefectsOpeningModeForMarked.IsChecked = true;
            }

            CurrentProfilePickerCbx_Binding();
        }

        public void CurrentProfilePickerCbx_Binding()
        {
            CurrentProfilePickerCbx.ItemsSource = null;

            if ( WorkSpace.Instance.Solution != null)
            {
                CurrentProfilePickerCbx.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>();
                CurrentProfilePickerCbx.DisplayMemberPath = nameof(ALMDefectProfile.Name).ToString();
                CurrentProfilePickerCbx.SelectedValuePath = nameof(ALMDefectProfile.ID).ToString();
                if (runSetActionAutomatedALMDefects.SelectedDefectsProfileID != 0)
                {
                    CurrentProfilePickerCbx.SelectedIndex = CurrentProfilePickerCbx.Items.IndexOf(((ObservableList<ALMDefectProfile>)CurrentProfilePickerCbx.ItemsSource).Where(x => (x.ID == runSetActionAutomatedALMDefects.SelectedDefectsProfileID)).FirstOrDefault());
                    if (CurrentProfilePickerCbx.SelectedIndex == -1)
                    {
                        CurrentProfilePickerCbx.SelectedIndex = CurrentProfilePickerCbx.Items.IndexOf(((ObservableList<ALMDefectProfile>)CurrentProfilePickerCbx.ItemsSource).Where(x => (x.IsDefault == true)).FirstOrDefault());
                    }
                }
                else
                {
                    CurrentProfilePickerCbx.SelectedIndex = CurrentProfilePickerCbx.Items.IndexOf(((ObservableList<ALMDefectProfile>)CurrentProfilePickerCbx.ItemsSource).Where(x => (x.IsDefault == true)).FirstOrDefault());
                }
            }
        }

        private void CurrentProfilePickerCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            runSetActionAutomatedALMDefects.SelectedDefectsProfileID = ((ALMDefectProfile)CurrentProfilePickerCbx.SelectedItem).ID;
        }
    }
}
