using System.Data;
using System.Windows.Controls;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for GridPage.xaml
    /// </summary>
    public partial class GridPage : UserControl
    {
        public GridPage()
        {
            InitializeComponent();
        }

        DataTable mExcelData;
        public DataTable ExcelData
        {
            get
            {
                return mExcelData;
            }
            set
            {
                mExcelData = value;
                xExcelDataGrid.ItemsSource = mExcelData.AsDataView();
            }
        }
    }
}
