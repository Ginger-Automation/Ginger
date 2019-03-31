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

using Ginger;
using Ginger.BusinessFlowLib;
using GingerCore;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for BusinessFlowViewPage.xaml
    /// </summary>
    public partial class BusinessFlowViewPage : Page
    {
        BusinessFlow mBusinessFlow;

        public BusinessFlowViewPage(BusinessFlow BF)
        {
            InitializeComponent();

            mBusinessFlow = BF;
            
            // Binding
            FlowNameLabel.BindControl(BF, nameof(BusinessFlow.Name));

            DescriptionLabel.BindControl(BF, nameof(BusinessFlow.Description));

            ApplciationLabel.BindControl(BF, nameof(BusinessFlow.Applications));

            BusinessFlowDiagramFrame.SetContent(new BusinessFlowDiagramPage(BF));
        }

        private void AutomateButton_Click(object sender, RoutedEventArgs e)
        {
            //WorkSpace.Instance.EventHandler.AutomateBusinessFlow(mBusinessFlow);            
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            BusinessFlowInfoPage p = new BusinessFlowInfoPage(mBusinessFlow);
            p.ShowAsWindow();
        }
    }
}
