using Newtonsoft.Json.Linq;

namespace Jira.Api.Test;

public class WorkflowSchemeServiceTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Fact]
	public async Task SetIssueTypeMappingAsync_UpdatesDraftByDefault()
	{
		var client = new Mock<IJiraRestClient>();
		object? body = null;
		client.Setup(c => c.ExecuteRequestAsync(Method.Put, "rest/api/2/workflowscheme/10000/issuetype/10001", It.IsAny<object>(), It.IsAny<CancellationToken>()))
			.Callback<Method, string, object?, CancellationToken>((_, _, requestBody, _) => body = requestBody)
			.ReturnsAsync(new JObject());

		var jira = JiraClient.CreateRestClient(client.Object);
		await jira.WorkflowSchemes.SetIssueTypeMappingAsync("10000", "10001", "Bug workflow", cancellationToken: CancellationToken);

		JObject.FromObject(body!).Should().BeEquivalentTo(JObject.Parse("{ 'issueType': '10001', 'workflow': 'Bug workflow', 'updateDraftIfNeeded': true }"));
	}

	[Fact]
	public async Task GetDraftIssueTypeMappingAsync_UsesDraftEndpoint()
	{
		var client = new Mock<IJiraRestClient>();
		client.Setup(c => c.ExecuteRequestAsync<RemoteWorkflowSchemeIssueTypeMapping>(Method.Get, "rest/api/2/workflowscheme/10000/draft/issuetype/10001", null, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new RemoteWorkflowSchemeIssueTypeMapping { IssueType = "10001", Workflow = "Bug workflow" });

		var jira = JiraClient.CreateRestClient(client.Object);
		var mapping = await jira.WorkflowSchemes.GetDraftIssueTypeMappingAsync("10000", "10001", CancellationToken);

		mapping.IssueType.Should().Be("10001");
		mapping.Workflow.Should().Be("Bug workflow");
	}

	[Fact]
	public async Task GetDraftAsync_MapsDraftMetadata()
	{
		var client = new Mock<IJiraRestClient>();
		client.Setup(c => c.ExecuteRequestAsync<RemoteWorkflowScheme>(Method.Get, "rest/api/2/workflowscheme/10000/draft", null, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new RemoteWorkflowScheme
		{
			Id = "10000",
			OriginalDefaultWorkflow = "jira",
			OriginalIssueTypeMappings = new Dictionary<string, string> { ["10001"] = "Bug workflow" },
			LastModified = "2026-09-20T12:00:00.000+0000"
		});
		var jira = JiraClient.CreateRestClient(client.Object);
		var scheme = await jira.WorkflowSchemes.GetDraftAsync("10000", CancellationToken);

		scheme.OriginalDefaultWorkflow.Should().Be("jira");
		scheme.OriginalIssueTypeMappings.Should().ContainSingle().Which.Value.Should().Be("Bug workflow");
		scheme.LastModified.Should().Be("2026-09-20T12:00:00.000+0000");
	}
}
