using GK.Talks.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace GK.Talks.Tests.Unit.Api;
[TestFixture]
public class ExceptionHandlingMiddlewareTests
{
    private DefaultHttpContext CreateContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        ctx.Request.Path = "/";
        return ctx;
    }

    [Test]
    public async Task Middleware_ArgumentException_Returns422()
    {
        static Task next(HttpContext ctx) => throw new ArgumentException("bad");
        ExceptionHandlingMiddleware middleware = new(next);

        var ctx = CreateContext();
        await middleware.InvokeAsync(ctx);

        Assert.That(ctx.Response.StatusCode, Is.EqualTo(422));

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body, Encoding.UTF8).ReadToEnd();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(root.GetProperty("title").GetString()?.ToLowerInvariant(), Is.EqualTo("invalid argument"));
            Assert.That(root.GetProperty("status").GetInt32(), Is.EqualTo(422));
            Assert.That(root.GetProperty("detail").GetString(), Is.EqualTo("bad"));
        }
    }

    [Test]
    public async Task Middleware_UnexpectedException_ReturnsMappedStatus_And_ProblemDetailsBody()
    {
        static Task next(HttpContext ctx) => throw new InvalidOperationException("boom");
        var middleware = new ExceptionHandlingMiddleware(next);

        var ctx = CreateContext();
        await middleware.InvokeAsync(ctx);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ctx.Response.ContentType, Is.EqualTo("application/problem+json"));
            Assert.That(ctx.Response.StatusCode, Is.EqualTo(409));
        }

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body, Encoding.UTF8).ReadToEnd();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(root.GetProperty("status").GetInt32(), Is.EqualTo(ctx.Response.StatusCode));
            Assert.That(root.GetProperty("detail").GetString()?.ToLowerInvariant(), Does.Contain("boom").Or.Not.Null);
        }
    }
}