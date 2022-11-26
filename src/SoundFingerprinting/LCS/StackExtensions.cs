namespace SoundFingerprinting.LCS;

using System.Collections.Generic;
using System.Linq;

internal static class StackExtensions
{
    public static bool TryPop<T>(this Stack<T> s, out T? result)
    {
        result = default;
        if (s.Any())
        {
            result = s.Pop();
            return true;
        }

        return false;
    }

    public static bool TryPeek<T>(this Stack<T> s, out T? result)
    {
        result = default;
        if (s.Any())
        {
            result = s.Peek();
            return true;
        }

        return false;
    }
}