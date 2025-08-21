using GK.Talks.Core.Aggregates.SpeakerAggregate;
using GK.Talks.Infrastructure.Data;
using GK.Talks.Tests.Shared;
using Microsoft.Data.Sqlite;

namespace GK.Talks.Tests.Integration.Infrastructure;
[TestFixture]
public class SpeakerRepositoryIntegrationTests : TestBase
{
    [Test]
    public void AddAndQuery_ByEmailAndId_ReturnsSpeaker()
    {
        var ctx = CreateInMemoryContext(out SqliteConnection conn);
        try
        {
            SpeakerRepository repo = new(ctx);

            Speaker speaker = BuildDomainSpeaker();
            var added = repo.AddAsync(speaker).Result;
            ctx.SaveChanges();

            var byId = repo.GetByIdAsync(speaker.Id).Result;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(byId, Is.Not.Null);
                Assert.That(speaker.Email.Address, Is.EqualTo(byId!.Email.Address));
            }

            var byEmail = repo.GetByEmailAsync(speaker.Email.Address).Result;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(byEmail, Is.Not.Null);
                Assert.That(speaker.Id, Is.EqualTo(byEmail!.Id));
            }
        }
        finally
        {
            conn.Dispose();
        }
    }
}