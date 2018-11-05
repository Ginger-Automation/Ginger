using GingerWPFUnitTest.POMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GingerTest.POMs.Common
{
    public class POMButtons : GingerPOMBase
    {
        DependencyObject mDependencyObject;

        public POMButtons(DependencyObject dependencyObject)
        {
            mDependencyObject = dependencyObject;
        }

        public ButtonPOM this[string text]
        {
            get
            {                
                Button b = (Button)FindElementByText<Button>(mDependencyObject, text);
                if (b != null)
                {
                    ButtonPOM buttonPOM = new ButtonPOM(b);
                    return buttonPOM;
                }
                else
                {
                    throw new Exception("Cannot find button with text: " + text);
                }
            }
        }
     
    }
}

