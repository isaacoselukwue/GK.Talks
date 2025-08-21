using GK.Talks.Application.Models;
using GK.Talks.Core.Aggregates.SpeakerAggregate;
using GK.Talks.Core.Interfaces;
using MediatR;

namespace GK.Talks.Application.Queries;
internal class GetSpeakerByIdQueryHandler(IRepository<Speaker> repo) : IRequestHandler<GetSpeakerByIdQuery, Result<Speaker>>
{
    private readonly IRepository<Speaker> _repo = repo;

    public async Task<Result<Speaker>> Handle(GetSpeakerByIdQuery request, CancellationToken cancellationToken)
    {
        Speaker? speaker = await _repo.GetByIdAsync(request.Id);
        if (speaker is null) return Result<Speaker>.Failure("Speaker does not exist");

        return Result<Speaker>.Success(speaker);
    }
}