using Amdocs.Ginger.Repository;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using GingerCore;
using Org.BouncyCastle.Asn1.Bsi;
using RepositorySerializerBenchmarks.Benchmarks.Util;
using RepositorySerializerBenchmarks.Enhancements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorySerializerBenchmarks.Benchmarks
{
    public class DeserializationBenchmarkConfig : ManualConfig
    {
        public DeserializationBenchmarkConfig()
        {

        }
    }

    [Config(typeof(DeserializationBenchmarkConfig))]
    [MemoryDiagnoser]
    [MinColumn]
    [MaxColumn]
    [RPlotExporter]
    [ShortRunJob]
    public class DeserializationBenchmark
    {
        private NewRepositorySerializer _newRepositorySerializer = null!;
        private BetterRepositorySerializer _betterRepositorySerializer = null!;

        [Params(1, 10, 100)]
        public int TestDataSize { get; set; }

        private string[] TestData { get; set; } = [];

        [GlobalSetup]
        public void GlobalSetup()
        {
            _newRepositorySerializer = new();
            _betterRepositorySerializer = new();
            TestData = TestDataGenerator.GenerateXMLs(TestDataSize);
        }

        [Benchmark(Baseline = true)]
        public void NewRepositorySerializer()
        {
            foreach (string repositoryItemBaseXML in TestData)
                Amdocs.Ginger.Repository.NewRepositorySerializer.DeserializeFromText(repositoryItemBaseXML);
        }

        [Benchmark]
        public void BetterRepositorySerializer()
        {
            BusinessFlowXMLSerializer.LazyLoad = false;
            foreach (string repositoryItemBaseXML in TestData)
                _betterRepositorySerializer.Deserialize<BusinessFlow>(repositoryItemBaseXML);
        }

        [Benchmark]
        public void BetterRepositorySerializer_Lazy()
        {
            BusinessFlowXMLSerializer.LazyLoad = true;
            foreach (string repositoryItemBaseXML in TestData)
                _betterRepositorySerializer.Deserialize<BusinessFlow>(repositoryItemBaseXML);
        }
    }
}
