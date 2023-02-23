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
using Amdocs.Ginger.Common.Run;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.Repository;
using Ginger.Drivers.CommunicationProtocol;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using GingerCoreNET.RunLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.GingerGridLib
{
    /// <summary>
    /// Interaction logic for GingerGridPage.xaml
    /// </summary>
    public partial class GingerGridPage : GingerUIPage
    {
        GingerGrid mGingerGrid;

        public GingerGridPage(GingerGrid GingerGrid)
        {
            InitializeComponent();
            mGingerGrid = GingerGrid;
            StatusLabel.BindControl(GingerGrid, nameof(GingerGrid.Status));
            mGingerGrid.NodeList.CollectionChanged += NodeList_CollectionChanged;
            ShowNodes();
            WorkSpace.Instance.PlugInsManager.PluginProcesses.CollectionChanged += PluginProcesses_CollectionChanged;
            ShowProcesses();
            InitRemoteServiceGrid();
        }



        private void PluginProcesses_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ShowProcesses();
            });
        }

        private void ShowProcesses()
        {
            xProcessesDataGrid.ItemsSource = WorkSpace.Instance.PlugInsManager.PluginProcesses.ToList();
        }

        private void NodeList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ShowNodes();

                if (WorkSpace.Instance.BetaFeatures.ShowSocketMonitor)
                {
                    ShowSocketMonitor();
                }

            });
        }

        private void ShowSocketMonitor()
        {
            if (mGingerGrid == null)
            {
                return;
            }
            //Check all nodes and create new Monitor if not exist
            foreach (GingerNodeInfo gingerNodeInfo in mGingerGrid.NodeList)
            {
                GingerNodeProxy gingerNodeProxy = mGingerGrid.GetNodeProxy(gingerNodeInfo);
                if (gingerNodeProxy.Monitor == null)
                {
                    gingerNodeProxy.Monitor = new GingerSocketMonitorWindow(gingerNodeProxy);
                    gingerNodeProxy.StartRecordingSocketTraffic();
                }
            }
        }


        // TODO: enable close monitors

        //private void ShowSocketMonitorCheckBox_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    foreach (GingerNodeInfo ff in mGingerGrid.NodeList)
        //    {
        //        GingerNodeProxy gingerNodeProxy = mGingerGrid.GetNodeProxy(ff);
        //        if (gingerNodeProxy.Monitor != null)
        //        {
        //            gingerNodeProxy.Monitor.CloseMonitor();;
        //            gingerNodeProxy.Monitor = null;
        //        }
        //    }
        // }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button OKButton = new Button();
            OKButton.Content = "OK";
        }

        private void ShowNodes()
        {
            // Base on view type we can show simple list or screen shots or...
            if (mGingerGrid.NodeList.Count == 0)
            {
                xServicesGrid.Children.Clear();
                // TODO: show a label no nodes found
                return;
            }
            ShowNodeList();
        }



        private void ShowNodeList()
        {
            DataGrid DG = new DataGrid();
            DG.IsReadOnly = true;
            DG.ItemsSource = mGingerGrid.NodeList.ToList();
            xServicesGrid.Children.Add(DG);
        }

        private void ShowUIGrid()
        {
            BuildUIGrid();
            int row = 0;
            int col = 0;

            foreach (GingerNodeInfo GNI in mGingerGrid.NodeList)
            {
                GingerNodeProxy GNA = mGingerGrid.GetNodeProxy(GNI);
                GingerGridNodePage p = new GingerGridNodePage(GNA);
                // Connect to LiveView Channel - this is not via Run act
                Frame f = new Frame();
                f.Content = p;
                xServicesGrid.Children.Add(f);

                Grid.SetRow(f, row);
                Grid.SetColumn(f, col);

                col++;
                if (col > xServicesGrid.ColumnDefinitions.Count)
                {
                    col = 0;
                    row++;
                }
            }
        }

        private void BuildUIGrid()
        {
            int total = mGingerGrid.NodeList.Count;
            if (total == 0) return;
            //TODO: verify the display UI per below algorithm, if good use it also in RunSet

            //First we decide how many rows columns
            int rows = (int)Math.Round(Math.Sqrt(total));
            int columns = (int)Math.Round((decimal)total / rows);


            for (int r = 0; r < rows; r++)
            {
                RowDefinition RD = new RowDefinition() { Height = new GridLength(100, GridUnitType.Star) };
                xServicesGrid.RowDefinitions.Add(RD);
            }

            for (int c = 0; c < columns; c++)
            {
                ColumnDefinition CD = new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Star) };
                xServicesGrid.ColumnDefinitions.Add(CD);
            }
        }

        private void ClearGingersGrid()
        {
            xServicesGrid.RowDefinitions.Clear();
            xServicesGrid.ColumnDefinitions.Clear();
            xServicesGrid.Children.Clear();
        }

        private void xUIViewButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGingersGrid();
            ShowUIGrid();
        }

        private void xTableButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGingersGrid();
            ShowNodes();
        }

        private void xPingButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (GingerNodeInfo GNI in mGingerGrid.NodeList)
            {
                GingerNodeProxy GNA = mGingerGrid.GetNodeProxy(GNI);
                GNA.GingerGrid = WorkSpace.Instance.LocalGingerGrid;
                GNA.Reserve();
                string rc = GNA.Ping();
                GNI.Ping = rc;
                GNA.Disconnect();
            }
            ShowProcesses();


        }

        private void xClearButton_Click(object sender, RoutedEventArgs e)
        {
            mGingerGrid.NodeList.Clear();
        }



        ObservableList<RemoteServiceGrid> mRemoteServiceGrids;
        private void InitRemoteServiceGrid()
        {
            mRemoteServiceGrids = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RemoteServiceGrid>();
            foreach (RemoteServiceGrid remoteServiceGrid in mRemoteServiceGrids)
            {
                StartTrackingRemoteServiceGrid(remoteServiceGrid);
            }
            xRemoteServiceGrid.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RemoteServiceGrid>();
            xRemoteServiceGrid.ShowRefresh = Visibility.Collapsed;
            xRemoteServiceGrid.btnSaveAllChanges.Click += BtnSaveAllChanges_Click;
            xRemoteServiceGrid.ShowSaveAllChanges = Visibility.Collapsed;
            xRemoteServiceGrid.btnAdd.Click += BtnAdd_Click;
            xRemoteServiceGrid.SetbtnClearAllHandler(BtnClearAll_Click);
            xRemoteServiceGrid.SetbtnDeleteHandler(btnDeleteSelected_Click);
            SetRemoteGridView();
        }

        private void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (mRemoteServiceGrids.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.NoItemToDelete);
                return;
            }
            List<RemoteServiceGrid> SelectedItemsList = xRemoteServiceGrid.grdMain.SelectedItems.Cast<RemoteServiceGrid>().ToList();
            foreach (RemoteServiceGrid selectedItem in SelectedItemsList)
            {
                WorkSpace.Instance.SolutionRepository.DeleteRepositoryItem(selectedItem);
            }
            if (xRemoteServiceGrid.Grid.SelectedItems.Count == 0)
            {
                WorkSpace.Instance.CurrentSelectedItem = null;
            }
        }

        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (mRemoteServiceGrids.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.NoItemToDelete);
                return;
            }

            if ((Reporter.ToUser(eUserMsgKey.SureWantToDeleteAll)) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                do
                {
                    // using repeated function calls below instead of usual iteration
                    //because list changes every time delete is called
                    WorkSpace.Instance.SolutionRepository.DeleteRepositoryItem(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RemoteServiceGrid>()[0]);
                } while (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RemoteServiceGrid>().Count != 0);
                WorkSpace.Instance.CurrentSelectedItem = null;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddRemoteGrid();
        }

        private void BtnSaveAllChanges_Click(object sender, RoutedEventArgs e)
        {
            foreach (RemoteServiceGrid remoteServiceGrid in mRemoteServiceGrids)
            {
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(remoteServiceGrid);
            }
        }

        private void XAddRemoteGrid_Click(object sender, RoutedEventArgs e)
        {
            AddRemoteGrid();
        }

        void AddRemoteGrid()
        {
            // TODO: createWizard
            int remoteGridCount = mRemoteServiceGrids.Count + 1;
            RemoteServiceGrid remoteServiceGrid = new RemoteServiceGrid() { Name = "Remote Grid " + remoteGridCount, Host = SocketHelper.GetLocalHostIP(), HostPort = 15555, Active = true };
            StartTrackingRemoteServiceGrid(remoteServiceGrid);
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(remoteServiceGrid);
            xRemoteServiceGrid.Grid.SelectedIndex = xRemoteServiceGrid.Grid.Items.Count-1;
        }

        private void SetRemoteGridView()
        {
            //# Default View
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(RemoteServiceGrid.Name), Header = "Name" });
            view.GridColsView.Add(new GridColView() { Field = nameof(RemoteServiceGrid.Host), Header = "Host" });
            view.GridColsView.Add(new GridColView() { Field = nameof(RemoteServiceGrid.HostPort), Header = "Port" });
            view.GridColsView.Add(new GridColView() { Field = nameof(RemoteServiceGrid.Active), Header = "Active" });

            xRemoteServiceGrid.SetAllColumnsDefaultView(view);
            xRemoteServiceGrid.InitViewItems();
        }

        protected override void IsVisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (xRemoteServiceGrid.Grid.Items.Count != 0 && xRemoteServiceGrid.Grid.SelectedItems.Count != 0 && xRemoteServiceGrid.Grid.SelectedItems[0] != null)
            {
                CurrentItemToSave = (RepositoryItemBase)xRemoteServiceGrid.Grid.SelectedItems[0];
                base.IsVisibleChangedHandler(sender, e);
            }
            else
            {
                WorkSpace.Instance.CurrentSelectedItem = null;
            }
        }

        private void xRemoteServiceGrid_SelectedItemChanged(object selectedItem)
        {
            if (selectedItem != null && selectedItem != WorkSpace.Instance.CurrentSelectedItem)
            {
                WorkSpace.Instance.CurrentSelectedItem = (Amdocs.Ginger.Repository.RepositoryItemBase)selectedItem;
            }
        }
        private void StartTrackingRemoteServiceGrid(RemoteServiceGrid RSG)
        {
            RSG.StartDirtyTracking();
        }
    }
}

