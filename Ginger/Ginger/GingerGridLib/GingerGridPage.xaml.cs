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
using GingerCoreNET.RunLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.GingerGridLib
{
    /// <summary>
    /// Interaction logic for GingerGridPage.xaml
    /// </summary>
    public partial class GingerGridPage : Page
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
        }

        private void PluginProcesses_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() => {
                ShowProcesses();
            });
        }

        private void ShowProcesses()
        {
            xProcessesDataGrid.ItemsSource = WorkSpace.Instance.PlugInsManager.PluginProcesses.ToList();
        }

        private void NodeList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(()=> {
                ShowNodes();
            });

        }

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
                GingerNodeProxy GNA = new GingerNodeProxy(GNI);
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
            foreach  (GingerNodeInfo GNI in  mGingerGrid.NodeList)
            {
                GingerNodeProxy GNA = new GingerNodeProxy(GNI);
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
    }
}
