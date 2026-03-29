namespace Jira.Api.Test;

public class CustomFieldCollectionTest
{
	[Fact]
	public void IndexByName_ShouldThrowIfUnableToFindRemoteValue()
	{
		var jira = TestableJira.Create();
		jira.SetupIssues(new RemoteIssue() { key = "123" });

		var issue = new RemoteIssue()
		{
		project = "bar",
		key = "foo",
		customFieldValues = [
			new(){
			customfieldId = "123",
			values = ["abc"]
			}
			]
		}.ToLocal(jira);

		var act = () => issue["CustomField"];
		act.Should().ThrowExactly<InvalidOperationException>();
	}

	[Fact]
	public void IndexByName_ShouldReturnRemoteValue()
	{
		//arrange
		var jira = TestableJira.Create();
		var customField = new CustomField(new RemoteField() { id = "123", name = "CustomField" });
		jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
			.Returns(Task.FromResult(Enumerable.Repeat(customField, 1)));

		var issue = new RemoteIssue()
		{
			project = "projectKey",
			key = "issueKey",
			customFieldValues = [
								new(){
									customfieldId = "123",
									values = ["abc"]
								}
							]
		}.ToLocal(jira);

		//assert
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
		// Arrange
		var jira = TestableJira.Create();
		var customField = new CustomField(new RemoteField() { id = "123", name = "CustomField" });
		jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
			.Returns(Task.FromResult(Enumerable.Repeat(customField, 1)));

		var issue = new RemoteIssue()
		{
			project = "projectKey",
			key = "issueKey",
			customFieldValues = [],
		}.ToLocal(jira);

		// Act / Assert
		var act = () => issue.CustomFields["NonExistantField"]!.Values[0];
		act.Should().ThrowExactly<InvalidOperationException>();
	}
}

