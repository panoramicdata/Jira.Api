using AwesomeAssertions;

namespace Jira.Api.Test;

public partial class IssueTest
{
	public class GetUpdatedFields(ITestOutputHelper outputHelper) : TestBase(outputHelper)
	{
		[Fact]
		public async Task ReturnsCustomFieldsAdded()
		{
			var jira = TestableJira.Create();
			var customField = new CustomField(new RemoteField() { id = "CustomField1", name = "My Custom Field" });
			var remoteIssue = new RemoteIssue()
			{
				key = "TST-1",
				project = "TST",
				type = new RemoteIssueType() { id = "1" }
			};

			jira.IssueService.SetupIssues(jira, remoteIssue);
			jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(customField, 1)));

			var issue = jira.CreateIssue("TST");
			issue["My Custom Field"] = "test value";

			var result = await GetUpdatedFieldsForIssueAsync(issue, CancellationToken);
			result.Should().ContainSingle();
			result.First().id.Should().Be("CustomField1");
		}

		[Fact]
		public async Task ExcludesCustomFieldsNotModified()
		{
			var jira = TestableJira.Create();
			var customField = new CustomField(new RemoteField() { id = "CustomField1", name = "My Custom Field" });
			var remoteCustomFieldValue = new RemoteCustomFieldValue()
			{
				customfieldId = "CustomField1",
				values = ["My Value"]
			};
			var remoteIssue = new RemoteIssue()
			{
				key = "TST-1",
				project = "TST",
				type = new RemoteIssueType() { id = "1" },
				customFieldValues = [remoteCustomFieldValue]
			};

			jira.IssueService.Setup(s => s.GetIssueAsync("TST-1", It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new Issue(jira, remoteIssue)));
			jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(customField, 1)));
			jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(new IssueType("1"), 1)));

			var issue = await jira.Issues.GetIssueAsync("TST-1", CancellationToken);

			var result = await GetUpdatedFieldsForIssueAsync(issue, CancellationToken);
			result.Should().BeEmpty();
		}

		[Fact]
		public async Task ReturnsCustomFieldThatWasModified()
		{
			var jira = TestableJira.Create();
			var customField = new CustomField(new RemoteField() { id = "CustomField1", name = "My Custom Field" });
			var remoteCustomFieldValue = new RemoteCustomFieldValue()
			{
				customfieldId = "CustomField1",
				values = ["My Value"]
			};
			var remoteIssue = new RemoteIssue()
			{
				key = "TST-1",
				project = "TST",
				type = new RemoteIssueType() { id = "1" },
				customFieldValues = [remoteCustomFieldValue]
			};

			jira.IssueService.Setup(s => s.GetIssueAsync("TST-1", It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new Issue(jira, remoteIssue)));
			jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(customField, 1)));
			jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(new IssueType("1"), 1)));

			var issue = await jira.Issues.GetIssueAsync("TST-1", CancellationToken);
			issue["My Custom Field"] = "My New Value";

			var result = await GetUpdatedFieldsForIssueAsync(issue, CancellationToken);
			result.Should().ContainSingle();
			result.First().id.Should().Be("CustomField1");
			result.First().values[0].Should().Be("My New Value");
		}

		[Fact]
		public async Task IfIssueTypeWithId_ReturnField()
		{
			var jira = TestableJira.Create();
			var issue = jira.CreateIssue("TST");
			issue.Priority = "5";

			jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(new IssuePriority("5"), 1)));

			var result = await GetUpdatedFieldsForIssueAsync(issue, CancellationToken);
			result.Should().ContainSingle();
			result[0].values[0].Should().Be("5");
		}

		[Fact]
		public async Task IfIssueTypeWithName_ReturnsFieldWithIdInferred()
		{
			var jira = TestableJira.Create();
			var issueType = new IssueType(new RemoteIssueType() { id = "2", name = "Task" });
			jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(issueType, 1)));
			var issue = jira.CreateIssue("FOO");
			issue.Type = "Task";

			var result = await GetUpdatedFieldsForIssueAsync(issue, CancellationToken);
			result.Should().ContainSingle();
			result[0].values[0].Should().Be("2");
		}

		[Fact]
		public async Task IfIssueTypeWithNameNotChanged_ReturnsNoFieldsChanged()
		{
			var jira = TestableJira.Create();
			var issueType = new IssueType(new RemoteIssueType() { id = "5", name = "Task" });
			jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(issueType, 1)));
			var remoteIssue = new RemoteIssue()
			{
				type = new RemoteIssueType() { id = "5" },
			};

			var issue = remoteIssue.ToLocal(jira);
			issue.Type = "Task";

			var fields = await GetUpdatedFieldsForIssueAsync(issue, CancellationToken);
			fields.Should().BeEmpty();
		}

		[Fact]
		public async Task ReturnEmptyIfNothingChanged()
		{
			var issue = CreateIssue();

			(await GetUpdatedFieldsForIssueAsync(issue, CancellationToken)).Should().BeEmpty();
		}

		[Fact]
		public async Task IfString_ReturnOneFieldThatChanged()
		{
			var issue = CreateIssue();
			issue.Summary = "foo";

			(await GetUpdatedFieldsForIssueAsync(issue, CancellationToken)).Should().ContainSingle();
		}

		[Fact]
		public async Task IfString_ReturnAllFieldsThatChanged()
		{
			var jira = TestableJira.Create();
			var issue = jira.CreateIssue("TST");
			issue.Summary = "foo";
			issue.Description = "foo";
			issue.Assignee = "foo";
			issue.Environment = "foo";
			issue.Reporter = "foo";
			issue.Type = "2";
			issue.Resolution = "3";
			issue.Priority = "4";

			jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(new IssuePriority("4"), 1)));
			jira.IssueResolutionService.Setup(s => s.GetResolutionsAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(new IssueResolution("3"), 1)));
			jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(new IssueType("2"), 1)));

			(await GetUpdatedFieldsForIssueAsync(issue, CancellationToken)).Should().HaveCount(8);
		}

		[Fact]
		public async Task IfStringEqual_ReturnNoFieldsThatChanged()
		{
			var remoteIssue = new RemoteIssue()
			{
				summary = "Summary"
			};

			var issue = remoteIssue.ToLocal(TestableJira.Create());

			issue.Summary = "Summary";

			(await GetUpdatedFieldsForIssueAsync(issue, CancellationToken)).Should().BeEmpty();
		}

		[Fact]
		public async Task IfComparableEqual_ReturnNoFieldsThatChanged()
		{
			var jira = TestableJira.Create();
			var remoteIssue = new RemoteIssue()
			{
				priority = new RemotePriority() { id = "5" },
			};

			var issue = remoteIssue.ToLocal(jira);
			issue.Priority = "5";

			jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(new IssuePriority("5"), 1)));
			(await GetUpdatedFieldsForIssueAsync(issue, CancellationToken)).Should().BeEmpty();
		}

		[Fact]
		public async Task IfComparable_ReturnsFieldsThatChanged()
		{
			var jira = TestableJira.Create();
			var issue = jira.CreateIssue("TST");
			issue.Priority = "5";

			jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(new IssuePriority("5"), 1)));

			(await GetUpdatedFieldsForIssueAsync(issue, CancellationToken)).Should().ContainSingle();
		}

		[Fact]
		public async Task IfDateTimeChanged_ReturnsFieldsThatChanged()
		{
			var issue = CreateIssue();
			issue.DueDate = new DateTime(2011, 10, 10);

			var fields = await GetUpdatedFieldsForIssueAsync(issue, CancellationToken);
			fields.Should().ContainSingle();
			fields[0].values[0].Should().Be("10/Oct/11");
		}

		[Fact]
		public async Task IfDateTimeUnChangd_ShouldNotIncludeItInFieldsThatChanged()
		{
			var remoteIssue = new RemoteIssue()
			{
				duedate = new DateTime(2011, 1, 1)
			};

			var issue = remoteIssue.ToLocal(TestableJira.Create());
			(await GetUpdatedFieldsForIssueAsync(issue, CancellationToken)).Should().BeEmpty();
		}

		[Fact]
		public async Task IfComponentsAdded_ReturnsFields()
		{
			var issue = new RemoteIssue() { key = "foo" }.ToLocal(TestableJira.Create());
			var component = new RemoteComponent() { id = "1", name = "1.0" };
			issue.Components.Add(component.ToLocal());

			var fields = await GetUpdatedFieldsForIssueAsync(issue, CancellationToken);
			fields.Should().ContainSingle();
			fields[0].id.Should().Be("components");
			fields[0].values[0].Should().Be("1");
		}

		[Fact]
		public async Task IfAddFixVersion_ReturnAllFieldsThatChanged()
		{
			var issue = new RemoteIssue() { key = "foo" }.ToLocal(TestableJira.Create());
			var version = new RemoteVersion() { id = "1", name = "1.0" };
			issue.FixVersions.Add(version.ToLocal(TestableJira.Create()));

			var fields = await GetUpdatedFieldsForIssueAsync(issue, CancellationToken);
			fields.Should().ContainSingle();
			fields[0].id.Should().Be("fixVersions");
			fields[0].values[0].Should().Be("1");
		}

		[Fact]
		public async Task IfAddAffectsVersion_ReturnAllFieldsThatChanged()
		{
			var issue = new RemoteIssue() { key = "foo" }.ToLocal(TestableJira.Create());
			var version = new RemoteVersion() { id = "1", name = "1.0" };
			issue.AffectsVersions.Add(version.ToLocal(TestableJira.Create()));

			var fields = await GetUpdatedFieldsForIssueAsync(issue, CancellationToken);
			fields.Should().ContainSingle();
			fields[0].id.Should().Be("versions");
			fields[0].values[0].Should().Be("1");
		}
	}
}




