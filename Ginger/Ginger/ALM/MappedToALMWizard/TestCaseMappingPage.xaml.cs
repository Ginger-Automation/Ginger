using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Ginger.UserControls;
using GingerCore;
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
using Button = System.Windows.Controls.Button;

namespace Ginger.ALM.MappedToALMWizard
{
    /// <summary>
    /// Interaction logic for TestCaseMappingPage.xaml
    /// </summary>
    public partial class TestCaseMappingPage : Page, IWizardPage
    {
        AddMappedToALMWizard mWizard;
        BusinessFlow BusinessFlow { get; set; }
        ObservableList<TestCaseGridParam> testCaseGridParams = new ObservableList<TestCaseGridParam>();
        ObservableList<TestCaseGridParam> MappedTestCasesList = new ObservableList<TestCaseGridParam>();
        public TestCaseMappingPage(BusinessFlow businessFlow)
        {
            InitializeComponent();
            BusinessFlow = businessFlow;
            xGingerBFName.Content = "Ginger: " + businessFlow.Name;
            Bind();
            SetElementsGridView();
        }

        private void SetElementsGridView()
        {
            xMapActivityGroupTestCaseGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            //view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            view.GridColsView.Add(new GridColView() { Field = nameof(TestCaseGridParam.ActivityGroup), Header = "Activity Group", WidthWeight = 25, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(TestCaseGridParam.TestCase), Header = "Test Case", WidthWeight = 25, AllowSorting = true });
            GridViewDef mRegularView = new GridViewDef(eGridView.RegularView.ToString());
            //mRegularView.GridColsView = new ObservableList<GridColView>();
            //mRegularView.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.StatusIcon), Visible = false });
            xMapActivityGroupTestCaseGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteTestCaseHandler));
            xMapActivityGroupTestCaseGrid.AddToolbarTool(eImageType.MapSigns, "Remove elements from mapped list", new RoutedEventHandler(RemoveElementsToMappedBtnClicked));
            //xMapActivityGroupTestCaseGrid.AddCustomView(mRegularView);
            xMapActivityGroupTestCaseGrid.SetAllColumnsDefaultView(view);
            xMapActivityGroupTestCaseGrid.InitViewItems();
        }
        private void RemoveElementsToMappedBtnClicked(object sender, RoutedEventArgs e)
        {
            ObservableList<TestCaseGridParam> ItemsToRemoveList = new ObservableList<TestCaseGridParam>();
            ItemsToRemoveList.Add((TestCaseGridParam)xMapActivityGroupTestCaseGrid.Grid.SelectedItems[0]);
            if (ItemsToRemoveList != null && ItemsToRemoveList.Count > 0)
            {
                //remove
                //xMapActivityGroupTestCaseGrid.Grid.Items.RemoveAt(xMapActivityGroupTestCaseGrid.Grid.SelectedIndex);
                xMapActivityGroupTestCaseGrid.btnDelete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                //for (int indx = 0; indx < ItemsToRemoveList.Count; indx++)
                //{
                //    //mPOM.MappedUIElements.Remove(ItemsToRemoveList[indx]);
                //    xMapActivityGroupTestCaseGrid.Grid.Items.RemoveAt(xMapActivityGroupTestCaseGrid.Grid.SelectedIndex);
                //}
                //add
                foreach (TestCaseGridParam EI in ItemsToRemoveList)
                {
                    almTCCbx.Items.Add(EI.TestCase);
                    gingerAGCbx.Items.Add(EI.ActivityGroup);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }
        private void DeleteTestCaseHandler(object sender, RoutedEventArgs e)
        {
            string data = "hello";
        }

        public enum eGridView
        {
            RegularView,
        }
        private void Bind()
        {
            if (mWizard != null)
            {
                gingerAGCbx.Items.Clear();
                BusinessFlow.ActivitiesGroups.ToList().ForEach(ag => gingerAGCbx.Items.Add(ag.Name));
                if(gingerAGCbx.Items.Count > 0)
                {
                    gingerAGCbx.SelectedItem = gingerAGCbx.Items[0];
                }
                almTCCbx.Items.Clear();
                mWizard.AlmTestSetDetails.Tests.ForEach(tc => almTCCbx.Items.Add(tc.TestName));
                if (almTCCbx.Items.Count > 0)
                {
                    almTCCbx.SelectedItem = almTCCbx.Items[0];
                }
            }
            //List<ElementInfo> ItemsToRemoveList = 
            ElementInfo elementInfo = new ElementInfo() { ElementName = "add to" };

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
                    //if (mWizard.ActivitiesGroupPreSet == false)
                    //{
                    //    //xGroupComboBox.ItemsSource = mWizard.Context.BusinessFlow.ActivitiesGroups;
                    //    //xGroupComboBox.DisplayMemberPath = nameof(ActivitiesGroup.Name);
                    //    //BindingHandler.ObjFieldBinding(xGroupComboBox, ComboBox.SelectedItemProperty, mWizard, nameof(AddActivityWizard.ParentActivitiesGroup));
                    //}
                    //else
                    //{
                    //    //xGroupPanel.Visibility = Visibility.Collapsed;
                    //}
                    //xRegularType.IsChecked = true;
                    break;
                case EventType.Active:
                    //xALMTSName.
                    xALMTSName.Content = "ALM: " + mWizard.AlmTestSetDetails.TestSetName;
                    Bind();
                    break;
                case EventType.LeavingForNextPage:
                    xMapActivityGroupTestCaseGrid.Grid.Items.Cast<TestCaseGridParam>()
                        .ToList().ForEach(ts => MappedTestCasesList.Add(ts));
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var data = new Test { Test1 = gingerAGCbx.SelectedValue.ToString(), Test2 = almTCCbx.SelectedValue.ToString() };
            TestCaseGridParam data2 = new TestCaseGridParam { ActivityGroup = gingerAGCbx.SelectedValue.ToString(), TestCase = almTCCbx.SelectedValue.ToString() };
            
            testCaseGridParams.Add(data2);
            xMapActivityGroupTestCaseGrid.DataSourceList = testCaseGridParams;
            almTCCbx.Items.RemoveAt(almTCCbx.SelectedIndex);
            gingerAGCbx.Items.RemoveAt(gingerAGCbx.SelectedIndex);
            //DataGridTest.Items.Add(data);
            if (gingerAGCbx.Items.Count > 0)
            {
                gingerAGCbx.SelectedItem = gingerAGCbx.Items[0];
            }
            if (almTCCbx.Items.Count > 0)
            {
                almTCCbx.SelectedItem = almTCCbx.Items[0];
            }
        }

        private void removeMapBtn_Click(object sender, RoutedEventArgs e)
        {
            //var selectedItem = DataGridTest.SelectedItem;
            //if (selectedItem != null)
            //{
            //    DataGridTest.Items.Remove(selectedItem);
            //}
        }
    }
    public class Test
    {
        public string Test1 { get; set; }
        public string Test2 { get; set; }
    }
}
