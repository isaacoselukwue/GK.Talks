using GK.Talks.Application.Models;
using MediatR;

namespace GK.Talks.Application.Commands;
public record RegisterSpeakerCommand(
        string FirstName,
        string LastName,
        string Email,
        int Experience,
        bool HasBlog,
        string? BlogUrl,
        List<string> Certifications,
        string Employer,
        List<SessionDto> Sessions,
        string? BrowserName,
        int? BrowserMajorVersion
    ) : IRequest<Result<int>>;

public record SessionDto(string Title, string Description);