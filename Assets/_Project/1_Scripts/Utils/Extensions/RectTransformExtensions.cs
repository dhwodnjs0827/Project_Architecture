using UnityEngine;

/// <summary>
/// RectTransform 확장 메서드
/// </summary>
public static class RectTransformExtensions
{
    /// <summary>
    /// RectTransform의 월드 좌표계 Rect 반환
    /// </summary>
    public static Rect GetWorldRect(this RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // corners[0] = 좌하단, corners[2] = 우상단
        Vector3 bottomLeft = corners[0];
        Vector3 topRight = corners[2];

        return new Rect(
            bottomLeft.x,
            bottomLeft.y,
            topRight.x - bottomLeft.x,
            topRight.y - bottomLeft.y
        );
    }

    /// <summary>
    /// RectTransform의 스크린 좌표계 Rect 반환
    /// </summary>
    public static Rect GetScreenRect(this RectTransform rectTransform, Camera camera = null)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // 카메라가 없으면 메인 카메라 사용
        if (camera == null)
            camera = Camera.main;

        // 월드 좌표를 스크린 좌표로 변환
        Vector3 bottomLeft = camera.WorldToScreenPoint(corners[0]);
        Vector3 topRight = camera.WorldToScreenPoint(corners[2]);

        return new Rect(
            bottomLeft.x,
            bottomLeft.y,
            topRight.x - bottomLeft.x,
            topRight.y - bottomLeft.y
        );
    }

    /// <summary>
    /// RectTransform의 로컬 좌표계 Rect 반환 (중심점 기준)
    /// </summary>
    public static Rect GetLocalRect(this RectTransform rectTransform)
    {
        return rectTransform.rect;
    }

    /// <summary>
    /// 특정 월드 좌표가 RectTransform 영역 내에 있는지 확인
    /// </summary>
    public static bool ContainsWorldPoint(this RectTransform rectTransform, Vector3 worldPoint)
    {
        return rectTransform.GetWorldRect().Contains(worldPoint);
    }

    /// <summary>
    /// 특정 스크린 좌표가 RectTransform 영역 내에 있는지 확인
    /// </summary>
    public static bool ContainsScreenPoint(this RectTransform rectTransform, Vector2 screenPoint, Camera camera = null)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, camera);
    }
}