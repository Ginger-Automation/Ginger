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

        public APINavPage(Context context)
        {
            InitializeComponent();

            mContext = context;

            AppApiModelsFolderTreeItem mAPIsRoot = new AppApiModelsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>());
            mItemTypeRootNode = mAPIsRoot;
            mAPIPage = new SingleItemTreeViewSelectionPage("API Models", eImageType.APIModel, mItemTypeRootNode, SingleItemTreeViewSelectionPage.eItemSelectionType.Multi, true,
                                        new Tuple<string, string>(nameof(ApplicationAPIModel.TargetApplicationKey) + "." + nameof(ApplicationAPIModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication),
                                            UCTreeView.eFilteroperationType.Equals);

            mItemTypeRootNode.SetTools(mAPIPage.xTreeView);
            mAPIPage.xTreeView.SetTopToolBarTools(mAPIsRoot.SaveAllTreeFolderItemsHandler, mAPIsRoot.AddAPIModelFromDocument);

            mContext.PropertyChanged += MContext_PropertyChanged;
            xAPIFrame.Content = mAPIPage;
        }

        private void MContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(mContext.Activity) || e.PropertyName is nameof(mContext.Target))
            {
                UpdateAPITree();
            }
        }

        private void UpdateAPITree()
        {
            mAPIPage.xTreeView.Tree.TreeNodesFilterByField = new Tuple<string, string>(nameof(ApplicationAPIModel.TargetApplicationKey) 
                                                                + "." + nameof(ApplicationAPIModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication);
            mAPIPage.xTreeView.Tree.FilterType = UCTreeView.eFilteroperationType.Equals;
            mAPIPage.xTreeView.Tree.RefresTreeNodeChildrens(mItemTypeRootNode);
        }
    }
}
