using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.SolutionCategories;
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Ginger.UserControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.SolutionWindows
{
    public enum eSolutionCategoriesPageMode { OptionalValuesDefinition, ValuesSelection}
    /// <summary>
    /// Interaction logic for SolutionCategoriesPage.xaml
    /// </summary>
    public partial class SolutionCategoriesPage : Page
    {
        eSolutionCategoriesPageMode mPageMode;
        ObservableList<SolutionCategory> mSolutionCategories;
        ObservableList<SolutionCategoryDefinition> mCategoriesDefinitions;

        public SolutionCategoriesPage(eSolutionCategoriesPageMode mode, ObservableList<SolutionCategoryDefinition> categoriesDefinitions = null)
        {
            InitializeComponent();

            mPageMode = mode;
            mCategoriesDefinitions = categoriesDefinitions;
            mSolutionCategories = WorkSpace.Instance.Solution.SolutionCategories;

            InitGrid();
        }

        private void InitGrid()
        {
            //bool isFieldReadOnly = (mPageViewMode == Ginger.General.eRIPageViewMode.View);

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(SolutionCategory.Category), Header = "Category", ReadOnly = true, WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = nameof(SolutionCategory.CategoryOptionalValuesString), Header = "Optional Values", WidthWeight = 80, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 8, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["OpenEditLocalParamPossibleValuesPage"] });

            ModelParametersGrid.SetAllColumnsDefaultView(view);
            ModelParametersGrid.InitViewItems();

            if (ModelParametersGrid.Grid != null)
            {
                ModelParametersGrid.Grid.BeginningEdit += grdLocalParams_BeginningEdit;
                ModelParametersGrid.Grid.CellEditEnding += grdLocalParams_CellEditEnding;
            }

            ModelParametersGrid.DataSourceList = mApplicationModel.AppModelParameters;

            ModelParametersGrid.ShowTitle = Visibility.Collapsed;
            ModelParametersGrid.ShowRefresh = Visibility.Collapsed;
            ModelParametersGrid.ShowEdit = Visibility.Collapsed;

            if (isFieldReadOnly)
            {
                ModelParametersGrid.IsReadOnly = true;

                ModelParametersGrid.ShowUpDown = Visibility.Collapsed;
                ModelParametersGrid.ShowAdd = Visibility.Collapsed;
                ModelParametersGrid.ShowClearAll = Visibility.Collapsed;
                ModelParametersGrid.ShowDelete = Visibility.Collapsed;
                ModelParametersGrid.ShowCopyCutPast = Visibility.Collapsed;
                ModelParametersGrid.ShowSearch = Visibility.Collapsed;

                ModelParametersGrid.ShowCopy = Visibility.Visible;
            }
            else
            {
                ModelParametersGrid.Grid.CanUserDeleteRows = false;
                ModelParametersGrid.ShowUpDown = Visibility.Visible;
                ModelParametersGrid.ShowAdd = Visibility.Visible;
                ModelParametersGrid.ShowClearAll = Visibility.Visible;
                ModelParametersGrid.ShowDelete = Visibility.Visible;
                ModelParametersGrid.ShowCopyCutPast = Visibility.Visible;

                ModelParametersGrid.AddToolbarTool(eImageType.Merge, "Merge Selected Parameters", new RoutedEventHandler(MergeSelectedParams));
                ModelParametersGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddParamsRow));
                ModelParametersGrid.AddToolbarTool("@Upgrade_16x16.png", "Upload to Global Parameters", new RoutedEventHandler(UploadToGlobalParam));
                ModelParametersGrid.AddToolbarTool("@Import_16x16.png", "Import Optional Values For Parameters", new RoutedEventHandler(ImportOptionalValuesForParameters));

                ModelParametersGrid.SetbtnDeleteHandler(new RoutedEventHandler(DeleteParams_Clicked));
                ModelParametersGrid.SetbtnClearAllHandler(new RoutedEventHandler(ClearAllParams_Clicked));
                ModelParametersGrid.AddToolbarTool(eImageType.ExcelFile, "Export Parameters to Excel File", new RoutedEventHandler(ExportOptionalValuesForParameters));
                ModelParametersGrid.AddToolbarTool(eImageType.DataSource, "Export Parameters to DataSource", new RoutedEventHandler(ExportParametersToDataSource));
            }

            if (mPageViewMode == Ginger.General.eRIPageViewMode.Add)
            {
                ModelParametersGrid.ShowPaste = Visibility.Visible;
            }
        }

        private void OpenEditLocalPossibleValuesPageButton_Click(object sender, RoutedEventArgs e)
        {
            IParentOptionalValuesObject parObj = (IParentOptionalValuesObject)ModelParametersGrid.CurrentItem;
            ModelOptionalValuesPage MDPVP = new ModelOptionalValuesPage(parObj);
            MDPVP.ShowAsWindow();
        }
    }
}
