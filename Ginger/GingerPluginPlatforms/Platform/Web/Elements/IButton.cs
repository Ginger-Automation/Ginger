using Ginger.Plugin.Platform.Web.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    /// <summary>
    /// Exposes the Functionality of a button
    /// </summary>
    public interface IButton:IGingerWebElement,IGetValue,IClick
    {

        /// <summary>
        /// Submits a Button
        /// </summary>

        void Submit();
       
    }
}
