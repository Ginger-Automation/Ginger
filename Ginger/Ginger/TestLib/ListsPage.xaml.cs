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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Extensions;
using GingerCore;
using GingerCore.Environments;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.TestLib
{
    /// <summary>
    /// Interaction logic for ListsPage.xaml
    /// </summary>
    public partial class ListsPage : Page
    {
        SolutionRepository mSolutionRepository;

        public ListsPage()
        {
            InitializeComponent();
            CreateSolutionRepository();
            InitControls();
        }

        private void InitControls()
        {
            //TODO: uncomments once we have BusinessFlowsFolderTreeItem which get RF
            //RepositoryFolder<BusinessFlow> RF1 = mSolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>();
            //BusinessFlowsFolderTreeItem t = new BusinessFlowsFolderTreeItem(RF1);
            //TVFrame.SetContent(new TreeViewExplorerPage(t));

            RepositoryFolder<BusinessFlow> RF2 = mSolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>();
            ObservableList<BusinessFlow> BFs = RF2.GetFolderItems();
            Grid1.ItemsSource = BFs;

            RepositoryFolder<BusinessFlow> RF3 = mSolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>();
            ObservableList<BusinessFlow> BFs2 = RF3.GetFolderItems();
            Grid2.ItemsSource = BFs2;

            // EnvsComboBox.in
            EnvsListBox.ItemsSource = mSolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
            EnvsListBox.DisplayMemberPath = nameof(ProjEnvironment.Name);

            EnvsComboBox.ItemsSource = mSolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
            EnvsComboBox.DisplayMemberPath = nameof(ProjEnvironment.Name);

            // Go get first env Guid
            Guid guid = mSolutionRepository.GetAllRepositoryItems<ProjEnvironment>()[0].Guid;
            ProjEnvironment env1 = mSolutionRepository.GetRepositoryItemByGuid<ProjEnvironment>(guid);
            EnvNameTextBox.BindControl(env1, nameof(ProjEnvironment.Name));
            
            AllBFsListBox.ItemsSource = mSolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            AllBFsListBox.DisplayMemberPath = nameof(BusinessFlow.Name);
        }

        private void CreateSolutionRepository()
        {
            string folder = @"c:\temp\GingerSolutionRepositoryTest";
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
            CreateSolutionItems();
        }

        private void CreateSolutionItems()
        {
            //Business Flows
            BusinessFlow BF1 = new BusinessFlow("BF1");
            mSolutionRepository.AddRepositoryItem(BF1);


            BusinessFlow BF2 = new BusinessFlow("BF2");
            mSolutionRepository.AddRepositoryItem(BF2);

            BusinessFlow BF3 = new BusinessFlow("BF3");
            mSolutionRepository.AddRepositoryItem(BF3);


            RepositoryFolder<BusinessFlow> BFRF = mSolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>();
            RepositoryFolder<BusinessFlow> SubFolder1 = (RepositoryFolder < BusinessFlow > )BFRF.AddSubFolder("SubFolder1");

            BusinessFlow BF4 = new BusinessFlow("BF4");
            SubFolder1.AddRepositoryItem(BF4);

            // Folder to delete later
            BFRF.AddSubFolder("SubFolderForDelete");

            //Environments
            ProjEnvironment env1 = new ProjEnvironment();
            env1.Name = "Env1";
            mSolutionRepository.AddRepositoryItem(env1);

            ProjEnvironment env2 = new ProjEnvironment();
            env2.Name = "Env2";
            mSolutionRepository.AddRepositoryItem(env2);
            //TODO: add more sample items for testing
        }

        string letters = "ZBADCE";
        int letterIndex = 0;
        private void AddBftoSR_Click(object sender, RoutedEventArgs e)
        {
            string name = letters.Substring(letterIndex, 1);
            letterIndex++;
            if (letterIndex >= letters.Length) letterIndex = 0;

            name += " New BF " + DateTime.Now;
            BusinessFlow BF1 = new BusinessFlow(name);
            mSolutionRepository.AddRepositoryItem(BF1);
        }

        private void AddEnvButton_Click(object sender, RoutedEventArgs e)
        {
            ProjEnvironment env = new ProjEnvironment();
            env.Name = "New Env " + DateTime.Now;
            mSolutionRepository.AddRepositoryItem(env);
        }

        private void DeleteSelectedEnvButton_Click(object sender, RoutedEventArgs e)
        {
            ProjEnvironment env = (ProjEnvironment)EnvsListBox.SelectedItem;
            mSolutionRepository.DeleteRepositoryItem(env);
        }

        private void InitBindedTreeButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItem root = new MenuItem() { Title = "Menu" };
            MenuItem childItem1 = new MenuItem() { Title = "Child item #1" };
            childItem1.Items.Add(new MenuItem() { Title = "Child item #1.1" });
            childItem1.Items.Add(new MenuItem() { Title = "Child item #1.2" });
            root.Items.Add(childItem1);
            root.Items.Add(new MenuItem() { Title = "Child item #2" });

            BindedTreeView.Items.Add(root);
        }

        private void AddBftoSubFolderButton_Click(object sender, RoutedEventArgs e)
        {
            RepositoryFolder<BusinessFlow> root =  mSolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>();
            RepositoryFolder <BusinessFlow> subfolder = root.GetSubFolders()[0];
            BusinessFlow BF = new BusinessFlow("BFSF BF " + DateTime.Now);            
            subfolder.AddRepositoryItem(BF);
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> BFs = mSolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            AllBFsListBox.ItemsSource = BFs.AsCollectionViewOrderBy(nameof(BusinessFlow.Name));
        }
    }

    public class MenuItem
    {
        public MenuItem()
        {
            this.Items = new ObservableList<MenuItem>();
        }

        public string Title { get; set; }
        
        public ObservableList<MenuItem> Items { get; set; }
    }
}
