namespace GK.Talks.Infrastructure.Configuration;
public class RegistrationRulesOptions
{
    public List<string> OutdatedTechnologies { get; set; } = [];

    public HashSet<string> AllowedEmployers { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pluralsight",
        "Microsoft",
        "Google"
    };

    public HashSet<string> BlockedEmailDomains { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "aol.com",
        "prodigy.com",
        "compuserve.com"
    };

    public int CertificationsRequired { get; set; } = 3;
    public int MinimumInternetExplorerMajorVersion { get; set; } = 9;
}
