using Ginger.Plugin.Platform.Web.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    /// <summary>
    /// Exposes Operation for A TextBox.
    /// </summary>
   public interface ITextBox:IGingerWebElement,IGetValue
    {
        /// <summary>
        /// Clears a TextBox.
        /// </summary>
        void ClearValue();
        /// <summary>
        /// Gets Font of the TextBox.
        /// </summary>
        /// <returns></returns>
        string GetFont();

        /// <summary>
        /// gets Font of the TextBox.
        /// </summary>
        /// <returns></returns>
        string GetText();

        /// <summary>
        /// Gets Length of Text in textBox
        /// </summary>
        /// <returns></returns>
        int GetTextLength();
        /// <summary>
        /// Tells if a Textbox Contains Value.
        /// </summary>
        /// <returns></returns>
        bool IsValuePopulated();
   
        //TODO: Enable multisetvalue
      //  void SetMultiValue(string[] values);
      /// <summary>
      /// Mimic Key Type on a Text Box.
      /// </summary>
      /// <param name="keys"></param>
        void SendKeys(string keys);
        /// <summary>
        /// Sets text on a TextBox.
        /// </summary>
        /// <param name="Text"></param>
        void SetText(string Text);
        /// <summary>
        /// Sets Value on a TextBox
        /// </summary>
        /// <param name="Text"></param>
        void SetValue(string Text);
    }
}
