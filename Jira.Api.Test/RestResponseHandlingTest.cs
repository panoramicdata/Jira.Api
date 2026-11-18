using Jira.Api.Exceptions;
using System.Net;
using System.Security.Authentication;

namespace Jira.Api.Test;

public class RestResponseHandlingTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	public static TheoryData<HttpStatusCode, string, string, Type> Failures => new()
	{
		{ HttpStatusCode.NotFound, "{}", "rest/api/2/issue/A-1", typeof(ResourceNotFoundException) },
		{ HttpStatusCode.Forbidden, "no", "rest/api/2/issue/A-1", typeof(AuthenticationException) },
		{ HttpStatusCode.Unauthorized, "no", "rest/api/2/issue/A-1", typeof(AuthenticationException) },
		{ HttpStatusCode.Unauthorized, "no", "rest/api/2/workflowscheme/1", typeof(WebSudoRequiredException) },
		{ HttpStatusCode.InternalServerError, "boom", "rest/api/2/issue/A-1", typeof(InvalidOperationException) },
		{ HttpStatusCode.OK, "plain text", "rest/api/2/issue/A-1", typeof(InvalidOperationException) },
		{ HttpStatusCode.OK, "{ not json", "rest/api/2/issue/A-1", typeof(InvalidOperationException) },
		{ HttpStatusCode.OK, "{\"errorMessages\":[\"bad\"]}", "rest/api/2/issue/A-1", typeof(InvalidOperationException) }
	};

	[Theory]
	[MemberData(nameof(Failures))]
	public async Task ExecuteRequestAsync_MapsFailures(HttpStatusCode status, string content, string resource, Type expected)
	{
		var (_, jira) = RoutedRestClient.Create(content, status);

		var act = () => jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, CancellationToken);

		(await act.Should().ThrowAsync<Exception>()).Which.Should().BeOfType(expected);
	}

	[Fact]
	public async Task ExecuteRequestAsync_WithEmptyContent_ReturnsEmptyObject()
	{
		var (_, jira) = RoutedRestClient.Create(string.Empty);

		var result = await jira.RestClient.ExecuteRequestAsync(Method.Get, "rest/api/2/serverInfo", null, CancellationToken);

		result.Should().BeOfType<JObject>().Which.Should().BeEmpty();
	}

	[Fact]
	public async Task ExecuteRequestAsync_ErrorMessage_Throws()
	{
		var (_, jira) = RoutedRestClient.Create((_, _) => new RestResponse { ErrorMessage = "connection refused" });

		var act = () => jira.RestClient.ExecuteRequestAsync(Method.Get, "rest/api/2/serverInfo", null, CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*connection refused*");
	}

	[Fact]
	public async Task ExecuteRequestAsync_GetWithBody_Throws()
	{
		var (_, jira) = RoutedRestClient.Create("{}");

		var act = () => jira.RestClient.ExecuteRequestAsync(Method.Get, "rest/api/2/serverInfo", "{}", CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>();
	}

	[Theory]
	[InlineData("{\"a\":1}")]
	[InlineData("plain")]
	public async Task ExecuteRequestAsync_SendsStringAndObjectBodies(string body)
	{
		var (rest, jira) = RoutedRestClient.Create("{}", settings: new JiraRestClientSettings("Test/1.0") { EnableRequestTrace = true });

		await jira.RestClient.ExecuteRequestAsync(Method.Post, "rest/api/2/issue", body, CancellationToken);
		await jira.RestClient.ExecuteRequestAsync(Method.Post, "rest/api/2/issue", new { a = 1 }, CancellationToken);

		rest.Calls.Should().HaveCount(2);
	}

	[Fact]
	public async Task ExecuteRequestAsync_Generic_DeserialisesTheBody()
	{
		var (_, jira) = RoutedRestClient.Create("{\"id\":\"1\",\"name\":\"Bug\"}");

		var result = await jira.RestClient.ExecuteRequestAsync<RemoteNamedObject>(Method.Get, "rest/api/2/issuetype/1", null, CancellationToken);

		result!.name.Should().Be("Bug");
	}

	[Fact]
	public async Task ExecuteRawRequestAsync_ReturnsTheResponseUntouched()
	{
		var (_, jira) = RoutedRestClient.Create("not json", HttpStatusCode.InternalServerError);

		var response = await jira.RestClient.ExecuteRawRequestAsync(new RestRequest("rest/api/2/issue"), CancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
		response.Content.Should().Be("not json");
	}

	[Fact]
	public async Task ExecuteRawRequestAsync_NullRequest_Throws()
	{
		var (_, jira) = RoutedRestClient.Create("{}");

		var act = () => jira.RestClient.ExecuteRawRequestAsync(null!, CancellationToken);

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	[Fact]
	public async Task ExecuteRequestAsync_RequestOverload_ValidatesTheResponse()
	{
		var (_, jira) = RoutedRestClient.Create("{}", HttpStatusCode.NotFound);

		var act = () => jira.RestClient.ExecuteRequestAsync(new RestRequest("rest/api/2/issue"), CancellationToken);

		await act.Should().ThrowAsync<ResourceNotFoundException>();
	}

	[Theory]
	[InlineData("")]
	[InlineData("bad agent!")]
	public void Settings_RejectInvalidUserAgents(string userAgent)
	{
		var act = () => new JiraRestClientSettings(userAgent);

		act.Should().Throw<Exception>();
	}
}
