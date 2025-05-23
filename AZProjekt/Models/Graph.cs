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
    }

    public void AddEdge(int sourceIndex, int destinationIndex, double weight)
    {
        AdjacencyList[sourceIndex].Add(new Edge(sourceIndex, destinationIndex, weight));
        AdjacencyList[destinationIndex].Add(new Edge(destinationIndex, sourceIndex, weight));
    }
    
    public List<List<double>> AdjacencyMatrix => AdjacencyList.Select(x => x.Select(y => y.Weight).ToList()).ToList();
}

public record Edge(int Source, int Destination, double Weight);

