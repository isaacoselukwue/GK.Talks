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

public class EmailAndBrowserQualificationStrategy : ISpeakerQualificationStrategy
{
    private readonly HashSet<string> _blocked;
    private readonly int _minIe;

    public EmailAndBrowserQualificationStrategy(IOptions<RegistrationRulesOptions> options)
    {
        var blocked = options?.Value?.BlockedEmailDomains ?? System.Linq.Enumerable.Empty<string>();
        _blocked = new HashSet<string>(blocked.Select(d => (d ?? string.Empty).Trim()), StringComparer.OrdinalIgnoreCase);
        _minIe = options?.Value?.MinimumInternetExplorerMajorVersion ?? 0;
    }

    public bool IsQualified(Speaker speaker)
    {
        if (speaker?.Email == null) return false;

        string email = speaker.Email.Address;
        string[] parts = email.Split('@', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return false;
        string domain = parts[^1].Trim();

        bool domainAllowed = !_blocked.Contains(domain);

        var browser = speaker.Browser;
        bool browserOk = true;
        if (browser is not null)
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