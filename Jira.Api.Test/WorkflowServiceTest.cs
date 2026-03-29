namespace Jira.Api.Test;

public class WorkflowServiceTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Fact]
	public async Task GetWorkflowsAsync_ReturnsMappedWorkflows()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var remoteWorkflows = new[]
		{
			new RemoteWorkflow
			{
				Name = "Software Simplified Workflow",
				Description = "Default software workflow",
				IsDefault = true,
				LastModifiedDate = "2025-01-01T10:00:00.000+0000",
				LastModifiedUser = "admin",
				LastModifiedUserAccountId = "account-1",
				Steps = 3
			},
			new RemoteWorkflow
			{
				Name = "Bug Workflow",
				Description = "Workflow for bugs",
				IsDefault = false,
				Steps = 5
			}
		};

		client.Setup(c => c.ExecuteRequestAsync<RemoteWorkflow[]>(
				Method.Get,
				"rest/api/2/workflow",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remoteWorkflows);

		var workflows = (await jira.Workflows.GetWorkflowsAsync(CancellationToken)).ToArray();

		workflows.Should().HaveCount(2);
		workflows[0].Name.Should().Be("Software Simplified Workflow");
		workflows[0].Description.Should().Be("Default software workflow");
		workflows[0].IsDefault.Should().BeTrue();
		workflows[0].LastModifiedUser.Should().Be("admin");
		workflows[0].LastModifiedUserAccountId.Should().Be("account-1");
		workflows[0].Steps.Should().Be(3);
		workflows[0].ToString().Should().Be("Software Simplified Workflow");
	}

	[Fact]
	public async Task GetWorkflowAsync_MatchesNameCaseInsensitively()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		client.Setup(c => c.ExecuteRequestAsync<RemoteWorkflow[]>(
				Method.Get,
				"rest/api/2/workflow",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(
			[
				new RemoteWorkflow { Name = "Software Simplified Workflow", Steps = 3 },
				new RemoteWorkflow { Name = "Bug Workflow", Steps = 5 }
			]);

		var workflow = await jira.Workflows.GetWorkflowAsync("bug workflow", CancellationToken);

		workflow.Name.Should().Be("Bug Workflow");
		workflow.Steps.Should().Be(5);
	}

	[Fact]
	public async Task GetWorkflowAsync_WhenWorkflowDoesNotExist_ThrowsInvalidOperationException()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		client.Setup(c => c.ExecuteRequestAsync<RemoteWorkflow[]>(
				Method.Get,
				"rest/api/2/workflow",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync([new RemoteWorkflow { Name = "Known Workflow", Steps = 1 }]);

		var act = () => jira.Workflows.GetWorkflowAsync("missing workflow", CancellationToken);

		await act.Should().ThrowExactlyAsync<InvalidOperationException>()
			.WithMessage("Workflow 'missing workflow' not found.");
	}

	[Theory]
	[InlineData("")]
	[InlineData(null)]
	public async Task GetWorkflowAsync_WhenNameIsNullOrEmpty_ThrowsArgumentException(string? workflowName)
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var act = () => jira.Workflows.GetWorkflowAsync(workflowName!, CancellationToken);

		await act.Should().ThrowAsync<ArgumentException>();
	}
}
