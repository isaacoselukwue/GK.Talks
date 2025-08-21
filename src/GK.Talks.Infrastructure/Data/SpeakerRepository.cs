using GK.Talks.Core.Aggregates.SpeakerAggregate;
using GK.Talks.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GK.Talks.Infrastructure.Data;
public class SpeakerRepository(AppDbContext db) : IRepository<Speaker>
{
    private readonly AppDbContext _db = db;

    public async Task<Speaker?> GetByIdAsync(int id) =>
        await _db.Speakers.Include(s => s.Sessions).FirstOrDefaultAsync(s => s.Id == id);

    public Task<Speaker> AddAsync(Speaker entity)
    {
        _db.Speakers.Add(entity);
        return Task.FromResult(entity);
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();

    public async Task<Speaker?> GetByEmailAsync(string email) =>
        await _db.Speakers.Include(s => s.Sessions)
                          .FirstOrDefaultAsync(s => s.Email != null && s.Email.Address == email);

    public IQueryable<Speaker> GetAll() => _db.Speakers.Include(s => s.Sessions);
}