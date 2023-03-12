using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class MusicTheory
{
    public static readonly int OCTAVE = 12;
    public static float lowPassFrequency = 20000f;

    public static List<int> MajorPentatonicScale = new List<int>() { 0, 2, 4, 7, 9, };
    public static List<int> MinorPentatonicScale = new List<int>() { 0, 3, 5, 7, 10 };

    public static List<int> MajorScale = new List<int>() { 0, 2, 4, 5, 7, 9, 11, };

    public enum Key
    {
        C, C_SHARP, D, D_SHARP,
        E, F, F_SHARP, G, G_SHARP,
        A, A_SHARP, B
    }
    public static bool InScale(List<int> scale, int note, Key key = 0)
    {
        return scale.Contains((note + (int) key) % OCTAVE);
    }

    internal static float RoundToNearestNoteInScale(float note, List<int> scale)
    {
        var octave = Math.Round(note / OCTAVE);
        return (float) (scale.OrderBy(n1=> Math.Abs((n1+octave*OCTAVE) - note)).FirstOrDefault() + octave*OCTAVE);
    }

    internal static int Transpose(int note, int octaves)
    {
        return note + octaves * OCTAVE;
    }

    private static Random Rand = new Random();

    public static List<int> WhiteTangents = new List<int>()
    {
        0,2,4,5,7,9,11
    };

    public static List<int> BlackTangents = new List<int>()
    {
        1,3,6,8,10
    };

    public static T GetRandomElement<T>(this IEnumerable<T> list)
    {
        if (list.Count() == 0) return default(T);
        return list.ElementAt(Rand.Next(list.Count()));
    }
    public static T RemoveAndReturnFirst<T>(this List<T> list)
    {
        T currentFirst = list.First();
        list.RemoveAt(0);
        return currentFirst;
    }

    internal static List<int> TransposeScale(List<int> scale, Key key)
    {
        return scale.Select(n => (n + (int) key) % OCTAVE).ToList();
    }
}
