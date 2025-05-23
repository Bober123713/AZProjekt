namespace Tuchu;

public static class Solver
{
    public static List<Edge> FindMst(Graph graph, int startVertex)
    {
        var inMst = new bool[graph.VertexCount];
        var key = new double[graph.VertexCount];
        var parent = new int[graph.VertexCount];

        for (var i = 0; i < graph.VertexCount; i++)
        {
            key[i] = int.MaxValue;
            inMst[i] = false;
        }

        key[startVertex] = 0;
        parent[startVertex] = -1;

        for (var count = 0; count < graph.VertexCount - 1; count++)
        {
            var u = MinKey(key, inMst, graph.VertexCount);
            inMst[u] = true;

            foreach (var edge in graph.AdjacencyList[u])
            {
                var v = edge.Destination;
                if (!inMst[v] && edge.Weight < key[v])
                {
                    parent[v] = u;
                    key[v] = edge.Weight;
                }
            }
        }

        // Construct MST
        var mst = new List<Edge>();
        for (var i = 0; i < graph.VertexCount; i++)
        {
            if (parent[i] != -1)
            {
                mst.Add(new Edge(parent[i], i, key[i]));
            }
        }

        return mst;
    }

    private static int MinKey(double[] key, bool[] inMst, int vertexCount)
    {
        var min = double.MaxValue;
        var minIndex = -1;

        for (var v = 0; v < vertexCount; v++)
        {
            if (inMst[v] || key[v] >= min) continue;
            min = key[v];
            minIndex = v;
        }

        return minIndex;
    }

// Function to build MST graph from list of edges
    public static Graph BuildMstGraph(Graph originalGraph, List<Edge> mstEdges)
    {
        var mstGraph = new Graph(originalGraph.VertexCount);

        // Copy vertex mapping
        foreach (var kvp in originalGraph.VertexMap)
        {
            mstGraph.AddVertex(kvp.Key, kvp.Value);
        }

        // Add MST edges
        foreach (var edge in mstEdges)
        {
            mstGraph.AddEdge(edge.Source, edge.Destination, edge.Weight);
        }

        return mstGraph;
    }

// Perform full tree walk (DFS) and build list W
    public static List<int> FullTreeWalk(Graph mstGraph, int root)
    {
        var w = new List<int>();
        var visited = new bool[mstGraph.VertexCount];

        FullTreeWalkRecursive(mstGraph, root, -1, w, visited);

        return w;
    }

    private static void FullTreeWalkRecursive(Graph graph, int u, int parent, List<int> w, bool[] visited)
    {
        w.Add(u); // Add current vertex to W
        visited[u] = true;

        foreach (var v in graph.AdjacencyList[u].Select(edge => edge.Destination).Where(v => v != parent && !visited[v]))
        {
            FullTreeWalkRecursive(graph, v, u, w, visited);
            w.Add(u); // Return to u after visiting subtree
        }
    }

// Find path in MST between two vertices
    public static List<int> FindPathInMst(Graph mstGraph, int source, int destination)
    {
        var parent = new Dictionary<int, int>();
        var visited = new HashSet<int>();

        var foundPath = DfsFindPath(mstGraph, source, destination, parent, visited);

        if (!foundPath)
            return [];

        var path = new List<int>();
        var current = destination;

        while (current != source)
        {
            path.Add(current);
            current = parent[current];
        }

        path.Add(source);
        path.Reverse();
        return path;
    }

    private static bool DfsFindPath(Graph graph, int vertex, int destination,
        Dictionary<int, int> parent, HashSet<int> visited)
    {
        if (vertex == destination)
            return true;

        visited.Add(vertex);

        foreach (var next in graph.AdjacencyList[vertex].Select(edge => edge.Destination).Where(next => !visited.Contains(next)))
        {
            parent[next] = vertex;
            if (DfsFindPath(graph, next, destination, parent, visited))
                return true;
        }

        return false;
    }

// Main approximation algorithm for Bottleneck TSP
    public static Solution ApproxBottleneckTsp(Graph graph, int root)
    {
        // Find MST
        var mstEdges = FindMst(graph, root);
        var mstGraph = BuildMstGraph(graph, mstEdges);

        // Perform full tree walk to get list W
        var w = FullTreeWalk(mstGraph, root);

        // Construct Hamiltonian cycle H
        var h = new List<int>();
        var visited = new bool[graph.VertexCount];

        // Add first vertex
        h.Add(w[0]);
        visited[w[0]] = true;

        var skipped = 0; // Counter for skipped consecutive vertices

        // Process remaining vertices in W
        for (var i = 1; i < w.Count; i++)
        {
            var currentVertex = w[i];

            if (!visited[currentVertex])
            {
                // If vertex not visited yet, add it to H
                h.Add(currentVertex);
                visited[currentVertex] = true;
                skipped = 0; // Reset skipped counter
            }
            else if (skipped == 2)
            {
                // If already skipped 2 consecutive vertices, must add this one
                // Only if it's different from the last vertex in H
                if (currentVertex != h[^1])
                {
                    h.Add(currentVertex);
                }

                skipped = 0; // Reset skipped counter
            }
            else
            {
                // Skip this vertex and increment counter
                skipped++;
            }
        }

        // Close the cycle by returning to the first vertex if needed
        if (h[^1] != h[0])
        {
            h.Add(h[0]);
        }

        // Calculate bottleneck cost
        var bottleneckCost = 0.0;
        for (var i = 0; i < h.Count - 1; i++)
        {
            var u = h[i];
            var v = h[i + 1];

            // Find direct edge cost in original graph
            var edgeCost = double.MaxValue;
            foreach (var edge in graph.AdjacencyList[u].Where(edge => edge.Destination == v))
            {
                edgeCost = edge.Weight;
                break;
            }

            bottleneckCost = Math.Max(bottleneckCost, edgeCost);
        }

        return new Solution(bottleneckCost, h.Select(i => i + 1).ToList());
    }
}