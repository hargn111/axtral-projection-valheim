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
        return tree || (stump && clearStumps) || IsUncappedSapling(name) || additional.Contains(name);
    }
    public static bool IsUncappedSapling(string name) => Name(name) == "Beech_small1" || Name(name) == "Beech_small2";
    public static List<T> Nearest<T>(IEnumerable<T> targets, Func<T, double> forward, int maximum, Func<T, bool>? countsTowardLimit = null)
    {
        int remaining = Math.Max(0, maximum);
        var result = new List<T>();
        foreach (var target in targets.OrderBy(forward))
        {
            if (countsTowardLimit == null || countsTowardLimit(target))
            {
                if (remaining == 0) continue;
                remaining--;
            }
            result.Add(target);
        }
        return result;
    }
}
