using GingerCore.Helpers;
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

namespace Ginger.Environments.GingerOpsEnvWizardLib
{
    /// <summary>
    /// Interaction logic for GingerOpsIntroPage.xaml
    /// </summary>
    public partial class GingerOpsIntroPage : Page, IWizardPage
    {
        public GingerOpsIntroPage()
        {
            InitializeComponent();

            TextBlockHelper TBH = new TextBlockHelper(xGOpsIntroTxtBlock);

            SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$PrimaryColor_Black")).ToString());
            //Application info
            TBH.AddFormattedText("For What GingerOps is Needed?", foregroundColor, true);
            TBH.AddLineBreak();
            TBH.AddFormattedText("Environments are needed for storing data like: login credentials, DB/Unix servers connection information and similar details which changed per testing environments.", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddFormattedText("Idea is that you will be able to map your automation flow to a generic detail of the Environment and then control the actual used data by simply changing the selected Environment to execute with", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddFormattedText("How to Integrate Environment Details in Automation Flow?", foregroundColor, true);
            TBH.AddLineBreak();
            TBH.AddFormattedText("Use the Value Expression editor for adding Environment value expression in any input field which support it", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddFormattedText("Important to Know:", foregroundColor, true);
            TBH.AddLineBreak();
            TBH.AddFormattedText("All configured Environments must have identical Applications/Parameters/DB/Unix names- only actual end point data value supposed to be different for each Environment ", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddLineBreak();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
        }
    }
}
