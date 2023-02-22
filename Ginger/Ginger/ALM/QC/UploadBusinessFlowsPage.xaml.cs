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

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Actions;
using Ginger.UserControls;

namespace Ginger.ALM.QC
{
    /// <summary>
    /// Interaction logic for UploadBusinessFlowsPage.xaml
    /// </summary>
    public partial class UploadBusinessFlowsPage : Page
    {
        private class BusinessFlowUpload : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public static partial class Fields
            {
                public static string Selected = "Selected";
                public static string Name = "Name";                
                public static string Status = "Status";
                public static string ExternalID = "ExternalID";                
            }

            private bool mSelected { get; set; }
            public bool Selected { get { return mSelected; } set { mSelected = value; OnPropertyChanged("Selected"); } }
            public string Name { get; set; }
            private string mStatus { get; set; }
            public string Status { get { return mStatus; } set { mStatus = value; OnPropertyChanged("Status"); } }
            private string mExternalID { get; set; }
            public string ExternalID { get { return mExternalID; } set { mExternalID = value; OnPropertyChanged("ExternalID"); } }

            public BusinessFlow BF { get; set; }

            public void OnPropertyChanged(string name)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        ObservableList<BusinessFlowUpload> BFUs = new ObservableList<BusinessFlowUpload>();

        ObservableList<BusinessFlow> mBusinessFlows;

        public UploadBusinessFlowsPage(ObservableList<BusinessFlow> BusinessFlows)
        {
            InitializeComponent();

            mBusinessFlows = BusinessFlows;

            UpdateBFGrid();

            grdBusinessFlows.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));

            SetBusinessFlowsGridView();
        }

        private void UpdateBFGrid()
        {
            BFUs.Clear();
            foreach (BusinessFlow BF in mBusinessFlows)
            {
                BusinessFlowUpload BFU = new BusinessFlowUpload();
                BFU.Selected = true;
                BFU.Name = BF.Name;
                BFU.BF = BF;
                BFU.ExternalID = BF.ExternalID;
                BFU.Status = "Active";

                BFUs.Add(BFU);
            }

            grdBusinessFlows.DataSourceList = BFUs;
        }

        private void SelectQCFolder()
        {
            QCTestPlanExplorerPage win = new QCTestPlanExplorerPage();
            win.ShowAsWindow();
            QCFolderTextBox.Text = win.SelectedPath;

        }
        
        private void SetBusinessFlowsGridView()
        {
            grdBusinessFlows.Title = "Upload " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString: "s");
            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = BusinessFlowUpload.Fields.Selected, WidthWeight = 50, StyleType = GridColView.eGridColStyleType.CheckBox, BindingMode = BindingMode.TwoWay });

            view.GridColsView.Add(new GridColView() { Field = BusinessFlowUpload.Fields.Name, WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = BusinessFlowUpload.Fields.Status, WidthWeight = 50 });
            view.GridColsView.Add(new GridColView() { Field = BusinessFlowUpload.Fields.ExternalID, WidthWeight = 50 });

            grdBusinessFlows.SetAllColumnsDefaultView(view);
            grdBusinessFlows.InitViewItems();
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            UpdateBFGrid();
        }
        
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button UploadButton = new Button();
            UploadButton.Content = "Upload";
            UploadButton.Click += new RoutedEventHandler(UploadBFs);

            GenericWindow genWin = null;
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { UploadButton });
        }

        private void UploadBFs(object sender, RoutedEventArgs e)
        {
        }

        private void SelectQCFolderButton_Click(object sender, RoutedEventArgs e)
        {
            SelectQCFolder();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.Name == "chk_selectAll")
            {
                if ((bool)cb.IsChecked)
                {
                    selectUnselectItems(true);
                }
                else
                {
                    selectUnselectItems(false);
                }
            }          
        }

        private void selectUnselectItems(bool selectBox = true)
        {
            foreach (BusinessFlowUpload BFU in BFUs)
            {
                BFU.Selected = selectBox; 
            }
        }
    }
}
