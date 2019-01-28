using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public interface ITextEditorToolBarItem
    {        
        string ToolText { get; }
        string ToolTip { get; }
        //string Image { get; }
        void Execute(ITextEditor textEditor);        
    }
}
