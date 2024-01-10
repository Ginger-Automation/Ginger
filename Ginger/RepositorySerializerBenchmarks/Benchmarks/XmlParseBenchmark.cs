using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Benchmarks
{
    [ShortRunJob]
    [MemoryDiagnoser]
    public class XmlParseBenchmark
    {
        private static readonly string XML = File.ReadAllText(
            Path.Combine(
                Directory.GetCurrentDirectory(), 
                "Benchmarks", 
                "Resources", 
                "TestBusinessFlow.Ginger.BusinessFlow.xml"));

        [Benchmark(Baseline = true)]
        public void XmlElement()
        {
            new XmlDocument().Load(new StringReader(XML));
        }

        [Benchmark]
        public void LiteXMLElement()
        {
            using XmlReader xmlReader = XmlReader.Create(new StringReader(XML));
            Enhancements.LiteXML.LiteXMLElement.Parse(xmlReader);
        }
    }
}
