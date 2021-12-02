using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Ginger.UserControls;
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

namespace Ginger.ALM.MappedToALMWizard
{
    /// <summary>
    /// Interaction logic for TestStepMappingPage.xaml
    /// </summary>
    public partial class TestStepMappingPage : Page, IWizardPage
    {
        AddMappedToALMWizard mWizard;
        public TestStepMappingPage()
        {
            InitializeComponent();
            Bind();
        }

        private void Bind()
        {
            // Bind Test Cases Grid
            xMapActivityGroupTestCaseGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(TestCaseGridParam.ActivityGroup), Header = "Activity Group", WidthWeight = 25, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(TestCaseGridParam.TestCase), Header = "Test Case", WidthWeight = 25, AllowSorting = true });
            GridViewDef mRegularView = new GridViewDef(eGridView.RegularView.ToString());
            //xMapActivityGroupTestCaseGrid.AddToolbarTool(eImageType.MapSigns, "Remove elements from mapped list", new RoutedEventHandler(RemoveElementsToMappedBtnClicked));
            xMapActivityGroupTestCaseGrid.SetAllColumnsDefaultView(view);
            xMapActivityGroupTestCaseGrid.InitViewItems();

            // Bind Test Steps Grid
            xMapActivityTestStepGrid.SetTitleLightStyle = true;
            GridViewDef view2 = new GridViewDef(GridViewDef.DefaultViewName);
            view2.GridColsView = new ObservableList<GridColView>();
            view2.GridColsView.Add(new GridColView() { Field = nameof(TestStepGridParam.Activity), Header = "Activity", WidthWeight = 25, AllowSorting = true });
            view2.GridColsView.Add(new GridColView() { Field = nameof(TestStepGridParam.TestStep), Header = "Test Step", WidthWeight = 25, AllowSorting = true });
            GridViewDef mRegularView2 = new GridViewDef(eGridView.RegularView.ToString());
            xMapActivityTestStepGrid.AddToolbarTool(eImageType.MapSigns, "Remove elements from mapped list", new RoutedEventHandler(RemoveElementsToMappedBtnClicked));
            xMapActivityTestStepGrid.SetAllColumnsDefaultView(view);
            xMapActivityTestStepGrid.InitViewItems();
        }
        public enum eGridView
        {
            RegularView,
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var data = new Test { Test1 = gingerAGCbx.SelectedValue.ToString(), Test2 = almTCCbx.SelectedValue.ToString() };
            TestStepGridParam data2 = new TestStepGridParam { Activity = xGingerActivityCbx.SelectedValue.ToString(), TestStep = xAlmTestStepCbx.SelectedValue.ToString() };
            ObservableList<TestStepGridParam> testStepGridParams = new ObservableList<TestStepGridParam>();
            testStepGridParams.Add(data2);
            xMapActivityTestStepGrid.DataSourceList = testStepGridParams;
            xAlmTestStepCbx.Items.RemoveAt(xAlmTestStepCbx.SelectedIndex);
            xGingerActivityCbx.Items.RemoveAt(xGingerActivityCbx.SelectedIndex);
            //DataGridTest.Items.Add(data);
            if (xGingerActivityCbx.Items.Count > 0)
            {
                xGingerActivityCbx.SelectedItem = xGingerActivityCbx.Items[0];
            }
            if (xAlmTestStepCbx.Items.Count > 0)
            {
                xAlmTestStepCbx.SelectedItem = xAlmTestStepCbx.Items[0];
            }
        }
        private void RemoveElementsToMappedBtnClicked(object sender, RoutedEventArgs e)
        {
            ObservableList<TestStepGridParam> ItemsToRemoveList = new ObservableList<TestStepGridParam>();
            ItemsToRemoveList.Add((TestStepGridParam)xMapActivityTestStepGrid.Grid.SelectedItems[0]);
            if (ItemsToRemoveList != null && ItemsToRemoveList.Count > 0)
            {
                //remove
                xMapActivityTestStepGrid.btnDelete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                //Add
                foreach (TestStepGridParam EI in ItemsToRemoveList)
                {
                    xGingerActivityCbx.Items.Add(EI.Activity);
                    xAlmTestStepCbx.Items.Add(EI.TestStep);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
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

                    break;
            }
        }
    }
    public class TestTS
    {
        public string Test1 { get; set; }
        public string Test2 { get; set; }
    }
}
