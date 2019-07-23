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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.UserControls;
using Ginger.UserControlsLib.VisualFlow;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers.Common;
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.VisualAutomate
{
    /// <summary>
    /// Interaction logic for VisualAutomatePage.xaml
    /// </summary>
    public partial class VisualAutomatePage : Page
    {
        BusinessFlow mBusinessFlow;        

        public VisualAutomatePage(BusinessFlow BF)
        {
            mBusinessFlow = BF;

            InitializeComponent();

            // Bind the title label to the BF Name
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(lblTitle, ContentProperty, BF, nameof(BusinessFlow.Name), BindingMode.OneWay);

            //TODO: if big flow takes time to load then show loading message
            if (mBusinessFlow.CurrentActivity ==null && mBusinessFlow.Activities.Count>0)
            {
                mBusinessFlow.CurrentActivity = mBusinessFlow.Activities[0];
            }

            ActivitiesComboBox.ItemsSource = mBusinessFlow.Activities;
            ActivitiesComboBox.DisplayMemberPath = nameof(Activity.ActivityName);

            CreateActivityDiagram((Activity)mBusinessFlow.CurrentActivity);

            SetActionsGridView();            
        }


        private void SetActionsGridView()
        {
            //Set the Tool Bar look
            AvailableActionsGrid.ShowEdit = System.Windows.Visibility.Collapsed;
            AvailableActionsGrid.ShowDelete = System.Windows.Visibility.Collapsed;

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = Act.Fields.Description, WidthWeight = 100 });            

            AvailableActionsGrid.SetAllColumnsDefaultView(view);
            AvailableActionsGrid.InitViewItems();
        }


        private void CreateActivityDiagram(Activity activity)
        {
        }
                
        public void ShowAsWindow()
        {
            GenericWindow genWin = null;
            GingerCore.General.LoadGenericWindow(ref genWin, null, eWindowShowStyle.Free, this.Title, this);
        }
        
        private void AvailableActionsGrid_RowDoubleClick(object sender, EventArgs e)
        {
            // we can decide based on the action added if we need to open the ActionEdit Page to get more info
            bool bOpenActionEditPage = true;
            // User want to add action to flow

            Act act = (Act)AvailableActionsGrid.CurrentItem;            

            //TODO: If this is set value action open VE
            //TODO: if it is validate - then do all and no show?

            if (act is ActUIElement)
            {
                ActUIElement AUIE = (ActUIElement)act;
                if (AUIE.ElementAction == ActUIElement.eElementAction.SetValue)
                {
                    ValueExpressionEditorPage pa = new ValueExpressionEditorPage(act, ActUIElement.Fields.Value, Context.GetAsContext(act.Context));
                    pa.ShowAsWindow(eWindowShowStyle.Dialog);
                    bOpenActionEditPage = false;
                }

                if (AUIE.ElementAction == ActUIElement.eElementAction.Click)
                {
                    bOpenActionEditPage = false;
                }
            }
            if (bOpenActionEditPage)
            {
                ActionEditPage p = new ActionEditPage(act);
                p.ShowAsWindow();
            }

            // TODO: allow cancel in window

            mBusinessFlow.CurrentActivity.Acts.Add(act);
            RefreshDiagram();
        }

        private void RefreshDiagram()
        {
            CreateActivityDiagram((Activity)mBusinessFlow.CurrentActivity);
        }

        private void ActivitiesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mBusinessFlow.CurrentActivity = (Activity)ActivitiesComboBox.SelectedItem;
            RefreshDiagram();
        }
    }
}
