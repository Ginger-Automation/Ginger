using System.Data;
using System.Windows.Controls;

namespace Ginger.UserControlsLib.UCDataGridView
{
    /// <summary>
    /// Interaction logic for GridPage.xaml
    /// </summary>
    public partial class ucDataGrid : UserControl
    {
        public ucDataGrid()
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
