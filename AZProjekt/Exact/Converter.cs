namespace AZProjekt.Exact;

public static class Converter
{
    private const int Base = 3;

    public static Dictionary<double, double> ToDictionary { get; set; } = [];
    public static Dictionary<double, double> FromDictionary { get; set; } = [];
    
    public static List<List<double>> Convert(List<List<double>> input)
    {
        ToDictionary = input.SelectMany(l => l)
            .Distinct()
            .Order()
            .Select((x, i) => new { Value = x, Index = i })
            .ToDictionary(x => x.Value, x => Math.Pow(Base, x.Index));
        FromDictionary = ToDictionary
            .ToDictionary(x => x.Value, x => x.Key);
        return input.Select(l => l.Select(x => ToDictionary[x]).ToList()).ToList();
    }
}

public class LocalConverter
{
    private const int Base = 3;
    public Dictionary<double, double> ToDictionary { get; set; } = [];
    public Dictionary<double, double> FromDictionary { get; set; } = [];
    
    public List<List<double>> Convert(List<List<double>> input)
    {
        ToDictionary = input.SelectMany(l => l)
            .Distinct()
            .Order()
            .Select((x, i) => new { Value = x, Index = i })
            .ToDictionary(x => x.Value, x => Math.Pow(Base, x.Index));
        FromDictionary = ToDictionary
            .ToDictionary(x => x.Value, x => x.Key);
        return input.Select(l => l.Select(x => ToDictionary[x]).ToList()).ToList();
    }
}