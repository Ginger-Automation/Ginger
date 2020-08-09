using Amdocs.Ginger.Repository;
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

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for ucShowItemID.xaml
    /// </summary>
    public partial class ucShowItemID : UserControl
    {
        RepositoryItemBase mRepoItem = null;

        public ucShowItemID()
        {
            InitializeComponent();

            InitView();
        }

        public void Init(RepositoryItemBase item)
        {
            mRepoItem = item;
            InitView();
        }

        private void InitView()
        {
            xShowIDBtn.Visibility = Visibility.Visible;
            xIDLbl.Visibility = Visibility.Collapsed;
            xCopyBtn.Visibility = Visibility.Collapsed;
        }

        private void xShowIDBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mRepoItem != null)
            {
                xIDLbl.Text = mRepoItem.Guid.ToString();
                xShowIDBtn.Visibility = Visibility.Collapsed;
                xIDLbl.Visibility = Visibility.Visible;
                xCopyBtn.Visibility = Visibility.Visible;
            }
        }

        private void xCopyBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(mRepoItem.Guid.ToString());
        }
    }
}
