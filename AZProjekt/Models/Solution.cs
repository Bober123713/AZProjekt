using System.Globalization;
using System.Text;

namespace Tuchu.Models;

public struct Solution(double cost, List<int> tour)
{
    public double Cost { get; set; } = cost;
    public List<int> Tour { get; set; } = tour;

    public static double operator *(int a, Solution b) => 
        b.Cost * a;

    public static bool operator <=(Solution a, double b) => 
        a.Cost <= b;

    public static bool operator >=(Solution a, double b) =>
        a.Cost >= b;

    public override string ToString()
    {
        var sb = new StringBuilder(Cost.ToString(CultureInfo.InvariantCulture));
        sb.AppendLine();
        foreach(var t in Tour)
            sb.Append($"{t} ");
        return sb.ToString();
    }
}