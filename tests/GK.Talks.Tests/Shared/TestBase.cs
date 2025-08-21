using GK.Talks.Application.Commands;
using GK.Talks.Core.Aggregates.SpeakerAggregate;
using GK.Talks.Core.Interfaces;
using GK.Talks.Core.ValueObjects;
using GK.Talks.Infrastructure.Data;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GK.Talks.Tests.Shared;

public abstract class TestBase
{
    protected MockRepository MockRepository = default!;
    protected Mock<IRepository<Speaker>> SpeakerRepoMock = default!;
    protected Mock<IMediator> MediatorMock = default!;
    protected Mock<IRegistrationFeeCalculator> FeeCalculatorMock = default!;
    protected List<ISpeakerQualificationStrategy> QualificationStrategies = default!;
    protected RegistrationRulesOptions RulesOptions = default!;

    [SetUp]
    public virtual void Setup()
    {
        MockRepository = new MockRepository(MockBehavior.Loose);
        SpeakerRepoMock = MockRepository.Create<IRepository<Speaker>>();
        MediatorMock = MockRepository.Create<IMediator>();
        FeeCalculatorMock = MockRepository.Create<IRegistrationFeeCalculator>();
        QualificationStrategies = new List<ISpeakerQualificationStrategy>();
        RulesOptions = new RegistrationRulesOptions
        {
            OutdatedTechnologies = new List<string>(),
            MinimumInternetExplorerMajorVersion = 9,
            BlockedEmailDomains = new List<string>()
        };
    }

    [TearDown]
    public virtual void TearDown()
    {
        //MockRepository.VerifyAll();
    }

    protected static AppDbContext CreateInMemoryContext(out SqliteConnection connection)
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        AppDbContext ctx = new(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    protected static SessionDto BuildSession(string title = "T", string desc = "D") => new(title, desc);

    protected static RegisterSpeakerCommand BuildRegisterCommand(
        string first = "Jane",
        string last = "Doe",
        string email = "jane@GK.com",
        int experience = 5,
        bool hasBlog = false,
        string? blogUrl = null,
        List<string>? certifications = null,
        string employer = "GK",
        List<SessionDto>? sessions = null,
        string browserName = "Chrome",
        int browserMajor = 90)
    {
        return new RegisterSpeakerCommand(
            first,
            last,
            email,
            experience,
            hasBlog,
            blogUrl,
            certifications ?? [],
            employer,
            sessions ?? [BuildSession()],
            browserName,
            browserMajor
        );
    }

    protected static Speaker BuildDomainSpeaker(
        string first = "Jane",
        string last = "Doe",
        string email = "jane@GK.com",
        int experience = 5,
        bool hasBlog = false,
        string? blogUrl = null,
        List<string>? certifications = null,
        string employer = "GK",
        List<Session>? sessions = null,
        Browser? browser = null)
    {
        FullName full = FullName.Create(first, last);
        Email emailVo = Email.Create(email);
        List<Session> sess = sessions ?? [new Session("T", "D")];
        Browser browserVo = browser ?? Browser.Create(null, null);

        return Speaker.Create(full, emailVo, experience, hasBlog, blogUrl, certifications ?? [], employer, sess, browserVo);
    }
}