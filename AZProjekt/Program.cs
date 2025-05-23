using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using AZProjekt;

// PONIZSZY KOD ZOSTAL ZAKOMENTOWANY I BYL UZYWANY DO TESTOWANIA DZIALANIA PROGRAMU PODCZAS JEGO TWORZENIA
/*while (true)
    try
    {
        Console.WriteLine("Please enter number of samples:");
        var samplesString = Console.ReadLine() ?? throw new InvalidOperationException();
        if (!int.TryParse(samplesString, out var samples))
            continue;

        var correctCount = 0;
    
        int solvedCount = 0;
        int lowerCount = 0;
        var incorrect = new ConcurrentQueue<string>();

        var sw = new Stopwatch();
        sw.Start();
        var task = Parallel.ForAsync(0, samples, (i, _) =>
        {
            var graph = Generator.Generate(9);
            var (correct, lower, approx, exact) = CompareSolutions(graph);
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
            if(lower)
                Interlocked.Increment(ref lowerCount);
            Interlocked.Increment(ref solvedCount);
            return ValueTask.CompletedTask;
        });

        while (!task.IsCompleted)
        {
            Console.WriteLine($"{solvedCount}/{samples} - {Math.Round((double)solvedCount / samples * 100, 4):F4}% - {incorrect.Count} WRONG - {lowerCount} LOWER - ELAPSED {sw.Elapsed:g} - ESTIMATED REMAINING {(sw.Elapsed * ((float)samples / (solvedCount + 1)) - sw.Elapsed):g}");
            await Task.Delay(250);
        }

        while (!incorrect.IsEmpty)
            if(incorrect.TryDequeue(out var graph))
                Console.WriteLine(graph);
        Console.WriteLine($"Correct: {correctCount}/{samples} ({Math.Round((double)correctCount / samples * 100, 4)}%)");
        Console.WriteLine($"Lower: {lowerCount}/{samples} ({Math.Round((double)lowerCount / samples * 100, 4)}%)");
        //Output.PrintResult(graph, approxSolution);
        //Output.PrintEdgeSkips(graph, approxSolution);
        //Output.PrintFullTreeWalk(graph);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        Console.WriteLine("Press return to continue...");
        Console.ReadLine();
    }*/

// W ramach testowania kodu mozna odkomentowac ponizszy inputDir 
//var inputDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\AZProjekt"));

var inputDir = Directory.GetCurrentDirectory();
var inputFiles = Directory.GetFiles(inputDir, "input_*.txt");

Console.WriteLine($"Znaleziono {inputFiles.Length} plików wejściowych w {inputDir}.");

foreach (var inputPath in inputFiles)
{
    try
    {
        // Importuj graf z pliku
        var graph = Importer.Import(inputPath);

        // Oblicz trasę aproksymacyjną
        var solution = Solver.ApproxBottleneckTsp(graph, out _);

        // Przygotuj nazwę pliku wyjściowego
        var outputPath = Path.ChangeExtension(inputPath, ".out.txt");

        // Zapisz rozwiązanie
        File.WriteAllText(outputPath, solution.ToString());

        Console.WriteLine($"Zapisano wynik do {Path.GetFileName(outputPath)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Błąd przy pliku {Path.GetFileName(inputPath)}: {ex.Message}");
    }
}

Console.WriteLine("Gotowe!");

Console.ReadLine();

static string PrintGraph(string[] graph)
{
    var sb = new StringBuilder();
    foreach(var line in graph)
        sb.AppendLine(line);
    return sb.ToString();
}

static string PrintGraph2(Graph graph)
{
    var sb = new StringBuilder();
    foreach(var row in graph.AdjacencyMatrix)
    {
        foreach(var column in row)
            sb.Append($"{column:000.0000} ");
        sb.AppendLine();
    }

    return sb.ToString();
}

static (bool, bool, Solution, Solution) CompareSolutions(string[] input)
{
    var graph = Importer.Import(input);
    
    var approxSolution = Solver.ApproxBottleneckTsp(graph, out var tour);
    var exactSolution = AZProjekt.Exact.Solver.ConvertAndSolve(graph);

    if (approxSolution.Cost < exactSolution.Cost)
    {
        var sb = new StringBuilder();
        sb.AppendLine("⚠️ Approximation gave better result than exact. Possible bug.");
        sb.AppendLine(PrintGraph2(graph));
        sb.AppendLine("---TOUR---");
        sb.AppendLine(string.Join(" ,", tour.Select(t => t + 1)));
        sb.AppendLine("---APPROX---");
        sb.AppendLine(approxSolution.ToString());
        for (var t = 0; t < approxSolution.Tour.Count - 1; t++)
        {
            sb.AppendLine($"{approxSolution.Tour[t]} -> {approxSolution.Tour[t + 1]}: {graph.AdjacencyMatrix[approxSolution.Tour[t] - 1][approxSolution.Tour[t + 1] - 1]}");
        }
        sb.AppendLine("---EXACT---");       
        sb.AppendLine(exactSolution.ToString());
        for (var t = 0; t < exactSolution.Tour.Count - 1; t++)
        {
            sb.AppendLine($"{exactSolution.Tour[t]} -> {exactSolution.Tour[t + 1]}: {graph.AdjacencyMatrix[exactSolution.Tour[t] - 1][exactSolution.Tour[t + 1] - 1]}");
        }
        Console.WriteLine(sb.ToString());
    }
    
    return (approxSolution <= 3 * exactSolution, exactSolution.Cost > approxSolution.Cost, approxSolution, exactSolution);
}
