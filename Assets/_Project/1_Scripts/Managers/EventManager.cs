using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 이벤트 매니저 - 전역 이벤트 시스템
/// </summary>
public static class EventManager
{
    private static readonly IDictionary<EventType, Action> events = new Dictionary<EventType, Action>();
    private static readonly IDictionary<EventType, Delegate> genericEvents = new Dictionary<EventType, Delegate>();

    #region Subscribe

    /// <summary>
    /// 이벤트 구독 (매개변수 없음)
    /// </summary>
    public static void Subscribe(EventType eventType, Action action)
    {
        if (events.TryGetValue(eventType, out var existingAction))
        {
            // 중복 체크
            if (existingAction.GetInvocationList().Contains(action))
            {
                CDebug.LogWarning($"[EventManager] 중복된 이벤트를 등록하려고 했습니다. EventType: {eventType}");
                return;
            }

            events[eventType] = existingAction + action;
        }
        else
        {
            events[eventType] = action;
        }
    }

    /// <summary>
    /// 이벤트 구독 (매개변수 있음)
    /// </summary>
    public static void Subscribe<T>(EventType eventType, Action<T> action)
    {
        if (genericEvents.TryGetValue(eventType, out var existingAction))
        {
            // 중복 체크
            if (existingAction.GetInvocationList().Contains(action))
            {
                CDebug.LogWarning($"[EventManager] 중복된 이벤트를 등록하려고 했습니다. EventType: {eventType}");
                return;
            }

            genericEvents[eventType] = Delegate.Combine(existingAction, action);
        }
        else
        {
            genericEvents[eventType] = action;
        }
    }

    #endregion

    #region Unsubscribe

    /// <summary>
    /// 이벤트 구독 해제 (매개변수 없음)
    /// </summary>
    public static void Unsubscribe(EventType eventType, Action action)
    {
        if (!events.TryGetValue(eventType, out var existingAction))
        {
            CDebug.LogWarning($"[EventManager] 등록된 이벤트가 없습니다. EventType: {eventType}");
            return;
        }

        // 등록된 메서드인지 체크
        if (!existingAction.GetInvocationList().Contains(action))
        {
            CDebug.LogWarning($"[EventManager] 등록되지 않은 이벤트를 해제하려고 했습니다. EventType: {eventType}");
            return;
        }

        var newAction = existingAction - action;

        // 남은 구독자가 없으면 딕셔너리에서 제거
        if (newAction == null)
        {
            events.Remove(eventType);
        }
        else
        {
            events[eventType] = newAction;
        }
    }

    /// <summary>
    /// 이벤트 구독 해제 (매개변수 있음)
    /// </summary>
    public static void Unsubscribe<T>(EventType eventType, Action<T> action)
    {
        if (!genericEvents.TryGetValue(eventType, out var existingAction))
        {
            CDebug.LogWarning($"[EventManager] 등록된 이벤트가 없습니다. EventType: {eventType}");
            return;
        }

        // 등록된 메서드인지 체크
        if (!existingAction.GetInvocationList().Contains(action))
        {
            CDebug.LogWarning($"[EventManager] 등록되지 않은 이벤트를 해제하려고 했습니다. EventType: {eventType}");
            return;
        }

        var newAction = Delegate.Remove(existingAction, action);

        // 남은 구독자가 없으면 딕셔너리에서 제거
        if (newAction == null)
        {
            genericEvents.Remove(eventType);
        }
        else
        {
            genericEvents[eventType] = newAction;
        }
    }

    #endregion

    #region Dispatch

    /// <summary>
    /// 이벤트 발행 (매개변수 없음)
    /// </summary>
    public static void Dispatch(EventType eventType)
    {
        if (events.TryGetValue(eventType, out var action))
        {
            action?.Invoke();
        }
    }

    /// <summary>
    /// 이벤트 발행 (매개변수 있음)
    /// </summary>
    public static void Dispatch<T>(EventType eventType, T eventData)
    {
        if (genericEvents.TryGetValue(eventType, out var action))
        {
            (action as Action<T>)?.Invoke(eventData);
        }
    }

    #endregion


    #region Clear

    /// <summary>
    /// 모든 이벤트 초기화
    /// </summary>
    public static void Clear()
    {
        events.Clear();
        genericEvents.Clear();
    }

    /// <summary>
    /// 특정 이벤트 초기화
    /// </summary>
    public static void Clear(EventType eventType)
    {
        events.Remove(eventType);
        genericEvents.Remove(eventType);
    }

    #endregion
}