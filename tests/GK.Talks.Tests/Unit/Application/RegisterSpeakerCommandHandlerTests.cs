using GK.Talks.Application.Commands;
using GK.Talks.Application.Models;
using GK.Talks.Application.Services;
using GK.Talks.Tests.Shared;
using Moq;

namespace GK.Talks.Tests.Unit.Application;
[TestFixture]
public class RegisterSpeakerCommandHandlerTests : TestBase
{
    [Test]
    public async Task Handler_DelegatesToService_AndReturnsResult()
    {
        Mock<ISpeakerRegistrationService> svcMock = new();
        RegisterSpeakerCommandHandler handler = new(svcMock.Object);

        RegisterSpeakerCommand cmd = BuildRegisterCommand();
        svcMock.Setup(s => s.RegisterAsync(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(42));

        var result = await handler.Handle(cmd, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(42));
        }
        svcMock.Verify(s => s.RegisterAsync(cmd, It.IsAny<CancellationToken>()), Times.Once);
    }
}