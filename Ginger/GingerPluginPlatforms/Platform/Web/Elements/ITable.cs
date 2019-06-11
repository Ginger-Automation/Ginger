using Ginger.Plugin.Platform.Web.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    /// <summary>
    /// Exposes operation that can be performed on Table Element
    /// </summary>
    public interface ITable:IGingerWebElement, IGetValue
    {


        /// <summary>
        /// Set A Values on A table element.
        /// </summary>
        /// <param name="Text"></param>
        void SetValue(string Text);

        /// <summary>
        /// Perfroms CLick on a Table Element
        /// </summary>
        void Click();
    }
}
