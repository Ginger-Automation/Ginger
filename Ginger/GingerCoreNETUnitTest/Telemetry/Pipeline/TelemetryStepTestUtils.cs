using Amdocs.Ginger.CoreNET.Telemetry;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.Telemetry.Pipeline
{
    internal static class TelemetryStepTestUtils
    {
        internal static ILogger NewConsoleLogger()
        {
            return LoggerFactory.Create(builder =>
                builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddConsole())
                .CreateLogger("Test-Console-Logger");
        }
    }
}
