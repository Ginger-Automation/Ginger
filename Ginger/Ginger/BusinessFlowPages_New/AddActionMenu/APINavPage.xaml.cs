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
            mContext.Activity.PropertyChanged += Activity_PropertyChanged;

            xTreeView.Tree.TreeNodesFilterByField = new Tuple<string, string>(nameof(ApplicationAPIModel.TargetApplicationKey) + "." + nameof(ApplicationAPIModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication);
            xTreeView.Tree.FilterType = UCTreeView.eFilteroperationType.Equals;
            TreeViewItem r = xTreeView.Tree.AddItem(itemTypeRootNode);

            r.IsExpanded = true;

            itemTypeRootNode.SetTools(xTreeView);
            xTreeView.SetTopToolBarTools(saveAllHandler, addHandler);

            xTreeView.Tree.ItemSelected += MainTreeView_ItemSelected;
            SetElementsGridView();

            //if (treeItemDoubleClickHandler != null)
            //{
            //    xTreeView.Tree.ItemDoubleClick += treeItemDoubleClickHandler;
            //}
        }

        private void MContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            mContext.Activity.PropertyChanged -= Activity_PropertyChanged;
            mContext.Activity.PropertyChanged += Activity_PropertyChanged;
            if (e.PropertyName is nameof(mContext.BusinessFlow) || e.PropertyName is nameof(mContext.Activity))
            {
                UpdateAPITree();
            }
        }

        private void UpdateAPITree()
        {
            xTreeView.Tree.TreeNodesFilterByField = new Tuple<string, string>(nameof(ApplicationAPIModel.TargetApplicationKey) + "." + nameof(ApplicationAPIModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication);
            xTreeView.Tree.FilterType = UCTreeView.eFilteroperationType.Equals;
            xTreeView.Tree.RefresTreeNodeChildrens(mItemTypeRootNode);
        }

        private void Activity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(mContext.Activity.TargetApplication))
            {
                UpdateAPITree();
            }
        }

        private void CurrentActivity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //xTreeView.Tree.TreeNodesFilterByField = new Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." + nameof(ApplicationPOMModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication);
            //xTreeView.Tree.FilterType = UCTreeView.eFilteroperationType.Equals;
        }

        private void MainTreeView_ItemSelected(object sender, EventArgs e)
        {
            //GridLength POMDetailsRegionHeight = new GridLength(400, GridUnitType.Star);
            //GridLength unloadedPOMDetailsHeight = new GridLength(0);

            //TreeViewItem TVI = (TreeViewItem)sender;
            //object tvItem = TVI.Tag;
            //ITreeViewItem mPOMObj = tvItem as ITreeViewItem;

            //ApplicationPOMModel mPOM = mPOMObj.NodeObject() as ApplicationPOMModel;
            //if (tvItem is ITreeViewItem)
            //{
            //    if (mPOM is ApplicationPOMModel)
            //    {
            //        if (xPOMDetails.Height.Value < POMDetailsRegionHeight.Value)
            //            xPOMDetails.Height = POMDetailsRegionHeight;

            //        xPOMItems.Height = new GridLength(400, GridUnitType.Auto);
            //        xMainElementsGrid.Visibility = Visibility.Visible;
            //        foreach (ElementInfo elem in mPOM.MappedUIElements)
            //        {
            //            elem.ParentGuid = mPOM.Guid;
            //        }
            //        xMainElementsGrid.DataSourceList = mPOM.MappedUIElements;
            //    }
            //    else
            //    {
            //        xMainElementsGrid.Visibility = Visibility.Collapsed;
            //        xPOMDetails.Height = unloadedPOMDetailsHeight;
            //        xPOMItems.Height = POMDetailsRegionHeight;
            //    }
            //    //ApplicationPOMModel appPOM = tvItem as ApplicationPOMModel
            //    //mPomAllElementsPage = new PomAllElementsPage(appPOM, this);
            //    //xPOMLDetailsFrame.Content = ((ITreeViewItem)tvItem).EditPage();
            //}
            //else
            //{
            //    //DetailsFrame.Content = "View/Edit page is not available yet for the tree item '" + tvItem.GetType().Name + "'";
            //}
        }

        private void SetElementsGridView()
        {
            //xMainElementsGrid.SetTitleLightStyle = true;
            //GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            //view.GridColsView = new ObservableList<GridColView>();

            //view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            //view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), Header = "Name", WidthWeight = 25, AllowSorting = true });

            //List<GingerCore.GeneralLib.ComboEnumItem> ElementTypeList = GingerCore.General.GetEnumValuesForCombo(typeof(eElementType));
            //view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnum), Header = "Type", WidthWeight = 15, AllowSorting = true, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = ElementTypeList });

            //view.GridColsView.Add(new GridColView() { Field = "", Header = "Highlight", WidthWeight = 10, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xHighlightButtonTemplate"] });
            ////view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, ReadOnly = true });
            //xMainElementsGrid.SetAllColumnsDefaultView(view);
            //xMainElementsGrid.InitViewItems();
            //xMainElementsGrid.ChangeGridView(eGridView.RegularView.ToString());

            //xMainElementsGrid.AddToolbarTool(eImageType.GoBack, "Add to Actions", new RoutedEventHandler(AddFromPOMNavPage));
            //xMainElementsGrid.Grid.SelectionChanged += Grid_SelectionChanged;
        }
    }
}
