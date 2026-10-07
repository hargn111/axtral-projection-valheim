using System;
using System.Collections.Generic;
using System.Linq;
namespace AxtralProjection.Core;
public static class TargetSelection
{
    public static string Name(string name) => name.Replace("(Clone)", "").Trim();
    public static HashSet<string> Parse(string names) => new HashSet<string>((names ?? "").Split(',').Select(Name).Where(n=>n.Length > 0), StringComparer.Ordinal);
    public static bool Allowed(string name, bool tree, bool stump, bool clearStumps, ISet<string> additional, ISet<string> excluded)
    {
        name = Name(name);
        if (excluded.Contains(name) || (stump && !clearStumps)) return false;
        return tree || (stump && clearStumps) || additional.Contains(name);
    }
    public static List<T> Nearest<T>(IEnumerable<T> candidates, Func<T, double> forward, int cap) => candidates.OrderBy(forward).Take(Math.Max(0, cap)).ToList();
}
