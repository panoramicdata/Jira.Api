namespace Jira.Api.Test;

public class CustomFieldCollectionTest
{
	[Fact]
	public void IndexByName_ShouldThrowIfUnableToFindRemoteValue()
	{
		var jira = TestableJira.Create();
		jira.SetupIssues(new RemoteIssue { key = "123" });

		var issue = CustomFieldTestData.CreateIssue(jira, "bar", "foo", [new() { customfieldId = "123", values = ["abc"] }]);

		var act = () => issue["CustomField"];
		act.Should().ThrowExactly<InvalidOperationException>();
	}

	[Fact]
	public void IndexByName_ShouldReturnRemoteValue()
	{
		var jira = CustomFieldTestData.CreateJiraWithCustomField();

		var issue = CustomFieldTestData.CreateIssue(jira, "projectKey", "issueKey", [new() { customfieldId = "123", values = ["abc"] }]);

		issue["CustomField"].Should().Be("abc");
		var customFieldX = issue.CustomFields["CustomField"];
		customFieldX.Should().NotBeNull();
		customFieldX.Id.Should().Be("123");

		issue["customfield"] = "foobar";
		issue["customfield"].Should().Be("foobar");
	}

	[Fact]
	public void WillThrowErrorIfCustomFieldNotFound()
	{
		var jira = CustomFieldTestData.CreateJiraWithCustomField();

		var issue = CustomFieldTestData.CreateIssue(jira, "projectKey", "issueKey", []);

		var act = () => issue.CustomFields["NonExistantField"]!.Values[0];
		act.Should().ThrowExactly<InvalidOperationException>();
	}
}
