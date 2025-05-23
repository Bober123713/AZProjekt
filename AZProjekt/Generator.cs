using System.Globalization;
using System.Text;

namespace Tuchu;

public static class Generator
{
    public static string[] Generate(
        int vertexCount,
        double maxCoordinateValue = 100.0,
        int? randomSeed = null)
    {
        if (vertexCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(vertexCount), "Vertex count must be positive.");

        var random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
        
        var coordinates = new List<(double x, double y)>();
        for (var i = 0; i < vertexCount; i++)
        {
            var x = random.NextDouble() * maxCoordinateValue;
            var y = random.NextDouble() * maxCoordinateValue;
            coordinates.Add((x, y));
        }

        var distances = new double[vertexCount, vertexCount];
        for (var i = 0; i < vertexCount; i++)
        {
            for (var j = 0; j < vertexCount; j++)
            {
                if (i == j)
                {
                    distances[i, j] = 0.0;
                }
                else
                {
                    var dx = coordinates[i].x - coordinates[j].x;
                    var dy = coordinates[i].y - coordinates[j].y;
                    distances[i, j] = Math.Sqrt(dx * dx + dy * dy);
                }
            }
        }
        
        var lines = new List<string> { $"{vertexCount} b" };
        
        for (var i = 0; i < vertexCount; i++)
        {
            var sb = new StringBuilder();
            
            for (var j = 0; j < vertexCount; j++) 
                sb.Append($" {distances[i, j].ToString("F3", CultureInfo.InvariantCulture)}");

            sb.Remove(0, 1);
            lines.Add(sb.ToString());
        }

        return lines.ToArray();
    }
}