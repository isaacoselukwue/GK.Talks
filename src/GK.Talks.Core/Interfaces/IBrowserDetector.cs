namespace GK.Talks.Core.Interfaces;
public interface IBrowserDetector
{
    ValueObjects.Browser Parse(string? userAgent);
}