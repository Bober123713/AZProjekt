using System.Collections.Concurrent;
using System.Text;
using Tuchu;

while (true)
{
    try
    {
        Console.WriteLine("Please number of samples:");
        var samplesString = Console.ReadLine() ?? throw new InvalidOperationException();
        if (!int.TryParse(samplesString, out var samples))
            continue;

        var correctCount = 0;
        
        int solvedCount = 0;
        var incorrect = new ConcurrentQueue<string>();
        
        var task = Parallel.ForAsync(0, samples, (i, _) =>
        {
            var graph = Generator.Generate(9);
            var (correct, approx, exact) = CompareSolutions(graph);
            if (correct)
                Interlocked.Increment(ref correctCount);
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine("----------------- SOLUTION INCORRECT -----------------");
                sb.AppendLine($"{Math.Round(approx.Cost / exact.Cost, 4)} WORSE");
                sb.AppendLine("--------------------------------------");
                sb.AppendLine(PrintGraph(graph));
                sb.AppendLine("----------------- APPROX -----------------");
                sb.AppendLine(approx.ToString());
                sb.AppendLine("----------------- EXACT -----------------");
                sb.AppendLine(exact.ToString());
                sb.AppendLine("--------------------------------------");
                incorrect.Enqueue(sb.ToString());
            }

            Interlocked.Increment(ref solvedCount);
            return ValueTask.CompletedTask;
        });

        while (!task.IsCompleted)
        {
            Console.WriteLine($"{solvedCount}/{samples} - {Math.Round((double)solvedCount / samples * 100, 4)}% - {incorrect.Count} WRONG");
            await Task.Delay(250);
        }

        while (!incorrect.IsEmpty)
            if(incorrect.TryDequeue(out var graph))
                Console.WriteLine(graph);
        Console.WriteLine($"Correct: {correctCount}/{samples} ({Math.Round((double)correctCount / samples * 100, 4)}%)");
        //Output.PrintResult(graph, approxSolution);
        //Output.PrintEdgeSkips(graph, approxSolution);
        //Output.PrintFullTreeWalk(graph);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        Console.WriteLine("Press return to continue...");
        Console.ReadLine();
    }
}

return;

static string PrintGraph(string[] graph)
{
    var sb = new StringBuilder();
    foreach(var line in graph)
        sb.AppendLine(line);
    return sb.ToString();
}

static (bool, Solution, Solution) CompareSolutions(string[] input)
{
    var graph = Importer.Import(input);
    
    var approxSolution = Solver.ApproxBottleneckTsp(graph, 0);
    var exactSolution = Tuchu.Exact.Solver.ConvertAndSolve(graph);

    //if (approxSolution.Cost < exactSolution.Cost)
    //{
    //    Console.WriteLine("⚠️ Approximation gave better result than exact. Possible bug.");
    //}
    
    return (approxSolution <= 3 * exactSolution, approxSolution, exactSolution);
}
