using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.BusinessFlowPages;
using Ginger.UserControls;
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

namespace Amdocs.Ginger.UserControls
{
    /// <summary>
    /// Interaction logic for UCElementDetails.xaml
    /// </summary>
    public partial class UCElementDetails : UserControl
    {
        public UCElementDetails()
        {
            InitializeComponent();

            ActUIElement UIElementAction = new ActUIElement();
            ActUIElementEditPage ActEditPage = new ActUIElementEditPage(UIElementAction);
            xActUIPageFrame.Content = ActEditPage;
        }
    }
}
