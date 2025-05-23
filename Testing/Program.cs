using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using AZProjekt;
using AZProjekt.Models;

public static class TestRunner
{
    public static void Main()
    {
        var small = new[] { 4, 5, 6, 7 };
        var medium = new[] { 8, 9, 10, 11, 12 };
        var large = new[] { 15, 18, 21 };

        int samplesPerSize = 10;
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string resultFile = $"results_{timestamp}.csv";

        using var writer = new StreamWriter(resultFile, false, Encoding.UTF8);
        writer.WriteLine("Category,Size,Sample,ApproxCost,ExactCost,CostRatio,ApproxTimeMs,ExactTimeMs,TimeRatio");

        RunTests("Small", small, samplesPerSize, writer);
        RunTests("Medium", medium, samplesPerSize, writer);
        RunTests("Large", large, samplesPerSize, writer);

        Console.WriteLine($"All tests completed. Results written to {resultFile}");
    }

    private static void RunTests(string category, int[] sizes, int samples, StreamWriter writer)
    {
        foreach (var size in sizes)
        {
            Console.WriteLine($"Testing {category} size {size}");

            for (int i = 0; i < samples; i++)
            {
                try
                {
                    var input = Generator.Generate(size);
                    var graph = Importer.Import(input);

                    // Time for Approximation
                    var swApprox = Stopwatch.StartNew();
                    var approx = Solver.ApproxBottleneckTsp(graph, 0);
                    swApprox.Stop();

                    // Time for Exact
                    var swExact = Stopwatch.StartNew();
                    var exact = AZProjekt.Exact.Solver.ConvertAndSolve(graph);
                    swExact.Stop();

                    double costRatio = approx.Cost / exact.Cost;
                    double approxTime = swApprox.Elapsed.TotalMilliseconds;
                    double exactTime = swExact.Elapsed.TotalMilliseconds;
                    double timeRatio = approxTime / (exactTime == 0 ? 1 : exactTime); // avoid division by zero

                    writer.WriteLine(
                        $"{category},{size},{i + 1}," +
                        $"{approx.Cost.ToString(CultureInfo.InvariantCulture)}," +
                        $"{exact.Cost.ToString(CultureInfo.InvariantCulture)}," +
                        $"{costRatio.ToString(CultureInfo.InvariantCulture)}," +
                        $"{approxTime.ToString(CultureInfo.InvariantCulture)}," +
                        $"{exactTime.ToString(CultureInfo.InvariantCulture)}," +
                        $"{timeRatio.ToString(CultureInfo.InvariantCulture)}"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error on size {size}, sample {i + 1}: {ex.Message}");
                }
            }
            
            Console.WriteLine("Finished Testing");
        }
    }
}
