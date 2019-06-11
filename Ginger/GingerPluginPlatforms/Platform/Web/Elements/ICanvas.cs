using Ginger.Plugin.Platform.Web.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    /// <summary>
    /// Exposes the Functionality for Canvas elements
    /// </summary>
    public interface ICanvas: IGingerWebElement
    {

     
        /// <summary>
        /// Draws an object on Canvas. 
        /// </summary>
        void DrawObject();

        /// <summary>
        /// Clicks on a relative position from Top Left Corner of the Canvas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void ClickXY(int x, int y);

    }
}
