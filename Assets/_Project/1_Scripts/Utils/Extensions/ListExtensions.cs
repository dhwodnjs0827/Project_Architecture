using System.Collections.Generic;

/// <summary>
/// List 확장 메서드
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// 랜덤 요소 반환
    /// </summary>
    public static T GetRandom<T>(this IList<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    /// <summary>
    /// 리스트 셔플
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}