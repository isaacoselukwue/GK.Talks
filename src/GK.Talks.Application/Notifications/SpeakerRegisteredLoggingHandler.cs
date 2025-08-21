using GK.Talks.Core.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GK.Talks.Application.Notifications;
internal class SpeakerRegisteredLoggingHandler(ILogger<SpeakerRegisteredLoggingHandler> logger) : INotificationHandler<SpeakerRegisteredEvent>
{
    private readonly ILogger<SpeakerRegisteredLoggingHandler> _logger = logger;

    public Task Handle(SpeakerRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SpeakerRegisteredEvent: Id={Id} Fee={Fee} Msg={Msg}",
            notification.SpeakerId, notification.RegistrationFee, notification.Message);
        return Task.CompletedTask;
    }
}