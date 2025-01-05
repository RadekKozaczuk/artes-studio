using System.Runtime.CompilerServices;
using UnityEngine;

public static class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float FastDistance2D(float x1, float y1, float x2, float y2)
    {
        float num1 = x1 - x2;
        float num2 = y1 - y2;
        return Mathf.Sqrt(num1 * num1 + num2 * num2);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float FastDistance2D(float x1, float y1, Vector2Int vector)
    {
        float num1 = x1 - vector.x;
        float num2 = y1 - vector.y;
        return Mathf.Sqrt(num1 * num1 + num2 * num2);
    }
    
    /// <summary>
    /// Z coefficient of a will be ignored.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float FastDistance2D(Vector3 a, Vector2Int b)
    {
        float num1 = a.x - b.x;
        float num2 = a.y - b.y;
        return Mathf.Sqrt(num1 * num1 + num2 * num2);
    }
}
