using Jira.Api.Exceptions;
using Jira.Api.Models;
using Jira.Api.Remote;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Jira.Api.Test;

/// <summary>
/// Tests for resolving a project's custom fields via create-metadata. Jira 9.0 removed the legacy
/// expand-based <c>issue/createmeta</c> endpoint (requests to it fall through to <c>issue/{key}</c> and
/// return "Issue Does Not Exist"), so the service must use the per-issue-type endpoints on Jira 8.4+/9+
/// and fall back to the legacy endpoint only on older servers.
/// </summary>
public class CustomFieldCreateMetaTest
{
	private static Mock<IJiraRestClient> CreateClientMock()
	{
		var client = new Mock<IJiraRestClient>();
		client.Setup(c => c.Settings).Returns(new JiraRestClientSettings("test"));
		return client;
	}

	private static void SetupResource(Mock<IJiraRestClient> client, string resourceContains, JToken response)
		=> client
			.Setup(c => c.ExecuteRequestAsync(
				It.IsAny<Method>(),
				It.Is<string>(s => s.Contains(resourceContains, StringComparison.Ordinal)),
				It.IsAny<object?>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(response);

	private static CustomFieldFetchOptions ForProject(string projectKey)
	{
		var options = new CustomFieldFetchOptions();
		options.ProjectKeys.Add(projectKey);
		return options;
	}

	[Fact]
	public async Task GetCustomFields_UsesJira9CreateMetaEndpoints()
	{
		var client = CreateClientMock();
		var jira = JiraClient.CreateRestClient(client.Object);

		SetupResource(client, "/issue/createmeta/BC/issuetypes?", JObject.Parse(
			"""{ "maxResults": 50, "startAt": 0, "total": 1, "values": [ { "id": "10001", "name": "Task" } ] }"""));

		SetupResource(client, "/issue/createmeta/BC/issuetypes/10001", JObject.Parse(
			"""
			{ "maxResults": 50, "startAt": 0, "total": 2, "values": [
				{ "fieldId": "summary", "name": "Summary", "required": true, "schema": { "type": "string", "system": "summary" } },
				{ "fieldId": "customfield_10050", "name": "My Custom Field", "required": false,
				  "schema": { "type": "string", "custom": "com.atlassian.jira.plugin.system.customfieldtypes:textfield", "customId": 10050 } }
			] }
			"""));

		var fields = (await jira.Fields.GetCustomFieldsAsync(ForProject("BC"), CancellationToken.None)).ToList();

		fields.Should().ContainSingle();
		fields[0].Id.Should().Be("customfield_10050");
		fields[0].Name.Should().Be("My Custom Field");
		fields[0].CustomType.Should().Be("com.atlassian.jira.plugin.system.customfieldtypes:textfield");
	}

	[Fact]
	public async Task GetCustomFields_FallsBackToLegacyCreateMeta_WhenNewEndpointNotFound()
	{
		var client = CreateClientMock();
		var jira = JiraClient.CreateRestClient(client.Object);

		// Simulate an older Jira that lacks the per-issue-type endpoint.
		client
			.Setup(c => c.ExecuteRequestAsync(
				It.IsAny<Method>(),
				It.Is<string>(s => s.Contains("/issue/createmeta/BC/issuetypes", StringComparison.Ordinal)),
				It.IsAny<object?>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new ResourceNotFoundException("The requested resource was not found."));

		SetupResource(client, "issue/createmeta?expand=", JObject.Parse(
			"""
			{ "projects": [ { "key": "BC", "issuetypes": [ { "fields": {
				"summary": { "name": "Summary", "schema": { "type": "string", "system": "summary" } },
				"customfield_10050": { "name": "Legacy Custom", "schema": { "type": "string", "custom": "com.atlassian.jira.plugin.system.customfieldtypes:textarea", "customId": 10050 } }
			} } ] } ] }
			"""));

		var fields = (await jira.Fields.GetCustomFieldsAsync(ForProject("BC"), CancellationToken.None)).ToList();

		fields.Should().ContainSingle();
		fields[0].Id.Should().Be("customfield_10050");
		fields[0].Name.Should().Be("Legacy Custom");
		fields[0].CustomType.Should().Be("com.atlassian.jira.plugin.system.customfieldtypes:textarea");
	}
}
