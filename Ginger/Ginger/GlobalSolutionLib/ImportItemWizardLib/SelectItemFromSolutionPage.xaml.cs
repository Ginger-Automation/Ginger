using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Ginger.Actions;
using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
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
//using System.Windows.Shapes;

namespace Ginger.GlobalSolutionLib.ImportItemWizardLib
{
    /// <summary>
    /// Interaction logic for SelectItemFromSolutionPage.xaml
    /// </summary>
    public partial class SelectItemFromSolutionPage : Page, IWizardPage
    {
        ImportItemWizard wiz;
        SingleItemTreeViewSelectionPage mTargetFolderSelectionPage;

        public SelectItemFromSolutionPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    wiz = (ImportItemWizard)WizardEventArgs.Wizard;
                    break;
                case EventType.Active:
                    //create RepoItemTreeView
                    DocumentsFolderTreeItem documentsFolderRoot = new DocumentsFolderTreeItem();
                    documentsFolderRoot.Path = wiz.SolutionFolder;// Path.Combine(wiz.SolutionFolder, "Documents");
                    documentsFolderRoot.Folder = wiz.SolutionFolder;
                    mTargetFolderSelectionPage = new SingleItemTreeViewSelectionPage("Solution", eImageType.File, documentsFolderRoot, SingleItemTreeViewSelectionPage.eItemSelectionType.Multi, true);
                    //mTargetFolderSelectionPage.xTreeView.xTreeViewTree.ValidationRules.Add(UCTreeView.eUcTreeValidationRules.NoItemSelected);
                    mTargetFolderSelectionPage.OnSelect += MTargetFolderSelectionPage_OnSelectItem;
                    TargetPath.Content = mTargetFolderSelectionPage;
                    break;
            }
            
        }
        private void MTargetFolderSelectionPage_OnSelectItem(object sender, SelectionTreeEventArgs e)
        {
            wiz.SelectedItems = e.SelectedItems;
        }
    }
}
