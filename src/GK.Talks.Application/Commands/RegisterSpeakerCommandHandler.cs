using GK.Talks.Application.Models;
using GK.Talks.Application.Services;
using MediatR;

namespace GK.Talks.Application.Commands;
public class RegisterSpeakerCommandHandler(
    ISpeakerRegistrationService registrationService) : IRequestHandler<RegisterSpeakerCommand, Result<int>>
{
    private readonly ISpeakerRegistrationService _registrationService = registrationService;

    public async Task<Result<int>> Handle(RegisterSpeakerCommand request, CancellationToken cancellationToken)
        => await _registrationService.RegisterAsync(request, cancellationToken);
}

public class RegistrationRulesOptions
{
    public List<string> OutdatedTechnologies { get; set; } = [];
}