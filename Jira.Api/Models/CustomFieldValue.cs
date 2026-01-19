using Newtonsoft.Json.Linq;

namespace Jira.Api.Models;

/// <summary>
/// A custom field associated with an issue
/// </summary>
public class CustomFieldValue
{
	private readonly Issue _issue;

	internal CustomFieldValue(string id, Issue issue)
	{
		Id = id;
		_issue = issue;
	}

	internal CustomFieldValue(string id, string name, Issue issue)
		: this(id, issue)
	{
		Name = name;
	}

	/// <summary>
	/// The values of the custom field
	/// </summary>
	public string[] Values { get; set; }

	/// <summary>
	/// Id of the custom field as defined in JIRA
	/// </summary>
	public string Id { get; private set; }

	internal JToken RawValue { get; set; }

	internal ICustomFieldValueSerializer? Serializer { get; set; }

	/// <summary>
	/// Name of the custom field as defined in JIRA
	/// </summary>
	public string Name
	{
		get
		{
			if (string.IsNullOrEmpty(field))
			{
				var customField = _issue.Jira.Fields.GetCustomFieldsAsync(default).Result.FirstOrDefault(f => f.Id == Id) ?? throw new InvalidOperationException($"Custom field with id '{Id}' was not found.");
				field = customField.Name;
			}

			return field;
		}
	}
}
