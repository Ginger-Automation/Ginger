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
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowWindows;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for BusinessFlowInfoPage.xaml
    /// </summary>
    public partial class BusinessFlowConfigurationsPage : Page
    {
        BusinessFlow mBusinessFlow;
        Context mContext;

        public BusinessFlowConfigurationsPage(BusinessFlow businessFlow, Context context)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;
            mContext = context;

            BindControls();
        }

        private void BindControls()
        {
            BindingHandler.ObjFieldBinding(xNameTxtBox, TextBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.Name));
            BindingHandler.ObjFieldBinding(xDescriptionTxt, TextBox.TextProperty, mBusinessFlow, nameof(BusinessFlow.Description));
            xTagsViewer.Init(mBusinessFlow.Tags);
            xRunDescritpion.Init(mContext, mBusinessFlow, nameof(BusinessFlow.RunDescription));
            General.FillComboFromEnumObj(xStatusComboBox, mBusinessFlow.Status);
            BindingHandler.ObjFieldBinding(xStatusComboBox, ComboBox.TextProperty, mBusinessFlow, BusinessFlow.Fields.Status);
            BindingHandler.ObjFieldBinding(xCreatedByTextBox, TextBox.TextProperty, mBusinessFlow.RepositoryItemHeader, nameof(RepositoryItemHeader.CreatedBy));
            BindingHandler.ObjFieldBinding(xAutoPrecentageTextBox, TextBox.TextProperty, mBusinessFlow, BusinessFlow.Fields.AutomationPrecentage, System.Windows.Data.BindingMode.OneWay);

            //// Per source we can show specific source page info
            //if (mBusinessFlow.Source == BusinessFlow.eSource.Gherkin)
            //{
            //    SourceGherkinPage SGP = new SourceGherkinPage(mBusinessFlow);
            //    SourceFrame.Content = SGP;
            //}

            xTargetsListBox.ItemsSource = mBusinessFlow.TargetApplications;
            xTargetsListBox.DisplayMemberPath = nameof(TargetApplication.AppName);
        }

        public void UpdateBusinessFlow(BusinessFlow updateBusinessFlow)
        {
            mBusinessFlow = updateBusinessFlow;
            mContext.BusinessFlow = mBusinessFlow;
            BindControls();
        }

        private void AddPlatformButton_Click(object sender, RoutedEventArgs e)
        {
            EditBusinessFlowAppsPage EBFP = new EditBusinessFlowAppsPage(mBusinessFlow);
            EBFP.ShowAsWindow();

            //make sure all Activities mapped to Application after change
            foreach (Activity activity in mBusinessFlow.Activities)
                if (mBusinessFlow.TargetApplications.Where(x => x.Name == activity.TargetApplication).FirstOrDefault() == null)
                    activity.TargetApplication = mBusinessFlow.TargetApplications[0].Name;
        }

    }
}