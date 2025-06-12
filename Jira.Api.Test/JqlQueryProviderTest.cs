using System.Linq;
using Jira.Api.Linq;
using Jira.Api.Remote;
using Xunit;

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

		Assert.Equal(1, queryable.Count());
	}

	[Fact]
	public void First()
	{
		var jira = TestableJira.Create();
		var provider = new JiraQueryProvider(jira.Translator.Object, jira.IssueService.Object);
		var queryable = new JiraQueryable<Issue>(provider);

		jira.SetupIssues(new RemoteIssue() { summary = "foo" }, new RemoteIssue());

		Assert.Equal("foo", queryable.First().Summary);
	}
}
