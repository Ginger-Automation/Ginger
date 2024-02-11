using Amdocs.Ginger.Common.Repository.Serialization;
using Amdocs.Ginger.Repository;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using RepositorySerializerBenchmarks.Benchmarks.Util;
using RepositorySerializerBenchmarks.Enhancements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorySerializerBenchmarks.Benchmarks
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
    [RPlotExporter]
    [SimpleJob]
    public class SerializationBenchmark
    {
        private NewRepositorySerializer _newRepositorySerializer = null!;
        private BetterRepositorySerializer _betterRepositorySerializer = null!;

        [Params(1, 10)]
        public int TestDataSize { get; set; }

        private RepositoryItemBase[] TestData { get; set; } = [];

        [GlobalSetup]
        public void GlobalSetup()
        {
            _newRepositorySerializer = new();
            _betterRepositorySerializer = new();
            TestData = TestDataGenerator.GenerateRIBs(TestDataSize);
        }

        [Benchmark(Baseline = true)]
        public void Old()
        {
            foreach (RepositoryItemBase repositoryItemBase in TestData)
                _newRepositorySerializer.SerializeToString(repositoryItemBase);
        }

        [Benchmark]
        public void New()
        {
            foreach (RepositoryItemBase repositoryItemBase in TestData)
                _betterRepositorySerializer.Serialize(repositoryItemBase);
        }
    }
}
