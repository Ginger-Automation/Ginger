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
using System.Windows.Shapes;

namespace Ginger.ConflictResolve
{
    /// <summary>
    /// Interaction logic for TreeViewComparisonWindow.xaml
    /// </summary>
    public partial class TreeViewComparisonWindow : Window
    {
        public Frame Frame => xFrame;

        public TreeViewComparisonWindow()
        {
            InitializeComponent();
            
        }
    }
}
