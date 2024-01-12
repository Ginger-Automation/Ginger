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

//Summary summary = BenchmarkRunner.Run<XmlParseBenchmark>();

//string xml = @"<ActDummy Active=""True"" Description=""Action 1"" Guid=""6986a4c2-169c-4a56-9703-a957f3865717"" ParentGuid=""00000000-0000-0000-0000-000000000000"" Platform=""NA"" RetryMechanismInterval=""5"" StatusConverter=""None"" WindowsToCapture=""OnlyActiveWindow"">
//<InputValues>
//<ActInputValue Guid=""332cccf2-ad02-495d-8580-aba57cd9b0e1"" Param=""Value"" ParentGuid=""00000000-0000-0000-0000-000000000000"" />
//</InputValues>
//</ActDummy>";
//using XmlReader xmlReader = XmlReader.Create(new StringReader(xml));

//_ = new ActDummy();
//string xml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "TestBusinessFlow.Ginger.BusinessFlow.xml"));
//TestDataGenerator.GenerateRIBs(1);

//await Task.Delay(5000);
//var b = new BetterRepositorySerializer().DeserializeViaRIBXmlReader<BusinessFlow>(xml, lazyLoad: false);
//Console.WriteLine("Deserialization Done");
//await Task.Delay(5000);
//string reXml = new BetterRepositorySerializer().Serialize(b);
//new BetterRepositorySerializer().DeserializeViaRIBXmlReader<BusinessFlow>(xml, lazyLoad: true);
//new BetterRepositorySerializer().DeserializeViaRIBXmlReader<BusinessFlow>(xml, lazyLoad: true);

//new BetterRepositorySerializer().DeserializeViaXmlReader<BusinessFlow>(xml, lazyLoad: false);
//new BetterRepositorySerializer().DeserializeViaXmlReader<BusinessFlow>(xml, lazyLoad: false);
//new BetterRepositorySerializer().DeserializeViaXmlReader<BusinessFlow>(xml, lazyLoad: false);


//NewRepositorySerializer.DeserializeFromText(xml);
//NewRepositorySerializer.DeserializeFromText(xml);
//NewRepositorySerializer.DeserializeFromText(xml);

//Console.WriteLine("Deserialization Complete");



//string xml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "dummy.xml"));
//using XmlReader xmlReader = XmlReader.Create(new StringReader(xml));
//Console.WriteLine("End");