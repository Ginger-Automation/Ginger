using Amdocs.Ginger.Repository;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using GingerCore;
using RepositorySerializerBenchmarks.Benchmarks;
using RepositorySerializerBenchmarks.Benchmarks.Util;
using RepositorySerializerBenchmarks.Enhancements;

//Summary summary = BenchmarkRunner.Run<SerializationBenchmark>();
Summary summary = BenchmarkRunner.Run<DeserializationBenchmark>();

//BetterRepositorySerializer betterRepositorySerializer = new();
//RepositoryItemBase[] testData = TestDataGenerator.Generate(size: 1);
//string xml = betterRepositorySerializer.Serialize(testData[0]);
//Console.WriteLine(xml);


//BetterRepositorySerializer betterRepositorySerializer = new();
//RepositoryItemBase[] testData = TestDataGenerator.Generate(size: 1);
//string xml = betterRepositorySerializer.Serialize(testData[0]);
//BusinessFlow businessFlow = betterRepositorySerializer.Deserialize<BusinessFlow>(xml);
//string newXml = betterRepositorySerializer.Serialize(businessFlow);
//Console.WriteLine(newXml);