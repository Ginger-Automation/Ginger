// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using ManasBenchmarkNoOptimization;

Console.WriteLine("Hello, World!");

BenchmarkRunner.Run<BenchMarkSolution>();