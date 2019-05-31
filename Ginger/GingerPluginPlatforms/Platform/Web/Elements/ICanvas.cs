using Ginger.Plugin.Platform.Web.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    public interface ICanvas: IGingerWebElement
    {

     
        void DrawObject();

        void ClickXY(int x, int y);

    }
}
