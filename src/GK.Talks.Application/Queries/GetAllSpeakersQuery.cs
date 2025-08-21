using GK.Talks.Application.Models;
using GK.Talks.Core.Aggregates.SpeakerAggregate;
using MediatR;

namespace GK.Talks.Application.Queries;
public record GetAllSpeakersQuery() : IRequest<Result<List<Speaker>>>;