using GingerCore;
using System.Windows.Controls;
using GingerWPF.BusinessFlowsLib;
namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for BusinessFlowComparePage.xaml
    /// </summary>
    public partial class BusinessFlowComparePage : Page
    {
        BusinessFlow mLocalFlow;
        BusinessFlow mServerFlow;
        public BusinessFlowComparePage()
        {
            InitializeComponent();
        }
        public BusinessFlowComparePage(BusinessFlow localFlow, BusinessFlow serverFlow)
        {
            InitializeComponent();
            mLocalFlow = localFlow;
            mServerFlow = serverFlow;
            PrepareLocalAndServerBusinessFlowView();
        }

        private void PrepareLocalAndServerBusinessFlowView()
        {
            BusinessFlowViewPage localBfView = new BusinessFlowViewPage(mLocalFlow, new Amdocs.Ginger.Common.Context(), General.eRIPageViewMode.View);
            BusinessFlowViewPage serverBfView = new BusinessFlowViewPage(mServerFlow, new Amdocs.Ginger.Common.Context(), General.eRIPageViewMode.View);
            xLocalBusinessFlowFrame.Content = localBfView;
            xServerBusinessFlowFrame.Content = serverBfView;
        }
    }
}
