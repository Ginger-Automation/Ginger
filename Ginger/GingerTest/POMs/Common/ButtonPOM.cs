using GingerWPFUnitTest.POMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GingerTest.POMs.Common
{
    public class ButtonPOM : GingerPOMBase
    {
        Button mButton;
        public ButtonPOM(Button button)
        {
            mButton = button;
        }

        public void Click()
        {
            Execute(() => {
                mButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            });
            
        }
    }
}
