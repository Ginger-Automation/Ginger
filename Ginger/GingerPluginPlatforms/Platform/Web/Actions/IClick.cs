using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Actions
{
    /// <summary>
    /// Interface Defining the supported click options
    /// </summary>
    public interface IClick
    {
        /// <summary>
        /// Default Click Operation
        /// </summary>
        void Click();
        /// <summary>
        /// Double Click operation
        /// </summary>
        void DoubleClick();
        /// <summary>
        /// Implement this for Asyn/Javascript based click. Where you need to click but doesnt need to hold the control till mouse release. 
        /// </summary>
        void JavascriptClick();
        /// <summary>
        /// Performs Multiple clicks. Dor Future Use
        /// </summary>
        void MultiClick();
        /// <summary>
        /// For mimicking Mouse click event.
        /// </summary>
        void MouseClick();

    }
}
