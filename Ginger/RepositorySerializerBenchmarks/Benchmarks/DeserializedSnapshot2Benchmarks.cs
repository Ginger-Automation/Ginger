using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using GingerCore.Actions;
using RepositorySerializerBenchmarks.Benchmarks.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Benchmarks
{
    public class DeserializedSnapshot2BenchmarksConfig : ManualConfig
    {
        public DeserializedSnapshot2BenchmarksConfig()
        {
            SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle.Default
                .WithTimeUnit(Perfolizer.Horology.TimeUnit.Millisecond)
                .WithSizeUnit(BenchmarkDotNet.Columns.SizeUnit.MB);
        }
    }

    [MinColumn]
    [MaxColumn]
    [MemoryDiagnoser]
    [Config(typeof(DeserializedSnapshot2BenchmarksConfig))]
    [SimpleJob]
    public class DeserializedSnapshot2Benchmarks
    {
        [Params(1)]
        public int TestDataSize { get; set; }

        public string[] TestData { get; set; } = [];

        [GlobalSetup]
        public void GlobalSetup()
        {
            LoadGingerCoreNETDLLIntoAppDomain();
            TestData = new string[TestDataSize];
            for (int index = 0; index < TestDataSize; index++)
                TestData[index] = GetRIBXml();
        }

        private string GetRIBXml()
        {
            return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "dummy.xml"));
        }

        private void LoadGingerCoreNETDLLIntoAppDomain()
        {
            new ActDummy();
        }

        private XmlReader[] Readers { get; set; } = [];

        [Benchmark(Baseline = true)]
        public void CreateRIBFromXmlReader()
        {
            foreach (XmlReader reader in Readers)
            {
                reader.MoveToContent();
                RepositoryItemBaseFactory.Create(reader.Name, new DeserializedSnapshot(reader));
            }
        }

        [Benchmark]
        public void LoadLiteXML()
        {
            foreach (XmlReader reader in Readers)
                DeserializedSnapshot2.Load(reader);
        }

        private DeserializedSnapshot2[] Snapshots { get; set; } = [];

        [IterationSetup]
        public void IterationSetup()
        {
            Readers = new XmlReader[TestDataSize];
            Snapshots = new DeserializedSnapshot2[TestDataSize];
            for (int index = 0; index < TestDataSize; index++)
            {
                Readers[index] = XmlReader.Create(new StringReader(TestData[index]));
                Snapshots[index] = DeserializedSnapshot2.Load(XmlReader.Create(new StringReader(TestData[index])));
            }
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            foreach (XmlReader reader in Readers)
                reader.Dispose();
        }

        [Benchmark]
        public void CreateRIBFromLiteXML()
        {
            foreach (DeserializedSnapshot2 snapshot in Snapshots)
                RepositoryItemBaseFactory.Create(snapshot.Name, snapshot);
        }
    }
}
