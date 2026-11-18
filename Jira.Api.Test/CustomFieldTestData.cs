namespace Jira.Api.Test;

internal static class CustomFieldTestData
{
	internal static TestableJira CreateJiraWithCustomField()
	{
		var jira = TestableJira.Create();
		var customField = new CustomField(new RemoteField { id = "123", name = "CustomField" });
		jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
			.Returns(Task.FromResult(Enumerable.Repeat(customField, 1)));
		return jira;
	}

	internal static Issue CreateIssue(TestableJira jira, string project, string key, RemoteCustomFieldValue[] customFieldValues)
		=> new RemoteIssue
		{
			project = project,
			key = key,
			customFieldValues = customFieldValues
		}.ToLocal(jira);

	internal static JsonSerializerSettings CreateSerializerSettings(RemoteField remoteField)
	{
		var serializerSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		};
		serializerSettings.Converters.Add(new RemoteIssueJsonConverter([remoteField], new Dictionary<string, ICustomFieldValueSerializer>()));
		return serializerSettings;
	}
}