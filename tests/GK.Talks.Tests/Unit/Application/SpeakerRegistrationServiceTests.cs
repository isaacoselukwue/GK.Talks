using GK.Talks.Application.Commands;
using GK.Talks.Application.Services;
using GK.Talks.Core.Aggregates.SpeakerAggregate;
using GK.Talks.Core.DomainEvents;
using GK.Talks.Tests.Shared;
using Moq;

namespace GK.Talks.Tests.Unit.Application;
[TestFixture]
public class SpeakerRegistrationServiceTests : TestBase
{
    private SpeakerRegistrationService _service = default!;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        FeeCalculatorMock.Setup(f => f.CalculateFee(It.IsAny<int>())).Returns(0);
    }

    private void CreateService()
    {
        _service = new SpeakerRegistrationService(
            SpeakerRepoMock.Object,
            QualificationStrategies,
            FeeCalculatorMock.Object,
            Microsoft.Extensions.Options.Options.Create(RulesOptions),
            MediatorMock.Object
        );
    }

    [Test]
    public async Task Register_NewSpeaker_HappyPath_AddsAndPublishes()
    {
        RegisterSpeakerCommand cmd = BuildRegisterCommand();
        SpeakerRepoMock.Setup(r => r.GetByEmailAsync(cmd.Email)).ReturnsAsync((Speaker?)null);

        SimpleStrategy strat = new(true);
        QualificationStrategies.Add(strat);

        SpeakerRepoMock.Setup(r => r.AddAsync(It.IsAny<Speaker>()))
            .ReturnsAsync((Speaker s) =>
            {
                var idProp = s.GetType().GetProperty("Id");
                if (idProp != null && idProp.CanWrite)
                    idProp.SetValue(s, 123);
                else
                {
                    var fld = s.GetType().GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    fld?.SetValue(s, 123);
                }
                return s;
            });

        SpeakerRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        MediatorMock.Setup(m => m.Publish(It.IsAny<SpeakerRegisteredEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        CreateService();

        var result = await _service.RegisterAsync(cmd, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(123));
        }

        MediatorMock.Verify(m => m.Publish(It.IsAny<SpeakerRegisteredEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        SpeakerRepoMock.Verify(r => r.AddAsync(It.IsAny<Speaker>()), Times.Once);
        SpeakerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task Register_ExistingSpeaker_Idempotent_ReturnsSameId_NoPublish()
    {
        RegisterSpeakerCommand cmd = BuildRegisterCommand();
        Speaker existing = BuildDomainSpeaker();
        existing.AddSessions(cmd.Sessions.Select(s => new Session(s.Title, s.Description)).ToList());
        var idProp = existing.GetType().GetProperty("Id");
        if (idProp != null && idProp.CanWrite) idProp.SetValue(existing, 77);
        else existing.GetType().GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(existing, 77);

        SpeakerRepoMock.Setup(r => r.GetByEmailAsync(cmd.Email)).ReturnsAsync(existing);

        CreateService();

        var result = await _service.RegisterAsync(cmd, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(77));
        }

        MediatorMock.Verify(m => m.Publish(It.IsAny<SpeakerRegisteredEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        SpeakerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task Register_ExistingSpeaker_WithNewSession_AddsAndPublishes()
    {
        RegisterSpeakerCommand cmd = BuildRegisterCommand();
        Speaker existing = BuildDomainSpeaker(sessions:
        [
            new Session("OtherTopic", "Different description")
        ]);
        var idProp = existing.GetType().GetProperty("Id");
        if (idProp != null && idProp.CanWrite) idProp.SetValue(existing, 88);
        else existing.GetType().GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(existing, 88);

        SpeakerRepoMock.Setup(r => r.GetByEmailAsync(cmd.Email)).ReturnsAsync(existing);
        SpeakerRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        MediatorMock.Setup(m => m.Publish(It.IsAny<SpeakerRegisteredEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        CreateService();

        var result = await _service.RegisterAsync(cmd, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(88));
        }

        MediatorMock.Verify(m => m.Publish(It.IsAny<SpeakerRegisteredEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        SpeakerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task Register_NewSpeaker_HappyPath_AddsAndPublishes_WithEventPayload()
    {
        RegisterSpeakerCommand cmd = BuildRegisterCommand();
        SpeakerRepoMock.Setup(r => r.GetByEmailAsync(cmd.Email)).ReturnsAsync((Speaker?)null);

        QualificationStrategies.Add(new SimpleStrategy(true));

        SpeakerRepoMock.Setup(r => r.AddAsync(It.IsAny<Speaker>()))
            .ReturnsAsync((Speaker s) =>
            {
                var idProp = s.GetType().GetProperty("Id");
                if (idProp != null && idProp.CanWrite) idProp.SetValue(s, 123);
                else
                {
                    var fld = s.GetType().GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    fld?.SetValue(s, 123);
                }
                return s;
            });

        SpeakerRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        SpeakerRegisteredEvent? captured = null;
        MediatorMock.Setup(m => m.Publish(It.IsAny<SpeakerRegisteredEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((evt, ct) => captured = (SpeakerRegisteredEvent)evt)
            .Returns(Task.CompletedTask);

        CreateService();

        var result = await _service.RegisterAsync(cmd, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(123));
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.SpeakerId, Is.EqualTo(123));
            Assert.That(captured.Message.ToLowerInvariant(), Does.Contain("registration fee"));
        }

        MediatorMock.Verify(m => m.Publish(It.IsAny<SpeakerRegisteredEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Register_QualificationFails_ReturnsFailureMessage()
    {
        RegisterSpeakerCommand cmd = BuildRegisterCommand();
        SpeakerRepoMock.Setup(r => r.GetByEmailAsync(cmd.Email)).ReturnsAsync((GK.Talks.Core.Aggregates.SpeakerAggregate.Speaker?)null);

        CreateService();

        var result = await _service.RegisterAsync(cmd, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("does not meet qualification").IgnoreCase);
        }
    }

    [Test]
    public async Task Register_NoSessionsApproved_ReturnsFailure_NoPublish()
    {
        RegisterSpeakerCommand cmd = BuildRegisterCommand(sessions:
        [
            new("Old Tech", "Uses Cobol")
        ]);

        RulesOptions.OutdatedTechnologies = ["cobol"];

        SpeakerRepoMock.Setup(r => r.GetByEmailAsync(cmd.Email)).ReturnsAsync((Speaker?)null);
        QualificationStrategies.Add(new SimpleStrategy(true));

        CreateService();

        var result = await _service.RegisterAsync(cmd, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("No sessions were approved").IgnoreCase);
        }

        MediatorMock.Verify(m => m.Publish(It.IsAny<SpeakerRegisteredEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Register_InvalidSpeaker_ThrowsArgumentException()
    {
        RegisterSpeakerCommand cmd = BuildRegisterCommand(hasBlog: true, blogUrl: null);

        SpeakerRepoMock.Setup(r => r.GetByEmailAsync(cmd.Email)).ReturnsAsync((Speaker?)null);
        QualificationStrategies.Add(new SimpleStrategy(true));
        CreateService();

        Assert.ThrowsAsync<System.ArgumentException>(async () => await _service.RegisterAsync(cmd, CancellationToken.None));
    }

    private class SimpleStrategy(bool pass) : Talks.Core.Interfaces.ISpeakerQualificationStrategy
    {
        private readonly bool _pass = pass;

        public bool IsQualified(Speaker speaker) => _pass;
    }
}