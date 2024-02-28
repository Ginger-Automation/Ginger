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
//Summary summary = BenchmarkRunner.Run<LazyLoadBenchmark>();
//Summary summary = BenchmarkRunner.Run<DeserializedSnapshot2Benchmarks>();


//_ = new ActDummy();
//string xml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "TestBusinessFlow.Ginger.BusinessFlow.xml"));
//BetterRepositorySerializer.UseDeserializer2 = true;
//BusinessFlow.LazyLoad = false;
//BusinessFlow bf = new BetterRepositorySerializer().Deserialize<BusinessFlow>(xml);

//BetterRepositorySerializer.UseDeserializer2 = true;
//BusinessFlow.LazyLoad = false;
//bf = new BetterRepositorySerializer().Deserialize<BusinessFlow>(xml);

//BetterRepositorySerializer.UseDeserializer2 = false;
//BusinessFlow.LazyLoad = false;
//bf = new BetterRepositorySerializer().Deserialize<BusinessFlow>(xml);
//Console.WriteLine("end");

//Console.WriteLine("waiting to start");
////await Task.Delay(10_000);
//Console.WriteLine("start");
//BusinessFlow.LazyLoad = false;
//Parallel.ForEach(TestDataGenerator.GenerateXMLs(10), repositoryItemBaseXML =>
//    new BetterRepositorySerializer().Deserialize<BusinessFlow>(repositoryItemBaseXML));
//foreach (string repositoryItemBaseXML in TestDataGenerator.GenerateXMLs(100))
//{
//    //NewRepositorySerializer.LazyLoad = false;
//    //RepositoryItemBase item = NewRepositorySerializer.DeserializeFromText(repositoryItemBaseXML);
//    BusinessFlow.LazyLoad = false;
//    RepositoryItemBase item = new BetterRepositorySerializer().Deserialize<BusinessFlow>(repositoryItemBaseXML);
//}
//Console.WriteLine("waiting to end");
////await Task.Delay(10_000);
//Console.WriteLine("end");

//_ = new ActDummy();
//string xml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "TestBusinessFlow.Ginger.BusinessFlow.xml"));
//BusinessFlow.LazyLoad = true;
//BusinessFlow businessFlow = new BetterRepositorySerializer().Deserialize<BusinessFlow>(xml);
//string newXml = new BetterRepositorySerializer().Serialize(businessFlow);




















































//Summary summary = BenchmarkRunner.Run<SerializationBenchmark>();
//Summary summary = BenchmarkRunner.Run<DeserializationBenchmark>();

//_ = new ActDummy();
//string xml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "TestBusinessFlow.Ginger.BusinessFlow.xml"));
//TestDataGenerator.GenerateRIBs(1);
