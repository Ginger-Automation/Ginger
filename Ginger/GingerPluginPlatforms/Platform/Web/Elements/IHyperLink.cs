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

using Ginger.Plugin.Platform.Web.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    /// <summary>
    /// Exposes operation for Hyperlink.
    /// </summary>
    public interface IHyperLink: IGingerWebElement,IClick
    {
        /// <summary>
        /// Gets The value of Hyperlink.
        /// </summary>
        /// <returns></returns>
        string GetValue();


//TODO:  Pending Definations for multiclick
        //        string MultiClicks();
    }
}
