using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
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
    /// Interaction logic for APINavPage.xaml
    /// </summary>
    public partial class APINavPage : Page
    {
        Context mContext;
        ITreeViewItem mItemTypeRootNode;

        public APINavPage(Context context, string itemTypeName, eImageType itemTypeIcon, ITreeViewItem itemTypeRootNode, RoutedEventHandler saveAllHandler = null, RoutedEventHandler addHandler = null, EventHandler treeItemDoubleClickHandler = null)
        {
            InitializeComponent();

            mContext = context;
            mItemTypeRootNode = itemTypeRootNode;
            GingerHelpProvider.SetHelpString(this, itemTypeName.TrimEnd(new char[] { 's' }));

            xTreeView.TreeTitle = itemTypeName;
            xTreeView.TreeIcon = itemTypeIcon;

            mContext.PropertyChanged += MContext_PropertyChanged;

            xTreeView.Tree.TreeNodesFilterByField = new Tuple<string, string>(nameof(ApplicationAPIModel.TargetApplicationKey) + "." + nameof(ApplicationAPIModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication);
            xTreeView.Tree.FilterType = UCTreeView.eFilteroperationType.Equals;
            TreeViewItem r = xTreeView.Tree.AddItem(itemTypeRootNode);

            r.IsExpanded = true;

            itemTypeRootNode.SetTools(xTreeView);
            xTreeView.SetTopToolBarTools(saveAllHandler, addHandler);
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

            xTreeView.Tree.TreeNodesFilterByField = new Tuple<string, string>(nameof(ApplicationAPIModel.TargetApplicationKey) + "." + nameof(ApplicationAPIModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication);
            xTreeView.Tree.FilterType = UCTreeView.eFilteroperationType.Equals;
            xTreeView.Tree.RefresTreeNodeChildrens(mItemTypeRootNode);
        }
    }
}
