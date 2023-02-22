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

namespace Amdocs.Ginger.Plugin.Core
{
    public interface ITextHandler
    {        
        /// <summary>
        /// The full text of the document being edited
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// get or set the current caret location
        /// </summary>
        int CaretLocation { get; set; }

        
        /// <summary>
        /// Add text at the end 
        /// </summary>
        /// <param name="text">text to appennd</param>
        void AppendText(string text);


        /// <summary>
        /// Insert text at caret position        
        /// </summary>
        /// <param name="text">text to insert</param>
        void InsertText(string text);

        /// <summary>
        /// Show a message to the user
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="text"></param>
        void ShowMessage(MessageType messageType, string text);
    }
}
