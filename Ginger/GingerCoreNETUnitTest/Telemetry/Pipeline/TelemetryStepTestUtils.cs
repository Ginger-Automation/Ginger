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
        internal static ITelemetryDB<TRecord> NewInMemoryTelemetryDB<TRecord>()
        {
            return new InMemoryTelemetryDB<TRecord>();
        }

        internal static ITelemetryCollector<TRecord> NewInMemoryTelemetryCollector<TRecord>()
        {
            return new InMemoryTelemetryCollector<TRecord>();
        }

        internal static ILogger NewConsoleLogger()
        {
            return LoggerFactory.Create(builder =>
                builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddConsole())
                .CreateLogger("Test-Console-Logger");
        }

        private sealed class InMemoryTelemetryDB<TRecord> : ITelemetryDB<TRecord>
        {
            private readonly List<TRecord> _records = [];

            public Task AddAsync(TRecord record)
            {
                _records.Add(record);
                return Task.CompletedTask;
            }

            public Task DeleteAsync(TRecord record)
            {
                for (var index = 0; index < _records.Count; index++)
                {
                    var savedRecord = _records[index];

                    if (savedRecord == null && record == null ||
                        savedRecord != null && savedRecord.Equals(record))
                    {
                        _records.RemoveAt(index);
                        break;
                    }
                }
                return Task.CompletedTask;
            }

            public Task MarkFailedToUpload(TRecord record)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class InMemoryTelemetryCollector<TRecord> : ITelemetryCollector<TRecord>
        {
            public Task<ITelemetryCollector<TRecord>.AddResult> AddAsync(IEnumerable<TRecord> records)
            {
                return Task.FromResult(new ITelemetryCollector<TRecord>.AddResult()
                {
                    Successful = true,
                });
            }
        }
    }
}
