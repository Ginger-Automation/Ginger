using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Amdocs.Ginger.Repository;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Selenium;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.ALM.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActAccessibilityTestingEditPage.xaml
    /// </summary>
    public partial class ActAccessibilityTestingEditPage : Page
    {
        private ActAccessibilityTesting mAct;
        PlatformInfoBase mPlatform;
        string mExistingPOMAndElementGuidString = null;
        public ActAccessibilityTestingEditPage(ActAccessibilityTesting act)
        {
            InitializeComponent();
            mAct = act;
            if (act.Platform == ePlatformType.NA)
            {
                act.Platform = GetActionPlatform();
            }
            mPlatform = PlatformInfoBase.GetPlatformImpl(act.Platform);
            List<eLocateBy> LocateByList = mPlatform.GetPlatformUIElementLocatorsList();
            xElementLocateByComboBox.BindControl(mAct, Act.Fields.LocateBy, LocateByList);
            xLocateValueVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(Act.Fields.LocateValue));

            List<ActAccessibilityTesting.eTags> supportedControlActions = GetStandardTags();
            //bind controls
            GingerCore.General.FillComboFromEnumObj(xStandardComboBox, mAct.Standard, supportedControlActions.Cast<object>().ToList());
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStandardComboBox, ComboBox.SelectedValueProperty, mAct, ActAccessibilityTesting.Fields.Standard);

            ValueUC.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam("Value"));
            xLocateValueVE.BindControl(Context.GetAsContext(mAct.Context), mAct, Act.Fields.LocateValue);
            xTargetRadioButton.Init(typeof(ActAccessibilityTesting.eTarget), xTargetRadioButtonPnl, mAct.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Target, ActAccessibilityTesting.eTarget.Page.ToString()), TargetRadioButton_Clicked);
            if ((act.GetInputParamValue(ActAccessibilityTesting.Fields.Target) == ActAccessibilityTesting.eTarget.Element.ToString()))
            {
                xValueLabel.Visibility = System.Windows.Visibility.Collapsed;
                ValueUC.Visibility = System.Windows.Visibility.Collapsed;
                xLocateByAndValuePanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                xValueLabel.Visibility = System.Windows.Visibility.Visible;
                ValueUC.Visibility = System.Windows.Visibility.Visible;
                xLocateByAndValuePanel.Visibility = System.Windows.Visibility.Collapsed;
                xValueLabel.Content = "URL:";
            }
            SetLocateValueFrame();
        }

        private ePlatformType GetActionPlatform()
        {
            ePlatformType platform;
            if (mAct.Context != null && (Context.GetAsContext(mAct.Context)).BusinessFlow != null)
            {
                string targetapp = (Context.GetAsContext(mAct.Context)).BusinessFlow.CurrentActivity.TargetApplication;
                platform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
            }
            else
            {
                platform = WorkSpace.Instance.Solution.ApplicationPlatforms[0].Platform;
            }
            return platform;
        }

        private void TargetRadioButton_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            RadioButton rbSender = sender as RadioButton;

            if (rbSender.Content.ToString() == ActAccessibilityTesting.eTarget.Page.ToString())
            {
                xValueLabel.Visibility = System.Windows.Visibility.Visible;
                ValueUC.Visibility = System.Windows.Visibility.Visible;
                xLocateByAndValuePanel.Visibility = System.Windows.Visibility.Collapsed;
                xValueLabel.Content = "URL:";
            }
            else
            {
                xValueLabel.Visibility = System.Windows.Visibility.Collapsed;
                ValueUC.Visibility = System.Windows.Visibility.Collapsed;
                xLocateByAndValuePanel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void SetLocateValueFrame()
        {
            if (xElementLocateByComboBox.SelectedItem == null)
            {
                xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            else
            {
                mAct.LocateBy = (eLocateBy)((ComboEnumItem)xElementLocateByComboBox.SelectedItem).Value;
            }

            eLocateBy SelectedLocType = (eLocateBy)((ComboEnumItem)xElementLocateByComboBox.SelectedItem).Value;

            switch (SelectedLocType)
            {
                case eLocateBy.POMElement:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Collapsed;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Visible;
                    Page p = new LocateByPOMElementPage(Context.GetAsContext(mAct.Context), null, null, mAct, nameof(ActBrowserElement.LocateValue));
                    xLocateValueEditFrame.ClearAndSetContent(p);
                    break;
                default:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }


        private void ElementLocateByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetLocateValueControls();
        }

        private void SetLocateValueControls()
        {
            if (xElementLocateByComboBox.SelectedItem == null)
            {
                xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            else
            {
                mAct.LocateBy = (eLocateBy)((ComboEnumItem)xElementLocateByComboBox.SelectedItem).Value;
            }

            eLocateBy SelectedLocType = (eLocateBy)((ComboEnumItem)xElementLocateByComboBox.SelectedItem).Value;

            switch (SelectedLocType)
            {
                case eLocateBy.POMElement:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Collapsed;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Visible;
                    Page p = new LocateByPOMElementPage(Context.GetAsContext(mAct.Context), null, null, mAct, nameof(ActAccessibilityTesting.LocateValue));
                    xLocateValueEditFrame.ClearAndSetContent(p);
                    break;
                default:
                    xLocateValueVE.Visibility = System.Windows.Visibility.Visible;
                    xLocateValueEditFrame.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }


        public List<ActAccessibilityTesting.eTags> GetStandardTags()
        {
            List<ActAccessibilityTesting.eTags> StandardTagList = new List<ActAccessibilityTesting.eTags>();

            StandardTagList.Add(ActAccessibilityTesting.eTags.wcag2a);
            StandardTagList.Add(ActAccessibilityTesting.eTags.wcag2aa);
            StandardTagList.Add(ActAccessibilityTesting.eTags.wcag21a);
            StandardTagList.Add(ActAccessibilityTesting.eTags.wcag21aa);
            return StandardTagList;
        }

    }
}
