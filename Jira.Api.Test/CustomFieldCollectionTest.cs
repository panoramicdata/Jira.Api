﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jira.Api.Remote;
using Xunit;

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

		Assert.Throws<InvalidOperationException>(() => issue["CustomField"]);
	}

	[Fact]
	public void IndexByName_ShouldReturnRemoteValue()
	{
		//arrange
		var jira = TestableJira.Create();
		var customField = new CustomField(new RemoteField() { id = "123", name = "CustomField" });
		jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
			.Returns(Task.FromResult(Enumerable.Repeat<CustomField>(customField, 1)));

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
		Assert.Equal("abc", issue["CustomField"]);
		Assert.Equal("123", issue.CustomFields["CustomField"].Id);

		issue["customfield"] = "foobar";
		Assert.Equal("foobar", issue["customfield"]);
	}

	[Fact]
	public void WillThrowErrorIfCustomFieldNotFound()
	{
		// Arrange
		var jira = TestableJira.Create();
		var customField = new CustomField(new RemoteField() { id = "123", name = "CustomField" });
		jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
			.Returns(Task.FromResult(Enumerable.Repeat<CustomField>(customField, 1)));

		var issue = new RemoteIssue()
		{
			project = "projectKey",
			key = "issueKey",
			customFieldValues = null,
		}.ToLocal(jira);

		// Act / Assert
		Assert.Throws<InvalidOperationException>(() => issue.CustomFields["NonExistantField"].Values[0]);
	}
}
