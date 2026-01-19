using AwesomeAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
		issue.CustomFields[0].Name.Should().Be("CustomField");
	}

	[Fact]
	public void WhenAddingArrayOfValues_CanSerializeAsStringArrayWhenNoSerializerIsFound()
	{
		// arrange issue
		var jira = TestableJira.Create();
		var remoteField = new RemoteField() { id = "remotefield_id", Schema = new RemoteFieldSchema() { Custom = "remotefield_type" }, IsCustomField = true, name = "Custom Field" };
		var customField = new CustomField(remoteField);
		var issue = new RemoteIssue() { project = "projectKey", key = "issueKey" }.ToLocal(jira);

		jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
			.Returns(Task.FromResult(Enumerable.Repeat(customField, 1)));

		issue.CustomFields.AddArray("Custom Field", ["val1", "val2"]);

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
		var fields = jObject["fields"];
		fields.Should().NotBeNull();
		var remoteFieldValue = fields["remotefield_id"];
		remoteFieldValue.Should().NotBeNull();
		var valueArray = remoteFieldValue.ToObject<string[]>();
		valueArray.Should().HaveCount(2);
		valueArray.Should().Contain("val1");
		valueArray.Should().Contain("val2");
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
		var remoteIssueWrapper = JsonConvert.DeserializeObject<RemoteIssueWrapper>(jObject.ToString(), serializerSettings);
		remoteIssueWrapper.Should().NotBeNull();
		var remoteIssue = remoteIssueWrapper.RemoteIssue;

		// assert
		var customFieldValues = remoteIssue.customFieldValues.First().values;
		customFieldValues.Should().HaveCount(2);
		customFieldValues.Should().Contain("val1");
		customFieldValues.Should().Contain("val2");
	}
}

