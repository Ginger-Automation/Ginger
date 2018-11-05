using System;
using System.Collections.Generic;
using System.Text;
using GingerCore.Environments;

namespace Amdocs.Ginger.Common
{
    public interface IProjEnvironment
    {
        EnvApplication GetApplication(string appName);
    }
}
