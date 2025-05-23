using System.Globalization;
using AZProjekt.Models;

namespace AZProjekt;

public enum InputFormat
{
    Euclidean,
    Matrix
}

public static class Importer
{
    public static Graph Import(string filePath)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);
            return Import(lines);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error importing graph: {ex.Message}");
        }
    }

    public static Graph Import(string[] input)
    {
        try
        {
            var (format, vertexCount) = GetHeaderData(input[0]);

            var graph = new Graph(vertexCount);
            
            for (var i = 0; i < vertexCount; i++) 
                graph.AddVertex($"{(char)('A' + i)}", i);

            switch (format)
            {
                case InputFormat.Euclidean:
                    if (ImportFormatEuclidean(vertexCount, input, graph))
                        throw new Exception("Import failed");
                    break;

                case InputFormat.Matrix:
                    if (ImportFormatMatrix(vertexCount, input, graph))
                        throw new Exception("Import failed");
                    break;

                default:
                    throw new Exception($"Unsupported format: {format}");
            }

            return graph;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error importing graph: {ex.Message}");
        }
    }
    
    private static (InputFormat format, int count) GetHeaderData(string header)
    {
        var firstLine = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (firstLine.Length < 2)
            throw new Exception("Invalid header format.");

        try
        {
            var vertexCount = int.Parse(firstLine[0]);
            var formatString = firstLine[1].ToLower();
            var format = formatString switch
            {
                "a" or "A" => InputFormat.Euclidean,
                "b" or "B" => InputFormat.Matrix,
                _ => throw new Exception($"Unsupported format: {formatString}")
            };
            return (format, vertexCount);
        }
        catch (Exception ex)
        {
            throw new Exception($"Invalid header format. \n {ex.Message}");
        }
    }
    
    private static bool ImportFormatEuclidean(int vertexCount, string[] lines, Graph graph)
    {
        List<(double x, double y)> coordinates = [];

        for (var i = 0; i < vertexCount; i++)
        {
            var parts = lines[i + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                Console.WriteLine($"Invalid coordinate format at line {i + 2}");
                return true;
            }

            var x = double.Parse(parts[0], CultureInfo.InvariantCulture);
            var y = double.Parse(parts[1], CultureInfo.InvariantCulture);
            coordinates.Add((x, y));
        }

        // Calculate Euclidean distances and add edges
        for (var i = 0; i < vertexCount; i++)
        {
            for (var j = i + 1; j < vertexCount; j++)
            {
                var dx = coordinates[i].x - coordinates[j].x;
                var dy = coordinates[i].y - coordinates[j].y;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                // Round to integer for simplicity in this example
                var weight = Math.Round(distance);
                graph.AddEdge(i, j, weight);
            }
        }

        return false;
    }

    private static bool ImportFormatMatrix(int vertexCount, string[] lines, Graph graph)
    {
        for (var i = 0; i < vertexCount; i++)
        {
            var parts = lines[i + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < vertexCount)
            {
                Console.WriteLine($"Invalid distance matrix format at line {i + 2}");
                return true;
            }

            for (var j = 0; j < vertexCount; j++)
            {
                if (i == j) continue; // Skip diagonal

                var distance = double.Parse(parts[j], CultureInfo.InvariantCulture);

                if (i > j)
                    continue;

                var weight = (int)Math.Round(distance);
                graph.AddEdge(i, j, weight);
                graph.AddEdge(j, i, weight);
            }
        }

        return false;
    }
}