using GK.Talks.Application.Models;
using GK.Talks.Core.Aggregates.SpeakerAggregate;
using GK.Talks.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GK.Talks.Application.Queries;
internal class GetAllSpeakersQueryHandler(IRepository<Speaker> repo) : IRequestHandler<GetAllSpeakersQuery, Result<List<Speaker>>>
{
    private readonly IRepository<Speaker> _repo = repo;

    public async Task<Result<List<Speaker>>> Handle(GetAllSpeakersQuery request, CancellationToken cancellationToken)
    {
        var speakers = await _repo.GetAll().ToListAsync(cancellationToken);
        return Result<List<Speaker>>.Success(speakers);
    }
}