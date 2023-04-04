using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.InputVariableRule;
using GingerCore;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for InputVariablesRules.xaml
    /// </summary>
    public partial class InputVariablesRules : Page
    {
        public BusinessFlow mBusinessFlow { get; set; }
       
        ObservableList<VariableBase> mVariableList = new ObservableList<VariableBase>();
        public ObservableList<VariableBase> variableList
        {
            get
            {
                return mVariableList;
            }
            set
            {
                mVariableList = value;
                OnPropertyChanged(nameof(variableList));
            }
        }
      
        GenericWindow _pageGenericWin = null;
        
        public InputVariablesRules(BusinessFlow businessFlow = null, bool IsReadOnly = false)
        {
            InitializeComponent();
            mBusinessFlow = businessFlow;                   
            SetGridView();
            GenerateStoreToVarsList();
            VariableRulesGrid.DataSourceList = mBusinessFlow.InputVariableRules;
            VariableRulesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddVariableRule));
            if(IsReadOnly)
            {
                VariableRulesGrid.IsEnabled = false;
            }
            else
            {
                VariableRulesGrid.IsEnabled = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AddVariableRule(object sender, RoutedEventArgs e)
        {
            InputVariableRule variableRule = new InputVariableRule();
            variableRule.Active = true;            
            variableRule.SourceVariableList = variableList;            
            mBusinessFlow.InputVariableRules.Add(variableRule);
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);                   
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(InputVariableRule.Active), WidthWeight = 5, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(InputVariableRule.SourceVariableGuid), Header = "Source Variable", WidthWeight = 20, BindingMode = BindingMode.TwoWay, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = UCSourceVariable.GetTemplate(nameof(InputVariableRule.SourceVariableList), nameof(InputVariableRule.SourceVariableGuid)) });
            view.GridColsView.Add(new GridColView() { Field = nameof(InputVariableRule.Operator), Header = "Operator", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = UCOperator.GetTemplate(nameof(InputVariableRule.Operator)) });
            view.GridColsView.Add(new GridColView() { Field = nameof(InputVariableRule.TriggerValue), Header = "Trigger Value", WidthWeight = 15, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = UCTriggerValue.GetTemplate(nameof(InputVariableRule.SelectedSourceVariable), nameof(InputVariableRule.TriggerValue)) });            
            view.GridColsView.Add(new GridColView() { Field = nameof(InputVariableRule.TargetVariableGuid), Header = "Target Variable", WidthWeight = 20, BindingMode = BindingMode.TwoWay, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = UCTargetVariable.GetTemplate(nameof(InputVariableRule.TargetVariableList), nameof(InputVariableRule.TargetVariableGuid), nameof(InputVariableRule.SourceVariableGuid)) });            
            view.GridColsView.Add(new GridColView() { Field = nameof(InputVariableRule.OperationType), Header = "Operation Configuration", WidthWeight = 26, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = UCOperationValue.GetTemplate(nameof(InputVariableRule.SelectedTargetVariable), nameof(InputVariableRule.OperationType), nameof(InputVariableRule.OperationValue), "", nameof(InputVariableRule.OperationValueList)) });            
            VariableRulesGrid.btnRefresh.Visibility = Visibility.Collapsed;
            VariableRulesGrid.btnEdit.Visibility = Visibility.Collapsed;
            VariableRulesGrid.SetAllColumnsDefaultView(view);
            VariableRulesGrid.InitViewItems();
        }

        private void GenerateStoreToVarsList()
        {            
            if (mBusinessFlow != null)
            {
                List<VariableBase> vList = mBusinessFlow.GetBFandActivitiesVariabeles(includeParentDetails: true, includeOnlySetAsInputValue: true)
                    .Where(x => x.VariableType.Equals("DateTime") || x.VariableType.Equals("Number")
                               || x.VariableType.Equals("String") || x.VariableType.Equals("Selection List")).ToList();
                if (vList !=null)
                {
                    variableList = new ObservableList<VariableBase>(vList);
                }
            }

            foreach (InputVariableRule ivr in mBusinessFlow.InputVariableRules)
            {
                ivr.SourceVariableList = variableList;               
            }
                      
        }


        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            this.Title = "Input " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " Rule";

            Button okBtn = new Button();
            okBtn.Content = "Ok";
            okBtn.Click += new RoutedEventHandler(okBtn_Click);

         
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);
          
            this.Width = 850;
            this.Height = 400;

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, showClosebtn:false);
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            VariableRulesGrid.StopGridSearch();
            VariableRulesGrid.grdMain.CommitEdit();
            //close window
            _pageGenericWin.Close();
        }

        private void UndoChangesAndClose()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            ((RepositoryItemBase)mBusinessFlow).RestoreFromBackup(true);
            Mouse.OverrideCursor = null;

            _pageGenericWin.Close();
        }

    }
}
