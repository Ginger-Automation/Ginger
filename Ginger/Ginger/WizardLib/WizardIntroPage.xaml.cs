using GingerCore;
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
using System.Windows.Resources;
using System.Windows.Shapes;

namespace Ginger.WizardLib
{
    /// <summary>
    /// Interaction logic for WizardIntroPage.xaml
    /// </summary>
    public partial class WizardIntroPage : Page, IWizardPage
    {
        public WizardIntroPage(string location)
        {
            InitializeComponent();

            RenderMarkDown(location);
            
        }

        private void RenderMarkDown(string location)
        {
            // For now we have a quick and dirty MD renderer, need to search Nuget or write the full md creator.
            string txt = GetMarkDown(location);
            string[] lines = txt.Split(Environment.NewLine.ToCharArray());
            TextBlockHelper TBH = new TextBlockHelper(xIntroTextBlock);
            TBH.AddLineBreak();
            SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("@Skin1_ColorA")).ToString());
            foreach (string line in lines)
            {
                string l = line.Replace("```$GingerCore.eTermResKey.BusinessFlow$```", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));

                //Remove BOM (byte-order mark) Char if exists
                if (l.StartsWith(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()), StringComparison.Ordinal))
                {
                    l = l.Replace(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()), "");
                }

                if (l.StartsWith("### "))
                {
                    l = l.Substring(4);
                    TBH.AddFormattedText(l, foregroundColor, true);
                }
                else if(l.Length == 0)
                {
                    TBH.AddLineBreak();
                }
                else
                {
                    TBH.AddFormattedText(l, foregroundColor);
                }
            }
        }

        public string GetMarkDown(string location)
        {
            // Add try catch - add make sure md marked as resource
            Uri uri = new Uri("pack://application:,,,/Ginger;component" + location);
            StreamResourceInfo MarkDown = Application.GetResourceStream(uri);
            byte[] b = new byte[MarkDown.Stream.Length];
            MarkDown.Stream.Read(b,0, (int)MarkDown.Stream.Length);
            string txt = System.Text.Encoding.UTF8.GetString(b);
            return txt;
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            // nothing to do
        }
    }
}
