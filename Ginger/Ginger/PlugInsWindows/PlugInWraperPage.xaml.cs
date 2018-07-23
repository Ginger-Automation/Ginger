#region License
/*
Copyright © 2014-2018 European Support Limited

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Ginger;
using GingerCore;
using GingerPlugIns;
using GingerCore.Actions.PlugIns;
using Ginger.UserControls;
using GingerPlugIns.ActionsLib;
using GingerCore.Helpers;
using GingerPlugIns.TextEditorLib;
using Ginger.UserControlsLib.TextEditor;
using GingerCore.Actions;
using Ginger.PlugInsLib;

namespace Ginger.PlugInsWindows
{
    /// <summary>
    /// Interaction logic for PlugInWraperPage.xaml
    /// </summary>
    public partial class PlugInWraperPage : Page
    {
        PlugInWrapper mPlugInWrapper;

        public PlugInWraperPage(PlugInWrapper p)
        {
            InitializeComponent();
            mPlugInWrapper = p;
            try
            {
                Init();
            }
            catch(Exception ex)
            {
                LoadErrorLbl.Visibility = Visibility.Visible;
                Reporter.ToUser(eUserMsgKeys.FailedToLoadPlugIn, p.Name, ex.Message);
            }
        }

        private void Init()
        {
            txtBlkDescritpion.Text = string.Empty;
            TextBlockHelper TBH = new TextBlockHelper(txtBlkDescritpion);

            GingerCore.General.ObjFieldBinding(PlugInNamelbl, Label.ContentProperty, mPlugInWrapper, nameof(PlugInWrapper.Name), BindingMode.OneWay);
            GingerCore.General.ObjFieldBinding(txtBlkDescritpion, TextBlock.TextProperty, mPlugInWrapper, nameof(PlugInWrapper.Description), BindingMode.OneWay); 
            GingerCore.General.ObjFieldBinding(PlugInTypelbl, Label.ContentProperty, mPlugInWrapper, nameof(PlugInWrapper.PlugInType), BindingMode.OneWay);
            GingerCore.General.ObjFieldBinding(PlugInVersionlbl, Label.ContentProperty, mPlugInWrapper, nameof(PlugInWrapper.PlugInVersion), BindingMode.OneWay);
            PlugInFolderlbl.Content = mPlugInWrapper.FullPlugInRootPath.ToLower().Replace(App.UserProfile.Solution.Folder.ToLower(), "~");

            SetActionsGrid();
            SetTextEditorGrid();
        }

        private void SetActionsGrid()
        {
            PlugInsActionsGrid.ShowEdit = Visibility.Collapsed;
            PlugInsActionsGrid.ShowUpDown = Visibility.Collapsed;
            PlugInsActionsGrid.ShowTitle = Visibility.Collapsed;

            if (mPlugInWrapper.TextEditors().Count() == 0)
            {
                TextEditorTab.Visibility = Visibility.Hidden;
                ActionsTab.Visibility = Visibility.Collapsed;
                PlugInsActionsGrid.ShowTitle = Visibility.Visible;
                PlugInsActionsGrid.SetTitleLightStyle = true;
            }

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = "Description", Header = "Action Type", AllowSorting = true, WidthWeight = 300, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = "UserDescription", Header = "Description", WidthWeight = 300, BindingMode = BindingMode.OneWay });
            PlugInsActionsGrid.SetAllColumnsDefaultView(view);
            PlugInsActionsGrid.InitViewItems();

            ObservableList<PlugInAction> list = new ObservableList<PlugInAction>();
            if (mPlugInWrapper.Actions != null)
            {
                foreach (PlugInAction Action in mPlugInWrapper.Actions)
                {
                    list.Add(Action);
                }
                PlugInsActionsGrid.DataSourceList = list;
            }
            else
            {
                //TODO : handle if no Actions Found
            }
        }

        private void SetTextEditorGrid()
        {
            PlugInsEditorActionsGrid.ShowEdit = Visibility.Collapsed;
            PlugInsEditorActionsGrid.ShowUpDown = Visibility.Collapsed;
            PlugInsEditorActionsGrid.ShowTitle = Visibility.Collapsed;
            
            GridViewDef view1 = new GridViewDef(GridViewDef.DefaultViewName);
            view1.GridColsView = new ObservableList<GridColView>();
            view1.GridColsView.Add(new GridColView() { Field = "EditorName", Header = "Editor Name", WidthWeight = 300, BindingMode = BindingMode.OneWay });
            view1.GridColsView.Add(new GridColView() { Field = "ExtensionsAsString", Header = "Supported File Extensions", WidthWeight = 300, BindingMode = BindingMode.OneWay });
            
            PlugInsEditorActionsGrid.SetAllColumnsDefaultView(view1);
            PlugInsEditorActionsGrid.InitViewItems();

            PlugInsEditorActionsGrid.DataSourceList = ConvertToObservalbe(mPlugInWrapper.TextEditors());
        }

        private ObservableList<PlugInTextFileEditorBase> ConvertToObservalbe(List<PlugInTextFileEditorBase> T)
        {
            ObservableList<PlugInTextFileEditorBase> OL = new ObservableList<PlugInTextFileEditorBase>();
            foreach (PlugInTextFileEditorBase PTE in T)
                OL.Add(PTE);
            return OL;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                try
                {
                    if (PlugInTab.SelectedItem != null)
                    {
                        foreach (TabItem tab in PlugInTab.Items)
                        {
                            foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                                if (ctrl.GetType() == typeof(TextBlock))
                                {
                                    if (PlugInTab.SelectedItem == tab)
                                        ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("@Skin1_ColorB");
                                    else
                                        ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("@Skin1_ColorA");

                                    ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error in PlugIn tabs style", ex);
                }
            }
        }
}