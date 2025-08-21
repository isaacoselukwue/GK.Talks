using GK.Talks.Core.Aggregates.SpeakerAggregate;
using GK.Talks.Core.Interfaces;
using GK.Talks.Core.ValueObjects;
using GK.Talks.Tests.Shared;

namespace GK.Talks.Tests.Unit.Core;
[TestFixture]
public class SpeakerAggregateTests : TestBase
{
    [Test]
    public void Create_WithNegativeExperience_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            BuildDomainSpeaker(experience: -1)
        );
    }

    [Test]
    public void Create_WithHasBlogButNoUrl_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            BuildDomainSpeaker(hasBlog: true, blogUrl: null)
        );
    }

    [Test]
    public void Create_WithNoSessions_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            FullName full = FullName.Create("A", "B");
            Email email = Email.Create("a@b.com");
            Speaker.Create(full, email, 1, false, null, new List<string>(), "Emp", new List<Session>(), Browser.Create(null, null));
        });
    }

    [Test]
    public void AddSessions_DeduplicatesByTitleAndDescription()
    {
        Speaker s = BuildDomainSpeaker();
        Session duplicate = new("T", "D");
        Session unique = new("New", "Desc");
        int before = s.Sessions.Count;
        s.AddSessions(new[] { duplicate, unique }.ToList());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(before + 1, Is.EqualTo(s.Sessions.Count));
            Assert.That(s.Sessions.Any(ss => ss.Title == "New" && ss.Description == "Desc"), Is.True);
        }
    }

    [Test]
    public void ApproveSessions_RejectsOutdated()
    {
        Session outdated = new("Old Tech", "Uses Cobol");
        Session upToDate = new("Modern", "Uses dotnet");
        Speaker s = BuildDomainSpeaker(sessions: [outdated, upToDate]);

        s.ApproveSessions(["cobol"]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(s.Sessions.First(x => x.Title == "Old Tech").Approved, Is.False);
            Assert.That(s.Sessions.First(x => x.Title == "Modern").Approved, Is.True);
        }
    }

    [Test]
    public void CalculateRegistrationFee_UsesProvidedCalculator()
    {
        Speaker s = BuildDomainSpeaker();
        TestFeeCalculator calc = new(123);
        s.CalculateRegistrationFee(calc);
        Assert.That(s.RegistrationFee, Is.EqualTo(123));
    }

    private class TestFeeCalculator(int value) : IRegistrationFeeCalculator
    {
        private readonly int _value = value;

        public int CalculateFee(int experienceInYears) => _value;
    }
}