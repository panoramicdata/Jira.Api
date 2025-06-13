using Jira.Api.Remote;
using RestSharp;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test.Integration;

public class RestTest
{
	private readonly Random _random = new();

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task ExecuteRestRequest(Jira jira)
	{
		var users = await jira.RestClient.ExecuteRequestAsync<JiraNamedResource[]>(Method.Get, "rest/api/2/user/assignable/multiProjectSearch?projectKeys=TST", null, default);

		Assert.True(users.Length >= 2);
		Assert.Contains(users, u => u.Name == "admin");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task ExecuteRawRestRequest(Jira jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test Summary " + _random.Next(int.MaxValue),
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(default);

		var rawBody = $"{{ \"jql\": \"Key=\\\"{issue.Key.Value}\\\"\" }}";
		var json = await jira.RestClient.ExecuteRequestAsync(Method.Post, "rest/api/2/search", rawBody, default);

		Assert.Equal(issue.Key.Value, json["issues"][0]["key"].ToString());
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

		var jira = Jira.CreateRestClient("http://farmasXXX.atlassian.net");

		var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() => jira.Issues.GetIssueAsync("TST-1", default));
	}
}
