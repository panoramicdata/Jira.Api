using Jira.Api.Linq;

namespace Jira.Api.Test;

public class JqlQueryProviderTest
{
	[Fact]
	public void Count()
	{
		var jira = TestableJira.Create();
		var provider = new JiraQueryProvider(jira.Translator.Object, jira.IssueService.Object);
		var queryable = new JiraQueryable<Issue>(provider);

		jira.SetupIssues(new RemoteIssue());

		queryable.Should().ContainSingle();
	}

	[Fact]
	public void First()
	{
		var jira = TestableJira.Create();
		var provider = new JiraQueryProvider(jira.Translator.Object, jira.IssueService.Object);
		var queryable = new JiraQueryable<Issue>(provider);

		jira.SetupIssues(new RemoteIssue() { summary = "foo" }, new RemoteIssue());

		queryable.First().Summary.Should().Be("foo");
	}
}

