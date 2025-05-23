using AZProjekt.Models;

namespace AZProjekt.Exact;

public class Solver
{
    public static Solution ConvertAndSolve(Graph graph)
    {
        var converter = new LocalConverter();
        var cost = converter.Convert(graph.AdjacencyMatrix);
        var n = cost.Count;
        if (n == 0) 
            throw new ArgumentException("Cost matrix must be non-empty.");
        if (n == 1)
            return new Solution(0.0, [1]);

        var N = 1 << n;
        var dp = new double[N, n];
        var parent = new int[N, n];
        const double INF = double.PositiveInfinity;

        for (var mask = 0; mask < N; mask++)
            for (var j = 0; j < n; j++)
                dp[mask, j] = INF;

        dp[1 << 0, 0] = 0.0;  

       
        for (var mask = 0; mask < N; mask++)
        {
            if ((mask & 1) == 0) continue;
            for (var j = 1; j < n; j++)
            {
                if ((mask & (1 << j)) == 0) continue;
                var prevMask = mask ^ (1 << j);
                for (var k = 0; k < n; k++)
                {
                    if ((prevMask & (1 << k)) == 0) continue;
                    var cand = dp[prevMask, k] + cost[k][j];
                    if (cand < dp[mask, j])
                    {
                        dp[mask, j] = cand;
                        parent[mask, j] = k;
                    }
                }
            }
        }

        var fullMask = N - 1;
        var bestCost = INF;
        var lastIndex = -1;
        for (var j = 1; j < n; j++)
        {
            var candidate = dp[fullMask, j] + cost[j][0];
            if (candidate >= bestCost) 
                continue;
            
            bestCost = candidate;
            lastIndex = j;
        }

        var stack = new List<int>();
        var maskCursor = fullMask;
        var cur = lastIndex;
        while (cur != 0)
        {
            stack.Add(cur);
            var prev = parent[maskCursor, cur];
            maskCursor ^= (1 << cur);
            cur = prev;
        }
        stack.Add(0);
        stack.Reverse(); 
        stack.Add(0);
        
        // Oblicz bottleneck w oparciu o oryginalne wagi z grafu
        var originalCosts = graph.AdjacencyMatrix;
        var realBottleneck = 0.0;
        for (int i = 0; i < stack.Count - 1; i++)
            realBottleneck = Math.Max(realBottleneck, originalCosts[stack[i]][stack[i + 1]]);
        realBottleneck = Math.Max(realBottleneck, originalCosts[stack[^1]][stack[0]]);

        var tour1Based = new List<int>(stack.Count);
        tour1Based.AddRange(stack.Select(v => v + 1));

        return new Solution(realBottleneck, tour1Based);

    }
    
    public static Solution Solve(List<List<double>> cost)
    {
        var n = cost.Count;
        if (n == 0) 
            throw new ArgumentException("Cost matrix must be non-empty.");
        if (n == 1)
            return new Solution(0.0, [1]);

        var N = 1 << n;
        var dp = new double[N, n];
        var parent = new int[N, n];
        const double INF = double.PositiveInfinity;

        for (var mask = 0; mask < N; mask++)
            for (var j = 0; j < n; j++)
                dp[mask, j] = INF;

        dp[1 << 0, 0] = 0.0;  

       
        for (var mask = 0; mask < N; mask++)
        {
            if ((mask & 1) == 0) continue;
            for (var j = 1; j < n; j++)
            {
                if ((mask & (1 << j)) == 0) continue;
                var prevMask = mask ^ (1 << j);
                for (var k = 0; k < n; k++)
                {
                    if ((prevMask & (1 << k)) == 0) continue;
                    var cand = dp[prevMask, k] + cost[k][j];
                    if (cand < dp[mask, j])
                    {
                        dp[mask, j] = cand;
                        parent[mask, j] = k;
                    }
                }
            }
        }

        var fullMask = N - 1;
        var bestCost = INF;
        var lastIndex = -1;
        for (var j = 1; j < n; j++)
        {
            var candidate = dp[fullMask, j] + cost[j][0];
            if (candidate >= bestCost) 
                continue;
            
            bestCost = candidate;
            lastIndex = j;
        }

        var stack = new List<int>();
        var maskCursor = fullMask;
        var cur = lastIndex;
        while (cur != 0)
        {
            stack.Add(cur);
            var prev = parent[maskCursor, cur];
            maskCursor ^= (1 << cur);
            cur = prev;
        }
        stack.Add(0);
        stack.Reverse(); 
        stack.Add(0);
        
        var bottleneck = 0.0;
        for (var i = 0; i < stack.Count - 1; i++)
            bottleneck = Math.Max(bottleneck, cost[ stack[i] ][ stack[i+1] ]);
        bottleneck = Math.Max(bottleneck, cost[ stack[^1] ][ stack[0] ]);
        
        var tour1Based = new List<int>(stack.Count);
        tour1Based.AddRange(stack.Select(v => v + 1));

        return new Solution(Converter.FromDictionary[bottleneck], tour1Based);
    }
}