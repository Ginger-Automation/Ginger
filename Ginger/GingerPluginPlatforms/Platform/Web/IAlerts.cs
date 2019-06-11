using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web
{

    /// <summary>
    /// Exposes the functionality to Control ALerts.
    /// </summary>
    public interface IAlerts
    {
        /// <summary>
        /// Dismisses an Alert.
        /// </summary>
        void DismissAlert();
        /// <summary>
        /// Accepts an Alert.
        /// </summary>
        void AcceptAlert();
        /// <summary>
        /// Get Text from An ALert.
        /// </summary>
        /// <returns></returns>
        string GetALertText();
        /// <summary>
        /// Set Text in Alert .
        /// </summary>
        /// <param name="Text"></param>
        void SendAlertText(string Text);
    }
}
