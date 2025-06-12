using Jira.Api.Remote;

namespace Jira.Api;

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

	public string CustomType => RemoteField.Schema?.Custom;

	public string CustomIdentifier => RemoteField.Schema?.CustomId;
}
