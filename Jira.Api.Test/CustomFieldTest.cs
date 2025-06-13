using Jira.Api.Remote;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test;

public class CustomFieldTest
{
	[Fact]
	public void Name_ShouldRetriveValueFromRemote()
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
		Assert.Equal("CustomField", issue.CustomFields[0].Name);
	}

	[Fact]
	public async Task WhenAddingArrayOfValues_CanSerializeAsStringArrayWhenNoSerializerIsFound()
	{
		// arrange issue
		var jira = TestableJira.Create();
		var remoteField = new RemoteField() { id = "remotefield_id", Schema = new RemoteFieldSchema() { Custom = "remotefield_type" }, IsCustomField = true, name = "Custom Field" };
		var customField = new CustomField(remoteField);
		var issue = new RemoteIssue() { project = "projectKey", key = "issueKey" }.ToLocal(jira);

		jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
			.Returns(Task.FromResult(Enumerable.Repeat(customField, 1)));

		await issue.CustomFields.AddArrayAsync("Custom Field", ["val1", "val2"], default);

		// arrange serialization
		var remoteIssue = issue.ToRemote();
		var converter = new RemoteIssueJsonConverter([remoteField], new Dictionary<string, ICustomFieldValueSerializer>());
		var serializerSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		};
		serializerSettings.Converters.Add(converter);
		var issueWrapper = new RemoteIssueWrapper(remoteIssue);

		// act
		var issueJson = JsonConvert.SerializeObject(issueWrapper, serializerSettings);

		// assert
		var jObject = JObject.Parse(issueJson);
		var remoteFieldValue = jObject["fields"]["remotefield_id"];
		var valueArray = remoteFieldValue.ToObject<string[]>();
		Assert.Equal(2, valueArray.Length);
		Assert.Contains("val1", valueArray);
		Assert.Contains("val2", valueArray);
	}
	private static readonly string[] _val1val2 = ["val1", "val2"];

	[Fact]
	public void CanDeserializeArrayOfStrings_WhenCustomFieldValueIsArrayAndNoSerializerIsRegistered()
	{
		// arrange issue
		var remoteField = new RemoteField() { id = "customfield_id", Schema = new RemoteFieldSchema() { Custom = "customfield_type" }, IsCustomField = true, name = "Custom Field" };
		var jObject = JObject.FromObject(new
		{
			fields = new
			{
				//project = "projectKey",
				key = "issueKey",
				customfield_id = _val1val2
			}
		});

		// arrange serialization
		var converter = new RemoteIssueJsonConverter([remoteField], new Dictionary<string, ICustomFieldValueSerializer>());
		var serializerSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		};
		serializerSettings.Converters.Add(converter);

		// act
		var remoteIssue = JsonConvert.DeserializeObject<RemoteIssueWrapper>(jObject.ToString(), serializerSettings).RemoteIssue;

		// assert
		var customFieldValues = remoteIssue.customFieldValues.First().values;
		Assert.Equal(2, customFieldValues.Length);
		Assert.Contains("val1", customFieldValues);
		Assert.Contains("val2", customFieldValues);
	}
}
