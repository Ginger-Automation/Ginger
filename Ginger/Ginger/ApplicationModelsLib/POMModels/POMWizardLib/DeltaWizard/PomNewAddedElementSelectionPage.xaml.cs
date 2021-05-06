#region License
/*
Copyright © 2014-2021 European Support Limited

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
using GingerCoreNET.Application_Models;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    /// <summary>
    /// Interaction logic for PomNewAddedElementSelectionPage.xaml
    /// </summary>
    public partial class PomNewAddedElementSelectionPage : Page
    {
        private PomDeltaViewPage mPomDeltaViewPage;
        private Agent mAgent;
        private PomDeltaUtils mPomDeltaUtils;


        public PomNewAddedElementSelectionPage(ObservableList<DeltaElementInfo> deltaElementInfos, PomDeltaUtils pomDeltaUtils, string searchText)
        {
            InitializeComponent();
            mPomDeltaUtils = pomDeltaUtils;

            mAgent = mPomDeltaUtils.Agent;

            mPomDeltaViewPage = new PomDeltaViewPage(deltaElementInfos);
            mPomDeltaViewPage.SetAgent(mAgent);
            mPomDeltaViewPage.xMainElementsGrid.Grid.Columns[1].Visibility = Visibility.Collapsed;
            mPomDeltaViewPage.xMainElementsGrid.btnMarkAll.Visibility = Visibility.Collapsed;

            mPomDeltaViewPage.xMainElementsGrid.txtSearch.Text = searchText;
            xNewPomElementsPageFrame.Content = mPomDeltaViewPage;
        }

        internal DeltaElementInfo SelectedElement()
        {
            return  mPomDeltaViewPage.ShowAsWindow("Added Elements");
        }
    }
}
