using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{    /// <summary>
     /// Exposes operation for Labels.
     /// </summary>
    public interface ILabel: IGingerWebElement
    {
        /// <summary>
        /// Gives the Font of Label.
        /// </summary>
        /// <returns></returns>
        string GetFont();
        /// <summary>
        /// Gets the Text of Label.
        /// </summary>
        /// <returns></returns>
        string GetText();

        /// <summary>
        /// Gets the Value of Label.
        /// </summary>
        /// <returns></returns>
        string Getvalue();
    }
}
