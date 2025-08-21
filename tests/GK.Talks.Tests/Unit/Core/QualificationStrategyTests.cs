using GK.Talks.Core.Aggregates.SpeakerAggregate;
using GK.Talks.Core.ValueObjects;
using GK.Talks.Infrastructure.Configuration;
using GK.Talks.Infrastructure.Services;
using GK.Talks.Tests.Shared;
using Microsoft.Extensions.Options;

namespace GK.Talks.Tests.Unit.Core;
[TestFixture]
public class QualificationStrategyTests : TestBase
{
    [Test]
    public void EmailBlocked_BlocksSpeakerRegardlessOfBrowser()
    {
        var options = Options.Create(new RegistrationRulesOptions
        {
            BlockedEmailDomains = ["blocked.com"]
        });

        EmailAndBrowserQualificationStrategy strat = new(options);

        Speaker blockedSpeaker = BuildDomainSpeaker(email: "hacker@blocked.com", browser: Browser.Create("Chrome", 91));
        Speaker allowedSpeaker = BuildDomainSpeaker(email: "user@good.com", browser: Browser.Create("Chrome", 91));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(strat.IsQualified(blockedSpeaker), Is.False, "Blocked email domain should fail qualification");
            Assert.That(strat.IsQualified(allowedSpeaker), Is.True, "Allowed email and modern browser should pass");
        }
    }

    [Test]
    public void InternetExplorer_MinimumVersion_IsEnforced()
    {
        var options = Options.Create(new RegistrationRulesOptions
        {
            MinimumInternetExplorerMajorVersion = 10,
            BlockedEmailDomains = []
        });

        EmailAndBrowserQualificationStrategy strat = new(options);

        Speaker ie9 = BuildDomainSpeaker(email: "user@good.com", browser: Browser.Create("InternetExplorer", 9));
        Speaker ie10 = BuildDomainSpeaker(email: "user@good.com", browser: Browser.Create("InternetExplorer", 10));
        Speaker chrome = BuildDomainSpeaker(email: "user@good.com", browser: Browser.Create("Chrome", 91));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(strat.IsQualified(ie9), Is.False, "IE9 should be rejected when minimum is 10");
            Assert.That(strat.IsQualified(ie10), Is.True, "IE10 should be accepted when minimum is 10");
            Assert.That(strat.IsQualified(chrome), Is.True, "Chrome should be accepted");
        }
    }
}