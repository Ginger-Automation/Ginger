using Amdocs.Ginger.Common.Repository.Serialization;
using Amdocs.Ginger.Repository;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using GingerCore;
using GingerCore.Actions;
using RepositorySerializerBenchmarks.Benchmarks;
using RepositorySerializerBenchmarks.Benchmarks.Util;
using RepositorySerializerBenchmarks.Enhancements;
using RepositorySerializerBenchmarks.Enhancements.LiteXML;
using System.Xml;

//Summary summary = BenchmarkRunner.Run<SerializationBenchmark>();
Summary summary = BenchmarkRunner.Run<DeserializationBenchmark>();

//Console.WriteLine("waiting to start");
//await Task.Delay(10_000);
//Console.WriteLine("start");
//foreach (string repositoryItemBaseXML in TestDataGenerator.GenerateXMLs(100))
//{
//    //NewRepositorySerializer.LazyLoad = false;
//    //RepositoryItemBase item = NewRepositorySerializer.DeserializeFromText(repositoryItemBaseXML);
//    BusinessFlow.LazyLoad = false;
//    RepositoryItemBase item = new BetterRepositorySerializer2().Deserialize<BusinessFlow>(repositoryItemBaseXML);
//}
//Console.WriteLine("waiting to end");
//await Task.Delay(10_000);
//Console.WriteLine("end");

//_ = new ActDummy();
//string xml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "TestBusinessFlow.Ginger.BusinessFlow.xml"));
//BusinessFlow businessFlow = new BetterRepositorySerializer2().Deserialize<BusinessFlow>(xml);
//string newXml = new BetterRepositorySerializer2().Serialize(businessFlow);
//Console.WriteLine("end");




















































//Summary summary = BenchmarkRunner.Run<SerializationBenchmark>();
//Summary summary = BenchmarkRunner.Run<DeserializationBenchmark>();

//_ = new ActDummy();
//string xml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "TestBusinessFlow.Ginger.BusinessFlow.xml"));
//TestDataGenerator.GenerateRIBs(1);
