// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using ManasBenchmark;

Console.WriteLine("Hello, World!");
BenchmarkRunner.Run<BenchMarkSolution>();
//new BenchMarkSolution().Run();