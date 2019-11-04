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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.Actions;
using Ginger.Actions._Common;
using Ginger.Extensions;
using Ginger.Help;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using GingerCore.GeneralLib;
using GingerWPF.DragDropLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for ucGrid.xaml
    /// </summary>
    public partial class ucGrid : UserControl, IDragDrop, IClipboardOperations
    {
        #region ##### Control Objects
        private IObservableList mObjList;

        public event RowChangedEventHandler RowChangedEvent;
        public event EventHandler RowDoubleClick;

        public delegate void RowChangedEventHandler(object sender, EventArgs e);

        // DragDrop event handler
        public event EventHandler ItemDropped;
        public delegate void ItemDroppedEventHandler(DragInfo DragInfo);

        public event EventHandler PreviewDragItem;
        public delegate void PreviewDragItemEventHandler(DragInfo DragInfo);

        public event MarkUnMarkAll MarkUnMarkAllActive;
        public delegate void MarkUnMarkAll(bool Status);

        public delegate void SelectedItemChangedHandler(object selectedItem);
        public event SelectedItemChangedHandler SelectedItemChanged;

        public event PasteItemEventHandler PasteItemEvent;

        public void OnSelectedItemChangedEvent(object selectedItem)
        {
            SelectedItemChangedHandler handler = SelectedItemChanged;
            if (handler != null)
            {
                handler(selectedItem);
            }
        }

        public bool ActiveStatus = false;
        private bool UsingDataTableAsSource = false;

        public ObservableList<Guid> Tags = null;
        ICollectionView mCollectionView;

        List <Button> mFloatingButtons = new List<Button>();

        public IObservableList DataSourceList
        {
            set
            {
                try
                {
                    if (mObjList != null)
                    {
                        mObjList.PropertyChanged -= ObjListPropertyChanged;
                    }
                    mObjList = value;
                    if (mObjList != null)
                    {
                        BindingOperations.EnableCollectionSynchronization(mObjList, mObjList);//added to allow collection changes from other threads
                    }

                    mCollectionView = CollectionViewSource.GetDefaultView(mObjList);

                    if (mCollectionView != null)
                    {
                        try
                        {
                            CollectFilterData();
                            mCollectionView.Filter = FilterGridRows;
                        }
                        catch (Exception ex)
                        {
                            grdMain.CommitEdit();
                            grdMain.CancelEdit();
                            mCollectionView.Filter = FilterGridRows;
                            Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                        }
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        grdMain.ItemsSource = mObjList;

                        // Make the first row selected
                        if (value != null && value.Count > 0)
                        {
                            grdMain.SelectedIndex = 0;
                            grdMain.CurrentItem = value[0];
                            // Make sure that in case we have only one item it will be the current - otherwise gives err when one record
                            mObjList.CurrentItem = value[0];                          
                        }
                    });
                    UpdateFloatingButtons();
                }
                catch (InvalidOperationException ioe)
                {
                    //Think this happen I tried to rename an activity I'd just added.
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to set ucGrid.DataSourceList", ioe);
                }
                if (mObjList != null)
                {
                    mObjList.PropertyChanged += ObjListPropertyChanged;
                    mObjList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(CollectionChangedMethod);
                }
            }
            get
            {
                return mObjList;
            }
        }

        public void ClearFilters()
        {            
            this.Dispatcher.Invoke(() =>
            {
                txtSearch.Text = string.Empty;
                Tags.Clear();
            });
        }


        string mFilterSearchText = null;
        List<Guid> mFilterSelectedTags = null;
        private void CollectFilterData()
        {
            //collect search values           
            this.Dispatcher.Invoke(() =>
            {
                mFilterSearchText = txtSearch.Text;
                mFilterSelectedTags = TagsViewer.GetSelectedTagsList();
            });
        }

        private bool FilterGridRows(object obj)
        {
            if (string.IsNullOrEmpty(mFilterSearchText) && (mFilterSelectedTags == null || mFilterSelectedTags.Count == 0))
                return true;

            //Filter by search text            
            if (!string.IsNullOrEmpty(mFilterSearchText))
            {
                if (TextFilter(obj, mFilterSearchText) == true)
                    return true;
            }

            //Filter by Tags            
            if (mFilterSelectedTags!=null && mFilterSelectedTags.Count > 0)
            {
                if (TagsFilter(obj, mFilterSelectedTags) == true)
                    return true;
            }

            return false;
        }

        private bool TextFilter(object obj, string textFilterValue)
        {
            string ObjTxt = ObjToString(obj);

            if (ObjTxt.Contains(textFilterValue.ToUpper()))
            {
                return true;
            }
            return false;
        }

        private bool TagsFilter(object obj, List<Guid> selectedTagsGUID)
        {
            if (obj is ISearchFilter)
            {
                return ((ISearchFilter)obj).FilterBy(eFilterBy.Tags, selectedTagsGUID);
            }
            return false;
        }




        /// <summary>
        ///  Function to return grid object/columns values as one long string + toUpper
        ///  Used for Search in Grid 
        ///  Take only properties of type String
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string ObjToString(object obj)
        {
            //TODO: add new Interface ISearchFilter - which if the object implemented will use it instead of below and will enable faster accurate search
            // meanwhile the below is better then what we had earlier since it didn't work correctly when searching records count more then what shown on the grid 
            // the non visible Grid rows where not calculated in the search

            StringBuilder sb = new StringBuilder();

            if (UsingDataTableAsSource == false)
            {
                foreach (KeyValuePair<string, DataGridColumn> entry in _CurrentGridCols)
                {
                    PropertyInfo PI = obj.GetType().GetProperty(entry.Key);
                    if (PI != null)
                    {
                        //TODO: add some more types if needed 
                        try
                        {
                            object o = PI.GetValue(obj);
                            if (o != null)
                            {
                                // we append the '~' so in case 2 string combined will not make the search string i.e: ABC  + DEF   - will not return true if search for CD
                                sb.Append(PI.GetValue(obj).ToString()).Append("~");
                            }
                        }
                        catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                    }
                }
            }
            else
            {
                try
                {
                    DataRowView row = (DataRowView)obj;
                    if (row != null)
                    {

                        foreach (object cell in row.Row.ItemArray)
                            sb.Append(cell.ToString()).Append("~");
                    }
                }
                catch(Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                }
            }
            return sb.ToString().ToUpper();
        }

        private string ObjToGuid(object obj)
        {
            //TODO: add new Interface ISearchFilter - which if the object implemented will use it instead of below and will enable faster accurate search
            // meanwhile the below is better then what we had earlier since it didn't work correctly when searching records count more then what shown on the grid 
            // the non visible Grid rows where not calculated in the search

            StringBuilder sb = new StringBuilder();

            if(obj.GetType().GetField("Tags") != null)
            {
                FieldInfo FI =obj.GetType().GetField("Tags");
                if (FI != null)
                {
                    //TODO: add some more types if needed 
                    if (FI.FieldType == typeof(ObservableList<Guid>))
                    {
                        object o = FI.GetValue(obj);
                        if (o != null)
                        {
                            foreach(Guid guid in (ObservableList<Guid>)o)
                            {
                                // we append the '~' so in case 2 string combined will not make the search string i.e: ABC  + DEF   - will not return true if search for CD
                                sb.Append(guid.ToString()).Append("~");
                            }
                        }
                    }
                }
            }
            return sb.ToString().ToUpper();
        }

       


        public ucGrid()
        {
            InitializeComponent();

            //Hook Drag Drop handler
            mIsSupportDragDrop = true;
            DragDrop2.HookEventHandlers(this);

            // Prevent bug when typing few letters fast in the search text box
            // Set undo to 0 so will not get the error: “Cannot Undo or Redo while undo unit is open.”            
            txtSearch.UndoLimit = 0;
            if (Tags == null)
                Tags = new ObservableList<Guid>();

            TagsViewer.Init(Tags);
            TagsViewer.TagsStackPanlChanged += TagsViewer_TagsStackPanlChanged;
        }

        private void TagsViewer_TagsStackPanlChanged(object sender, EventArgs e)
        {            
            this.Dispatcher.Invoke(() =>
            {
                grdMain.CommitEdit();
                grdMain.CancelEdit();
                CollectFilterData();
                mCollectionView.Refresh();               
            });            
        }



        //for grid view        
        private Dictionary<string, DataGridColumn> _CurrentGridCols = new Dictionary<string, DataGridColumn>();
        private Dictionary<string, GridViewDef> _GridViews = new Dictionary<string, GridViewDef>();
        private string _DefaultViewName { get; set; }
        public string SelectedViewName
        {
            get
            {
                if (comboView.SelectedItem != null)
                    return comboView.SelectedItem.ToString();
                else
                    return string.Empty;
            }
        }

        #endregion ##### Control Objects

        private void CollectionChangedMethod(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                //different kind of changes that may have occurred in collection
                if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Move)
                {
                    Renum();
                }
            });
        }

        public Boolean AllowReorderRow { get; set; }
        
        #region #####Grid Handlers
        public object Title
        {
            get { return xSimpleHeaderTitle.Content; }

            //TODO: FIXME - due to STA driver like IB move Activity will change the caption
            // Send the MainWindow Dispatcher for updates using main thread
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    xSimpleHeaderTitle.Content = value;
                });

            }
        }
        public DataGrid Grid
        {
            get { return grdMain; }
            set { grdMain = value; }
        }

        public DataGridSelectionMode SelectionMode
        {
            get { return grdMain.SelectionMode; }
            set { grdMain.SelectionMode = value; }
        }
        public Visibility ShowHeader
        {
            get { return xSimpleHeader.Visibility; }
            set { xSimpleHeader.Visibility = value; }
        }
        public Visibility ShowTitle
        {
            get { return xSimpleHeaderTitle.Visibility; }
            set { xSimpleHeaderTitle.Visibility = value; }
        }

        public Style TitleStyle
        {
            get { return xSimpleHeaderTitle.Style; }
            set { xSimpleHeaderTitle.Style = value; }
        }

        public bool SetTitleLightStyle
        {
            get { return (bool)GetValue(SetTitleLightStyleProperty); }
            set { SetValue(SetTitleLightStyleProperty, value); UpdateTitleStyle(); }
        }

        public static readonly DependencyProperty SetTitleLightStyleProperty =
            DependencyProperty.Register("SetTitleLightStyle", typeof(bool),
            typeof(ucGrid));

        public void UpdateTitleStyle()
        {
            xSimpleHeaderTitle.Style = (Style)TryFindResource("$ucGridTitleLightStyle");           
        }

        public void SetGridEnhancedHeader(eImageType itemTypeIcon, string itemTypeName= "",  RoutedEventHandler saveAllHandler = null, RoutedEventHandler addHandler = null)
        {
            GingerHelpProvider.SetHelpString(this, itemTypeName.TrimEnd(new char[] { 's' }));

            xSimpleHeaderTitle.Visibility = Visibility.Collapsed;
            xEnhancedHeader.Visibility = Visibility.Visible;

            xEnhancedHeaderIcon.ImageType = itemTypeIcon;

            if (string.IsNullOrEmpty(itemTypeName))
            { 
                xEnhancedHeaderTitle.Content = xSimpleHeaderTitle.Content.ToString();
            }
            else
            { 
                xEnhancedHeaderTitle.Content = itemTypeName;
            }

            if (saveAllHandler != null)
            {
                xEnhancedHeaderSaveAllButton.Click += saveAllHandler;
                xEnhancedHeaderSaveAllButton.Visibility = Visibility.Visible;
            }
            else
                xEnhancedHeaderSaveAllButton.Visibility = Visibility.Collapsed;

            if (addHandler != null)
            {
                xEnhancedHeaderAddButton.Click += addHandler;
                xEnhancedHeaderAddButton.Visibility = Visibility.Visible;
            }
            else
                xEnhancedHeaderAddButton.Visibility = Visibility.Collapsed;
        }

        public Visibility ShowRefresh
        {
            get { return btnRefresh.Visibility; }
            set { btnRefresh.Visibility = value; }
        }
        public Visibility ShowAdd
        {
            get { return btnAdd.Visibility; }
            set { btnAdd.Visibility = value; }
        }
        public Visibility ShowDelete
        {
            get { return btnDelete.Visibility; }
            set { btnDelete.Visibility = value; }
        }
        public Visibility ShowDuplicate
        {
            get { return btnDuplicate.Visibility; }
            set { btnDuplicate.Visibility = value; }
        }
        public Visibility ShowCopyCutPast
        {
            set
            {
                btnCopy.Visibility = value;
                btnCut.Visibility = value;
                btnPaste.Visibility = value;
            }
        }
      public Visibility ShowCopy
        {
            get { return btnCopy.Visibility; }
            set { btnCopy.Visibility = value; }
        }
        public Visibility ShowCut
        {
            get { return btnCut.Visibility; }
            set { btnCut.Visibility = value; }
        }
        public Visibility ShowPaste
        {
            get { return btnPaste.Visibility; }
            set { btnPaste.Visibility = value; }
        }
        public Visibility ShowEdit
        {
            get { return btnEdit.Visibility; }
            set { btnEdit.Visibility = value; }
        }
        public Visibility ShowClearAll
        {
            get { return btnClearAll.Visibility; }
            set { btnClearAll.Visibility = value; }
        }
		 public Visibility ShowSaveAllChanges
        {
            get { return btnSaveAllChanges.Visibility; }
            set { btnSaveAllChanges.Visibility = value; }
        }
        public Visibility ShowSaveSelectedChanges
        {
            get { return btnSaveSelectedChanges.Visibility; }
            set { btnSaveSelectedChanges.Visibility = value; }
        }

        public Visibility ShowTagsFilter
        {
            get { return TagsViewer.Visibility; }
            set { TagsViewer.Visibility = value; }
        }
        public bool EnableTagsPanel
        {
            get { return TagsViewer.xTagsStackPanl.IsEnabled; }
            set { TagsViewer.xTagsStackPanl.IsEnabled = value;
                if(value == true)
                    TagsViewer.xAddTagBtn.Visibility = Visibility.Visible;
                else
                    TagsViewer.xAddTagBtn.Visibility = Visibility.Collapsed;
            }
        }

        private bool mIsSupportDragDrop;
        public bool IsSupportDragDrop
        {
            get { return mIsSupportDragDrop; }
            set {
                    if (mIsSupportDragDrop == value)
                        return;
                    else if (value == false)
                    {
                        DragDrop2.UnHookEventHandlers(this);
                        mIsSupportDragDrop = value;
                        return;
                    }
                    else if (value == true)
                    {
                        DragDrop2.HookEventHandlers(this);
                        mIsSupportDragDrop = value;
                        return;
                    }

                }
        }

        public Visibility ShowUpDown
        {
            get { return btnUp.Visibility; }
            set
            {
                btnUp.Visibility = value;
                btnDown.Visibility = value;
                if (value == System.Windows.Visibility.Visible)
                {
                    // AllowDrag = true;
                    AllowReorderRow = true;
                }
                else
                    AllowReorderRow = false;
            }
        }
        public Visibility ShowUndo
        {
            get { return btnUndo.Visibility; }
            set { btnUndo.Visibility = value; }
        }
        public Visibility ShowSearch
        {
            get { return lblSearch.Visibility; }
            set
            {
                lblSearch.Visibility = value;
                txtSearch.Visibility = value;
                btnClearSearch.Visibility = value;
            }
        }
        public Visibility ShowToolsBar
        {
            get { return ToolsTray.Visibility; }
            set { ToolsTray.Visibility = value; }
        }
        public Visibility ShowViewCombo
        {
            get
            {
                return comboView.Visibility;
            }
            set
            {
                lblView.Visibility = value;
                comboView.Visibility = value;
            }
        }
        public bool IsReadOnly
        {
            get { return grdMain.IsReadOnly; }
            set { grdMain.IsReadOnly = value; }
        }
        private void SetUpDownButtons()
        {
            if (mObjList.Count == 0)
            {
                btnUp.IsEnabled = false;
                btnDown.IsEnabled = false;
                return;
            }

            //TODO: need to add effect for disabled button
            int i = GetCurrentRow();
            if (i == 0)
            {
                btnUp.IsEnabled = false;
            }
            else
            {
                btnUp.IsEnabled = true;
            }

            if (i < mObjList.Count - 1)
            {
                btnDown.IsEnabled = true;
            }
            else
            {
                btnDown.IsEnabled = false;
            }
        }

        public bool AllowHorizentalScroll = false;

        public void UseGridWithDataTableAsSource(DataTable dataTable,bool autoGenCol=true)
        {
            UsingDataTableAsSource = true;
            grdMain.AutoGenerateColumns = autoGenCol;

            DataView dtView = dataTable.AsDataView();
            ObservableList<DataRowView> rowslist = new ObservableList<DataRowView>();
            foreach (DataRowView row in dtView) rowslist.Add(row);
            mObjList = rowslist;
            mObjList.PropertyChanged += ObjListPropertyChanged;
            grdMain.ItemsSource = dtView; 
        }
        #endregion #####Grid Handlers

        #region ##### Button Methods
        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            int i = GetCurrentRow();
            mObjList.Move(i, i + 1);
            SetUpDownButtons();
            ScrollToViewCurrentItem();
        }
        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            int i = GetCurrentRow();
            mObjList.Move(i, i - 1);
            SetUpDownButtons();
            ScrollToViewCurrentItem();
        }

        public void ScrollToViewCurrentItem()
        {
            if(mObjList.CurrentItem!=null)
                grdMain.ScrollIntoView(mObjList.CurrentItem);
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (mObjList.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.NoItemToDelete);
                return;
            }

            if ((Reporter.ToUser(eUserMsgKey.SureWantToDeleteAll)) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                mObjList.SaveUndoData();
                mObjList.ClearAll();
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (grdMain.SelectedItems.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.SelectItemToDelete);
                    return;
                }
                Mouse.OverrideCursor = Cursors.Wait;

                mObjList.SaveUndoData();

                List<object> SelectedItemsList = grdMain.SelectedItems.Cast<object>().ToList();

                foreach (object o in SelectedItemsList)
                {
                    mObjList.Remove(o);
                    RemoveFromLiteDB(o);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void RemoveFromLiteDB(object o)
        {
            LiteDbReportBase reportBase = new LiteDbReportBase();
            reportBase.RemoveObjFromLiteDB(o);
        }

        private void btnClearSearchText_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ///Do Not implement as this is up to the page to hook the click event
        }
        private void btnDuplicate_Click(object sender, RoutedEventArgs e)
        {
            ///Do Not implement as this is up to the page to hook the click event
        }
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            ClipboardOperationsHandler.CopySelectedItems(this);
        }
        private void btnCut_Click(object sender, RoutedEventArgs e)
        {
            ClipboardOperationsHandler.CutSelectedItems(this);
        }        
        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            ClipboardOperationsHandler.PasteItems(this);
        }

        private void btnSaveAllChanges_Click(object sender, RoutedEventArgs e)
        {
            ///Do Not implement as this is up to the page to hook the click event
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ///Do Not implement as this is up to the page to hook the click event
        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            mObjList.Undo();
        }
		
		public void SetbtnDeleteHandler(RoutedEventHandler handler)
        {
            btnDelete.Click -= new System.Windows.RoutedEventHandler(btnDelete_Click);
            btnDelete.Click += handler;
        }

        public void SetbtnClearAllHandler(RoutedEventHandler handler)
        {
            btnClearAll.Click -= new System.Windows.RoutedEventHandler(btnClearAll_Click);
            btnClearAll.Click += handler;
        }

        public void SetbtnCopyHandler(RoutedEventHandler handler)
        {
            btnCopy.Click -= new System.Windows.RoutedEventHandler(btnCopy_Click);
            btnCopy.Click += handler;
        }

        public void SetbtnPastHandler(RoutedEventHandler handler)
        {
            btnPaste.Click -= new System.Windows.RoutedEventHandler(btnPaste_Click);
            btnPaste.Click += handler;
        }
        
        private void comboView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setDefaultView();
        }

        public void setDefaultView()
        {
            if (_GridViews.ContainsKey(comboView.SelectedItem.ToString()))
            {
                SetView(_GridViews[comboView.SelectedItem.ToString()]);
                SetGridColumnsWidth();
            }
        }
        #endregion ##### Button Methods

        #region ##### Events

        public bool DisableUserItemSelectionChange = false;

        private bool SkipItemSelection = false;
        private void grdMain_SelectionChanged(object sender, MouseButtonEventArgs e)
        {
            if (DisableUserItemSelectionChange)
                SkipItemSelection = true;
            else
                SkipItemSelection = false;
        }


        private void grdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SkipItemSelection)//avoid user item selection in run time 
            {
                SkipItemSelection = false;
                return;
            }

            if (mObjList == null) return;

            if (mObjList.CurrentItem == grdMain.SelectedItem) return;

            DataGridRow r = (DataGridRow)grdMain.ItemContainerGenerator.ContainerFromItem(grdMain.SelectedItem);
            if (r != null)
            {
                // Make sure selected row is visible to user
                r.BringIntoView();
            }

            if (mObjList != null)
                mObjList.CurrentItem = grdMain.SelectedItem; // .CurrentItem;

            SetUpDownButtons();                        
            e.Handled = true;

            if (RowChangedEvent != null)
            {
                RowChangedEvent.Invoke(sender, new EventArgs());
            }

            OnSelectedItemChangedEvent(grdMain.SelectedItem);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Cancel editing incase of user started search without going out of cell edit.
            this.grdMain.CancelEdit(DataGridEditingUnit.Row);
            if (txtSearch.Text.Length > 0)
            {
                SetBtnImage(btnClearSearch, "@Clear_16x16.png");
                btnClearSearch.IsEnabled = true;
            }
            else
            {
                SetBtnImage(btnClearSearch, "@DisabledClear_16x16.png");
                btnClearSearch.IsEnabled = false;
            }

            string search = txtSearch.Text.ToUpper();

            mObjList.CurrentItem = null;


            if (UsingDataTableAsSource == false)
            {
                mObjList.CurrentItem = null;

                if (mCollectionView != null)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        CollectFilterData();
                        mCollectionView.Refresh();
                    });
                }

                MoveToFirst();
            }
            else
            {
                if (string.IsNullOrEmpty(search))
                    ((DataView)grdMain.ItemsSource).RowFilter = string.Empty;
                else
                {
                    string filter = string.Empty;
                    foreach (DataColumn col in ((DataView)grdMain.ItemsSource).Table.Columns)
                        if((col.DataType.Name.Equals("String")))
                            filter += "[" + col.ColumnName + "] LIKE '%" + search + "%' OR ";
                    filter = filter.Substring(0, filter.LastIndexOf("OR"));
                    ((DataView)grdMain.ItemsSource).RowFilter = filter;
                }
            }
        }

        public void StopGridSearch()
        {
            txtSearch.Text = string.Empty;
        }

        private void MoveToFirst()
        {
            if (!mCollectionView.IsEmpty)
            {
                mCollectionView.MoveCurrentToFirst();
            }
        }

        private void grdMain_Loaded(object sender, RoutedEventArgs e)
        {
            SetGridColumnsWidth();
        }

        private void grdMain_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Type t = e.Row.Item.GetType();
            if (t == typeof(System.Data.DataRowView))
            {
                int index = -1;
                foreach (System.Data.DataRowView item in mObjList)
                {
                    index++;
                    if (item.Row.RowState != DataRowState.Deleted && item.Row.ItemArray[0] == (((System.Data.DataRowView)e.Row.Item).Row.ItemArray[0]))
                    {
                        e.Row.Header = index + 1;
                        break;
                    }
                }
            }
            else
                e.Row.Header = mObjList.IndexOf(e.Row.Item) + 1;
        }

        private void Row_DoubleClick(object sender, System.Windows.RoutedEventArgs e)
        {
            EventHandler handler = RowDoubleClick;
            if (handler != null)
            {
                handler(CurrentItem, new EventArgs());
            }
        }
        #endregion ##### Events

        #region ##### External Methods
        public void AddSeparator()
        {
            Separator s = new Separator();
            toolbar.Items.Add(s);
        }
        public void AddButton(string txt, RoutedEventHandler handler)
        {
            Button b = new Button();
            b.AddHandler(Button.ClickEvent, handler);
            b.Content = txt;
            toolbar.Items.Add(b);
        }
        public Label AddLabel(string txt)
        {
            Label lb = new Label();

            lb.Content = txt;
            toolbar.Items.Add(lb);
            return lb;
        }


 public TextBox AddTextBox(string txt, string label, string fieldName, TextChangedEventHandler handler)
        {
            TextBox t = new TextBox();
            t.Text = txt;
            t.AddHandler(TextBox.TextChangedEvent, handler);


            if (label.Trim() != "")
                AddLabel(label);

            if (fieldName.Trim() != "")
                t.Name = fieldName;
            toolbar.Items.Add(t);
            return t;
        }


        public ComboBox AddComboBox(Type eType, string label, string fieldName, RoutedEventHandler handler)
        {
            ComboBox cmb = new ComboBox();
            GingerCore.General.FillComboFromEnumType(cmb, eType);

            cmb.AddHandler(ComboBox.SelectionChangedEvent, handler);
            cmb.Style = this.FindResource("@InputComboBoxStyle") as Style;

            if (label.Trim() != "")
                AddLabel(label);

            if (fieldName.Trim() != "")
                cmb.Name = fieldName;
            //cmb.SelectedIndex = 0;
            toolbar.Items.Add(cmb);
            return cmb;
        }

        public CheckBox AddCheckBox(string txt, RoutedEventHandler handler)
        {
            DockPanel pnl = new DockPanel();
            pnl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            pnl.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            pnl.LastChildFill = false;

            CheckBox b = new CheckBox();
            b.Margin = new Thickness(3, 0, 0, 0);
            b.Height = Double.NaN;
            b.Width = Double.NaN;
            b.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            b.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            b.Padding = new Thickness(2, 0, 0, 0);
            b.Content = txt;
            if (handler != null)
                b.AddHandler(CheckBox.ClickEvent, handler);

            pnl.Children.Add(b); //using dock panel for getting regular check box design
            toolbar.Items.Add(pnl);
            return b;
        }
        
        #endregion ##### External Methods

        #region ##### Internal Methods
        private void ObjListPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            GingerCore.General.DoEvents();
            if (e.PropertyName == "CurrentItem")
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (mObjList.CurrentItem != grdMain.SelectedItem)
                    {
                        grdMain.CurrentItem = mObjList.CurrentItem;
                        int index = grdMain.Items.IndexOf(mObjList.CurrentItem);
                        grdMain.SelectedIndex = index;
                    }
                });
                UpdateFloatingButtons();
            }
        }
        public object CurrentItem
        {
            get
            {
                object o = null;
                this.Dispatcher.Invoke(() =>
                {
                    o = grdMain.SelectedItem;
            });
                return o;
            }
        }

        private int GetCurrentRow()
        {
            return grdMain.Items.IndexOf(grdMain.CurrentItem);
        }

        public IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;
            if (itemsSource == null) yield return null;

            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }

        public List<object> GetVisibileGridItems()
        {
            List<object> visibleItems = new List<object>();

            foreach (DataGridRow row in GetDataGridRows(Grid))
            {
                visibleItems.Add(row.Item);
            }
            return visibleItems;
        }

        public DataGridRow GetDataGridRow(object rowObject)
        {
            return (DataGridRow)this.Grid.ItemContainerGenerator.ContainerFromItem(rowObject);
        }

        public DataGridCell GetDataGridCell(DataGridRow row, int cellColumnIndex)
        {
            DataGridCellsPresenter presenter = General.GetVisualChild<DataGridCellsPresenter>(row);
            return (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(cellColumnIndex);
        }

        /// <summary>
        /// Define the default grid view which supposed to include all grid columns
        /// </summary>
        /// <param name="view"></param>
        public void SetAllColumnsDefaultView(GridViewDef view)
        {            
            _DefaultViewName = view.Name;
            if (!_GridViews.ContainsKey(view.Name))
                _GridViews.Add(view.Name, view);          
            if (!comboView.Items.Contains(view.Name))
                comboView.Items.Add(view.Name);            
        }

        public void updateAndSelectCustomView(GridViewDef view)
        {
            if (!_GridViews.ContainsKey(view.Name))
                _GridViews.Add(view.Name, view);            
            
            if (_GridViews[view.Name].GridColsView.Count != view.GridColsView.Count)
                _GridViews[view.Name] = view;

            if (!comboView.Items.Contains(view.Name))
                comboView.Items.Add(view.Name);

            SetView(view);
            SetGridColumnsWidth();
        }

        /// <summary>
        /// Define the columns with changes that need to be done on top of the default view
        /// </summary>
        /// <param name="view"></param>
        public void AddCustomView(GridViewDef view)
        {
            if (!_GridViews.ContainsKey(view.Name))
                _GridViews.Add(view.Name, view);
            if (!comboView.Items.Contains(view.Name))
            {
                comboView.Items.Add(view.Name);
                if (comboView.Items.Count > 1)
                    ShowViewCombo = System.Windows.Visibility.Visible;
            }
        }


public void RemoveCustomView(string viewName)
        {
            if (_GridViews.ContainsKey(viewName))
                _GridViews.Remove(viewName);
            if (comboView.Items.Contains(viewName))
            {
                if (Convert.ToString(comboView.SelectedItem) == viewName && _DefaultViewName != viewName)
                {
                    comboView.SelectedItem = _DefaultViewName;
                }
                if (Convert.ToString(comboView.SelectedItem) != viewName)
                {
                    comboView.Items.Remove(viewName);
                    if (comboView.Items.Count <= 1)
                        ShowViewCombo = System.Windows.Visibility.Collapsed;
                }
            }
        }
        internal void InitViewItems()
        {
            if (_DefaultViewName == null || _DefaultViewName == string.Empty)
            {
                if (_GridViews.Count() >= 1)
                {
                    _DefaultViewName = _GridViews.Keys.First();
                }
            }
            if (comboView.Items.Contains(_DefaultViewName))
                comboView.SelectedItem = _DefaultViewName;            
        }

        public void ChangeGridView(string viewName)
        {
            if (comboView != null)
            {
                if (comboView.Items.Contains(viewName))
                {
                    comboView.SelectedItem = viewName;
                }
            }
        }
      
        public void DisableGridColoumns()
        {           
            foreach (DataGridColumn gridCol in grdMain.Columns)
            {         
                if(gridCol.GetType()== typeof(DataGridCheckBoxColumn))
                {
                    ((DataGridCheckBoxColumn)gridCol).IsReadOnly = true;
                    ((DataGridCheckBoxColumn)gridCol).ElementStyle = FindResource("@ReadOnlyCheckBoxGridCellElemntStyle") as Style;
                }
                else if(gridCol.GetType() == typeof(DataGridComboBoxColumn))
                {
                    ((DataGridComboBoxColumn)gridCol).IsReadOnly = true;
                    ((DataGridComboBoxColumn)gridCol).ElementStyle = FindResource("@ReadOnlyGridCellElemntStyle") as Style;
                }
                else if(gridCol.GetType() == typeof(DataGridTemplateColumn))
                {
                    ((DataGridTemplateColumn)gridCol).CellStyle = FindResource("@ReadOnlyGridCellElemntStyle") as Style;
                }
                else if (gridCol.GetType() == typeof(DataGridTextColumn))
                {
                    ((DataGridTextColumn)gridCol).CellStyle = FindResource("@ReadOnlyGridCellElemntStyle") as Style;
                }
                else
                {
                    gridCol.IsReadOnly = false;
                }                
            }           
        }
        private void SetView(GridViewDef view)
        {
            try
            {
                if (view.Name == _DefaultViewName)
                {
                    //clearing current view
                    grdMain.Columns.Clear();
                    _CurrentGridCols.Clear();
                }
                else
                {
                    if (_GridViews.ContainsKey(_DefaultViewName))
                    {
                        SetView(_GridViews[_DefaultViewName]);
                    }
                }

                foreach (GridColView colView in view.GridColsView)
                {
                    DataGridColumn gridCol = null;       
                    
                    if (_CurrentGridCols.ContainsKey(colView.Field))
                        gridCol = _CurrentGridCols[colView.Field];
                    
                    else if ((gridCol == null) || (colView.BindingMode != null) || (colView.CellTemplate != null))
                    {
                        //Set the col binding
                        Binding binding = new Binding(colView.Field);
                        if (colView.BindingMode != null)
                            binding.Mode = (BindingMode)colView.BindingMode;
                        else
                            binding.Mode = BindingMode.TwoWay;
                        binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                        //Set the col cells style type
                        switch (colView.StyleType)
                        {
                            case GridColView.eGridColStyleType.Image:
                                gridCol = BindImageColumn(colView.Field);
                                break;

                            case GridColView.eGridColStyleType.ImageMaker:
                                gridCol = BindImageMakerColumn(colView.Field);
                                break;

                            case GridColView.eGridColStyleType.CheckBox:
                                gridCol = new DataGridCheckBoxColumn();
                                ((DataGridCheckBoxColumn)gridCol).Binding = binding;
                                if (colView.MaxWidth == null)
                                    colView.MaxWidth = 100;
                                if (colView.ReadOnly == true)
                                    ((DataGridCheckBoxColumn)gridCol).ElementStyle = FindResource("@ReadOnlyCheckBoxGridCellElemntStyle") as Style;
                                else
                                    ((DataGridCheckBoxColumn)gridCol).ElementStyle = FindResource("@GridCellCheckBoxStyle") as Style;
                                break;

                            case GridColView.eGridColStyleType.ComboBox:
                                gridCol = new DataGridComboBoxColumn();
                                // We can get a list which was converted from enum value and contains value and text CEI style
                                if (colView.CellValuesList is List<ComboEnumItem>)
                                {
                                    ((DataGridComboBoxColumn)gridCol).DisplayMemberPath = ComboEnumItem.Fields.text;
                                    ((DataGridComboBoxColumn)gridCol).SelectedValuePath = ComboEnumItem.Fields.Value;
                                    ((DataGridComboBoxColumn)gridCol).SelectedValueBinding = binding;
                                }
                                else
                                {
                                    //or we can get just simple list of strings if not CEI list
                                    // Assume we got List<string>
                                    if (colView.ComboboxDisplayMemberField != null && colView.ComboboxDisplayMemberField != "")
                                        ((DataGridComboBoxColumn)gridCol).DisplayMemberPath = colView.ComboboxDisplayMemberField;
                                    if (colView.ComboboxSelectedValueField != null && colView.ComboboxSelectedValueField != "")
                                    {
                                        ((DataGridComboBoxColumn)gridCol).SelectedValuePath = colView.ComboboxSelectedValueField;
                                        ((DataGridComboBoxColumn)gridCol).SelectedValueBinding = binding;
                                    }
                                    else
                                        ((DataGridComboBoxColumn)gridCol).TextBinding = binding;
                                }

                                if (!colView.ComboboxSortBy.IsNullOrEmpty() && colView.CellValuesList is IObservableList)
                                    ((DataGridComboBoxColumn)gridCol).ItemsSource = ((IObservableList)colView.CellValuesList).AsCollectionViewOrderBy(colView.ComboboxSortBy);
                                else
                                    ((DataGridComboBoxColumn)gridCol).ItemsSource = (IEnumerable)colView.CellValuesList;

                                break;

                            case GridColView.eGridColStyleType.Link:
                                gridCol = new DataGridHyperlinkColumn();
                                ((DataGridHyperlinkColumn)gridCol).Binding = binding;
                                break;

                            case GridColView.eGridColStyleType.Template:
                                gridCol = new DataGridTemplateColumn();
                                ((DataGridTemplateColumn)gridCol).CellTemplate = colView.CellTemplate;
                                if (colView.ReadOnly == true)
                                    ((DataGridTemplateColumn)gridCol).CellStyle = FindResource("@ReadOnlyGridCellElemntStyle") as Style;
                                break;

                            case GridColView.eGridColStyleType.Text:
                            default:
                                gridCol = new DataGridTextColumn();
                                ((DataGridTextColumn)gridCol).Binding = binding;

                                if (colView.HorizontalAlignment == System.Windows.HorizontalAlignment.Center)
                                    ((DataGridTextColumn)gridCol).ElementStyle = FindResource("@TextBlockGridCellElemntStyle_CenterAlign") as Style;
                                else if (colView.HorizontalAlignment == System.Windows.HorizontalAlignment.Right)
                                    ((DataGridTextColumn)gridCol).ElementStyle = FindResource("@TextBlockGridCellElemntStyle_RightAlign") as Style;

                                if (colView.PropertyConverter != null)
                                    ((DataGridTextColumn)gridCol).ElementStyle = SetColumnPropertyConverter(colView.PropertyConverter.Converter, colView.PropertyConverter.Property);

                                if (colView.Style != null)
                                    ((DataGridTextColumn)gridCol).ElementStyle = colView.Style;
                                break;
                        }
                    }

                    //Set the col design
                    if (colView.Header != null && colView.Header != string.Empty)
                        gridCol.Header = colView.Header;
                    else
                        gridCol.Header = colView.Field;

                    if (colView.WidthWeight != null)
                    {
                        gridCol.Width = (double)colView.WidthWeight;
                    }
                    else
                    {
                        gridCol.Width = gridCol.Header.ToString().Length * 10;
                    }

                    if (colView.MaxWidth != null)
                    {
                        gridCol.MaxWidth = (double)colView.MaxWidth;
                    }

                    if (colView.AllowSorting != null && colView.AllowSorting == true)
                    {
                        gridCol.CanUserSort = true;
                        grdMain.CanUserSortColumns = true;
                    }
                    else
                    {
                        gridCol.CanUserSort = false;
                    }

                    if (colView.Order != null)
                        gridCol.DisplayIndex = (int)colView.Order;

                    if (colView.SortDirection != null)
                        gridCol.SortDirection = colView.SortDirection;

                    if (colView.Visible == false)
                        gridCol.Visibility = System.Windows.Visibility.Collapsed;

                    if (colView.ReadOnly == true)
                        gridCol.IsReadOnly = true;

                    //Add the column to the grid 
                    if (!grdMain.Columns.Contains(gridCol))
                    {                       
                        grdMain.Columns.Add(gridCol);
                        _CurrentGridCols.Add(colView.Field, gridCol);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.FailedToloadTheGrid, ex.Message);
            }
        }

        public static DataTemplate GetDataColGridTemplate(string Path)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(UCDataColGrid));
            factory.SetBinding(UCDataColGrid.DataContextProperty, new Binding(Path));
            template.VisualTree = factory;
            return template;
        }


        public static DataTemplate GetGridComboBoxTemplate(List<ComboEnumItem> valuesList, string selectedValueField, bool allowEdit = false, bool selectedByDefault = false,
                                        string readonlyfield = "", bool isreadonly = false, SelectionChangedEventHandler comboSelectionChangedHandler = null)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory combo = new FrameworkElementFactory(typeof(ComboBox));

            combo.SetValue(ComboBox.ItemsSourceProperty, valuesList);
            combo.SetValue(ComboBox.DisplayMemberPathProperty, nameof(ComboEnumItem.text));
            combo.SetValue(ComboBox.SelectedValuePathProperty,nameof(ComboEnumItem.Value)); 

            Binding selectedValueBinding = new Binding(selectedValueField);
            selectedValueBinding.Mode = BindingMode.TwoWay;
            selectedValueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            combo.SetBinding(ComboBox.SelectedValueProperty, selectedValueBinding);
            if (isreadonly)
            {
                Binding ReadonlyBinding = new Binding(readonlyfield);
                ReadonlyBinding.Mode = BindingMode.OneWay;
                ReadonlyBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                combo.SetBinding(ComboBox.IsHitTestVisibleProperty, ReadonlyBinding);
            }
            else
            {
                if (allowEdit == true)
                    combo.SetValue(ComboBox.IsEditableProperty, true);
            }
            if (selectedByDefault == true)
            {
                combo.SetValue(ComboBox.SelectedIndexProperty, 0);
            }
            if (comboSelectionChangedHandler != null)
                combo.AddHandler(ComboBox.SelectionChangedEvent, comboSelectionChangedHandler);

            template.VisualTree = combo;
            return template;
        }

        public static DataTemplate GetGridComboBoxTemplate(string valuesListField, string selectedValueField, bool allowEdit = false, bool selectedByDefault = false, 
                                                            string readonlyfield ="", bool isreadonly=false, SelectionChangedEventHandler comboSelectionChangedHandler = null)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory combo = new FrameworkElementFactory(typeof(ComboBox));         

            Binding valuesBinding = new Binding(valuesListField);
            valuesBinding.Mode = BindingMode.TwoWay;
            valuesBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            combo.SetBinding(ComboBox.ItemsSourceProperty, valuesBinding);

            Binding selectedValueBinding = new Binding(selectedValueField);
            selectedValueBinding.Mode = BindingMode.TwoWay;
            selectedValueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            combo.SetBinding(ComboBox.TextProperty, selectedValueBinding);
            if (isreadonly)
            {
                Binding ReadonlyBinding = new Binding(readonlyfield);
                ReadonlyBinding.Mode = BindingMode.OneWay;
                ReadonlyBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                combo.SetBinding(ComboBox.IsHitTestVisibleProperty, ReadonlyBinding);
            }
            else
            {
                if (allowEdit == true)
                    combo.SetValue(ComboBox.IsEditableProperty, true);
            }
            if (selectedByDefault == true)
            {
                combo.SetValue(ComboBox.SelectedIndexProperty, 0);
            }

            if (comboSelectionChangedHandler != null)
                combo.AddHandler(ComboBox.SelectionChangedEvent, comboSelectionChangedHandler);

            template.VisualTree = combo;
            return template;
        }

        public static DataTemplate GetStoreToTemplate(string StoreTo, string StoretoValue, ObservableList<string> mVariableList, string VariableList = "", string SupportSetValue = "", string varLabel = "", ObservableList<GlobalAppModelParameter> mAppGlobalParamList = null)//Actreturnva
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory Storeto = new FrameworkElementFactory(typeof(UCStoreTo));

            if (mVariableList != null)
            {
                //ObservableList<string> varList = new ObservableList<string>();
                //foreach (string str in mVariableList)
                //    varList.Add(str);
                //Storeto.SetValue(UCStoreTo.ItemsSourceProperty, varList);
                Storeto.SetValue(UCStoreTo.ItemsSourceProperty, mVariableList);
            }                
            else
            {
                Binding comboItemsSourceBinding = new Binding(VariableList);
                comboItemsSourceBinding.Mode = BindingMode.TwoWay;
                comboItemsSourceBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                Storeto.SetBinding(UCStoreTo.ItemsSourceProperty, comboItemsSourceBinding);
            }

            if (mAppGlobalParamList != null)
            {
                ObservableList<ComboItem> appModelGlobalParamsComboItemsList = new ObservableList<ComboItem>();
                foreach (GlobalAppModelParameter param in mAppGlobalParamList)
                {
                    appModelGlobalParamsComboItemsList.Add(new ComboItem() { text = param.PlaceHolder, Value = param.Guid });
                }

                Storeto.SetValue(UCStoreTo.ItemsSourceGlobalParamProperty, appModelGlobalParamsComboItemsList);
            }

            if (varLabel != "")
            {
                Storeto.SetValue(UCStoreTo.VarLabelProperty, varLabel);
            }

            Binding selectedStoreToBinding = new Binding(StoreTo);
            selectedStoreToBinding.Mode = BindingMode.TwoWay;
            selectedStoreToBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Storeto.SetBinding(UCStoreTo.CheckedProperty, selectedStoreToBinding);
                        
            Binding selectedValueBinding = new Binding(StoretoValue);
            selectedValueBinding.Mode = BindingMode.TwoWay;
            selectedValueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Storeto.SetBinding(UCStoreTo.TextProperty, selectedValueBinding);

            if(SupportSetValue != "")
            {
                Binding allowStoreBinding = new Binding(SupportSetValue);
                allowStoreBinding.Mode = BindingMode.OneWay;
                allowStoreBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                Storeto.SetBinding(UCStoreTo.AllowStoreProperty, allowStoreBinding);
            }

            template.VisualTree = Storeto;
            return template;
        }

        public static DataTemplate getDataColValueExpressionTemplate(string Path,Context context)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(UCValueExpression));
            factory.SetBinding(UCDataColGrid.DataContextProperty, new Binding(Path));
            factory.SetValue(UCValueExpression.ContextProperty, context); 
            template.VisualTree = factory;
          

            return template;
        }
 

        public static DataTemplate getDataColActionDetailsTemplate(string Path)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(UCActionDetails));
            factory.SetBinding(UCDataColGrid.DataContextProperty, new Binding(Path));
            template.VisualTree = factory;

            return template;
        }
        
        public void SetGridColumnsWidth()
        {
            if (mainDockPanel.ActualWidth == 0) return;
            ////Splitting the available free space between all visible columns based on their WidthWeight

            SetGridRowHeaderWidth();
            if (Double.IsNaN(grdMain.RowHeaderWidth))
                grdMain.RowHeaderWidth = 0;
            double pnlWidth = grdMain.ActualWidth - grdMain.RowHeaderWidth;
            double gridColsWidthWeight = 0;
            List<DataGridColumn> visibleCols = new List<DataGridColumn>();
            if (grdMain.Columns.Count > 0)
            {
                foreach (DataGridColumn col in grdMain.Columns)
                {
                    if (col.Visibility == System.Windows.Visibility.Visible)
                    {
                        gridColsWidthWeight += col.Width.Value;
                        visibleCols.Add(col);
                    }
                }

                if (visibleCols.Count > 0)
                {
                    double widthWeightUnitSize = pnlWidth / gridColsWidthWeight;
                    widthWeightUnitSize = Math.Floor(widthWeightUnitSize * 100) / 100;//rounding down
                    foreach (DataGridColumn visCol in visibleCols)
                    {
                        if (!Double.IsNaN(widthWeightUnitSize))
                            visCol.Width = visCol.Width.Value * widthWeightUnitSize;
                    }

                    if (AllowHorizentalScroll == false)
                        FixLastColumn();
                }
            }
            UpdateFloatingButtons();
        }

        private void SetGridRowHeaderWidth()
        {
            if (mObjList == null) return;

            // Calculate the needed row header based on rows count , each digit needs width 8, min 25 - 3 digits
            // assuming no one will present more the 99000 records on grid...
            int RowHeaderWidth = 25;
            if (mObjList.Count > 999)
            {
                RowHeaderWidth += 8;
            }
            if (mObjList.Count > 9999)
            {
                RowHeaderWidth += 8;
            }

            grdMain.RowHeaderWidth = RowHeaderWidth;
        }

        private void FixLastColumn()
        {
            IEnumerable<DataGridColumn> VisibileCols = from col in grdMain.Columns where col.Visibility == System.Windows.Visibility.Visible select col;
            int LastColIndex = VisibileCols.Max(c => c.DisplayIndex);
            DataGridColumn LastCol = (from c in VisibileCols where c.DisplayIndex == LastColIndex select c).Single();
            LastCol.Width = new DataGridLength(LastCol.Width.Value, DataGridLengthUnitType.Star);
        }

        private void grdMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetGridColumnsWidth();
        }

        public DataGridTemplateColumn BindImageColumn(string ImageField)
        {
            // Here we add a new Image column 
            DataGridTemplateColumn imgColumn = new DataGridTemplateColumn();
            imgColumn.Header = "";
            FrameworkElementFactory imageFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Image));
            Binding b = new Binding();
            b.Path = new PropertyPath(ImageField);
            b.Converter = new ImageToSourceConverter();
            imageFactory.SetValue(System.Windows.Controls.Image.SourceProperty, b);
            DataTemplate dataTemplate = new DataTemplate();
            dataTemplate.VisualTree = imageFactory;
            imgColumn.CellTemplate = dataTemplate;       
            return imgColumn;
        }

        public DataGridTemplateColumn BindImageMakerColumn(string ImageField)
        {
            // Here we add a new Image column 
            DataGridTemplateColumn imgColumn = new DataGridTemplateColumn();
            imgColumn.Header = "";
            FrameworkElementFactory imageFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Image));
            Binding b = new Binding();
            b.Path = new PropertyPath(ImageField);
            b.Converter = new ImageMakerToSourceConverter();
            imageFactory.SetValue(System.Windows.Controls.Image.SourceProperty, b);
            DataTemplate dataTemplate = new DataTemplate();
            dataTemplate.VisualTree = imageFactory;
            imgColumn.CellTemplate = dataTemplate;
            return imgColumn;
        }

        public class ImageToSourceConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                    System.Globalization.CultureInfo culture)
            {
                System.Drawing.Image image = (System.Drawing.Image)value;
                if (image != null)
                {
                    MemoryStream ms = new MemoryStream();
                    image.Save(ms, image.RawFormat);
                    ms.Seek(0, SeekOrigin.Begin);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                    return bi;
                }
                return null;
            }

            public object ConvertBack(object value, Type targetType,
                object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class ImageMakerToSourceConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                    System.Globalization.CultureInfo culture)
            {
                eImageType imageType = (eImageType)value;
                return ImageMakerControl.GetImageSource(imageType);                
            }

            public object ConvertBack(object value, Type targetType,
                object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Clear all tool bar default tools except "Search"
        /// </summary>
        public void ClearTools()
        {
            ShowRefresh = System.Windows.Visibility.Collapsed;
            ShowEdit = System.Windows.Visibility.Collapsed;
            ShowAdd = System.Windows.Visibility.Collapsed;
            ShowDelete = System.Windows.Visibility.Collapsed;
            ShowClearAll = System.Windows.Visibility.Collapsed;
            ShowUpDown = System.Windows.Visibility.Collapsed;
        }


        public void AddToolbarTool(string toolImage, string toolTip = "", RoutedEventHandler clickHandler = null, Visibility toolVisibility = System.Windows.Visibility.Visible)
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + toolImage));
            AddToolbarTool(image, toolTip, clickHandler, toolVisibility);
        }

        public void AddToolbarTool(eImageType imageType, string toolTip = "", RoutedEventHandler clickHandler = null, Visibility toolVisibility = System.Windows.Visibility.Visible, int imageSize = 16)
        {
            ImageMakerControl image = new ImageMakerControl();
            image.Width = imageSize;
            image.Height = imageSize;
            image.ImageType = imageType;
            AddToolbarTool(image, toolTip, clickHandler, toolVisibility);
        }


        private void AddToolbarTool(object userControl, string toolTip = "", RoutedEventHandler clickHandler = null, Visibility toolVisibility = System.Windows.Visibility.Visible)
        {
            Button tool = new Button();
            tool.Visibility = toolVisibility;
            tool.ToolTip = toolTip;

            tool.Content = userControl;
            tool.Click += clickHandler;

            toolbar.Items.Remove(lblSearch);
            toolbar.Items.Remove(txtSearch);
            toolbar.Items.Remove(btnClearSearch);
            toolbar.Items.Remove(lblView);
            toolbar.Items.Remove(comboView);
            toolbar.Items.Remove(TagsViewer);
            toolbar.Items.Add(tool);
            toolbar.Items.Add(lblSearch);
            toolbar.Items.Add(txtSearch);
            toolbar.Items.Add(btnClearSearch);
            toolbar.Items.Add(TagsViewer);
            toolbar.Items.Add(lblView);
            toolbar.Items.Add(comboView);
        }


        public void AddComboBoxToolbarTool(string lableContent,Type enumType, SelectionChangedEventHandler view_SelectionChanged, string defaultOptionText = null)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;

            comboBox.Width = 100;
            List<ComboEnumItem> itemsList = GingerCore.General.GetEnumValuesForCombo(enumType);
            if (defaultOptionText != null)
            {
                ComboEnumItem existingDefaultItem = itemsList.Where(x => x.text == defaultOptionText).FirstOrDefault();
                if (existingDefaultItem != null)
                {
                    comboBox.ItemsSource = itemsList;
                    comboBox.SelectedItem = existingDefaultItem;
                }
                else
                {
                    ComboEnumItem newDefaultItem = new ComboEnumItem() { text = defaultOptionText, Value = null };
                    List<ComboEnumItem> itemsListWithNewDefaultText = new List<ComboEnumItem>();
                    itemsListWithNewDefaultText.Add(newDefaultItem);
                    foreach (ComboEnumItem CEI in itemsList)
                    {
                        itemsListWithNewDefaultText.Add(CEI);
                    }
                    comboBox.ItemsSource = itemsListWithNewDefaultText;
                    comboBox.SelectedIndex = 0;
                }
            }

            comboBox.SelectionChanged += view_SelectionChanged;

            Label label = new Label();
            label.Content = lableContent;

            //toolbar.Items.Remove(lblSearch);
            //toolbar.Items.Remove(txtSearch);
            //toolbar.Items.Remove(btnClearSearch);
            //toolbar.Items.Remove(lblView);
            //toolbar.Items.Remove(comboView);
            //toolbar.Items.Remove(TagsViewer);
            //toolbar.Items.Add(label);
            //toolbar.Items.Add(comboBox);
            //toolbar.Items.Add(lblSearch);
            //toolbar.Items.Add(txtSearch);
            //toolbar.Items.Add(btnClearSearch);
            //toolbar.Items.Add(TagsViewer);
            //toolbar.Items.Add(lblView);
            toolbar.Items.Add(label);
            toolbar.Items.Add(comboBox);
        }


        /// <summary>
        /// Get the Control which placed s a child in a cell contains DataTemplate
        /// </summary>
        /// <typeparam name="T">The require Control type</typeparam>
        /// <param name="rowObject"></param>
        /// <param name="cellColumnIndex"></param>
        /// <param name="ObjType"></param>
        /// <returns></returns>
        public object GetDataTemplateCellControl<T>(object rowObject, int cellColumnIndex) where T : Visual
        {
            DataGridRow row = GetDataGridRow(rowObject);
            DataGridCell cell = GetDataGridCell(row, cellColumnIndex);
            return General.GetVisualChild<T>(cell);
        }


        #endregion ##### Internal Methods

        internal Style SetColumnPropertyConverter(IValueConverter Converter, DependencyProperty Property)
        {
            Style columnStyle = new Style(typeof(TextBlock));
            Binding b = new Binding();
            Setter s = new Setter();
            s.Property = Property;
            b.Path = new PropertyPath(TextBlock.TextProperty);
            b.RelativeSource = RelativeSource.Self;
            b.Converter = Converter;
            s.Value = b;
            columnStyle.Setters.Add(s);
            
            return columnStyle;
        }
        
        public void Renum()
        {
            foreach (var item in grdMain.Items)
            {
                var row = (DataGridRow)grdMain.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                    row.Header = row.GetIndex() + 1;
            }
        }

        public void SetBtnImage(Button btn, string imageName)
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + imageName));
            btn.Content = image;
        }
        
        void IDragDrop.StartDrag(DragInfo Info)
        {
            // Get the item under the mouse, or nothing, avoid selecting scroll bars. or empty areas etc..
            var row = (DataGridRow)ItemsControl.ContainerFromElement(this.grdMain, (DependencyObject)Info.OriginalSource);

            if (row != null)
            {
                int selectedItemsCount = this.GetSelectedItems().Count;
                //no drag if we are in the middle of Edit
                if (row.IsEditing) return;

                // No drag if we are in grid cell which is not the regular TextBlock = regular cell not in edit mode
                if (Info.OriginalSource.GetType() != typeof(TextBlock))
                {
                    return;
                }

                Info.DragSource = this;
                if (selectedItemsCount > 1)
                {
                    Info.Data = this.GetSelectedItems();
                    int identityTextLength = row.Item.ToString().ToCharArray().Length;
                    if(identityTextLength > 16)
                    {
                        identityTextLength = 16;
                    }
                    Info.Header = row.Item.ToString().Substring(0, identityTextLength) + ".. + " + (selectedItemsCount-1);
                }
                else
                {
                    Info.Data = row.Item;
                    Info.Header = row.Item.ToString();
                }
                //TODO: Do not use REpo since it will move to UserControls2
                // Each object dragged should override ToString to return nice text for header                
            }
        }

        void IDragDrop.DragOver(DragInfo Info)
        {

        }

        void IDragDrop.DragEnter(DragInfo Info)
        {
            Info.DragTarget = this;

            if (Info.DragTarget == Info.DragSource)
            {
                Info.DragIcon = DragInfo.eDragIcon.DoNotDrop;
            }
            else
            {
                EventHandler handler = PreviewDragItem;
                if (handler != null)
                {
                    handler(Info, new EventArgs());
                }
            }
        }

        void IDragDrop.Drop(DragInfo Info)
        {
            // first check if we did drag and drop in the same grid then it is a move - reorder
            if (Info.DragSource == this)
            {
                if (!(btnUp.Visibility == System.Windows.Visibility.Visible)) return;  // Do nothing if reorder up/down arrow are not allowed
                return;
            }

            // OK this is a dropped from external
            EventHandler handler = ItemDropped;
            if (handler != null)
            {
                handler(Info, new EventArgs());
            }
            // TODO: if in same grid then do move, 
        }        


        public void SetTitleStyle(Style titleStyle)
        {
            xSimpleHeaderTitle.Style = titleStyle;
        }

        private void Btn_MarkUnMarkAll(object sender, RoutedEventArgs e)
        {
            if (ActiveStatus)
            {
                ((Button)sender).Content = FindResource("UnMark");
                ((Button)sender).ToolTip = "Mark All As InActive";
            }
            else
            {
                ((Button)sender).Content = FindResource("Mark");
                ((Button)sender).ToolTip = "Mark All As Active";
            }
            MarkUnMarkAllActive(ActiveStatus);
            ActiveStatus = !ActiveStatus;
        }
    
        public void AddFloatingImageButton(string btnImage, string tooltip, RoutedEventHandler handler, int column)
        {
            Button b = new Button();
            Image image = new Image();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + btnImage));
            b.Content = image;
            b.ToolTip = tooltip;
            b.Style = this.FindResource("@GridFloatingImageButtonStyle") as Style;
            b.Width = 20;
            b.Height = 20;
            b.AddHandler(Button.ClickEvent, handler);
            b.Tag = column;
            b.Visibility = System.Windows.Visibility.Collapsed;
            MainAreaGrid.Children.Add(b);
            mFloatingButtons.Add(b);
        }

        private void UpdateFloatingButtons()
        {
            if (mFloatingButtons == null || mFloatingButtons.Count == 0) return;
            this.Dispatcher.Invoke(() =>
            {                
                    foreach (Button b in mFloatingButtons)
                        b.Visibility = System.Windows.Visibility.Collapsed;

                    if (grdMain.SelectedItem == null) return;

                    // Put the button on the current Grid Row in the column end
                    Dictionary<int, double> colFlotingBtnsSize = new Dictionary<int, double>();
                    double bHieght = 0;
                    DataGridRow r = (DataGridRow)grdMain.ItemContainerGenerator.ContainerFromItem(grdMain.SelectedItem);
                    if (r == null)
                    {
                        grdMain.UpdateLayout();
                        grdMain.ScrollIntoView(grdMain.Items[grdMain.SelectedIndex]);
                        r = (DataGridRow)grdMain.ItemContainerGenerator.ContainerFromItem(grdMain.SelectedItem);
                        if (r == null) return;
                    }
                    Point rel = r.TranslatePoint(new Point(0, 0), grdMain);
                    bHieght = rel.Y;

                    foreach (Button b in mFloatingButtons)
                    {
                        b.Visibility = System.Windows.Visibility.Visible;

                        int col = (int)b.Tag;

                        double marginleft = 0;
                        if (!double.IsNaN(grdMain.RowHeaderWidth))
                            marginleft = grdMain.RowHeaderWidth;
                        for (int i = 0; i <= col; i++)
                        {
                            marginleft += grdMain.Columns[i].ActualWidth;
                        }

                        if (colFlotingBtnsSize.Keys.Contains(col))
                            colFlotingBtnsSize[col] = colFlotingBtnsSize[col] + b.ActualWidth + 2;
                        else
                            colFlotingBtnsSize.Add(col, b.ActualWidth);
                        marginleft -= colFlotingBtnsSize[col] + 2;

                        b.Margin = new Thickness(marginleft, bHieght, 0, 0);
                        b.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        b.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    }              
            });
        }

        private void ClearFloatingButtons()
        {
            if (mFloatingButtons == null || mFloatingButtons.Count == 0) return;

            //clean existing buttons
            foreach (Button b in mFloatingButtons)
                b.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void grdMain_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange > 0)
                ClearFloatingButtons();
        }

        private int GetCheckedRowCount()
        {
            int count = 0;
            for(int i = 0; i < Grid.Items.Count; i++)
            {
                var cItem = grdMain.Items[i];
                var mycheckbox = grdMain.Columns[0].GetCellContent(cItem) as CheckBox;
                if (mycheckbox != null && (bool)mycheckbox.IsChecked)
                {
                    count++;
                }
            }
            return count;
        }

        public static readonly DependencyProperty RowsCountProperty = DependencyProperty.Register(
                    "RowsCount", typeof(int), typeof(ucGrid), new PropertyMetadata(0));        

        public int RowsCount
        {
            get { return (int)this.GetValue(RowsCountProperty); }
            set { this.SetValue(RowsCountProperty, value); }
        }

        public enum eUcGridValidationRules
        {
            CantBeEmpty,
            OnlyOneItem,
            CheckedRowCount
        }

        public List<eUcGridValidationRules> ValidationRules = new List<eUcGridValidationRules>();


        public bool HasValidationError()
        {
            bool validationRes = false;
            foreach(eUcGridValidationRules rule in ValidationRules)
            {
                switch (rule)
                {
                    case eUcGridValidationRules.CantBeEmpty:
                        if (Grid.Items.Count == 0) validationRes= true;
                        break;

                    case eUcGridValidationRules.OnlyOneItem:
                        if (Grid.Items.Count != 1) validationRes= true;
                        break;

                    case eUcGridValidationRules.CheckedRowCount:
                        {
                            int count = GetCheckedRowCount();
                            if (count <= 0)
                            {
                                validationRes = true; 
                            }
                        }
                        break;
                }
            }

            //set border color based on validation
            if (validationRes == true)
                Grid.BorderBrush = System.Windows.Media.Brushes.Red;
            else
                Grid.BorderBrush = FindResource("$Color_DarkBlue") as Brush;

            return validationRes;
        }

        public ObservableList<RepositoryItemBase> GetSelectedItems()
        {
            ObservableList<RepositoryItemBase> selectedItemsList = new ObservableList<RepositoryItemBase>();
            foreach (object selectedItem in grdMain.SelectedItems)
            {
                if (selectedItem is RepositoryItemBase)
                {
                    selectedItemsList.Add((RepositoryItemBase)selectedItem);
                }
            }
            return selectedItemsList;
        }

        public IObservableList GetSourceItemsAsIList()
        {
            return DataSourceList;
        }

        public ObservableList<RepositoryItemBase> GetSourceItemsAsList()
        {
            ObservableList<RepositoryItemBase> list = new ObservableList<RepositoryItemBase>();
            foreach (RepositoryItemBase item in mObjList)
            {
                list.Add(item);
            }
            return list;
        }

        public void OnPasteItemEvent(PasteItemEventArgs.ePasteType pasteType, RepositoryItemBase item)
        {
            PasteItemEvent?.Invoke(new PasteItemEventArgs(pasteType, item));
        }

        public void SetSelectedIndex(int index)
        {
            grdMain.SelectedIndex = index;
        }

        private void DoKeyboardCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (Keyboard.IsKeyDown(Key.C))
                {
                    //Do Copy
                    if (ShowCopy == Visibility.Visible)
                    {
                        ClipboardOperationsHandler.CopySelectedItems(this);
                    }
                }
                else if (Keyboard.IsKeyDown(Key.X))
                {
                    //Do Cut
                    if (ShowCut == Visibility.Visible)
                    {
                        ClipboardOperationsHandler.CutSelectedItems(this);
                    }
                }
                else if (Keyboard.IsKeyDown(Key.V))
                {
                    //Do Paste
                    if (ShowPaste == Visibility.Visible)
                    {
                        ClipboardOperationsHandler.PasteItems(this);
                    }
                }
            }
            else if (Keyboard.IsKeyDown(Key.Delete))
            {
                //delete selected
                if (ShowDelete == Visibility.Visible)
                {
                    btnDelete_Click(null, null);
                }
            }
        }
    }  
}
