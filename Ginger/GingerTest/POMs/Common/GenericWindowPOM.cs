using Ginger;
using GingerWPFUnitTest.POMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GingerTest.POMs.Common
{
    public class GenericWindowPOM : GingerPOMBase
    {
        GenericWindow mGenericWindow;
        public GenericWindowPOM(GenericWindow GenericWindow)
        {
            mGenericWindow = GenericWindow;
        }

        public Page LoadedPage()
        {
            Page page = null;
            Execute(() => { 
                Frame f = (Frame)FindElementByAutomationID<Frame>(mGenericWindow, "PageFrame AID");
                page = (Page)f.Content;
            });
            return page;
        }


        public POMButtons Buttons
        {
            get
            {
                return new POMButtons(mGenericWindow);
            }
        }


            

        
    }
}
