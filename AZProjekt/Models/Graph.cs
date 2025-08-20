namespace AZProjekt.Models;

public class Graph
{
    public int VertexCount { get; set; }
    public List<Edge>[] AdjacencyList { get; set; }
    public Dictionary<string, int> VertexMap { get; set; }
    public Dictionary<int, string> ReverseVertexMap { get; set; }

    public Graph(int vertexCount)
    {
        VertexCount = vertexCount;
        AdjacencyList = new List<Edge>[vertexCount];
        for (var i = 0; i < vertexCount; i++)
        {
            AdjacencyList[i] = [];
        }

        VertexMap = new Dictionary<string, int>();
        ReverseVertexMap = new Dictionary<int, string>();
    }

    public void AddVertex(string vertexName, int index)
    {
        VertexMap[vertexName] = index;
        ReverseVertexMap[index] = vertexName;
    }

    public void AddEdge(string source, string destination, double weight)
    {
        var sourceIndex = VertexMap[source];
        var destinationIndex = VertexMap[destination];
        AddEdge(sourceIndex, destinationIndex, weight);
        AddEdge(destinationIndex, sourceIndex, weight);
    }

    public void AddEdge(int sourceIndex, int destinationIndex, double weight = 1)
    {
        AdjacencyList[sourceIndex].Add(new Edge(sourceIndex, destinationIndex, weight));
    }

    public List<List<double>> AdjacencyMatrix => GetAdjacencyMatrix();

    private List<List<double>> GetAdjacencyMatrix()
    {
        var matrix = new List<List<double>>();
        foreach (var (vertexEdges, index) in AdjacencyList.Select((x, i) => (x, i)))
        {
            matrix.Add([]);
            foreach (var edge in vertexEdges)
            {
                matrix[index].Add(edge.Weight);
            }
            matrix[index].Insert(index, 0);
        }

        return matrix;
    }
}

public record Edge(int Source, int Destination, double Weight = 1);

