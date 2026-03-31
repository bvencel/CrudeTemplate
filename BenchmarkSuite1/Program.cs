using BenchmarkDotNet.Running;

namespace BenchmarkSuite1
{
    internal class Program
    {
#pragma warning disable IDE0060 // Remove unused parameter
        static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
