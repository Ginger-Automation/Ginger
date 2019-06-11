using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Actions
{
    /// <summary>
    /// Interfaces grouping all the common methods required for Getting Value
    /// </summary>
    public interface IGetValue
    {
        /// <summary>
        /// Default Method to Get value of a webcontrol as String
        /// </summary>
        /// <returns></returns>
        string GetValue();
    }
}
