using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

/// <summary>
/// Custom Debug 유틸 클래스
/// <para>개발 시, CUSTOM_DEBUG 심볼 추가 필수!</para>
/// <para>빌드 시, CUSTOM_DEBUG 심볼 제외 필수!</para>
/// </summary>
public static class CDebug
{
    private const string DEBUG_SYMBOL = "CUSTOM_DEBUG";

    #region Assert

    [Conditional(DEBUG_SYMBOL)]
    public static void Assert(bool condition)
    {
        Debug.Assert(condition);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void Assert(bool condition, object message)
    {
        Debug.Assert(condition, message);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void Assert(bool condition, object message, Object context)
    {
        Debug.Assert(condition, message, context);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void AssertFormat(bool condition, string format, params object[] args)
    {
        Debug.AssertFormat(condition, format, args);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void AssertFormat(bool condition, Object context, string format, params object[] args)
    {
        Debug.AssertFormat(condition, context, format, args);
    }

    #endregion

    #region Log

    [Conditional(DEBUG_SYMBOL)]
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void Log(object message, Object context)
    {
        Debug.Log(message, context);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogFormat(string format, params object[] args)
    {
        Debug.LogFormat(format, args);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogFormat(Object context, string format, params object[] args)
    {
        Debug.LogFormat(context, format, args);
    }

    #endregion

    #region LogWarning

    [Conditional(DEBUG_SYMBOL)]
    public static void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogWarning(object message, Object context)
    {
        Debug.LogWarning(message, context);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogWarningFormat(string format, params object[] args)
    {
        Debug.LogWarningFormat(format, args);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogWarningFormat(Object context, string format, params object[] args)
    {
        Debug.LogWarningFormat(context, format, args);
    }

    #endregion

    #region LogError

    [Conditional(DEBUG_SYMBOL)]
    public static void LogError(object message)
    {
        Debug.LogError(message);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogError(object message, Object context)
    {
        Debug.LogError(message, context);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogErrorFormat(string format, params object[] args)
    {
        Debug.LogErrorFormat(format, args);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogErrorFormat(Object context, string format, params object[] args)
    {
        Debug.LogErrorFormat(context, format, args);
    }

    #endregion

    #region LogException

    [Conditional(DEBUG_SYMBOL)]
    public static void LogException(Exception exception)
    {
        Debug.LogException(exception);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogException(Exception exception, Object context)
    {
        Debug.LogException(exception, context);
    }

    #endregion

    #region LogAssertion

    [Conditional(DEBUG_SYMBOL)]
    public static void LogAssertion(object message)
    {
        Debug.LogAssertion(message);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogAssertion(object message, Object context)
    {
        Debug.LogAssertion(message, context);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogAssertionFormat(string format, params object[] args)
    {
        Debug.LogAssertionFormat(format, args);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void LogAssertionFormat(Object context, string format, params object[] args)
    {
        Debug.LogAssertionFormat(context, format, args);
    }

    #endregion

    #region DrawLine

    [Conditional(DEBUG_SYMBOL)]
    public static void DrawLine(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        Debug.DrawLine(start, end, color);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
    {
        Debug.DrawLine(start, end, color, duration);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool depthTest)
    {
        Debug.DrawLine(start, end, color, duration, depthTest);
    }

    #endregion

    #region DrawRay

    [Conditional(DEBUG_SYMBOL)]
    public static void DrawRay(Vector3 start, Vector3 dir)
    {
        Debug.DrawRay(start, dir);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color)
    {
        Debug.DrawRay(start, dir, color);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
    {
        Debug.DrawRay(start, dir, color, duration);
    }

    [Conditional(DEBUG_SYMBOL)]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration, bool depthTest)
    {
        Debug.DrawRay(start, dir, color, duration, depthTest);
    }

    #endregion
}