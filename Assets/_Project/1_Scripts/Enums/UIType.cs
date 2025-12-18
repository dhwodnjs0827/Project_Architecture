/// <summary>
/// UI 종류
/// <para>각 UI 타입 값: Sort Order (값이 클 수록 상위에 렌더링)</para>
/// </summary>
public enum UIType
{
    HUD = 0, // HP, 점수, 미니맵 등
    UI = 100, // 메인 메뉴, 인벤토리, 설정 등
    Popup = 200, // 확인 창, 보상 창, 알림 팝업 등
    Tooltip = 300, // 아이템 설명, 툴팁 등
    Loading = 400, // 로딩 화면 페이드 등
    System = 500, // 시스템 알림, 에러 메시지, 토스트 메시지 등
}