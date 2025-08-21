using GK.Talks.Api.Contracts;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GK.Talks.Tests.Integration.Api;
[TestFixture]
public class MinimalApiIntegrationTests
{
    private CustomWebApplicationFactory _factory = default!;
    private HttpClient _client = default!;

    [SetUp]
    public void SetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Post_Speakers_HappyPath_ReturnsCreated_And_PublishesEvent()
    {
        RegisterSpeakerDto dto = new(
            FirstName: "Jane",
            LastName: "Doe",
            Email: "jane@GK.com",
            Experience: 5,
            HasBlog: false,
            BlogUrl: null,
            Certifications: [],
            Employer: "GK",
            Sessions: [new("T", "D")]
        );

        var content = JsonContent.Create(dto, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var res = await _client.PostAsync("/speakers", content);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(_factory.NotificationHandler.Events.Count, Is.GreaterThanOrEqualTo(1));
        }
        var evt = _factory.NotificationHandler.Events.First();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.SpeakerId, Is.GreaterThan(0));
            Assert.That(evt.Message, Does.Contain("registration").IgnoreCase);
        }
    }

    [Test]
    public async Task Get_Speakers_ReturnsOk_WithDtos()
    {
        RegisterSpeakerDto dto = new(
            FirstName: "Alice",
            LastName: "Smith",
            Email: "alice@GK.com",
            Experience: 3,
            HasBlog: false,
            BlogUrl: null,
            Certifications: [],
            Employer: "GK",
            Sessions: [new("Title", "Desc")]
        );
        var content = JsonContent.Create(dto, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var post = await _client.PostAsync("/speakers", content);
        Assert.That(post.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var get = await _client.GetAsync("/speakers");
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await get.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("alice").IgnoreCase);
        Assert.That(body, Does.Contain("GK.com").IgnoreCase);
    }
}