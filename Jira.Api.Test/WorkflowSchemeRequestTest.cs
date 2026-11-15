namespace Jira.Api.Test;

public class WorkflowSchemeRequestTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	private const string Scheme = "{\"id\":10,\"name\":\"S\",\"description\":\"d\",\"defaultWorkflow\":\"jira\",\"draft\":false,\"workflow\":\"wf\",\"issueType\":\"1\",\"issueTypeMappings\":{\"1\":\"wf\"}}";
	private const string Base = "rest/api/2/workflowscheme/10";

	public static TheoryData<Func<IWorkflowSchemeService, Task>, Method, string> Calls => new()
	{
		{ s => s.GetWorkflowSchemeAsync("10", CancellationToken), Method.Get, Base },
		{ s => s.GetWorkflowSchemeForProjectAsync("ABC", CancellationToken), Method.Get, "rest/api/2/project/ABC/workflowscheme" },
		{ s => s.CreateWorkflowSchemeAsync("S", "d", "jira", CancellationToken), Method.Post, "rest/api/2/workflowscheme" },
		{ s => s.UpdateWorkflowSchemeAsync("10", "S", "d", "jira", CancellationToken), Method.Put, Base },
		{ s => s.DeleteWorkflowSchemeAsync("10", CancellationToken), Method.Delete, Base },
		{ s => s.CreateDraftAsync("10", CancellationToken), Method.Post, Base + "/createdraft" },
		{ s => s.GetDraftAsync("10", CancellationToken), Method.Get, Base + "/draft" },
		{ s => s.UpdateDraftAsync("10", "S", "d", "jira", CancellationToken), Method.Put, Base + "/draft" },
		{ s => s.DeleteDraftAsync("10", CancellationToken), Method.Delete, Base + "/draft" },
		{ s => s.GetIssueTypeMappingAsync("10", "1", CancellationToken), Method.Get, Base + "/issuetype/1" },
		{ s => s.SetIssueTypeMappingAsync("10", "1", "wf", true, CancellationToken), Method.Put, Base + "/issuetype/1" },
		{ s => s.DeleteIssueTypeMappingAsync("10", "1", true, CancellationToken), Method.Delete, Base + "/issuetype/1" },
		{ s => s.GetWorkflowMappingAsync("10", "wf", CancellationToken), Method.Get, Base + "/workflow?workflowName=wf" },
		{ s => s.SetWorkflowMappingAsync("10", "wf", ["1"], false, true, CancellationToken), Method.Put, Base + "/workflow" },
		{ s => s.DeleteWorkflowMappingAsync("10", "wf", true, CancellationToken), Method.Delete, Base + "/workflow?workflowName=wf" },
		{ s => s.GetDefaultWorkflowAsync("10", CancellationToken), Method.Get, Base + "/default" },
		{ s => s.SetDefaultWorkflowAsync("10", "wf", true, CancellationToken), Method.Put, Base + "/default" },
		{ s => s.DeleteDefaultWorkflowAsync("10", true, CancellationToken), Method.Delete, Base + "/default" },
		{ s => s.GetDraftIssueTypeMappingAsync("10", "1", CancellationToken), Method.Get, Base + "/draft/issuetype/1" },
		{ s => s.SetDraftIssueTypeMappingAsync("10", "1", "wf", CancellationToken), Method.Put, Base + "/draft/issuetype/1" },
		{ s => s.DeleteDraftIssueTypeMappingAsync("10", "1", CancellationToken), Method.Delete, Base + "/draft/issuetype/1" },
		{ s => s.GetDraftWorkflowMappingAsync("10", "wf", CancellationToken), Method.Get, Base + "/draft/workflow?workflowName=wf" },
		{ s => s.SetDraftWorkflowMappingAsync("10", "wf", ["1"], false, CancellationToken), Method.Put, Base + "/draft/workflow" },
		{ s => s.DeleteDraftWorkflowMappingAsync("10", "wf", CancellationToken), Method.Delete, Base + "/draft/workflow?workflowName=wf" },
		{ s => s.GetDraftDefaultWorkflowAsync("10", CancellationToken), Method.Get, Base + "/draft/default" },
		{ s => s.SetDraftDefaultWorkflowAsync("10", "wf", CancellationToken), Method.Put, Base + "/draft/default" },
		{ s => s.DeleteDraftDefaultWorkflowAsync("10", CancellationToken), Method.Delete, Base + "/draft/default" }
	};

	[Theory]
	[MemberData(nameof(Calls))]
	public async Task Operation_SendsExpectedRequest(Func<IWorkflowSchemeService, Task> call, Method method, string resource)
	{
		var (rest, jira) = RoutedRestClient.Create((_, _) => RoutedRestClient.Json(Scheme));

		try
		{
			await call(jira.WorkflowSchemes);
		}
		catch (InvalidOperationException)
		{
			// Response shape differences are irrelevant here: only the request is asserted.
		}

		rest.Calls.Should().Contain(c => c.Method == method && c.Resource.StartsWith(resource.Split('?')[0], StringComparison.Ordinal));
	}

	[Fact]
	public async Task GetWorkflowSchemeAsync_RejectsBlankIds()
	{
		var (_, jira) = RoutedRestClient.Create(Scheme);

		var act = () => jira.WorkflowSchemes.GetWorkflowSchemeAsync(string.Empty, CancellationToken);

		await act.Should().ThrowAsync<ArgumentException>();
	}
}
