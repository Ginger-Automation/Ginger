using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Actions
{
    /// <summary>
    /// Exposes the methods for Elements supporting Selection functionality
    /// </summary>
   public interface ISelect
    {
        /// <summary>
        /// Clears the selected value if any.
        /// </summary>
        void ClearValue();

        /// <summary>
        /// Returns the List of Valid Values which a user can select.
        /// </summary>
        /// <returns></returns>
        List<string> GetValidValue();
        /// <summary>
        /// Let you know if value is populated in Selectio element
        /// </summary>
        /// <returns></returns>
        bool IsValuePopulated();

        /// <summary>
        /// Selects an option by Value.
        /// </summary>
        /// <param name="Value"></param>
        void Select(string Value);

        /// <summary>
        /// Selects an option by Index.
        /// </summary>
        /// <param name="index"></param>
        void SelectByIndex(int index);

        /// <summary>
        /// selects an option by text.
        /// </summary>
        /// <param name="Text"></param>
        void SelectByText(string Text);


        /// <summary>
        /// Returns the Selected value as string.
        /// </summary>
        /// <returns></returns>
        string GetSelectedValue();
    }
}
