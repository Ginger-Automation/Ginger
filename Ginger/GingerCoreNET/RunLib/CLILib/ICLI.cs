﻿using Ginger.Run;
using Ginger.SolutionGeneral;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public interface ICLI
    {
        string Identifier { get; }

        string FileExtension { get; }

        // Cretae CLI content from runsetExecutor
        string CreateContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper);

        // Parse the content and load it into runsetExecutor
        void LoadContent(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor);

        void Execute(RunsetExecutor runsetExecutor);

        bool IsFileBasedConfig { get; }
    }
}
