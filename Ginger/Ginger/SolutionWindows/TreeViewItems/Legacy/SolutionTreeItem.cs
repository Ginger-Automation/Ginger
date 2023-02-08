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
using Amdocs.Ginger.Common.Enums;
using Ginger.SolutionGeneral;
using Ginger.Imports.CDL;
using GingerCore;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Ginger.SolutionWindows.TreeViewItems
{
    public class SolutionTreeItem : TreeViewItemBase, ITreeViewItem
    {
        private SolutionPage mSolutionPage;
        public Solution Solution { get; set;}

        Object ITreeViewItem.NodeObject()
        {
            return Solution;
        }
        override public string NodePath()
        {
            return Solution.Folder + @"\";
        }
        override public Type NodeObjectType()
        {
            return typeof(Solution);
        }

        StackPanel ITreeViewItem.Header()
        {
            // return TreeViewUtils.CreateItemHeader("Solution '" + Solution.Name + "'", "@Solution_16x16.png", SourceControlFileInfo.GetItemSourceControlImage(Solution.Folder), Solution, Solution.Fields.Name);        
            // it is also possible to build your own Stack Panel and return
            //Solution is a special case since we want to have: Solution 'Solname'   - so we create 3 labels

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            // create Image
            Image image = new Image();
            string ImageFile = "@Solution_16x16.png";
            image.Source = new BitmapImage(new Uri(@"/Images/" + ImageFile, UriKind.RelativeOrAbsolute));

             // Source Control Image
             Image SCimage = new Image();
             SCimage.Source = Ginger.SourceControl.SourceControlUI.GetItemSourceControlImage(Solution.Folder, ref ItemSourceControlStatus);

            // Labels
            Label lbl1 = new Label();
            lbl1.Content = "Solution - '";

            Label lbl2 = new Label();
            lbl2.FontWeight = FontWeights.ExtraBold;
            lbl2.Margin = new Thickness(-10, 0, 0, 0);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(lbl2, Label.ContentProperty, Solution, nameof(Solution.Name));

            Label lbl3 = new Label();
            lbl3.Margin = new Thickness(-10, 0, 0, 0);
            lbl3.Content = "'";
            
            // Add into stack
            stack.Children.Add(image);
            stack.Children.Add(SCimage);
            stack.Children.Add(lbl1);
            stack.Children.Add(lbl2);
            stack.Children.Add(lbl3);

            return stack;
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();
            return Childrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mSolutionPage == null)
            {
                mSolutionPage = new SolutionPage();
            }

            return mSolutionPage;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            TreeViewUtils.AddMenuItem(mContextMenu, "Refresh", RefreshSolution, null, eImageType.Refresh);
            mTreeView.AddToolbarTool(eImageType.Refresh, "Refresh", RefreshSolution);
            TreeViewUtils.AddMenuItem(mContextMenu, "Save Solution Configurations", Save, null, "@Save_16x16.png");
            mTreeView.AddToolbarTool("@Save_16x16.png", "Save Solution Configurations", Save);
            TreeViewUtils.AddMenuItem(mContextMenu, "Save All", SaveAll, null, "@SaveAll_16x16.png");
            mTreeView.AddToolbarTool("@SaveAll_16x16.png", "Save All", SaveAll);

            if (WorkSpace.Instance.BetaFeatures.ShowCDL)
            {
                TreeViewUtils.AddMenuItem(mContextMenu, "Import CDL", ImportCDL, null, "@Import_16x16.png");
            }

            AddFolderNodeBasicManipulationsOptions(mContextMenu,"Solution",false,false,false,false,false,false,false,false,false, true);
            AddSourceControlOptions(mContextMenu);
        }

        private void ImportCDL(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new ImportCDLWizard());            
        }

        private void RefreshSolution(object sender, RoutedEventArgs e)
        {            
        }
        
        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }
        
        private void Save(object sender, System.Windows.RoutedEventArgs e)
        {
            base.SaveTreeItem(Solution);
        }

        private void SaveAll(object sender, System.Windows.RoutedEventArgs e)
        {
            throw new NotImplementedException();            
        }
   }
}
