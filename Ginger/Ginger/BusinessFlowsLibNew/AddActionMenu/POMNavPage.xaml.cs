using Amdocs.Ginger.Common.Enums;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.Help;
using GingerWPF.UserControlsLib.UCTreeView;
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

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for POMNavAction.xaml
    /// </summary>
    public partial class POMNavPage : Page
    {
        readonly PomAllElementsPage mPomAllElementsPage;
        public POMNavPage()
        {
            InitializeComponent();
        }

        public TreeView1 TreeView
        {
            get { return xTreeView; }
        }

        public POMNavPage(string itemTypeName, eImageType itemTypeIcon, ITreeViewItem itemTypeRootNode, RoutedEventHandler saveAllHandler = null, RoutedEventHandler addHandler = null, EventHandler treeItemDoubleClickHandler = null)
        {
            InitializeComponent();

            GingerHelpProvider.SetHelpString(this, itemTypeName.TrimEnd(new char[] { 's' }));

            xTreeView.TreeTitle = itemTypeName;
            xTreeView.TreeIcon = itemTypeIcon;

            TreeViewItem r = xTreeView.Tree.AddItem(itemTypeRootNode);
            r.IsExpanded = true;

            itemTypeRootNode.SetTools(xTreeView);
            xTreeView.SetTopToolBarTools(saveAllHandler, addHandler);

            xTreeView.Tree.ItemSelected += MainTreeView_ItemSelected;

            //if (treeItemDoubleClickHandler != null)
            //{
            //    xTreeView.Tree.ItemDoubleClick += treeItemDoubleClickHandler;
            //}
        }

        private void MainTreeView_ItemSelected(object sender, EventArgs e)
        {
            TreeViewItem TVI = (TreeViewItem)sender;
            object tvItem = TVI.Tag;

            if (tvItem is ITreeViewItem)
            {
                //mPomAllElementsPage = new PomAllElementsPage();
                //DetailsFrame.Content = ((ITreeViewItem)tvItem).EditPage();
            }
            else
            {
                //DetailsFrame.Content = "View/Edit page is not available yet for the tree item '" + tvItem.GetType().Name + "'";
            }
        }

    }
}
