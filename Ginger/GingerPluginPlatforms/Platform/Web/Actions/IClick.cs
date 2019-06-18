using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Actions
{
    public interface IClick
    {

        void Click();
        
        void DoubleClick();

        void JavascriptClick();

        void MultiClick();

        void MouseClick();

    }
}
