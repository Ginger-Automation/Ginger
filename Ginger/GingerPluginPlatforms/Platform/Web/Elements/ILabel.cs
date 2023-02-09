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
