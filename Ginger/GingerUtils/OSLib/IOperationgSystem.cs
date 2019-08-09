using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GingerUtils.OSLib
{
    public interface IOperationgSystem
    {
        string UserAgent { get; }

        Process Dotnet(string cmd);
        string GetFirstLocalHostIPAddress();
    }
}
