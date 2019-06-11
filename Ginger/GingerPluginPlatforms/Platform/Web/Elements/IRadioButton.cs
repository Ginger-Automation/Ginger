using Ginger.Plugin.Platform.Web.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    /// <summary>
    /// Exposes Funationality for Radio Button.
    /// </summary>
    public interface IRadioButton : IGingerWebElement, IClick
    {
        /// <summary>
        /// Get The current value of Raid Button.
        /// </summary>
        /// <returns></returns>
        string GetValue();
    }
}
