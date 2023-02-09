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
using GingerCore;
using System;
using System.Linq;
using System.Windows.Controls;
using Ginger.UserControls;
using System.Windows;
using System.ComponentModel;

namespace Ginger.Activities
{
    /// <summary>
    /// Interaction logic for SpecificErrorHandler.xaml
    /// </summary>
    public partial class ErrorHandlerMappingPage : Page
    {
        GenericWindow _pageGenericWin = null;
        public bool OKButtonClicked = false;
        BusinessFlow mBusinessFlow;
        ObservableList<ErrorHandler> lstCurrentBusinessFlowErrorHandler = new ObservableList<ErrorHandler>();
        Activity mActivity;
        public ErrorHandlerMappingPage(Activity activity, BusinessFlow businessFlow)
        {
            InitializeComponent();
            SetGridsView();
            mActivity = activity;
            mBusinessFlow = businessFlow;

            lstCurrentBusinessFlowErrorHandler = new ObservableList<ErrorHandler>(mBusinessFlow.Activities.Where(a => a.GetType() == typeof(ErrorHandler) && a.Active == true
                      && ((ErrorHandler)a).HandlerType == eHandlerType.Error_Handler).Cast<ErrorHandler>().ToList());

            if (mActivity.MappedErrorHandlers.Count!= 0)
            {
                int counter = 0;
                foreach (ErrorHandler _errorHandler in lstCurrentBusinessFlowErrorHandler)
                {
                    if (mActivity.MappedErrorHandlers.Contains(_errorHandler.Guid))
                    {
                        _errorHandler.IsSelected = true;
                        counter++;
                    }
                    else
                        _errorHandler.IsSelected = false;
                }
                if (mActivity.MappedErrorHandlers.Count != counter)
                {
                    Reporter.ToUser(eUserMsgKey.MissingErrorHandler);
                    grdErrorHandler.DataSourceList = lstCurrentBusinessFlowErrorHandler;
                    return;
                }
            }
            else
            {
                // this is needed to clear the selections made in the list of error handlers of current business flow, when mapping 'specific error handlers' type to a new activity
                foreach (ErrorHandler _errorHandler in lstCurrentBusinessFlowErrorHandler)
                {
                    if (_errorHandler.IsSelected)
                    {
                        _errorHandler.IsSelected = false;
                    }
                }
            }
            #region
            // all error handler guids
            //ObservableList<Guid> listGUID = new ObservableList<Guid>(mBusinessFlow.Activities.Where(a => a.GetType() == typeof(ErrorHandler) && a.Active == true
            //            && ((GingerCore.ErrorHandler)a).HandlerType == GingerCore.ErrorHandler.eHandlerType.Error_Handler).Select(x => x.Guid).ToList());
            ////mapped guids
            //if (mActivity.MappedErrorHandlers != null)
            //{
            //    foreach (Guid errGuid in mActivity.MappedErrorHandlers )
            //    {
            //        //if (listGUID.Contains(errGuid))
            //        //{
            //        ErrorHandler _activity = mBusinessFlow.Activities.Where(x => x.Guid == errGuid && x.Active == true).Cast<ErrorHandler>().FirstOrDefault();

            //        if (_activity != null )
            //        {
            //            _activity.IsSelected = true;
            //            lstErrorHandler.Add(_activity);
            //        }


            //       // }
            //    }
            //}
            #endregion
            grdErrorHandler.DataSourceList = lstCurrentBusinessFlowErrorHandler;
            grdErrorHandler.Title = "Name of Error Handlers in "+ mBusinessFlow.Name + "";
            grdErrorHandler.btnMarkAll.Visibility = System.Windows.Visibility.Visible;
            grdErrorHandler.MarkUnMarkAllActive += MarkUnMarkAllActivities;
        }               

        private void SetGridsView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(ErrorHandler.IsSelected), WidthWeight = 2.5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox, Header = "Select" });
            defView.GridColsView.Add(new GridColView() { Field = nameof(Activity.ActivityName), WidthWeight = 15, Header = "Name of Error Handler" });
            grdErrorHandler.SetAllColumnsDefaultView(defView);
            grdErrorHandler.InitViewItems();
            grdErrorHandler.SetTitleLightStyle = true;
        }
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            Button okBtn = new Button();
            okBtn.Content = "Ok";
            okBtn.Click += new RoutedEventHandler(okBtn_Click);

            Button closeBtn = new Button();
            closeBtn.Content = "Close";
            closeBtn.Click += new RoutedEventHandler(closeBtn_Click);

            ObservableList<Button> winButtons = new ObservableList<Button>();
            
            winButtons.Add(closeBtn); winButtons.Add(okBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, GingerDicser.GetTermResValue(eTermResKey.Activity) + "-Error Handler Mapping", this, winButtons, false, string.Empty, CloseWinClicked);
        }
        private void grdErrorHandler_RowChangedEvent(object sender, EventArgs e)
        {
            if (mBusinessFlow != null)
            {
                mBusinessFlow.CurrentActivity = (Activity)grdErrorHandler.CurrentItem;
                if (mBusinessFlow.CurrentActivity != null)
                   ((Activity) mBusinessFlow.CurrentActivity).PropertyChanged += CurrentActivity_PropertyChanged;
            }
        }
        private void MarkUnMarkAllActivities(bool ActiveStatus)
        {
            if (grdErrorHandler.DataSourceList.Count <= 0) return;
            if (grdErrorHandler.DataSourceList.Count > 0)
            {
                ObservableList<ErrorHandler> lstMarkUnMarkActivities = (ObservableList<ErrorHandler>)grdErrorHandler.DataSourceList;
                foreach (ErrorHandler act in lstMarkUnMarkActivities)
                {
                    act.IsSelected = ActiveStatus;
                }
                grdErrorHandler.DataSourceList = lstMarkUnMarkActivities;
            }
        }
        private void CurrentActivity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HandlerType")
                grdErrorHandler.setDefaultView();
        }
        private void CloseWinClicked(object sender, EventArgs e)
        {
            _pageGenericWin.Close();
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            _pageGenericWin.Close();
        }
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            OKButtonClicked = true;
            mActivity.MappedErrorHandlers.Clear();
            mActivity.MappedErrorHandlers =  new ObservableList<Guid>(lstCurrentBusinessFlowErrorHandler.Cast<ErrorHandler>().Where(x => x.IsSelected).Select(x => x.Guid));
            _pageGenericWin.Close();
        }
    }
}
