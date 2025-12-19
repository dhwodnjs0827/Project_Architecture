/// <summary>
/// 오브젝트 풀링 인터페이스
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// pool에서 가져올 때 호출
    /// </summary>
    /// <remarks>
    /// Get() 호출 -> SetActive(ture) -> IPoolable.OnGet() 호출 -> 오브젝트 반환
    /// </remarks>
    public void OnGet();

    /// <summary>
    /// pool에 반환할 때 호출
    /// </summary>
    /// <remarks>
    /// Release() 호출 -> IPoolable.OnRelease() 호출 -> SetActive(false) -> pool에 반환
    /// </remarks>
    public void OnRelease();
}