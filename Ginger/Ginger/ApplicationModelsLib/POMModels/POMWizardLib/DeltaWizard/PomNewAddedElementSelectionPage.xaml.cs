using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore;
using GingerCoreNET.Application_Models;
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

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    /// <summary>
    /// Interaction logic for PomNewAddedElementSelectionPage.xaml
    /// </summary>
    public partial class PomNewAddedElementSelectionPage : Page
    {
        private PomDeltaViewPage mPomDeltaViewPage;
        private Agent mAgent;
        private PomDeltaUtils mPomDeltaUtils;


        public PomNewAddedElementSelectionPage(ObservableList<DeltaElementInfo> deltaElementInfos, PomDeltaUtils pomDeltaUtils, string searchText)
        {
            InitializeComponent();
            mPomDeltaUtils = pomDeltaUtils;

            mAgent = mPomDeltaUtils.Agent;

            mPomDeltaViewPage = new PomDeltaViewPage(deltaElementInfos);
            mPomDeltaViewPage.SetAgent(mAgent);
            mPomDeltaViewPage.xMainElementsGrid.Grid.Columns[1].Visibility = Visibility.Collapsed;
            mPomDeltaViewPage.xMainElementsGrid.btnMarkAll.Visibility = Visibility.Collapsed;

            mPomDeltaViewPage.xMainElementsGrid.txtSearch.Text = searchText;
            xNewPomElementsPageFrame.Content = mPomDeltaViewPage;
        }

        internal DeltaElementInfo SelectedElement()
        {
            return  mPomDeltaViewPage.ShowAsWindow("Added Elements");
        }
    }
}
