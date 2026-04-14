using System.Globalization;

namespace Egov.Integrations.MConnect.Events;

internal static class TimestampFormatter
{
    private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

    /// <summary>
    /// Converts a <see cref="DateTimeOffset"/> value to a string using RFC-3339 format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The sub-second precision in the result is determined by the first of the following conditions
    /// to be met:
    /// If the value is a whole number of seconds, the result contains no fractional-second
    /// indicator at all.
    /// If the sub-second value is a whole number of milliseconds, the result will contain three
    /// digits of sub-second precision.
    /// If the sub-second value is a whole number of microseconds, the result will contain six
    /// digits of sub-second precision.
    /// Otherwise, the result will contain 7 digits of sub-second precision. (This is the maximum
    /// precision of <see cref="DateTimeOffset"/>.)
    /// </para>
    /// <para>
    /// If the UTC offset is zero, this is represented as a suffix of 'Z';
    /// otherwise, the offset is represented in the "+HH:mm" or "-HH:mm" format.
    /// </para>
    /// </remarks>
    /// <param name="value">The value to convert to an RFC-3339 format string.</param>
    /// <returns>The formatted string.</returns>
    public static string Format(DateTimeOffset value)
    {
        var ticks = value.Ticks;
        var formatString = value.Offset.Ticks == 0 ?
            // UTC+0 branch: hard-code 'Z'
            (ticks % TimeSpan.TicksPerSecond == 0 ? "yyyy-MM-dd'T'HH:mm:ss'Z'" :
                ticks % TimeSpan.TicksPerMillisecond == 0 ? "yyyy-MM-dd'T'HH:mm:ss.fff'Z'" :
            ticks % TicksPerMicrosecond == 0 ? "yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'" :
            "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'") :
            // Non-UTC branch: use zzz to format the offset
            (ticks % TimeSpan.TicksPerSecond == 0 ? "yyyy-MM-dd'T'HH:mm:sszzz" :
            ticks % TimeSpan.TicksPerMillisecond == 0 ? "yyyy-MM-dd'T'HH:mm:ss.fffzzz" :
            ticks % TicksPerMicrosecond == 0 ? "yyyy-MM-dd'T'HH:mm:ss.ffffffzzz" :
            "yyyy-MM-dd'T'HH:mm:ss.fffffffzzz");
        return value.ToString(formatString, CultureInfo.InvariantCulture);
    }
}