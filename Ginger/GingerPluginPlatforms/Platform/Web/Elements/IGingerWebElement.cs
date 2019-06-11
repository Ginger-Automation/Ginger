using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{

    /// <summary>
    /// Exposes the common functionality of all web elements.
    /// </summary>
   public interface IGingerWebElement
    {












        /// <summary>
        /// Holds the instance of actual WebElement
        /// </summary>
        object Element { get; set; }

        /// <summary>
        /// Perform Drag and Drop Operation
        /// </summary>
        /// <param name="DragDropType">it SUpports Selenium and Javascript Drag and drop</param>
        /// <param name="Element"></param>
        void DragAndDrop(string DragDropType,IGingerWebElement Element);
        /// <summary>
        /// Get Value of the provided Attribute
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        string GetAttribute(string attributeName);
        /// <summary>
        /// Give you the Height of a WebControl.
        /// </summary>
        /// <returns></returns>
        int GetHeight();

        //TODO:Enable Item count 
        //int GetItemCount();

        /// <summary>
        /// Give you the Size of a WebControl.
        /// </summary>
        /// <returns></returns
        Size GetSize();

        /// <summary>
        /// Give you the style of a WebControl.
        /// </summary>
        /// <returns></returns
        string GetStyle();

        /// <summary>
        /// Give you the Width of a WebControl.
        /// </summary>
        /// <returns></returns
        int GetWidth();

        /// <summary>
        /// Hovers over a Web Control
        /// </summary>
        void Hover();
        /// <summary>
        /// Tells if a Webcontrol is enabled.
        /// </summary>
        /// <returns></returns>
        bool IsEnabled();
        /// <summary>
        /// Tells if a Webcontrol is Visible.
        /// </summary>
        /// <returns></returns>
        bool IsVisible();
        /// <summary>
        /// Performs Right Click.
        /// </summary>
        /// <returns></returns>
        void RightClick();

        /// <summary>
        /// Run Javascript on an current element.
        /// </summary>
        /// <param name="Script"></param>
        /// <returns></returns>
        string RunJavascript(string Script);

        /// <summary>
        /// Performs scroll operation on current Element.
        /// </summary>
        void ScrollToElement();

        /// <summary>
        /// Sets Focus to Current Element
        /// </summary>
        void SetFocus();
     

    }
}
