using AZProjekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AZProjekt;
public static class Solver
{
    /// <summary>
    /// Main approximation algorithm for Bottleneck TSP with controlled skipping
    /// </summary>
    public static Solution ApproxBottleneckTsp(Graph graph, out List<int> w, int root = 0)
    {
        w = [];
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
            
        if (root < 0 || root >= graph.VertexCount)
            throw new ArgumentOutOfRangeException(nameof(root));
            
        // Handle special case for small graphs
        if (graph.VertexCount <= 1)
            return new Solution(0.0, new List<int> { root + 1 });
            
        if (graph.VertexCount == 2)
            return new Solution(graph.AdjacencyMatrix[0][1], new List<int> { 1, 2, 1 });

        // Find MST
        var mstEdges = FindMst(graph, root);
        var mstGraph = BuildMstGraph(graph, mstEdges);

        // Perform full tree walk to get list W
        w = FullTreeWalk(mstGraph, root);

        // Construct Hamiltonian cycle H with controlled skipping
        var h = new List<int>();
        var visited = new bool[graph.VertexCount];
        
        
        var i = 0;
        var skipCount = 0;

        while (i < w.Count)
        {
            // Skip one element if we've already visited something and haven't skipped yet
            if (visited.Any(v => v) && skipCount < 1) {
                skipCount++;
            }
            // Add unvisited elements to result
            else if (!visited[w[i]]) {
                h.Add(w[i]);
                visited[w[i]] = true;
                skipCount = 0;
            }

            // Handle special case: root-X-root pattern
            if (i < w.Count - 2 && w[i] == root && w[i + 2] == root) {
                h.Add(w[i + 1]);
                visited[w[i + 1]] = true;
                skipCount = 0;
                i += 2;
                continue;
            }

            // Handle special case: when element after next is visited but next isn't
            if (i < w.Count - 2 && visited[w[i + 2]] && !visited[w[i + 1]]) {
                h.Add(w[i + 1]);
                visited[w[i + 1]] = true;
                skipCount = 0;
                continue;
            }

            // Move to next element
            i++;
        }

        // Ensure we have a cycle by adding the first vertex at the end if needed
        if (h.Count > 0 && h[0] != h[^1])
        {
            h.Add(h[0]);
        }

        // Ensure all vertices are visited
        //EnsureAllVerticesVisited(h, graph);

        // Calculate bottleneck cost
        var bottleneckCost = CalculateBottleneck(graph, h);

        // Convert to 1-based indexing for the solution
        var tour1Based = h.Select(v => v + 1).ToList();
        
        return new Solution(bottleneckCost, tour1Based);
    }
    
    /// <summary>
    /// Ensures all vertices are visited in the tour
    /// </summary>
    private static void EnsureAllVerticesVisited(List<int> tour, Graph graph)
    {
        var visited = new bool[graph.VertexCount];
        
        // Mark all vertices in the tour as visited
        foreach (var vertex in tour)
        {
            visited[vertex] = true;
        }
        
        // Find missing vertices
        var missing = new List<int>();
        for (var i = 0; i < visited.Length; i++)
        {
            if (!visited[i])
            {
                missing.Add(i);
            }
        }
        
        if (missing.Count == 0)
            return;
            
        // For each missing vertex, find the best place to insert it
        foreach (var vertex in missing)
        {
            var bestCost = double.MaxValue;
            var bestPosition = -1;
            
            // Try inserting at each position in the tour (except the last)
            for (var i = 0; i < tour.Count - 1; i++)
            {
                var u = tour[i];
                var v = tour[i + 1];
                
                // Calculate cost of inserting vertex between u and v
                var newCost = Math.Max(
                    graph.AdjacencyMatrix[u][vertex],
                    graph.AdjacencyMatrix[vertex][v]
                );
                
                if (newCost < bestCost)
                {
                    bestCost = newCost;
                    bestPosition = i + 1;
                }
            }
            
            // Insert the vertex at the best position
            if (bestPosition != -1)
            {
                tour.Insert(bestPosition, vertex);
            }
        }
    }
    
    /// <summary>
    /// Calculates the bottleneck value of a tour
    /// </summary>
    private static double CalculateBottleneck(Graph graph, List<int> tour)
    {
        var maxEdgeWeight = double.MinValue;
        
        for (var i = 0; i < tour.Count - 1; i++)
        {
            var u = tour[i];
            var v = tour[i + 1];
            var weight = graph.AdjacencyMatrix[u][v];
            maxEdgeWeight = Math.Max(maxEdgeWeight, weight);
        }
        
        return maxEdgeWeight;
    }
    
    /// <summary>
    /// Finds the Minimum Spanning Tree using Prim's algorithm
    /// </summary>
    public static List<Edge> FindMst(Graph graph, int startVertex)
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));

        if (startVertex < 0 || startVertex >= graph.VertexCount)
            throw new ArgumentOutOfRangeException(nameof(startVertex));

        var inMst = new bool[graph.VertexCount];
        var key = new double[graph.VertexCount];
        var parent = new int[graph.VertexCount];

        // Initialize all keys as infinity and MST status as false
        for (var i = 0; i < graph.VertexCount; i++)
        {
            key[i] = double.MaxValue; // Use double.MaxValue instead of int.MaxValue
            inMst[i] = false;
        }

        // Start with the given vertex
        key[startVertex] = 0;
        parent[startVertex] = -1;

        // Build the MST with (V-1) edges
        for (var count = 0; count < graph.VertexCount - 1; count++)
        {
            var u = MinKey(key, inMst, graph.VertexCount);

            // If no valid vertex found, the graph might be disconnected
            if (u == -1)
                break;

            inMst[u] = true;

            // Update key values of adjacent vertices
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
            if (!inMst[v] && key[v] < min)
            {
                min = key[v];
                minIndex = v;
            }
        }

        return minIndex;
    }

    /// <summary>
    /// Builds a graph representing the MST from a list of edges
    /// </summary>
    public static Graph BuildMstGraph(Graph originalGraph, List<Edge> mstEdges)
    {
        if (originalGraph == null)
            throw new ArgumentNullException(nameof(originalGraph));

        if (mstEdges == null)
            throw new ArgumentNullException(nameof(mstEdges));

        var mstGraph = new Graph(originalGraph.VertexCount);

        // Copy vertex mapping
        foreach (var kvp in originalGraph.VertexMap)
        {
            mstGraph.AddVertex(kvp.Key, kvp.Value);
        }

        // Add MST edges (bidirectional for traversal)
        foreach (var edge in mstEdges)
        {
            mstGraph.AddEdge(edge.Source, edge.Destination, edge.Weight);
            mstGraph.AddEdge(edge.Destination, edge.Source, edge.Weight);
        }

        return mstGraph;
    }

    /// <summary>
    /// Performs a full tree walk (DFS) and builds list W
    /// </summary>
    public static List<int> FullTreeWalk(Graph mstGraph, int root)
    {
        if (mstGraph == null)
            throw new ArgumentNullException(nameof(mstGraph));

        if (root < 0 || root >= mstGraph.VertexCount)
            throw new ArgumentOutOfRangeException(nameof(root));

        var w = new List<int>();
        var visited = new bool[mstGraph.VertexCount];

        FullTreeWalkRecursive(mstGraph, root, -1, w, visited);

        return w;
    }

    private static void FullTreeWalkRecursive(Graph graph, int u, int parent, List<int> w, bool[] visited)
    {
        w.Add(u); // Add current vertex to W
        visited[u] = true;

        // Visit all unvisited neighbors
        foreach (var edge in graph.AdjacencyList[u])
        {
            var v = edge.Destination;
            if (v != parent && !visited[v])
            {
                FullTreeWalkRecursive(graph, v, u, w, visited);
                w.Add(u); // Return to u after visiting subtree
            }
        }
    }

    /// <summary>
    /// Finds a path in the MST between two vertices
    /// </summary>
    public static List<int> FindPathInMst(Graph mstGraph, int source, int destination)
    {
        if (mstGraph == null)
            throw new ArgumentNullException(nameof(mstGraph));

        if (source < 0 || source >= mstGraph.VertexCount)
            throw new ArgumentOutOfRangeException(nameof(source));

        if (destination < 0 || destination >= mstGraph.VertexCount)
            throw new ArgumentOutOfRangeException(nameof(destination));

        var parent = new Dictionary<int, int>();
        var visited = new HashSet<int>();

        var foundPath = DfsFindPath(mstGraph, source, destination, parent, visited);

        if (!foundPath)
            return new List<int>(); // Return empty list if no path found

        // Reconstruct the path
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

        foreach (var edge in graph.AdjacencyList[vertex])
        {
            var next = edge.Destination;
            if (!visited.Contains(next))
            {
                parent[next] = vertex;
                if (DfsFindPath(graph, next, destination, parent, visited))
                    return true;
            }
        }

        return false;
    }
}