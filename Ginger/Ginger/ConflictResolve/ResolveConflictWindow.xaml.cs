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
        public ResolveConflictWindow(List<string> conflictPaths, Dictionary<string, string> filePathItemNameDct)
        {
            IsResolved = false;
            InitializeComponent();
            mConflictPaths = conflictPaths;
            filePathDct = filePathItemNameDct;
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
                newObjConflict.ItemName = filePathDct[newObjConflict.ConflictPath];
                conflictResolves.Add(newObjConflict);
            }
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;
            view.GridColsView.Add(new GridColView() { Field = nameof(ConflictResolve.ItemName), Header = "Name", WidthWeight = 70, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
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
