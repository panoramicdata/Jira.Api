using System;

namespace Jira.Api.Linq;

/// <summary>
/// Attribute that can be applied to properties that map to different JQL field names
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class JqlFieldNameAttribute(string name) : Attribute
{
	public string Name { get; set; } = name;
}
