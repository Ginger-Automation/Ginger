using Amdocs.Ginger.CoreNET.RunLib;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManasBenchmarkNoOptimization
{
    public class SerializationBenchmarkConfig : ManualConfig
    {
        public SerializationBenchmarkConfig()
        {
            SummaryStyle =
                BenchmarkDotNet.Reports.SummaryStyle.Default
                .WithTimeUnit(Perfolizer.Horology.TimeUnit.Millisecond)
                .WithSizeUnit(BenchmarkDotNet.Columns.SizeUnit.MB);
        }
    }

    [Config(typeof(SerializationBenchmarkConfig))]
    [MemoryDiagnoser]
    [MinColumn]
    [MaxColumn]
    [ShortRunJob]
    public class BenchMarkSolution
    {
        CLIProcessor cli = new();
        [Benchmark]
        public void Run()
        {

            string[] args = [
                "run",
                "--env",
                "Default",
                "--encryptionKey",
                "Manas@2019",
                "--runset",
                "10Runners",
                "--solution",
                "C:\\Users\\manaska\\Documents\\ManasSeleniumDriverTest"
            ];


            cli.ExecuteArgs(args).GetAwaiter().GetResult();
        }
    }
}
