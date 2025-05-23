namespace Tuchu;

public static class Output
{
    public static void PrintResult(Graph graph, Solution solution)
    {
        Console.WriteLine("Bottleneck TSP Tour:");
        foreach (var vertex in solution.Tour) 
            Console.Write(graph.ReverseVertexMap[vertex] + " ");

        Console.WriteLine("\nBottleneck Cost: " + Math.Round(solution.Cost, 4));
    }

    public static void PrintEdgeSkips(Graph graph, Solution solution)
    {
        Console.WriteLine("\nVerifying tour edges:");
        for (var i = 0; i < solution.Tour.Count - 1; i++)
        {
            var u = solution.Tour[i];
            var v = solution.Tour[i + 1];

            var pathInMst = Solver.FindPathInMst(
                Solver.BuildMstGraph(graph, Solver.FindMst(graph, 0)), u, v);

            var skippedVertices = pathInMst.Count - 2; // -2 because we don't count endpoints

            Console.WriteLine($"Edge {graph.ReverseVertexMap[u]}-{graph.ReverseVertexMap[v]}: " +
                              $"Skips {skippedVertices} vertices in MST " +
                              $"(path: {string.Join("-", pathInMst.Select(vertex => graph.ReverseVertexMap[vertex]))})");
        }
    }

    public static void PrintFullTreeWalk(Graph graph)
    {
        var w = Solver.FullTreeWalk(Solver.BuildMstGraph(graph, Solver.FindMst(graph, 0)), 0);
        Console.WriteLine("\nFull Tree Walk W:");
        Console.WriteLine(string.Join(" ", w.Select(v => graph.ReverseVertexMap[v])));
    }

}