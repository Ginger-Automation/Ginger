#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
