using System.Collections.Generic;

namespace Amdocs.Ginger.Plugin.Core
{
    public interface ITextEditor
    {

        /// <summary>
        /// Name of the text editor
        /// </summary>
        string Name { get; }

        /// <summary>
        /// What type of extensions this editor can edit, for example: .txt, .json, .vbs - use lower case and include . i.e: { ".vbs", ".txt"}
        /// </summary>
        List<string> Extensions { get; }

        List<ITextEditorToolBarItem> Tools { get; }
        
        byte[] HighlightingDefinition { get; }

        IFoldingStrategy FoldingStrategy { get; }


        // Will be set by Ginger with ITextHandler impl
        ITextHandler TextHandler { get; set; }
        //string Text { get; set; }

        //int CaretLocation { get; }

        // void ShowMessage(MessageType messageType, string text);

    }
}
