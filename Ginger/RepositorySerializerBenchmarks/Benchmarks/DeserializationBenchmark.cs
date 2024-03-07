using Amdocs.Ginger.Common.Repository.Serialization;
using Amdocs.Ginger.Repository;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using GingerCore;
using GingerCore.Actions;
using NPOI.SS.Formula.PTG;
using Org.BouncyCastle.Asn1.Bsi;
using RepositorySerializerBenchmarks.Benchmarks.Util;
using RepositorySerializerBenchmarks.Enhancements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace RepositorySerializerBenchmarks.Benchmarks
{
    public class DeserializationBenchmarkConfig : ManualConfig
    {
        public DeserializationBenchmarkConfig()
        {
            SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle.Default
                .WithTimeUnit(Perfolizer.Horology.TimeUnit.Millisecond)
                .WithSizeUnit(BenchmarkDotNet.Columns.SizeUnit.MB);
        }
    }

    [Config(typeof(DeserializationBenchmarkConfig))]
    [MinColumn]
    [MaxColumn]
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
            LoadGingerCoreNETDLLIntoAppDomain();
            TestData = TestDataGenerator.GenerateXMLs(TestDataSize);
        }

        private void LoadGingerCoreNETDLLIntoAppDomain()
        {
            new ActDummy();
        }

        //[Benchmark(Baseline = true)]
        //public void Old_Full()
        //{
        //    NewRepositorySerializer.LazyLoad = false;
        //    foreach (string repositoryItemBaseXML in TestData)
        //        Amdocs.Ginger.Repository.NewRepositorySerializer.DeserializeFromText(repositoryItemBaseXML);
        //}

        [Benchmark(Baseline = true)]
        public void Old_Lazy()
        {
            //NewRepositorySerializer.LazyLoad = true;
            foreach (string repositoryItemBaseXML in TestData)
                Amdocs.Ginger.Repository.NewRepositorySerializer.DeserializeFromText(repositoryItemBaseXML);
        }

        //[Benchmark]
        //public void New_Full()
        //{
        //    BusinessFlow.LazyLoad = false;
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.Deserialize<BusinessFlow>(repositoryItemBaseXML);
        //}

        [Benchmark]
        public void New_Lazy()
        {
            BusinessFlow.LazyLoad = true;
            foreach (string repositoryItemBaseXML in TestData)
                _betterRepositorySerializer.Deserialize<BusinessFlow>(repositoryItemBaseXML);
        }
    }
}
