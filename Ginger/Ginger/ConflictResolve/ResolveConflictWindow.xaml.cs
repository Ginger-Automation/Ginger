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
using Ginger.SourceControl;
using Ginger.UserControls;
using GingerCore;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Ginger.ConflictResolve.ConflictResolve;

namespace Ginger.ConflictResolve
{
    /// <summary>
    /// Interaction logic for ResolveConflictWindow.xaml
    /// </summary>
    public partial class ResolveConflictWindow : Page
    {
        GenericWindow genWin = null;
        List<string> mConflictPaths;
        public bool IsResolved { get; set; }
        ObservableList<ConflictResolve> conflictResolves = new ObservableList<ConflictResolve>();
        Dictionary<string, string> filePathDct = new Dictionary<string, string>();
        public ResolveConflictWindow(List<string> conflictPaths)
        {
            IsResolved = false;
            InitializeComponent();
            mConflictPaths = conflictPaths;
            SetGridView();
        }
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button resolveBtn = new Button();
            resolveBtn.Content = "Resolve";
            resolveBtn.Click += new RoutedEventHandler(resolve_Click);

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Source Control Conflicts", this, new ObservableList<Button> { resolveBtn }, true, "Do Not Resolve", CloseWindow);
        }
        private void resolve_Click(object sender, EventArgs e)
        {
            IsResolved = true;
            Reporter.ToStatus(eStatusMsgKey.ResolveSourceControlConflicts);
            foreach (ConflictResolve conflictResolve in conflictResolves)
            {
                switch (conflictResolve.resolveOperations)
                {
                    case eResolveOperations.AcceptServer:
                        SourceControlIntegration.ResolveConflicts(WorkSpace.Instance.Solution.SourceControl, conflictResolve.ConflictPath, eResolveConflictsSide.Server);
                        break;
                    case eResolveOperations.KeepLocal:
                        SourceControlIntegration.ResolveConflicts(WorkSpace.Instance.Solution.SourceControl, conflictResolve.ConflictPath, eResolveConflictsSide.Local);
                        break;
                    default:
                        //do nothing
                        break;
                }
            }
            Reporter.HideStatusMessage();
            CloseWindow();
        }

        private void SetGridView()
        {
            foreach (string conflictName in mConflictPaths)
            {
                ConflictResolve newObjConflict = new ConflictResolve();
                newObjConflict.ConflictPath = conflictName;
                newObjConflict.resolveOperations = eResolveOperations.AcceptServer;
                conflictResolves.Add(newObjConflict);
            }
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;
            view.GridColsView.Add(new GridColView() { Field = nameof(ConflictResolve.RelativeConflictPath), Header = "Conflicted File", WidthWeight = 150, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            viewCols.Add(new GridColView() { Field = nameof(ConflictResolve.resolveOperations), Header = "Operation", WidthWeight = 90, BindingMode = BindingMode.TwoWay, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(GingerCore.General.GetEnumValuesForCombo(typeof(eResolveOperations)), nameof(ConflictResolve.resolveOperations), false, true) }); ;

            xConflictingItemsGrid.SetAllColumnsDefaultView(view);
            xConflictingItemsGrid.InitViewItems();
            xConflictingItemsGrid.SetTitleLightStyle = true;
            xConflictingItemsGrid.DataSourceList = this.conflictResolves;
        }
        private void CloseWindow(object sender, EventArgs e)
        {
            IsResolved = false;
            CloseWindow();
        }
        private void CloseWindow()
        {
            genWin.Close();
        }
    }
}
