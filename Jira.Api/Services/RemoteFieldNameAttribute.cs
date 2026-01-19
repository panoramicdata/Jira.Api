namespace Jira.Api.Services;

/// <summary>
/// Attribute that can be applied to properties to modify the name of the remotefield used when updating an issue
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class RemoteFieldNameAttribute(string remoteFieldName) : Attribute
{
	public string Name { get; set; } = remoteFieldName;
}
