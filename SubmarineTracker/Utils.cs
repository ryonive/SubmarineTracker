using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Utility;
using SubmarineTracker.Data;

namespace SubmarineTracker;

public static class Utils
{
    public static string ToStr(SeString content) => content.ToString();
    public static string ToStr(Lumina.Text.SeString content) => content.ToDalamudString().ToString();
    public static string UpperCaseStr(Lumina.Text.SeString content) => string.Join(" ", content.ToDalamudString().ToString().Split(' ').Select(t => string.Concat(t[0].ToString().ToUpper(), t.AsSpan(1))));
    public static string ToTime(TimeSpan time) => $"{(int)time.TotalHours:#00}:{time:mm}:{time:ss}";

    public static string MapToShort(int key) => MapToShort((uint)key);
    public static string MapToShort(uint key)
    {
        return key switch
        {
            1 => "Deep-sea",
            2 => "Sea of Ash",
            3 => "Sea of Jade",
            4 => "Sirensong",
            _ => ""
        };
    }

    public static string MapToThreeLetter(int key) => MapToThreeLetter((uint) key);
    public static string MapToThreeLetter(uint key)
    {
        return key switch
        {
            1 => "DSS",
            2 => "SOA",
            3 => "SOJ",
            4 => "SSS",
            _ => ""
        };
    }

    public static string NumToLetter(uint num)
    {
        var index = (int)(num - 1);  // 0 indexed

        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        var value = "";

        if (index >= letters.Length)
            value += letters[(index / letters.Length) - 1];

        value += letters[index % letters.Length];

        return value;
    }

    public static string FormattedRouteBuild(string name, Submarines.RouteBuild build)
    {
        var route = "No Route";
        if (build.Sectors.Any())
        {
            var startPoint = Submarines.FindVoyageStartPoint(build.Sectors.First());
            route = $"{MapToThreeLetter(build.Map + 1)}: {string.Join(" -> ", build.Sectors.Select(p => NumToLetter(p - startPoint)))}";;
        }

        return $"{name.Replace("%", "%%")} (R: {build.Rank} B: {build.GetSubmarineBuild.BuildIdentifier()})" +
               $"\n{route}";
    }


    public static SeString ErrorMessage(string error)
    {
        return new SeStringBuilder()
               .AddUiForeground("[Submarine Tracker] ", 540)
               .AddUiForeground($"{error}", 17)
               .BuiltString;
    }

    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        if (!dict.TryGetValue(key, out TValue val))
        {
            val = new TValue();
            dict.Add(key, val);
        }

        return val;
    }

    public class ListComparer : IEqualityComparer<List<uint>>
    {
        public bool Equals(List<uint>? x, List<uint>? y)
        {
            if (x == null)
                return false;
            if (y == null)
                return false;

            return x.Count == y.Count && !x.Except(y).Any();
        }

        public int GetHashCode(List<uint> obj)
        {
            var hash = 19;
            foreach (var element in obj.OrderBy(x => x))
            {
                hash = (hash * 31) + element.GetHashCode();
            }

            return hash;
        }
    }
}

public static class StringExt
{
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "...")
    {
        return value?.Length > maxLength
                   ? string.Concat(value.AsSpan(0, maxLength), truncationSuffix)
                   : value;
    }
}

public static class Extensions
{
    public static void Swap<T>(this List<T> list, int i, int j)
    {
        (list[i], list[j]) = (list[j], list[i]);
    }
}
