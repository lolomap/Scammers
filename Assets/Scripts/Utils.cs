
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;

public static class Utils
{
    public enum Comparison
    {
        Gt,
        Lt,
        GtE,
        LtE,
        Eq,
        Exists
    }
    
    public static readonly ExpressionEvaluator Evaluator = new();
    public static readonly Random Random = new();

    public static bool Compare<T>(T a, T b, Comparison oper) where T : IComparable
    {
        return oper switch
        {
            Comparison.Gt => a.CompareTo(b) > 0,
            Comparison.Lt => a.CompareTo(b) < 0,
            Comparison.GtE => a.CompareTo(b) >= 0,
            Comparison.LtE => a.CompareTo(b) <= 0,
            Comparison.Eq => a.CompareTo(b) == 0,
            Comparison.Exists => a.CompareTo(0) > 0,
            _ => false
        };
    }
    
    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = Random.Next(n + 1);  
            (list[k], list[n]) = (list[n], list[k]);
        }  
    }

}
