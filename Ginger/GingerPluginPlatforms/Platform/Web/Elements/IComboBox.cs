using Ginger.Plugin.Platform.Web.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    /// <summary>
    /// Exposes the Funationality of ComboBox
    /// </summary>
    public interface IComboBox: IGingerWebElement, ISelect
    {
        /// <summary>
        /// Sets Value for for a combobox
        /// </summary>
        /// <param name="Value"></param>
        void SetValue(string Value);
    }
}
