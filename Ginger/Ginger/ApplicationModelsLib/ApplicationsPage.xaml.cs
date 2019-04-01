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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.ApplicationPlatformsLib;
using GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems;
using GingerWPF.UserControlsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.ApplicationModelsLib
{
    /// <summary>
    /// Interaction logic for ApplicationsPage.xaml
    /// </summary>
    public partial class ApplicationsPage : Page
    {
        public ApplicationsPage()
        {
            InitializeComponent();

            InitTreeView();
        }

        private void InitTreeView()
        {            
            ObservableList<ApplicationPlatform> AMs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPlatform>();

            ObservableList<ITreeViewItem> AMTVs = new ObservableList<ITreeViewItem>();

            foreach (ApplicationPlatform AM in AMs)
            {
                ApplicationTreeItem TVI = new ApplicationTreeItem(AM);
                AMTVs.Add(TVI);                
            }

            MainFrame.SetContent(new TreeViewExplorerPage(AMTVs));
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {            
            AddApplicationPage p = new AddApplicationPage();
            p.ShowAsWindow(null);         
        }
    }
}
