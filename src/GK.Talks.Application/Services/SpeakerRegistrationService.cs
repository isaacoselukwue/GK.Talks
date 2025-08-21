using GK.Talks.Application.Commands;
using GK.Talks.Application.Models;
using GK.Talks.Core.Aggregates.SpeakerAggregate;
using GK.Talks.Core.DomainEvents;
using GK.Talks.Core.Interfaces;
using GK.Talks.Core.ValueObjects;
using MediatR;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace GK.Talks.Application.Services;

internal class SpeakerRegistrationService(
    IRepository<Speaker> speakerRepository,
    IEnumerable<ISpeakerQualificationStrategy> qualificationStrategies,
    IRegistrationFeeCalculator feeCalculator,
    IOptions<RegistrationRulesOptions> rulesOptions,
    IMediator mediator) : ISpeakerRegistrationService
{
    private readonly IRepository<Speaker> _speakerRepository = speakerRepository;
    private readonly IEnumerable<ISpeakerQualificationStrategy> _qualificationStrategies = qualificationStrategies;
    private readonly IRegistrationFeeCalculator _feeCalculator = feeCalculator;
    private readonly RegistrationRulesOptions _rules = rulesOptions.Value;
    private readonly IMediator _mediator = mediator;
    public async Task<Result<int>> RegisterAsync(RegisterSpeakerCommand request, CancellationToken cancellationToken)
    {
        List<Session> incomingSessions = MapIncomingSessions(request);

        Speaker? existing = await _speakerRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
            return await HandleExistingSpeakerAsync(existing, incomingSessions, cancellationToken);

        return await HandleNewSpeakerAsync(request, incomingSessions, cancellationToken);
    }

    private static List<Session> MapIncomingSessions(RegisterSpeakerCommand request)
        => request.Sessions?.Select(s => new Session(s.Title, s.Description)).ToList()
           ?? [];

    private async Task<Result<int>> HandleExistingSpeakerAsync(Speaker existing, List<Session> incomingSessions, CancellationToken cancellationToken)
    {
        List<Session> newSessions = [.. incomingSessions
            .Where(ns => !existing.Sessions.Any(es =>
                string.Equals(es.Title, ns.Title, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(es.Description, ns.Description, StringComparison.OrdinalIgnoreCase)))];

        if (newSessions.Count == 0)
        {
            return Result<int>.Success(existing.Id);
        }

        existing.AddSessions(newSessions);
        existing.ApproveSessions(_rules.OutdatedTechnologies);

        if (!existing.Sessions.Any(s => s.Approved))
            return Result<int>.Failure("No sessions were approved.");

        existing.CalculateRegistrationFee(_feeCalculator);
        await _speakerRepository.SaveChangesAsync();

        var msg = $"Added {newSessions.Count} new session(s); registration fee: {existing.RegistrationFee}.";
        await PublishRegisteredEvent(existing.Id, existing.RegistrationFee, msg, cancellationToken);

        return Result<int>.Success(existing.Id);
    }

    private async Task<Result<int>> HandleNewSpeakerAsync(RegisterSpeakerCommand request, List<Session> incomingSessions, CancellationToken cancellationToken)
    {
        var browserVo = Browser.Create(request.BrowserName, request.BrowserMajorVersion);
        var speaker = Speaker.Create(
            FullName.Create(request.FirstName, request.LastName),
            Email.Create(request.Email),
            request.Experience,
            request.HasBlog,
            request.BlogUrl,
            request.Certifications,
            request.Employer,
            incomingSessions,
            browserVo
        );

        var (anyPassed, failedTypeNames) = EvaluateQualifications(speaker);
        if (!anyPassed)
        {
            var reasons = failedTypeNames.Select(FriendlyNameFromType).ToList();
            var message = reasons.Count > 0
                ? $"Speaker does not meet qualification standards: {string.Join("; ", reasons)}."
                : "Speaker does not meet qualification standards.";
            return Result<int>.Failure(message);
        }

        speaker.ApproveSessions(_rules.OutdatedTechnologies);

        if (!speaker.Sessions.Any(s => s.Approved))
            return Result<int>.Failure("No sessions were approved.");

        speaker.CalculateRegistrationFee(_feeCalculator);

        var newSpeaker = await _speakerRepository.AddAsync(speaker);
        await _speakerRepository.SaveChangesAsync();

        string eventMsg = $"Speaker registered; registration fee: {newSpeaker.RegistrationFee}.";
        await PublishRegisteredEvent(newSpeaker.Id, newSpeaker.RegistrationFee, eventMsg, cancellationToken);

        return Result<int>.Success(newSpeaker.Id);
    }

    private (bool anyPassed, IEnumerable<string> failedTypeNames) EvaluateQualifications(Speaker speaker)
    {
        var evals = _qualificationStrategies
            .Select(s => new { Strategy = s, Passed = s.IsQualified(speaker) })
            .ToList();

        bool anyPassed = evals.Any(e => e.Passed);
        IEnumerable<string> failed = evals.Where(e => !e.Passed).Select(e => e.Strategy.GetType().Name).Distinct();
        return (anyPassed, failed);
    }

    private static string FriendlyNameFromType(string typeName)
        => typeName switch
        {
            "ExperienceQualificationStrategy" => "sufficient experience",
            "HasBlogQualificationStrategy" => "has a blog",
            "CertificationsQualificationStrategy" => "enough certifications",
            "EmployerQualificationStrategy" => "allowed employer",
            "EmailDomainQualificationStrategy" => "acceptable email domain",
            "BrowserQualificationStrategy" => "supported browser",
            _ => Regex.Replace(Regex.Replace(typeName, "(QualificationStrategy|Strategy)$", ""), "([a-z])([A-Z])", "$1 $2")
        };

    private Task PublishRegisteredEvent(int id, int fee, string message, CancellationToken cancellationToken)
        => _mediator.Publish(new SpeakerRegisteredEvent(id, fee, message), cancellationToken);

}