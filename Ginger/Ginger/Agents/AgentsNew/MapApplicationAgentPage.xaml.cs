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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using GingerCore.Platforms;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.AgentsLib
{
    /// <summary>
    /// Interaction logic for MapApplicationAgentPage.xaml
    /// </summary>
    public partial class MapApplicationAgentPage : Page
    {
        ObservableList<IApplicationAgent> mApps;
        public MapApplicationAgentPage(ObservableList<IApplicationAgent> Apps)
        {
            InitializeComponent();

            mApps = Apps;
            AppsListBox.ItemsSource = Apps;
            AppsListBox.SelectionChanged += AppsListBox_SelectionChanged;
        }

        private void AppsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mApps.CurrentItem = (ApplicationAgent)AppsListBox.SelectedItem;
        }

        public GenericWindow ShowAsWindow(System.Windows.Window owner)
        {
            Button AddBtn = new Button();
            AddBtn.Content = "Add Agent";
            AddBtn.Name = "AddAgent";
            AddBtn.Click += new RoutedEventHandler(AddAgentButton_Click);

            ObservableList<Button> Buttons = new ObservableList<Button>();
            Buttons.Add(AddBtn);

            GenericWindow genWin = null;
            GenericWindow.LoadGenericWindow(ref genWin, owner, eWindowShowStyle.Free, this.Title, this, Buttons);
            return genWin;
        }

        private void AddAgentButton_Click(object sender, RoutedEventArgs e)
        {
            // Temp dummy 
            mApps.Add(new ApplicationAgent() { AppName = "koko" });
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationAgent a = (ApplicationAgent)mApps.CurrentItem;
           ((Agent) a.Agent).StartDriver();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            //ApplicationAgent a = (ApplicationAgent)mApps.CurrentItem;
            //a.Agent.CloseDriver();
        }

        private void SelectAgentButton_Click(object sender, RoutedEventArgs e)
        {
            //ApplicationAgent AP = (ApplicationAgent)mApps.CurrentItem;
            //if (AP == null) return;  //TODO: fix me must have selected row, so make the select button row selected

            //RepositoryFolder<NewAgent> f = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<NewAgent>();
            //NewAgentsFolderTreeItem agents = new NewAgentsFolderTreeItem(f, eBusinessFlowsTreeViewMode.ReadOnly);
            //SingleItemTreeViewSelectionPage p = new SingleItemTreeViewSelectionPage("Select Agent", eImageType.Agent, agents);
            //List<object> selected = p.ShowAsWindow("Select Agent for Application - '" + AP.AppName + "'");
            //if (selected != null)
            //{
            //    NewAgent agent = (NewAgent)selected[0];
            //    ((ApplicationAgent)mApps.CurrentItem).Agent = agent;
            //}
        }

        private void AttachDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            ////TODO: start only once !!!!!!!!!!!!!!!

            ////First we search in the same folder as GingerWPF.exe - if is runtime/install folder all exes in the same folder
            //string FilePath = Assembly.GetExecutingAssembly().Location.Replace("GingerWPF.exe", "GingerWPFDriverWindow.exe");
            //if (!File.Exists(FilePath))
            //{
            //    // in case we are in debug
            //    FilePath = FilePath.Replace(@"\GingerWPF\", @"\GingerWPFDriverWindow\");

            //    if (!File.Exists(FilePath))
            //    {
            //        throw new Exception("Cannot find GingerWPFDriverWindow.exe");
            //    }
            //}
            //System.Diagnostics.Process.Start(FilePath);
            //// TODO: start in port... and send back keep connection           

            //ApplicationAgent a = (ApplicationAgent)mApps.CurrentItem;
            //a.Agent.AttachDisplay();
        }
    }
}
