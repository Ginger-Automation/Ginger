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

using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using GingerWPF.UserControlsLib;
using GingerWPF.WizardLib;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for POMsManagerPage.xaml
    /// </summary>
    public partial class POMsManagerPage : Page
    {
        public POMsManagerPage(RepositoryFolder<ApplicationPOMModel> RF)
        {
            InitializeComponent();
            
            ApplicationPOMsTreeItem t = new ApplicationPOMsTreeItem(RF);
            MainFrame.SetContent(new TreeViewExplorerPage(t));
            
            InitGrid();
            RefreshPOMs();
            
        }

        private void RefreshPOMs()
        {
            //ObservableList<ApplicationPOM> list = new ObservableList<ApplicationPOM>();

            //// Read all POMs folder
            //foreach (string s in Directory.EnumerateDirectories(mPOMsFolder))
            //{
            //    string pomfolder = System.IO.Path.GetFileNameWithoutExtension(s);

            //    string ext = RepositoryItem.FileExt(typeof(ApplicationPOM));
            //    string pomfile = System.IO.Path.Combine(s, pomfolder) + "." + ext + ".xml";  // TODO: use same const which exist in SavePOMWizardPage                
            //    ApplicationPOM pom = (ApplicationPOM)RepositoryItem.LoadFromFile(pomfile);
            //    list.Add(pom);
            //}

            //MainGrid.DataSourceList = list;
        }

        private void InitGrid()
        {
            //GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            //defView.GridColsView = new ObservableList<GridColView>();
            //defView.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPOM.Name), WidthWeight = 100  });
            //defView.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPOM.Description), WidthWeight = 300 });

            //MainGrid.SetAllColumnsDefaultView(defView);
            //MainGrid.InitViewItems();

            //MainGrid.btnAdd.Click += AddPOM;
            //MainGrid.btnRefresh.Click += BtnRefresh_Click;
            //MainGrid.btnEdit.Click += BtnEdit_Click;
            //MainGrid.ShowUpDown = Visibility.Collapsed;
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            //POM POM = (POM)MainGrid.CurrentItem;
            //AddEditPOMWizard wiz = new AddEditPOMWizard(mWindowExplorer, POM);
            //wiz.ShowWizard();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshPOMs();
        }

        private void AddPOM(object sender, RoutedEventArgs e)
        {
            //AddEditPOMWizard wiz = new AddEditPOMWizard();
            //wiz.ShowWizard();
            ////TODO: add event for Wiz close so we can hook and call refresh
        }

        

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {            

            // GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this);
        }

        private void MainGrid_RowChangedEvent(object sender, EventArgs e)
        {
            //ApplicationPOM POM = (ApplicationPOM)MainGrid.CurrentItem;
            //if (POM.ScreenShot == null)
            //{
            //    string filename = POM.FileName.Replace("xml", "Screenshot.bmp");  // TODO: use same const                
            //    //POM.ScreenShot = General.LoadBitmapFromFile(filename);
            //}
            
            //ScreenShotViewPage p = new ScreenShotViewPage(POM.Name, POM.ScreenShot);
            //POMFrame.Content = p;
            
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new AddPOMWizard());                        
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
