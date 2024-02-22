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
    [SimpleJob]
    public class DeserializationBenchmark
    {
        private NewRepositorySerializer _newRepositorySerializer = null!;
        private BetterRepositorySerializer _betterRepositorySerializer = null!;

        [Params(1)]
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

        [Benchmark(Baseline = true)]
        public void Old_Full()
        {
            NewRepositorySerializer.LazyLoad = false;
            foreach (string repositoryItemBaseXML in TestData)
                Amdocs.Ginger.Repository.NewRepositorySerializer.DeserializeFromText(repositoryItemBaseXML);
        }

        //[Benchmark]
        //public void Old_Lazy()
        //{
        //    NewRepositorySerializer.LazyLoad = true;
        //    foreach (string repositoryItemBaseXML in TestData)
        //        Amdocs.Ginger.Repository.NewRepositorySerializer.DeserializeFromText(repositoryItemBaseXML);
        //}

        [Benchmark]
        public void New_Full()
        {
            BusinessFlow.LazyLoad = false;
            BetterRepositorySerializer.UseDeserializer2 = false;
            foreach (string repositoryItemBaseXML in TestData)
                _betterRepositorySerializer.Deserialize<BusinessFlow>(repositoryItemBaseXML);
        }

        //[Benchmark]
        //public void New_Lazy()
        //{
        //    BusinessFlow.LazyLoad = true;
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.Deserialize<BusinessFlow>(repositoryItemBaseXML);
        //}

        //[Benchmark]
        //public void Old_Full_Parallel()
        //{
        //    NewRepositorySerializer.LazyLoad = false;
        //    Parallel.ForEach(TestData, repositoryItemBaseXML =>
        //        Amdocs.Ginger.Repository.NewRepositorySerializer.DeserializeFromText(repositoryItemBaseXML));
        //}

        //[Benchmark]
        //public void Old_Lazy_Parallel()
        //{
        //    NewRepositorySerializer.LazyLoad = true;
        //    Parallel.ForEach(TestData, repositoryItemBaseXML =>
        //        Amdocs.Ginger.Repository.NewRepositorySerializer.DeserializeFromText(repositoryItemBaseXML));
        //}

        //[Benchmark]
        //public void New_Full_Parallel()
        //{
        //    BusinessFlow.LazyLoad = false;
        //    Parallel.ForEach(TestData, repositoryItemBaseXML =>
        //        _betterRepositorySerializer.Deserialize<BusinessFlow>(repositoryItemBaseXML));
        //}

        //[Benchmark]
        //public void New_Lazy_Parallel()
        //{
        //    BusinessFlow.LazyLoad = true;
        //    Parallel.ForEach(TestData, repositoryItemBaseXML =>
        //        _betterRepositorySerializer.Deserialize<BusinessFlow>(repositoryItemBaseXML));
        //}

        [Benchmark]
        public void New2_Full()
        {
            BusinessFlow.LazyLoad = false;
            BetterRepositorySerializer.UseDeserializer2 = true;
            foreach (string repositoryItemBaseXML in TestData)
                _betterRepositorySerializer.Deserialize<BusinessFlow>(repositoryItemBaseXML);
        }






        #region old benchmarks
        //[Benchmark]
        //public void New_Full_XmlDocument()
        //{
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.DeserializeViaXmlDocument<BusinessFlow>(repositoryItemBaseXML, lazyLoad: false);
        //}

        //[Benchmark]
        //public void New_Lazy_XmlDocument()
        //{
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.DeserializeViaXmlDocument<BusinessFlow>(repositoryItemBaseXML, lazyLoad: true);
        //}

        //[Benchmark]
        //public void New_Full_XmlReader()
        //{
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.DeserializeViaXmlReader<BusinessFlow>(repositoryItemBaseXML, lazyLoad: false);
        //}

        //[Benchmark]
        //public void New_Lazy_XmlReader()
        //{
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.DeserializeViaXmlReader<BusinessFlow>(repositoryItemBaseXML, lazyLoad: true);
        //}

        //[Benchmark]
        //public void New_Full_LiteXMLElement()
        //{
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.DeserializeViaLiteXMLElement<BusinessFlow>(repositoryItemBaseXML, lazyLoad: false);
        //}

        //[Benchmark]
        //public void New_Lazy_LiteXMLElement()
        //{
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.DeserializeViaLiteXMLElement<BusinessFlow>(repositoryItemBaseXML, lazyLoad: true);
        //}

        //[Benchmark]
        //public void New_Full_RIBXmlReader_PropertyParser()
        //{
        //    RepositoryItemBase.UsePropertyParsers = true;
        //    RepositoryItemHeader.UsePropertyParsers = true;
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.DeserializeViaRIBXmlReader<BusinessFlow>(repositoryItemBaseXML, lazyLoad: false);
        //}

        //[Benchmark]
        //public void New_Lazy_RIBXmlReader_PropertyParser()
        //{
        //    RepositoryItemBase.UsePropertyParsers = true;
        //    RepositoryItemHeader.UsePropertyParsers = true;
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.DeserializeViaRIBXmlReader<BusinessFlow>(repositoryItemBaseXML, lazyLoad: true);
        //}

        //[Benchmark]
        //public void New_Full_RIBXmlReader()
        //{
        //    RepositoryItemBase.UsePropertyParsers = false;
        //    RepositoryItemHeader.UsePropertyParsers = false;
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.DeserializeViaRIBXmlReader<BusinessFlow>(repositoryItemBaseXML, lazyLoad: false);
        //}

        //[Benchmark]
        //public void New_Lazy_RIBXmlReader()
        //{
        //    RepositoryItemBase.UsePropertyParsers = false;
        //    RepositoryItemHeader.UsePropertyParsers = false;
        //    foreach (string repositoryItemBaseXML in TestData)
        //        _betterRepositorySerializer.DeserializeViaRIBXmlReader<BusinessFlow>(repositoryItemBaseXML, lazyLoad: true);
        //}
        #endregion
    }
}
