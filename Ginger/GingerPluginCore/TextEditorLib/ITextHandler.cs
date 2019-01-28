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
