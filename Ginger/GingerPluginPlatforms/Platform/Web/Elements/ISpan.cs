using Ginger.Plugin.Platform.Web.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    /// <summary>
    /// Exposes Operation On Span Elements
    /// </summary>
    public interface ISpan:IGingerWebElement, IGetValue
    {
        /// <summary>
        /// Sets Value in A Span Element
        /// </summary>
        /// <param name="Text"></param>
        void SetValue(string Text);
    }
}
