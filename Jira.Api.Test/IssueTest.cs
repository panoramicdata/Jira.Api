using AwesomeAssertions;

namespace Jira.Api.Test;

public partial class IssueTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	public class Constructor
	{
		[Fact]
		public void ShouldSetDefaultValues()
		{
			var issue = CreateIssue("ProjectKey");
			issue.AffectsVersions.Should().BeEmpty();
			issue.Assignee.Should().BeNull();
			issue.Components.Should().BeEmpty();
			issue.Created.Should().BeNull();
			issue.CustomFields.Should().BeEmpty();
			issue.Description.Should().BeNull();
			issue.DueDate.Should().BeNull();
			issue.Environment.Should().BeNull();
			issue.Key.Should().BeNull();
			issue.Priority.Should().BeNull();
			issue.Project.Should().Be("ProjectKey");
			issue.Reporter.Should().BeNull();
			issue.Resolution.Should().BeNull();
			issue.Status.Should().BeNull();
			issue.Summary.Should().BeNull();
			issue.Type.Should().BeNull();
			issue.Updated.Should().BeNull();
			issue.Votes.Should().BeNull();
		}

		[Fact]
		public void FromRemote_ShouldPopulateFields()
		{
			var remoteIssue = new RemoteIssue()
			{
				affectsVersions = [new() { id = "remoteVersion" }],
				assignee = "assignee",
				components = [new() { id = "remoteComponent" }],
				created = new DateTime(2011, 1, 1),
				customFieldValues = [new() { customfieldId = "customField" }],
				description = "description",
				duedate = new DateTime(2011, 3, 3),
				environment = "environment",
				fixVersions = [new() { id = "remoteFixVersion" }],
				key = "key",
				priority = new RemotePriority() { id = "priority" },
				project = "project",
				reporter = "reporter",
				resolution = new RemoteResolution() { id = "resolution" },
				status = new RemoteStatus() { id = "status" },
				summary = "summary",
				type = new RemoteIssueType() { id = "type" },
				updated = new DateTime(2011, 2, 2),
				votesData = new RemoteVotes() { votes = 1, hasVoted = true }
			};

			var issue = remoteIssue.ToLocal(TestableJira.Create());

			issue.AffectsVersions.Should().ContainSingle();
			issue.Assignee.Should().Be("assignee");
			issue.Components.Should().ContainSingle();
			issue.Created.Should().Be(new DateTime(2011, 1, 1));
			issue.CustomFields.Should().ContainSingle();
			issue.Description.Should().Be("description");
			issue.DueDate.Should().Be(new DateTime(2011, 3, 3));
			issue.Environment.Should().Be("environment");
			issue.Key.Value.Should().Be("key");
			issue.Priority.Id.Should().Be("priority");
			issue.Project.Should().Be("project");
			issue.Reporter.Should().Be("reporter");
			issue.Resolution.Id.Should().Be("resolution");
			issue.Status.Id.Should().Be("status");
			issue.Summary.Should().Be("summary");
			issue.Type.Id.Should().Be("type");
			issue.Updated.Should().Be(new DateTime(2011, 2, 2));
			issue.Votes.Should().Be(1);
			issue.HasUserVoted.Should().BeTrue();
		}
	}

	public class ToRemote
	{
		[Fact]
		public void IfFieldsNotSet_ShouldLeaveFieldsNull()
		{
			var issue = CreateIssue("ProjectKey");

			var remoteIssue = issue.ToRemote();

			remoteIssue.affectsVersions.Should().BeNull();
			remoteIssue.assignee.Should().BeNull();
			remoteIssue.components.Should().BeNull();
			remoteIssue.created.Should().BeNull();
			remoteIssue.customFieldValues.Should().BeNull();
			remoteIssue.description.Should().BeNull();
			remoteIssue.duedate.Should().BeNull();
			remoteIssue.environment.Should().BeNull();
			remoteIssue.key.Should().BeNull();
			remoteIssue.priority.Should().BeNull();
			remoteIssue.project.Should().Be("ProjectKey");
			remoteIssue.reporter.Should().BeNull();
			remoteIssue.resolution.Should().BeNull();
			remoteIssue.status.Should().BeNull();
			remoteIssue.summary.Should().BeNull();
			remoteIssue.type.Should().BeNull();
			remoteIssue.updated.Should().BeNull();
			remoteIssue.votesData.Should().BeNull();
		}

		[Fact]
		public void IfFieldsSet_ShouldPopulateFields()
		{
			var jira = TestableJira.Create();
			var issue = jira.CreateIssue("ProjectKey");
			var version = new RemoteVersion() { id = "1" }.ToLocal(issue.Jira);
			var component = new RemoteComponent() { id = "1" }.ToLocal();

			jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
				.Returns(Task.FromResult(Enumerable.Repeat(new IssueType("4", "issuetype"), 1)));
			jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(CancellationToken.None))
				.Returns(Task.FromResult(Enumerable.Repeat(new IssuePriority("1", "priority"), 1)));

			issue.AffectsVersions.Add(version);
			issue.Assignee = "assignee";
			issue.Components.Add(component);
			// issue.CustomFields <-- requires extra setup, test below
			issue.Description = "description";
			issue.DueDate = new DateTime(2011, 1, 1);
			issue.Environment = "environment";
			issue.FixVersions.Add(version);
			// issue.Key <-- should be non-settable
			issue.Priority = "1";
			// issue.Project <-- should be non-settable
			issue.Reporter = "reporter";
			issue.Summary = "summary";
			issue.Type = "4";

			var remoteIssue = issue.ToRemote();

			remoteIssue.affectsVersions.Should().ContainSingle();
			remoteIssue.assignee.Should().Be("assignee");
			remoteIssue.components.Should().ContainSingle();
			remoteIssue.created.Should().BeNull();
			remoteIssue.description.Should().Be("description");
			remoteIssue.duedate.HasValue.Should().BeTrue();
			remoteIssue.duedate!.Value.Should().Be(new DateTime(2011, 1, 1));
			remoteIssue.environment.Should().Be("environment");
			remoteIssue.key.Should().BeNull();
			remoteIssue.priority.id.Should().Be("1");
			remoteIssue.project.Should().Be("ProjectKey");
			remoteIssue.reporter.Should().Be("reporter");
			remoteIssue.resolution.Should().BeNull();
			remoteIssue.status.Should().BeNull();
			remoteIssue.summary.Should().Be("summary");
			remoteIssue.type.id.Should().Be("4");
			remoteIssue.updated.Should().BeNull();
		}

		[Fact]
		public void ToRemote_IfTypeSetByName_FetchId()
		{
			var jira = TestableJira.Create();
			var issue = jira.CreateIssue("ProjectKey");
			var issueType = new IssueType(new RemoteIssueType() { id = "1", name = "Bug" });
			jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
				.Returns(Task.FromResult(Enumerable.Repeat(issueType, 1)));

			issue.Type = "Bug";

			var remoteIssue = issue.ToRemote();
			remoteIssue.type.id.Should().Be("1");
		}
	}

	public class AddAttachment
	{
		[Fact]
		public async Task AddAttachment_IfIssueNotCreated_ShouldThrowAnException()
		{
			var issue = CreateIssue();

			var act = () => issue.AddAttachmentAsync("foo", [1], CancellationToken);
			await act.Should().ThrowExactlyAsync<InvalidOperationException>();
		}
	}

	public class WorkflowTransition
	{
		[Fact]
		public async Task IfTransitionNotFound_ShouldThrowAnException()
		{
			var jira = TestableJira.Create();
			var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

			var act = () => issue.WorkflowTransitionAsync("foo", null, default);
			await act.Should().ThrowExactlyAsync<InvalidOperationException>();
			}
			}

			public class GetComments
			{
		[Fact]
		public async Task IfIssueNotCreated_ShouldThrowException()
		{
			var issue = CreateIssue();

			var act = () => issue.GetCommentsAsync(default);
			await act.Should().ThrowExactlyAsync<InvalidOperationException>();
		}

		[Fact]
		public async Task IfIssueIsCreated_ShouldLoadComments()
		{
			//arrange
			var jira = TestableJira.Create();
			jira.IssueService.Setup(j => j.GetCommentsAsync("issueKey", It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(new Comment() { Body = "the comment" }, 1)));
			var issue = (new RemoteIssue() { key = "issueKey" }).ToLocal(jira);

			//act
			var comments = await issue.GetCommentsAsync(CancellationToken);

			//assert
			comments.Should().ContainSingle();
			comments.First().Body.Should().Be("the comment");
		}
	}

	private static Issue CreateIssue(string project = "TST")
	{
		return TestableJira.Create().CreateIssue(project);
	}

	private static Task<RemoteFieldValue[]> GetUpdatedFieldsForIssueAsync(Issue issue, CancellationToken cancellationToken)
	{
		return ((IRemoteIssueFieldProvider)issue).GetRemoteFieldValuesAsync(cancellationToken);
	}
}