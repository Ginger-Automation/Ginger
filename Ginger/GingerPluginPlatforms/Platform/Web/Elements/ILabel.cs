using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    public interface ILabel: IGingerWebElement
    {

        string GetFont();

        string GetText();
        string Getvalue();
    }
}
