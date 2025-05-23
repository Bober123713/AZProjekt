using AZProjekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AZProjekt.Exact;

public class Solver
{
    private const double Inf = double.PositiveInfinity;

    /// <summary>
    /// Converts the graph's adjacency matrix using LocalConverter and solves the TSP
    /// </summary>
    public static Solution ConvertAndSolve(Graph graph)
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
            
        var converter = new LocalConverter();
        var cost = converter.Convert(graph.AdjacencyMatrix);
        
        // Solve using the converted cost matrix
        var (tour, _) = SolveTsp(cost);
        
        // Calculate bottleneck using original weights
        double bottleneck = CalculateBottleneck(tour, graph.AdjacencyMatrix);
        
        // Convert to 1-based indices for the final solution
        var tour1Based = tour.Select(v => v + 1).ToList();
        
        return new Solution(bottleneck, tour1Based);
    }
    
    /// <summary>
    /// Solves the TSP directly using the provided cost matrix
    /// </summary>
    public static Solution Solve(List<List<double>> cost)
    {
        if (cost == null)
            throw new ArgumentNullException(nameof(cost));
            
        var (tour, _) = SolveTsp(cost);
        
        // Calculate bottleneck
        double bottleneck = CalculateBottleneck(tour, cost);
        
        // Convert to 1-based indices for the final solution
        var tour1Based = tour.Select(v => v + 1).ToList();
        
        return new Solution(Converter.FromDictionary[bottleneck], tour1Based);
    }
    
    /// <summary>
    /// Core TSP solving algorithm using dynamic programming
    /// </summary>
    private static (List<int> tour, double cost) SolveTsp(List<List<double>> cost)
    {
        var n = cost.Count;
        
        // Validate input
        if (n == 0) 
            throw new ArgumentException("Cost matrix must be non-empty.");
            
        // Check if matrix is square
        for (int i = 0; i < n; i++)
        {
            if (cost[i].Count != n)
                throw new ArgumentException("Cost matrix must be square.");
        }
        
        // Handle special case for n=1
        if (n == 1)
            return (new List<int> { 0, 0 }, 0.0);
            
        // Check for potential overflow
        if (n > 30)
            throw new ArgumentException("Graph too large for bit manipulation approach (max 30 nodes).");

        var N = 1 << n;
        var dp = new double[N, n];
        var parent = new int[N, n];

        // Initialize dp table
        for (var mask = 0; mask < N; mask++)
            for (var j = 0; j < n; j++)
                dp[mask, j] = Inf;

        dp[1, 0] = 0.0;  // Base case: start at node 0

        // Fill dp table
        for (var mask = 0; mask < N; mask++)
        {
            if ((mask & 1) == 0) continue; // Must include starting node
            
            for (var j = 1; j < n; j++)
            {
                if ((mask & (1 << j)) == 0) continue; // Skip if j is not in the subset
                
                var prevMask = mask ^ (1 << j);
                
                for (var k = 0; k < n; k++)
                {
                    if ((prevMask & (1 << k)) == 0) continue; // Skip if k is not in the subset
                    
                    var candidate = dp[prevMask, k] + cost[k][j];
                    if (candidate < dp[mask, j])
                    {
                        dp[mask, j] = candidate;
                        parent[mask, j] = k;
                    }
                }
            }
        }

        // Find the best ending node
        var fullMask = N - 1;
        var bestCost = Inf;
        var lastIndex = -1;
        
        for (var j = 1; j < n; j++)
        {
            var candidate = dp[fullMask, j] + cost[j][0]; // Add cost to return to start
            if (candidate < bestCost)
            {
                bestCost = candidate;
                lastIndex = j;
            }
        }

        // Reconstruct the tour
        var tour = new List<int>();
        var maskCursor = fullMask;
        var cur = lastIndex;
        
        while (cur != 0)
        {
            tour.Add(cur);
            var prev = parent[maskCursor, cur];
            maskCursor ^= (1 << cur);
            cur = prev;
        }
        
        tour.Add(0); // Add starting node
        tour.Reverse(); // Reverse to get correct order
        
        return (tour, bestCost);
    }
    
    /// <summary>
    /// Calculates the bottleneck value for a tour using the given cost matrix
    /// </summary>
    private static double CalculateBottleneck(List<int> tour, double[][] originalCosts)
    {
        double bottleneck = 0.0;
        
        for (int i = 0; i < tour.Count - 1; i++)
        {
            bottleneck = Math.Max(bottleneck, originalCosts[tour[i]][tour[i + 1]]);
        }
        
        // Add the edge from last to first node to complete the cycle
        bottleneck = Math.Max(bottleneck, originalCosts[tour[^1]][tour[0]]);
        
        return bottleneck;
    }
    
    /// <summary>
    /// Overloaded method for List<List<double>> cost matrix
    /// </summary>
    private static double CalculateBottleneck(List<int> tour, List<List<double>> cost)
    {
        double bottleneck = 0.0;
        
        for (int i = 0; i < tour.Count - 1; i++)
        {
            bottleneck = Math.Max(bottleneck, cost[tour[i]][tour[i + 1]]);
        }
        
        // Add the edge from last to first node to complete the cycle
        bottleneck = Math.Max(bottleneck, cost[tour[^1]][tour[0]]);
        
        return bottleneck;
    }
}
