using GK.Talks.Core.Interfaces;
using GK.Talks.Core.ValueObjects;
using System.Text.RegularExpressions;

namespace GK.Talks.Infrastructure.Services;
internal class BrowserDetector : IBrowserDetector
{
    /// <summary>
    /// Source from Mozilla: https://developer.mozilla.org/en-US/docs/Web/API/User-Agent_Client_Hints_API
    /// </summary>
    /// <param name="userAgent"></param>
    /// <returns></returns>
    public Browser Parse(string? userAgent)
    {
        var ua = userAgent ?? string.Empty;

        var m = Regex.Match(ua, @"MSIE\s+(\d+)");
        if (m.Success && int.TryParse(m.Groups[1].Value, out var v)) return Browser.Create("InternetExplorer", v);

        m = Regex.Match(ua, @"rv:(\d+)\.?\d*\).*Trident");
        if (m.Success && int.TryParse(m.Groups[1].Value, out v)) return Browser.Create("InternetExplorer", v);

        m = Regex.Match(ua, @"Edg/(\d+)");
        if (m.Success && int.TryParse(m.Groups[1].Value, out v)) return Browser.Create("Edge", v);

        m = Regex.Match(ua, @"Chrome/(\d+)");
        if (m.Success && int.TryParse(m.Groups[1].Value, out v)) return Browser.Create("Chrome", v);

        m = Regex.Match(ua, @"Firefox/(\d+)");
        if (m.Success && int.TryParse(m.Groups[1].Value, out v)) return Browser.Create("Firefox", v);

        m = Regex.Match(ua, @"Version/(\d+).*Safari");
        if (m.Success && int.TryParse(m.Groups[1].Value, out v)) return Browser.Create("Safari", v);

        return Browser.Create(null, null);
    }
}