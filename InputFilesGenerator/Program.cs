using System;
using System.Globalization;
using System.IO;
using Tuchu;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Generator Plików Wejściowych ===");

        // Typ instancji
        string type;
        do
        {
            Console.Write("Podaj typ instancji (a - Euclidean, b - Matrix): ");
            type = Console.ReadLine()?.Trim().ToLower();
        } while (type != "a" && type != "b");

        // Rozmiar grafu
        int size;
        while (true)
        {
            Console.Write("Podaj liczbę wierzchołków (np. 9): ");
            if (int.TryParse(Console.ReadLine(), out size) && size > 0)
                break;
            Console.WriteLine("Podano nieprawidłową wartość. Spróbuj ponownie.");
        }

        // Liczba plików
        int count;
        while (true)
        {
            Console.Write("Ile plików chcesz wygenerować?: ");
            if (int.TryParse(Console.ReadLine(), out count) && count > 0)
                break;
            Console.WriteLine("Podano nieprawidłową wartość. Spróbuj ponownie.");
        }

        string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Inputs");
        Directory.CreateDirectory(outputDir);

        for (int i = 0; i < count; i++)
        {
            string[] lines = (type == "a")
                ? GenerateEuclideanFormat(size)
                : GenerateMatrixFormat(size);

            string filename = Path.Combine(outputDir, $"input_{type}_{size}_{i + 1}.txt");
            File.WriteAllLines(filename, lines);
        }

        Console.WriteLine($"\nWygenerowano {count} plików w katalogu: {outputDir}");
        Console.WriteLine("Naciśnij Enter, aby zakończyć.");
        Console.ReadLine();
    }

    static string[] ModifyToMatrixFormat(string[] lines, int size)
    {
        lines[0] = $"{size} b"; // podmiana typu
        return lines;
    }
    
    static string[] GenerateEuclideanFormat(int size)
    {
        var rnd = new Random(Guid.NewGuid().GetHashCode());
        var lines = new List<string> { $"{size} a" };

        for (int i = 0; i < size; i++)
        {
            double x = rnd.NextDouble() * 100;
            double y = rnd.NextDouble() * 100;
            lines.Add($"{x.ToString("F3", CultureInfo.InvariantCulture)} {y.ToString("F3", CultureInfo.InvariantCulture)}");
        }

        return lines.ToArray();
    }

    static string[] GenerateMatrixFormat(int size)
    {
        return Generator.Generate(size, randomSeed: Guid.NewGuid().GetHashCode());
    }

}
