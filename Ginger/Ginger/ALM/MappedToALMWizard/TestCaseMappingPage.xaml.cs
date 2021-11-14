using GingerCore.GeneralLib;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.ALM.MappedToALMWizard
{
    /// <summary>
    /// Interaction logic for TestCaseMappingPage.xaml
    /// </summary>
    public partial class TestCaseMappingPage : Page, IWizardPage
    {
        AddMappedToALMWizard mWizard;
        public TestCaseMappingPage()
        {
            InitializeComponent();
            Bind();
        }

        private void Bind()
        {
            gingerAGCbx.Items.Add("AG1");
            gingerAGCbx.Items.Add("AG2");
            gingerAGCbx.Items.Add("AG3");

            almTCCbx.Items.Add("TC1");
            almTCCbx.Items.Add("TC2");
            almTCCbx.Items.Add("TC3");
            almTCCbx.Items.Add("TC4");
        }

        public List<ComboItem> GeneratecomboBoxItemsList()
        {
            List<ComboItem> comboBoxItemsList = new List<ComboItem>();

            ComboItem CBI1 = new ComboItem();
            CBI1.text = "Value 1";
            CBI1.Value = "Value1";

            ComboItem CBI2 = new ComboItem();
            CBI2.text = "Value 2";
            CBI2.Value = "Value2";

            ComboItem CBI3 = new ComboItem();
            CBI3.text = "Value 3";
            CBI3.Value = "Value3";

            comboBoxItemsList.Add(CBI1);
            comboBoxItemsList.Add(CBI2);
            comboBoxItemsList.Add(CBI3);

            return comboBoxItemsList;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var data = new Test { Test1 = gingerAGCbx.SelectedValue.ToString(), Test2 = almTCCbx.SelectedValue.ToString() };

            DataGridTest.Items.Add(data);
        }

        private void removeMapBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DataGridTest.SelectedItem;
            if (selectedItem != null)
            {
                DataGridTest.Items.Remove(selectedItem);
            }
        }
    }
    public class Test
    {
        public string Test1 { get; set; }
        public string Test2 { get; set; }
    }
}
