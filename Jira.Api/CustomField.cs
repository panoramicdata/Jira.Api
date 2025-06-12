using System;
using Jira.Api.Remote;

namespace Jira.Api;

public class CustomField : JiraNamedEntity
{
	private readonly RemoteField _remoteField;

	/// <summary>
	/// Creates an instance of a CustomField from a remote field definition.
	/// </summary>
	public CustomField(RemoteField remoteField)
		: base(remoteField)
	{
		_remoteField = remoteField;

		if (string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(CustomIdentifier))
		{
			Id = $"customfield_{CustomIdentifier}";
		}
	}

	internal RemoteField RemoteField
	{
		get
		{
			return _remoteField;
		}
	}

	public string CustomType
	{
		get
		{
			return _remoteField.Schema?.Custom;
		}
	}

	public string CustomIdentifier
	{
		get
		{
			return _remoteField.Schema?.CustomId;
		}
	}
}
