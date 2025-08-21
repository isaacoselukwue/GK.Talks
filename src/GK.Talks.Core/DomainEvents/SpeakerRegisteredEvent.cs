using MediatR;

namespace GK.Talks.Core.DomainEvents;
public record SpeakerRegisteredEvent(int SpeakerId, int RegistrationFee, string Message) : INotification;