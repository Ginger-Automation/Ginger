using amdocs.ginger.GingerCoreNET;
using GingerWPF.WizardLib;
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

namespace Ginger.DatabaseLib
{
    /// <summary>
    /// Interaction logic for AddDataBaseWizardPage.xaml
    /// </summary>
    public partial class AddDataBaseWizardPage : Page, IWizardPage
    {
        public AddDataBaseWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            var v = WorkSpace.Instance.PlugInsManager.GetDatabaseList();
            xDatabaseList.ItemsSource = v;
            //mWizard = (AddActivityWizard)WizardEventArgs.Wizard;
            //switch (WizardEventArgs.EventType)
            //{
            //    case EventType.Init:
            //        if (mWizard.ActivitiesGroupPreSet == false)
            //        {
            //            xGroupComboBox.ItemsSource = mWizard.Context.BusinessFlow.ActivitiesGroups;
            //            xGroupComboBox.DisplayMemberPath = nameof(ActivitiesGroup.Name);
            //            BindingHandler.ObjFieldBinding(xGroupComboBox, ComboBox.SelectedItemProperty, mWizard, nameof(AddActivityWizard.ParentActivitiesGroup));
            //        }
            //        else
            //        {
            //            xGroupPanel.Visibility = Visibility.Collapsed;
            //        }
            //        xRegularType.IsChecked = true;
            //        break;
            //}
        }
    }
}
