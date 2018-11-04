#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
            SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$Color_DarkBlue")).ToString());
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
