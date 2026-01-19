namespace Jira.Api.Services;

internal class WorkflowService(JiraClient jira) : IWorkflowService
{
	private readonly JiraClient _jira = jira;

	public async Task<IEnumerable<Workflow>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
	{
		var remoteWorkflows = await _jira.RestClient.ExecuteRequestAsync<RemoteWorkflow[]>(
			Method.Get,
			"rest/api/2/workflow",
			null,
			cancellationToken).ConfigureAwait(false);

		return remoteWorkflows.Select(w => new Workflow(w));
	}

	public async Task<Workflow> GetWorkflowAsync(string workflowName, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(workflowName);

		var workflows = await GetWorkflowsAsync(cancellationToken).ConfigureAwait(false);
		var workflow = workflows.FirstOrDefault(w => w.Name.Equals(workflowName, StringComparison.OrdinalIgnoreCase));

		return workflow ?? throw new InvalidOperationException($"Workflow '{workflowName}' not found.");
	}
}
