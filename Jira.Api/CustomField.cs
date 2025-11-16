using Jira.Api.Remote;

namespace Jira.Api;

/// <summary>
/// Represents a custom field in JIRA
/// </summary>
public class CustomField : JiraNamedEntity
{
	/// <summary>
	/// Creates an instance of a CustomField from a remote field definition.
	/// </summary>
	public CustomField(RemoteField remoteField)
		: base(remoteField)
	{
		RemoteField = remoteField;

		if (string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(CustomIdentifier))
		{
			Id = $"customfield_{CustomIdentifier}";
		}
	}

	internal RemoteField RemoteField { get; init; }

	/// <summary>
	/// The custom field type (e.g., "com.atlassian.jira.plugin.system.customfieldtypes:select")
	/// </summary>
	public string CustomType => RemoteField.Schema?.Custom;

	/// <summary>
	/// The numeric identifier for the custom field
	/// </summary>
	public string CustomIdentifier => RemoteField.Schema?.CustomId;
}
