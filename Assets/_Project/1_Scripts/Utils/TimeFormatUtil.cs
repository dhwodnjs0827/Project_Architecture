/// <summary>
/// 시간 표시 유틸 클래스
/// </summary>
public static class TimeFormatUtil
{
    /// <summary>
    /// MM:SS 형식 (03:45)
    /// </summary>
    public static string ToMMSS(float seconds)
    {
        if (seconds < 0)
        {
            seconds = 0;
        }

        var min = (int)(seconds / 60);
        var sec = (int)(seconds % 60);

        return $"{min:D2}:{sec:D2}";
    }

    /// <summary>
    /// HH:MM:SS 형식 (01:30:00)
    /// </summary>
    public static string ToHHMMSS(float seconds)
    {
        if (seconds < 0)
        {
            seconds = 0;
        }

        var hour = (int)(seconds / 3600);
        var min = (int)((seconds % 3600) / 60);
        var sec = (int)(seconds % 60);

        return $"{hour:D2}:{min:D2}:{sec:D2}";
    }

    /// <summary>
    /// SS 형식 - 초만 표시 (45)
    /// </summary>
    public static string ToSS(float seconds)
    {
        if (seconds < 0)
        {
            seconds = 0;
        }

        var sec = (int)seconds;

        return $"{sec:D2}";
    }

    /// <summary>
    /// SS.ms 형식 - 밀리초 포함 (12.34)
    /// </summary>
    public static string ToSSms(float seconds)
    {
        if (seconds < 0)
        {
            seconds = 0;
        }

        var sec = (int)seconds;
        var ms = (int)((seconds - sec) * 100);

        return $"{sec:D2}.{ms:D2}";
    }

    /// <summary>
    /// MM:SS.ms 형식 - 밀리초 포함 (03:45.67)
    /// </summary>
    public static string ToMMSSms(float seconds)
    {
        if (seconds < 0)
        {
            seconds = 0;
        }

        var min = (int)(seconds / 60);
        var sec = (int)(seconds % 60);
        var ms = (int)((seconds - (int)seconds) * 100);

        return $"{min:D2}:{sec:D2}.{ms:D2}";
    }

    /// <summary>
    /// 자동 포맷 - 시간에 따라 적절한 포맷 선택
    /// <para>1시간 이상: HH:MM:SS</para>
    /// <para>1분 이상: MM:SS</para>
    /// <para>1분 미만: SS</para>
    /// </summary>
    public static string ToAuto(float seconds)
    {
        if (seconds < 0)
        {
            seconds = 0;
        }

        return seconds switch
        {
            >= 3600 => ToHHMMSS(seconds),
            >= 60 => ToMMSS(seconds),
            _ => ToSS(seconds)
        };
    }

    /// <summary>
    /// 자동 포맷 (밀리초 포함)
    /// <para>1시간 이상: HH:MM:SS</para>
    /// <para>1분 이상: MM:SS.ms</para>
    /// <para>1분 미만: SS.ms</para>
    /// </summary>
    public static string ToAutoMs(float seconds)
    {
        if (seconds < 0)
        {
            seconds = 0;
        }

        return seconds switch
        {
            >= 3600 => ToHHMMSS(seconds),
            >= 60 => ToMMSSms(seconds),
            _ => ToSSms(seconds)
        };
    }
}