namespace GK.Talks.Core.ValueObjects;
public sealed class Browser
{
    public enum BrowserName { Unknown, InternetExplorer, Chrome, Firefox, Edge, Safari }

    public BrowserName Name { get; }
    public int MajorVersion { get; }

    private Browser(BrowserName name, int majorVersion)
    {
        Name = name;
        MajorVersion = majorVersion;
    }

    public static Browser Create(string? name, int? majorVersion)
    {
        if (string.IsNullOrWhiteSpace(name)) return new Browser(BrowserName.Unknown, majorVersion ?? 0);
        if (!Enum.TryParse<BrowserName>(name, true, out var n)) n = BrowserName.Unknown;
        return new Browser(n, majorVersion ?? 0);
    }

    public override string ToString() => $"{Name}/{MajorVersion}";
}