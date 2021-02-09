using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.BusinessFlowPages;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions.Common;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
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

namespace Ginger
{
    public class GridFilters
    {
    }

    /// <summary>
    /// Interaction logic for UCElementDetails.xaml
    /// </summary>
    public partial class UCElementDetails : UserControl
    {
        public Agent mAgent { get; set; }
        public bool ShowTestBtn { get; set; }

        PlatformInfoBase mPlatform { get; set; }

        public bool ShowHelpColumn { get; set; }

        private ElementInfo mSelectedElement;
        public ElementInfo SelectedElement 
        {
            get
            {
                return mSelectedElement;
            }
            set
            {
                if(value != mSelectedElement)
                {
                    mSelectedElement = value;
                    RefreshPropertiesAndLocators();
                }
            }
        }

        public bool ShowAutoLearnedColumn { get; set; }

        public bool ShowActionTab
        {
            get
            {
                return xAddActionTab.Visibility == Visibility.Visible;
            }
            set
            {
                if (value == true)
                {
                    xAddActionTab.Visibility = Visibility.Visible;
                }
                else
                {
                    xAddActionTab.Visibility = Visibility.Collapsed;
                }
            }
        }

        public UCElementDetails()
        {
            InitializeComponent();

            ActUIElement UIElementAction = new ActUIElement();
            ActUIElementEditPage ActEditPage = new ActUIElementEditPage(UIElementAction);
            xActUIPageFrame.Content = ActEditPage;
        }

        private void ElementDetailsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (xElementDetailsTabs.SelectedItem != null)
                {
                    foreach (TabItem tab in xElementDetailsTabs.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (xElementDetailsTabs.SelectedItem == tab)
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$Color_DarkBlue");

                                ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in POM Edit Page tabs style", ex);
            }
        }

        public enum eViewPage
        {
            POM, Explorer
        }

        private void xRunActBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void xAddActBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        internal void InitGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 65 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, ReadOnly = true });

            List<ComboEnumItem> locateByList = GetPlatformLocatByList();

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, });

            xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xLocatorsGrid.InitViewItems();
        }

        private List<ComboEnumItem> GetPlatformLocatByList()
        {
            List<ComboEnumItem> locateByComboItemList = new List<ComboEnumItem>();

            List<eLocateBy> platformLocateByList = mPlatform.GetPlatformUIElementLocatorsList();

            foreach (var locateBy in platformLocateByList)
            {
                if (!locateBy.Equals(eLocateBy.POMElement))
                {
                    ComboEnumItem comboEnumItem = new ComboEnumItem();
                    comboEnumItem.text = GingerCore.General.GetEnumValueDescription(typeof(eLocateBy), locateBy);
                    comboEnumItem.Value = locateBy;
                    locateByComboItemList.Add(comboEnumItem);
                }
            }

            return locateByComboItemList;
        }

        public void RefreshPropertiesAndLocators()
        {
            if(mSelectedElement != null)
            {
                if (mSelectedElement.Properties == null || mSelectedElement.Properties.Count == 0)
                    mSelectedElement.Properties = mSelectedElement.GetElementProperties();

                if (mSelectedElement.Locators == null || mSelectedElement.Locators.Count == 0)
                    mSelectedElement.Locators = mSelectedElement.GetElementLocators();

                xPropertiesGrid.DataSourceList = mSelectedElement.Properties;
                xLocatorsGrid.DataSourceList = mSelectedElement.Locators;

                Dispatcher.Invoke(() =>
                {
                    xPropertiesTextBlock.Text = string.Format("Properties ({0})", mSelectedElement.Properties.Count);
                    xLocatorsTextBlock.Text = string.Format("Locators ({0})", mSelectedElement.Locators.Count);
                });
            }
        }
    }
}
