using GK.Talks.Application.Commands;
using GK.Talks.Application.Models;

namespace GK.Talks.Application.Services;
public interface ISpeakerRegistrationService
{
    Task<Result<int>> RegisterAsync(RegisterSpeakerCommand command, CancellationToken cancellationToken);
}