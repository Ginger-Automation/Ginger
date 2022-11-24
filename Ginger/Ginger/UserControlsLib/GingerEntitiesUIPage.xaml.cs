using amdocs.ginger.GingerCoreNET;
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
    /// Interaction logic for GingerUIPage.xaml
    /// </summary>
    public abstract class GingerEntitiesUIPage : Page
    {
        protected RepositoryItemBase currentItem { get; set; }
        public GingerEntitiesUIPage()
        {
            IsVisibleChanged += IsVisibleChangedHandler;
        }

        protected virtual void IsVisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (currentItem != null)
            {
                if((bool)e.NewValue)
                {
                    WorkSpace.Instance.CurrentSelectedItem = currentItem;
                }
                else
                {
                    if(WorkSpace.Instance.CurrentSelectedItem == currentItem) 
                    {
                        WorkSpace.Instance.CurrentSelectedItem = null;
                    }
                }
            }
        }
    }
}
