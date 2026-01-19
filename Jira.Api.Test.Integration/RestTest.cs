using AwesomeAssertions;
using Jira.Api.Exceptions;
using RestSharp;

namespace Jira.Api.Test.Integration;

public class RestTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	private readonly Random _random = new();

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task ExecuteRestRequest(JiraClient jira)
	{
		var users = await jira.RestClient.ExecuteRequestAsync<JiraNamedResource[]>(Method.Get, "rest/api/2/user/assignable/multiProjectSearch?projectKeys=TST", null, CancellationToken);

		(users.Length >= 2).Should().BeTrue();
		users.Should().Contain(u => u.Name == "admin");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task ExecuteRawRestRequest(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test Summary " + _random.Next(int.MaxValue),
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(CancellationToken);

		var rawBody = $"{{ \"jql\": \"Key=\\\"{issue.Key.Value}\\\"\" }}";
		var json = await jira.RestClient.ExecuteRequestAsync(Method.Post, "rest/api/2/search", rawBody, CancellationToken);
		var issues = json["issues"];
		issues.Should().ContainSingle();
		var firstIssue = issues[0];
		firstIssue.Should().NotBeNull();
		var key = firstIssue["key"];
		key.Should().NotBeNull();
		key.ToString().Should().Be(issue.Key.Value);
	}

	[Fact]
	public async Task WillThrowErrorIfSiteIsUnreachable()
	{
#if NET452
            // Standard has a different behavior than Framework, it throws the same exception but with a different message:
            // System.InvalidOperationException: 'Error Message: The request was aborted: Could not create SSL/TLS secure channel.'
            // This workaround fixes the test: https://stackoverflow.com/questions/2859790/the-request-was-aborted-could-not-create-ssl-tls-secure-channel.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
#endif

		var jira = JiraClient.CreateRestClient("http://farmasXXX.atlassian.net");

		var act = () => jira.Issues.GetIssueAsync("TST-1", CancellationToken);
		await act.Should().ThrowExactlyAsync<ResourceNotFoundException>();
	}
}