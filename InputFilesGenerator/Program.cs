using AZProjekt;

Console.WriteLine("=== Generator Plików Wejściowych ===");

// Typ instancji
string type;
do
{
    Console.Write("Podaj format (a - Euclidean, b - Matrix): ");
    type = Console.ReadLine()?.Trim().ToLower() ?? string.Empty;
} while (type is not ("a" or "b"));

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

// W ramach testowania kodu mozna odkomentowac ponizszy outputDir
//var outputDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\AZProjekt"));
var outputDir = Path.Combine(Directory.GetCurrentDirectory());
//var outputDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;


Directory.CreateDirectory(outputDir);

for (var i = 0; i < count; i++)
{
    var lines = type == "a"
        ? GenerateEuclideanFormat(size)
        : GenerateMatrixFormat(size);

    var filename = Path.Combine(outputDir, $"input_{type}_{size}_{i + 1}.txt");
    File.WriteAllLines(filename, lines);
}

Console.WriteLine($"\nWygenerowano {count} plików w katalogu: {outputDir}");
Console.WriteLine("Naciśnij Enter, aby zakończyć.");
Console.ReadLine();
return;

string[] GenerateEuclideanFormat(int euclideanSize) => 
    Generator.Generate(euclideanSize, format: InputFormat.Euclidean, randomSeed: Guid.NewGuid().GetHashCode());

string[] GenerateMatrixFormat(int matrixSize) => 
    Generator.Generate(matrixSize, format: InputFormat.Matrix, randomSeed: Guid.NewGuid().GetHashCode());
