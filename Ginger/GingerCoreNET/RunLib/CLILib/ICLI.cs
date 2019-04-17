using Ginger.Run;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public interface ICLI
    {
        string Identifier { get; }

        // Cretae CLI content from runsetExecutor
        string CreateContent(RunsetExecutor runsetExecutor);

        // Parse the content and load it into runsetExecutor
        void LoadContent(string content, RunsetExecutor runsetExecutor);

        bool Execute(RunsetExecutor runsetExecutor);
        
    }
}
