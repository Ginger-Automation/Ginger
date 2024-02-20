using Amdocs.Ginger.Common.Repository.Serialization;
using Amdocs.Ginger.Repository;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using GingerCore;
using GingerCore.Actions;
using LiteDB;
using RepositorySerializerBenchmarks.Benchmarks.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorySerializerBenchmarks.Benchmarks
{
    public class LazyLoadBenchmarkConfig : ManualConfig
    {
        public LazyLoadBenchmarkConfig()
        {
            SummaryStyle =
                BenchmarkDotNet.Reports.SummaryStyle.Default
                .WithTimeUnit(Perfolizer.Horology.TimeUnit.Millisecond)
                .WithSizeUnit(BenchmarkDotNet.Columns.SizeUnit.MB);
        }
    }

    [Config(typeof(LazyLoadBenchmarkConfig))]
    [MemoryDiagnoser]
    [MinColumn]
    [MaxColumn]
    [RPlotExporter]
    [SimpleJob]
    public class LazyLoadBenchmark
    {
        private BetterRepositorySerializer _betterRepositorySerializer = null!;

        [Params(1, 10)]
        public int TestDataSize { get; set; }

        private string[] TestData { get; set; } = [];

        [GlobalSetup]
        public void GlobalSetup()
        {
            _betterRepositorySerializer = new();
            LoadGingerCoreNETDLLIntoAppDomain();
            TestData = TestDataGenerator.GenerateXMLs(TestDataSize);
        }

        private void LoadGingerCoreNETDLLIntoAppDomain()
        {
            new ActDummy();
        }

        [Benchmark(Baseline = true)]
        public void ReadOuterXml()
        {
            BusinessFlow.LazyLoad = true;
            BusinessFlow.Lite = false;
            foreach (string repositoryItemBaseXML in TestData)
                _betterRepositorySerializer.Deserialize<BusinessFlow>(repositoryItemBaseXML);
        }

        [Benchmark]
        public void LiteXml()
        {
            BusinessFlow.LazyLoad = true;
            BusinessFlow.Lite = true;
            foreach (string repositoryItemBaseXML in TestData)
                _betterRepositorySerializer.Deserialize<BusinessFlow>(repositoryItemBaseXML);
        }
    }
}
