using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.Help;
using GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems;
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
    /// Interaction logic for APINavPage.xaml
    /// </summary>
    public partial class APINavPage : Page
    {
        Context mContext;
        ITreeViewItem mItemTypeRootNode;
        SingleItemTreeViewSelectionPage mAPIPage;
        //public APINavPage(Context context, string itemTypeName, eImageType itemTypeIcon, ITreeViewItem itemTypeRootNode, RoutedEventHandler saveAllHandler = null, RoutedEventHandler addHandler = null, EventHandler treeItemDoubleClickHandler = null)
        //{
        //    InitializeComponent();

        //    mContext = context;
        //    mItemTypeRootNode = itemTypeRootNode;
        //    GingerHelpProvider.SetHelpString(this, itemTypeName.TrimEnd(new char[] { 's' }));

        //    xTreeView.TreeTitle = itemTypeName;
        //    xTreeView.TreeIcon = itemTypeIcon;

        //    mContext.PropertyChanged += MContext_PropertyChanged;

        //    xTreeView.Tree.TreeNodesFilterByField = new Tuple<string, string>(nameof(ApplicationAPIModel.TargetApplicationKey) + "." + nameof(ApplicationAPIModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication);
        //    xTreeView.Tree.FilterType = UCTreeView.eFilteroperationType.Equals;
        //    TreeViewItem r = xTreeView.Tree.AddItem(itemTypeRootNode);

        //    r.IsExpanded = true;

        //    itemTypeRootNode.SetTools(xTreeView);
        //    xTreeView.SetTopToolBarTools(saveAllHandler, addHandler);
        //}

        public APINavPage(Context context)
        {
            InitializeComponent();

            mContext = context;
            mItemTypeRootNode = new AppApiModelsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>());
            mAPIPage = new SingleItemTreeViewSelectionPage("API Models", eImageType.APIModel, mItemTypeRootNode, SingleItemTreeViewSelectionPage.eItemSelectionType.Multi, true,
                                        new Tuple<string, string>(nameof(ApplicationAPIModel.TargetApplicationKey) + "." + nameof(ApplicationAPIModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication),
                                            UCTreeView.eFilteroperationType.Equals);

            mContext.PropertyChanged -= MContext_PropertyChanged;
            mContext.PropertyChanged += MContext_PropertyChanged;
            xAPIFrame.Content = mAPIPage;
        }

        private void MContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(mContext.BusinessFlow) || e.PropertyName is nameof(mContext.Activity) || e.PropertyName is nameof(mContext.Target))
            {
                UpdateAPITree();
            }
        }

        private void UpdateAPITree()
        {
            if (mContext.BusinessFlow.CurrentActivity == null)
                mContext.BusinessFlow.CurrentActivity = mContext.BusinessFlow.Activities[0];

            mAPIPage.xTreeView.Tree.TreeNodesFilterByField = new Tuple<string, string>(nameof(ApplicationAPIModel.TargetApplicationKey) + "." + nameof(ApplicationAPIModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication);
            mAPIPage.xTreeView.Tree.FilterType = UCTreeView.eFilteroperationType.Equals;
            mAPIPage.xTreeView.Tree.RefresTreeNodeChildrens(mItemTypeRootNode);
        }
    }
}
