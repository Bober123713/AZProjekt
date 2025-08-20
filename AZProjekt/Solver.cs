namespace AZProjekt
{
    public static class Solver
    {
        /// <summary>
        /// 3-aproksymacja Bottleneck TSP zakładając spełnienie nierówności trójkąta.
        /// </summary>
        public static Solution ApproxBottleneckTsp(Graph graph, out int[] tour0Based)
        {
            var n = graph.VertexCount;
            switch (n)
            {
                case 0:
                    tour0Based = [];
                    return new Solution(0.0, []);
                case 1:
                    tour0Based = [0];
                    return new Solution(0.0, [1, 1]);
            }
            
            // Konstrukcja BST
            var T = BuildBottleneckSpanningTreeKruskal(graph);

            // Preorder Full Tree Walk
            var W = EulerWalkVerticesOfTree(T, 0, n);
            
            // Prekomputacja ostatniego wystąpienia danego wierzchołka w W
            var last = new int[n];
            for (var i = 0; i < W.Count; i++) last[W[i]] = i;

            // Cykl Hamiltoński ze skracaniem
            var visited = new bool[n];
            var H = new List<int>(n);
            var hops = 0;

            for (var i = 0; i < W.Count; i++)
            {
                if (i > 0) 
                    hops++;
                
                var v = W[i];
                if (visited[v] || (hops < 3 && i != last[v])) 
                    continue;
                
                H.Add(v);
                visited[v] = true;
                hops = 0;
                
                if (H.Count == n) 
                    break;
            }

            tour0Based = H.ToArray();

            // Konwersja bazy 0 na baze 1
            var tour1Based = H.Select(x => x + 1).ToList();
            tour1Based.Add(tour1Based[0]); // domknięcie cyklu

            var bottleneck = 0.0;
            var adjacencyMatrix = graph.AdjacencyMatrix;
            for (var i = 0; i < H.Count; i++)
            {
                var a = H[i];
                var b = H[(i + 1) % H.Count];
                bottleneck = Math.Max(bottleneck, adjacencyMatrix[a][b]);
            }

            return new Solution(bottleneck, tour1Based);
        }

        // ====== FUNKCJE POMOCNICZE ======
        private static List<int>[] BuildBottleneckSpanningTreeKruskal(Graph g)
        {
            var n = g.VertexCount;
            
            var edges = new List<(int u, int v, double c)>(n * (n - 1) / 2);
            for (var u = 0; u < n; u++)
                foreach (var (_, v, weight) in g.AdjacencyList[u])
                    if (u < v) edges.Add((u, v, weight));

            edges.Sort((a, b) => a.c.CompareTo(b.c));

            var uf = new Dsu(n);
            var adj = new List<int>[n];
            for (var i = 0; i < n; i++) adj[i] = [];

            var maxEdgeInTree = 0.0;
            var added = 0;
            foreach (var (u, v, c) in edges)
            {
                if (!uf.Union(u, v)) 
                    continue;
                
                adj[u].Add(v);
                adj[v].Add(u);
                if (c > maxEdgeInTree) maxEdgeInTree = c;
                added++;
                if (added == n - 1) break;
            }
            
            return adj;
        }

        private static List<int> EulerWalkVerticesOfTree(List<int>[] tree, int root, int n)
        {
            // Symulacja rekurencji za pomocą stacka
            var walk = new List<int>(2 * n - 1);
            var stack = new Stack<(int v, int parent, int idx)>();
            stack.Push((root, -1, 0));
            walk.Add(root);

            while (stack.Count > 0)
            {
                var (v, parent, idx) = stack.Pop();

                if (idx < tree[v].Count)
                {
                    var u = tree[v][idx];
                    stack.Push((v, parent, idx + 1));
                    if (u == parent) continue;

                    stack.Push((u, v, 0));
                    walk.Add(u);
                }
                else
                {
                    if (parent != -1) walk.Add(parent);
                }
            }
            
            return walk;
        }
        
        private sealed class Dsu
        {
            private readonly int[] _parent;
            private readonly byte[] _rank;
            public Dsu(int n)
            {
                _parent = new int[n];
                _rank = new byte[n];
                for (var i = 0; i < n; i++) _parent[i] = i;
            }
            private int Find(int x)
            {
                if (_parent[x] != x) _parent[x] = Find(_parent[x]);
                return _parent[x];
            }
            public bool Union(int a, int b)
            {
                int ra = Find(a), rb = Find(b);
                if (ra == rb) return false;
                if (_rank[ra] < _rank[rb]) _parent[ra] = rb;
                else if (_rank[ra] > _rank[rb]) _parent[rb] = ra;
                else { _parent[rb] = ra; _rank[ra]++; }
                return true;
            }
        }
    }
}
