using GK.Talks.Core.Interfaces;
using GK.Talks.Core.ValueObjects;

namespace GK.Talks.Core.Aggregates.SpeakerAggregate;
public class Speaker
{
    public int Id { get; private set; }
    public FullName Name { get; private set; }
    public Email Email { get; private set; }
    public Browser Browser { get; private set; }
    public int ExperienceInYears { get; private set; }
    public bool HasBlog { get; private set; }
    public string? BlogUrl { get; private set; }
    public IReadOnlyCollection<string> Certifications => _certifications.AsReadOnly();
    private readonly List<string> _certifications;
    public string Employer { get; private set; }
    public int RegistrationFee { get; private set; }
    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();
    private readonly List<Session> _sessions;

    private Speaker() 
    {
        Name = default!;
        Email = default!;
        Employer = string.Empty;
        _certifications = [];
        _sessions = [];
        Browser = Browser.Create(null, null);
    }

    private Speaker(
        FullName name,
        Email email,
        int experience,
        bool hasBlog,
        string? blogUrl,
        List<string> certifications,
        string employer,
        List<Session> sessions,
        Browser browser)
    {
        Name = name;
        Email = email;
        ExperienceInYears = experience;
        HasBlog = hasBlog;
        BlogUrl = blogUrl;
        _certifications = certifications ?? [];
        Employer = employer;
        _sessions = sessions ?? [];
        Browser = browser ?? Browser.Create(null, null);
    }

    public static Speaker Create(FullName name, Email email, int experience, bool hasBlog, string? blogUrl, List<string> certifications, string employer, List<Session> sessions, Browser browser)
    {
        if (experience < 0) throw new ArgumentException("Experience cannot be negative.", nameof(experience));
        if (hasBlog && string.IsNullOrWhiteSpace(blogUrl)) throw new ArgumentException("Blog URL is required if speaker has a blog.", nameof(blogUrl));
        if (sessions is null || sessions.Count == 0) throw new ArgumentException("At least one session is required.", nameof(sessions));

        return new Speaker(name, email, experience, hasBlog, blogUrl, certifications, employer, sessions, browser);
    }

    public void ApproveSessions(IEnumerable<string> outdatedTechnologies)
    {
        if (Sessions.Count == 0) throw new InvalidOperationException("Cannot approve sessions when none are provided.");

        foreach (Session session in _sessions)
        {
            bool isOutdated = outdatedTechnologies.Any(tech =>
                session.Title.Contains(tech, StringComparison.OrdinalIgnoreCase) ||
                session.Description.Contains(tech, StringComparison.OrdinalIgnoreCase));

            if (!isOutdated)
            {
                session.Approve();
            }
        }
    }

    public void CalculateRegistrationFee(IRegistrationFeeCalculator feeCalculator)
    {
        RegistrationFee = feeCalculator.CalculateFee(this.ExperienceInYears);
    }
    public void AddSessions(IEnumerable<Session> sessions)
    {
        if (sessions is null) return;
        foreach (Session s in sessions)
        {
            bool exists = _sessions.Any(es =>
                string.Equals(es.Title, s.Title, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(es.Description, s.Description, StringComparison.OrdinalIgnoreCase));
            if (!exists)
                _sessions.Add(s);
        }
    }
}