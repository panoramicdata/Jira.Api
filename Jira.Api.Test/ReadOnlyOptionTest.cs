using RestSharp;
using System.Net;

namespace Jira.Api.Test;

/// <summary>
/// Credential-free: a refused write throws before any request is sent.
/// </summary>
public class ReadOnlyOptionTest
{
	private sealed class StubJiraRestClient(JiraRestClientSettings settings)
		: JiraRestClient("https://jira.example.com", "user", "password", settings)
	{
		public List<RestRequest> Sent { get; } = [];

		protected override Task<RestResponse> SendRequestAsync(RestRequest request, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			Sent.Add(request);
			return Task.FromResult(new RestResponse { StatusCode = HttpStatusCode.OK, Content = "{}" });
		}
	}

	private static StubJiraRestClient CreateClient(bool isReadOnly)
		=> new(new JiraRestClientSettings("Test/1.0") { IsReadOnly = isReadOnly });

	[Fact]
	public void IsReadOnly_DefaultsToFalse()
		=> new JiraRestClientSettings("Test/1.0").IsReadOnly.Should().BeFalse();

	[Theory]
	[InlineData(Method.Post, "rest/api/2/issue")]
	[InlineData(Method.Put, "rest/api/2/issue/ABC-1")]
	[InlineData(Method.Delete, "rest/api/2/issue/ABC-1")]
	public async Task Write_IsRefused_WhenIsReadOnly(Method method, string resource)
	{
		var client = CreateClient(isReadOnly: true);

		var act = () => client.ExecuteRequestAsync(method, resource, "{}", TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*IsReadOnly*");
		client.Sent.Should().BeEmpty();
	}

	[Fact]
	public async Task Write_IsAllowed_WhenNotReadOnly()
	{
		var client = CreateClient(isReadOnly: false);

		await client.ExecuteRequestAsync(Method.Delete, "rest/api/2/issue/ABC-1", null, TestContext.Current.CancellationToken);

		client.Sent.Should().ContainSingle();
	}

	[Fact]
	public async Task Get_IsAllowed_WhenIsReadOnly()
	{
		var client = CreateClient(isReadOnly: true);

		await client.ExecuteRequestAsync(Method.Get, "rest/api/2/issue/ABC-1", null, TestContext.Current.CancellationToken);

		client.Sent.Should().ContainSingle();
	}

	[Fact]
	public async Task JqlSearchPost_IsAllowed_WhenIsReadOnly()
	{
		var client = CreateClient(isReadOnly: true);

		await client.ExecuteRequestAsync(Method.Post, "rest/api/2/search", new { jql = "project = ABC" }, TestContext.Current.CancellationToken);

		client.Sent.Should().ContainSingle();
	}

	[Fact]
	public async Task RawRequestWrite_IsRefused_WhenIsReadOnly()
	{
		var client = CreateClient(isReadOnly: true);

		var act = () => client.ExecuteRawRequestAsync(new RestRequest("rest/api/2/issue/ABC-1", Method.Delete), TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*IsReadOnly*");
		client.Sent.Should().BeEmpty();
	}
}
