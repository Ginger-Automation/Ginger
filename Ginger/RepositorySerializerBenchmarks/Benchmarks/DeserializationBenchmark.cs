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
            SummaryStyle = 
                BenchmarkDotNet.Reports.SummaryStyle.Default
                .WithTimeUnit(Perfolizer.Horology.TimeUnit.Millisecond)
                .WithSizeUnit(BenchmarkDotNet.Columns.SizeUnit.MB);
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
        public void Old_Lazy()
        {
            foreach (string repositoryItemBaseXML in TestData)
                Amdocs.Ginger.Repository.NewRepositorySerializer.DeserializeFromText(repositoryItemBaseXML);
        }

        [Benchmark]
        public void New_Full_XmlDocument()
        {
            foreach (string repositoryItemBaseXML in TestData)
                _betterRepositorySerializer.DeserializeViaXmlDocument<BusinessFlow>(repositoryItemBaseXML, lazyLoad: false);
        }

        [Benchmark]
        public void New_Lazy_XmlDocument()
        {
            foreach (string repositoryItemBaseXML in TestData)
                _betterRepositorySerializer.DeserializeViaXmlDocument<BusinessFlow>(repositoryItemBaseXML, lazyLoad: true);
        }

        [Benchmark]
        public void New_Full_XmlReader()
        {
            foreach (string repositoryItemBaseXML in TestData)
                _betterRepositorySerializer.DeserializeViaXmlReader<BusinessFlow>(repositoryItemBaseXML, lazyLoad: false);
        }

        [Benchmark]
        public void New_Lazy_XmlReader()
        {
            foreach (string repositoryItemBaseXML in TestData)
                _betterRepositorySerializer.DeserializeViaXmlReader<BusinessFlow>(repositoryItemBaseXML, lazyLoad: true);
        }
    }
}
