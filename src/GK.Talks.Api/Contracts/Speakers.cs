using GK.Talks.Core.Aggregates.SpeakerAggregate;

namespace GK.Talks.Api.Contracts;
public record SessionDto(string Title, string Description);
public record RegisterSpeakerDto(
    string FirstName,
    string LastName,
    string Email,
    int Experience,
    bool HasBlog,
    string? BlogUrl,
    List<string>? Certifications,
    string Employer,
    List<SessionDto>? Sessions
);
public record SpeakerDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    int ExperienceInYears,
    bool HasBlog,
    string? BlogUrl,
    List<string> Certifications,
    string Employer,
    int RegistrationFee,
    List<SessionDto> Sessions
);

internal static class SpeakerMappings
{
    public static SessionDto ToDto(this Session s) => new(s.Title, s.Description);

    public static SpeakerDto ToDto(this Speaker s) => new(
            s.Id,
            s.Name?.FirstName ?? string.Empty,
            s.Name?.LastName ?? string.Empty,
            s.Email?.Address ?? string.Empty,
            s.ExperienceInYears,
            s.HasBlog,
            s.BlogUrl,
            s.Certifications?.ToList() ?? [],
            s.Employer ?? string.Empty,
            s.RegistrationFee,
            s.Sessions?.Where(ss => ss.Approved).Select(ss => ss.ToDto()).ToList() ?? []
        );

    public static List<SpeakerDto> ToDtos(this IEnumerable<Speaker> speakers) =>
        speakers?.Select(s => s.ToDto()).ToList() ?? [];
}