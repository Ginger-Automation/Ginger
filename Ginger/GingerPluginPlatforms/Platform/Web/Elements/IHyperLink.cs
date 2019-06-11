using Ginger.Plugin.Platform.Web.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    /// <summary>
    /// Exposes operation for Hyperlink.
    /// </summary>
    public interface IHyperLink: IGingerWebElement,IClick
    {
        /// <summary>
        /// Gets The value of Hyperlink.
        /// </summary>
        /// <returns></returns>
        string GetValue();


//TODO:  Pending Definations for multiclick
        //        string MultiClicks();
    }
}
