using GK.Talks.Core.Aggregates.SpeakerAggregate;
using GK.Talks.Core.Interfaces;
using GK.Talks.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace GK.Talks.Infrastructure.Services;
public class ExperienceQualificationStrategy : ISpeakerQualificationStrategy
{
    public bool IsQualified(Speaker speaker) => speaker.ExperienceInYears > 10;
}
public class HasBlogQualificationStrategy : ISpeakerQualificationStrategy
{
    public bool IsQualified(Speaker speaker) => speaker.HasBlog;
}
public class CertificationsQualificationStrategy : ISpeakerQualificationStrategy
{
    public bool IsQualified(Speaker speaker) => speaker.Certifications.Count > 3;
}

public class EmployerQualificationStrategy(IOptions<RegistrationRulesOptions> options) : ISpeakerQualificationStrategy
{
    private readonly HashSet<string> _allowed = options.Value.AllowedEmployers;

    public bool IsQualified(Speaker speaker) =>
        !string.IsNullOrWhiteSpace(speaker.Employer) && _allowed.Contains(speaker.Employer);
}

internal class EmailAndBrowserQualificationStrategy : ISpeakerQualificationStrategy
{
    private readonly HashSet<string> _blocked;
    private readonly int _minIe;

    public EmailAndBrowserQualificationStrategy(IOptions<RegistrationRulesOptions> options)
    {
        _blocked = options.Value.BlockedEmailDomains ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _minIe = options.Value.MinimumInternetExplorerMajorVersion;
    }

    public bool IsQualified(Speaker speaker)
    {
        if (speaker?.Email == null) return false;

        var email = speaker.Email.ToString();
        var parts = email.Split('@', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return false;
        var domain = parts[^1];

        var domainAllowed = !_blocked.Contains(domain);

        var browser = speaker.Browser;
        var browserOk = true;
        if (browser != null)
        {
            if (string.Equals(browser.Name.ToString(), "InternetExplorer", StringComparison.OrdinalIgnoreCase)
                && browser.MajorVersion < _minIe)
            {
                browserOk = false;
            }
        }

        return domainAllowed && browserOk;
    }
}