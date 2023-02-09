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

using GingerWPF.DragDropLib;
using GingerCoreNET.GeneralLib;
using GingerWPF.GeneralLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Amdocs.Ginger.UserControls;
using Amdocs.Ginger.Common.Enums;

namespace GingerWPF.UserControlsLib.UCTreeView
{
    /// <summary>
    /// Interaction logic for TreeView.xaml
    /// </summary>
    public partial class TreeView2 : UserControl, ITreeView
    {
        public UCTreeView Tree => TreeViewTree;

        public string TreeTitle
        {
            get { return treeViewTitle.Content.ToString(); }
            set { treeViewTitle.Content = value; }
        }

        public Visibility ShowTitle
        {
            get { return treeViewTitle.Visibility; }
            set { treeViewTitle.Visibility = value; }
        }

        public object TreeTooltip
        {
            get { return treeViewTitle.ToolTip; }
            set { treeViewTitle.ToolTip = value; }
        }

        public Style TreeTitleStyle
        {
            get { return treeViewTitle.Style; }
            set { treeViewTitle.Style = value; }
        }       

        public TreeView2()
        {
            InitializeComponent();

            TreeViewTree.ItemSelected += TreeViewTree_ItemSelected;
        }

        private void TreeViewTree_ItemSelected(object sender, EventArgs e)
        {
            ClearCustomeTools();
            if(TreeViewTree.CurrentSelectedTreeViewItem != null)
                TreeViewTree.CurrentSelectedTreeViewItem.SetTools(this);
        }

        private void treeViewClearSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            treeViewTxtSearch.Text = "";
        }

        public void AddToolbarTool(string toolImage, string toolTip = "", RoutedEventHandler clickHandler = null, Visibility toolVisibility = Visibility.Visible, object CommandParameter = null)
        {
            //TODO: Enable sticky tool like swithc to Grid

            Button tool = new Button();
            tool.Visibility = toolVisibility;
            tool.ToolTip = toolTip;
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/Images/" + toolImage, UriKind.RelativeOrAbsolute));
            tool.Content = image;
            tool.Click += clickHandler;
            tool.CommandParameter = CommandParameter;

            treeViewToolbar.Items.Remove(lblSearch);
            treeViewToolbar.Items.Remove(treeViewTxtSearch);
            treeViewToolbar.Items.Remove(treeViewClearSearchBtn);
            treeViewToolbar.Items.Add(tool);
            treeViewToolbar.Items.Add(lblSearch);
            treeViewToolbar.Items.Add(treeViewTxtSearch);
            treeViewToolbar.Items.Add(treeViewClearSearchBtn);
        }
        public void AddToolbarTool(eImageType imageType, string toolTip = "", RoutedEventHandler clickHandler = null, Visibility toolVisibility = System.Windows.Visibility.Visible, object CommandParameter = null)
        {
            ImageMakerControl image = new ImageMakerControl();
            image.Width = 16;
            image.Height = 16;
            image.ImageType = imageType;
            AddToolbarTool(image, toolTip, clickHandler, toolVisibility, CommandParameter);
        }

        private void AddToolbarTool(object userControl, string toolTip = "", RoutedEventHandler clickHandler = null, Visibility toolVisibility = System.Windows.Visibility.Visible, object CommandParameter = null)
        {
            Button tool = new Button();
            tool.Visibility = toolVisibility;
            tool.ToolTip = toolTip;

            tool.Content = userControl;
            tool.Click += clickHandler;
            tool.CommandParameter = CommandParameter;

            treeViewToolbar.Items.Remove(lblSearch);
            treeViewToolbar.Items.Remove(treeViewTxtSearch);
            treeViewToolbar.Items.Remove(treeViewClearSearchBtn);
            treeViewToolbar.Items.Add(tool);
            treeViewToolbar.Items.Add(lblSearch);
            treeViewToolbar.Items.Add(treeViewTxtSearch);
            treeViewToolbar.Items.Add(treeViewClearSearchBtn);
        }
        public void ClearCustomeTools()
        {
            treeViewToolbar.Items.Clear();
            //add the search tool
            treeViewToolbar.Items.Add(lblSearch);
            treeViewToolbar.Items.Add(treeViewTxtSearch);
            treeViewToolbar.Items.Add(treeViewClearSearchBtn);
        }

        private void treeViewTxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (treeViewTxtSearch.Text.Length > 0)
            {
                SetBtnImage(treeViewClearSearchBtn, "@Clear_16x16.png");
                treeViewClearSearchBtn.IsEnabled = true;
            }
            else
            {
                SetBtnImage(treeViewClearSearchBtn, "@DisabledClear_16x16.png");
                treeViewClearSearchBtn.IsEnabled = false;
            }

            string txt = treeViewTxtSearch.Text;
            TreeViewTree.FilterItemsByText(TreeViewTree.TreeItemsCollection, txt);
        }

        public void SetBtnImage(Button btn, string imageName)
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/Images/" + imageName, UriKind.RelativeOrAbsolute));
            btn.Content = image;
        }

        public void SearchTree(string txt)
        {
            treeViewTxtSearch.Text = txt;
        }
    }
}
