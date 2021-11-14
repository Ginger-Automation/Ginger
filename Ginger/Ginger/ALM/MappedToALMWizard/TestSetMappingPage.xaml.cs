using Ginger.ALM.ZephyrEnt;
using Ginger.ALM.ZephyrEnt.TreeViewItems;
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
using static Ginger.ALM.ZephyrEnt.ZephyrEntPlanningExplorerPage;

namespace Ginger.ALM.MappedToALMWizard
{
    /// <summary>
    /// Interaction logic for TestSetMappingPage.xaml
    /// </summary>
    public partial class TestSetMappingPage : Page, IWizardPage
    {
        AddMappedToALMWizard mWizard;
        public TestSetMappingPage()
        {
            InitializeComponent();
            load_frame.Content = new ZephyrEntPlanningExplorerPage(eExplorerTestPlanningPageUsageType.Import, "");
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizard = (AddMappedToALMWizard)WizardEventArgs.Wizard;
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    if (mWizard.ActivitiesGroupPreSet == false)
                    {
                        //xGroupComboBox.ItemsSource = mWizard.Context.BusinessFlow.ActivitiesGroups;
                        //xGroupComboBox.DisplayMemberPath = nameof(ActivitiesGroup.Name);
                        //BindingHandler.ObjFieldBinding(xGroupComboBox, ComboBox.SelectedItemProperty, mWizard, nameof(AddActivityWizard.ParentActivitiesGroup));
                    }
                    else
                    {
                        //xGroupPanel.Visibility = Visibility.Collapsed;
                    }
                    //xRegularType.IsChecked = true;
                    break;
            }
        }
    }
}
