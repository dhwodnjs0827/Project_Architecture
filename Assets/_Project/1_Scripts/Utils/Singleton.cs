using System;

/// <summary>
/// 순수 C# 싱글톤 베이스 클래스
/// 데이터 관리, 서비스 클래스 등에 사용
/// </summary>
public abstract class Singleton<T> where T : class, new()
{
    private static readonly Lazy<T> lazyInstance = new Lazy<T>(() => new T());

    public static T Instance => lazyInstance.Value;

    /// <summary>
    /// 인스턴스가 생성되었는지 여부 (Lazy 초기화 전 체크용)
    /// </summary>
    public static bool IsInitialized => lazyInstance.IsValueCreated;
}