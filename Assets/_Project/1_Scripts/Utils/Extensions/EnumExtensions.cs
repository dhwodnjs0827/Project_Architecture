using System;

/// <summary>
/// enum 확장 메서드
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// enum을 int로 변환
    /// </summary>
    public static int ToInt<T>(this T value) where T : Enum
    {
        return Convert.ToInt32(value);
    }

    /// <summary>
    /// 다음 enum 값 반환 (순환)
    /// </summary>
    public static T Next<T>(this T value) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        int index = Array.IndexOf(values, value);
        return values[(index + 1) % values.Length];
    }

    /// <summary>
    /// 이전 enum 값 반환 (순환)
    /// </summary>
    public static T Previous<T>(this T value) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        int index = Array.IndexOf(values, value);
        return values[(index - 1 + values.Length) % values.Length];
    }

    /// <summary>
    /// 랜덤 Enum 값 반환
    /// </summary>
    public static T GetRandom<T>() where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        return values[UnityEngine.Random.Range(0, values.Length)];
    }
}