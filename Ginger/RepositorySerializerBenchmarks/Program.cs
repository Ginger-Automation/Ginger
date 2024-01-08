using Amdocs.Ginger.Repository;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using GingerCore;
using RepositorySerializerBenchmarks.Benchmarks;
using RepositorySerializerBenchmarks.Benchmarks.Util;
using RepositorySerializerBenchmarks.Enhancements;
using System.Xml;

Summary summary = BenchmarkRunner.Run<SerializationBenchmark>();
//Summary summary = BenchmarkRunner.Run<DeserializationBenchmark>();


//string xml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "TestBusinessFlow.Ginger.BusinessFlow.xml"));
//using XmlReader xmlReader = XmlReader.Create(new StringReader(xml));
//while (xmlReader.Read())
//    continue;

//BetterRepositorySerializer betterRepositorySerializer = new();
//RepositoryItemBase[] testData = TestDataGenerator.GenerateRIBs(size: 1);
//string xml = betterRepositorySerializer.Serialize(testData[0]);
//Console.WriteLine(xml);


//BetterRepositorySerializer betterRepositorySerializer = new();
//RepositoryItemBase[] testData = TestDataGenerator.GenerateRIBs(size: 1);
//string xml = betterRepositorySerializer.Serialize(testData[0]);
//BusinessFlow businessFlow = betterRepositorySerializer.Deserialize<BusinessFlow>(xml);
//string newXml = betterRepositorySerializer.Serialize(businessFlow);
////Console.WriteLine(newXml);
//Console.WriteLine("End");

//string xml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "TestBusinessFlow.Ginger.BusinessFlow.xml"));
//using XmlReader xmlReader = XmlReader.Create(new StringReader(xml));
//BetterRepositorySerializer betterRepositorySerializer = new();
//BusinessFlow businessFlow = betterRepositorySerializer.Deserialize2<BusinessFlow>(xml);

//Console.ReadKey();