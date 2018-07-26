using GingerWPFUnitTest.POMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GingerTest.POMs.Common
{
    public class WindowPOM  : GingerPOMBase
    {
        Window mWindow;

        public WindowPOM(Window window)
        {
            mWindow = window;
        }

        public double Width
        {
            get
            {
                double w = 0;
                Execute(() => { 
                    w = mWindow.Width;
                });
                return w;
            } }
    }
}
