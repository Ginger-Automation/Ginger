using Amdocs.Ginger.Repository;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using RepositorySerializerBenchmarks.Benchmarks;
using RepositorySerializerBenchmarks.Benchmarks.Util;
using RepositorySerializerBenchmarks.Enhancements;

//Summary summary = BenchmarkRunner.Run<SerializerBenchmark>();

//BetterRepositorySerializer betterRepositorySerializer = new();
//RepositoryItemBase[] testData = TestDataGenerator.Generate(size: 1);
//string xml = betterRepositorySerializer.Serialize(testData[0]);
//Console.WriteLine(xml);


BetterRepositorySerializer betterRepositorySerializer = new();
RepositoryItemBase[] testData = TestDataGenerator.Generate(size: 1);
string xml = betterRepositorySerializer.Serialize(testData[0]);
betterRepositorySerializer.Deserialize(xml);