using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jira.Api;

/// <summary>
/// Possible values of operations property in IssueFieldEditMetadata.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum IssueFieldEditMetadataOperation
{
	/// <summary>
	/// Set operation
	/// </summary>
	[EnumMember(Value = "SET")]
	SET = 1,

	/// <summary>
	/// Add operation
	/// </summary>
	[EnumMember(Value = "ADD")]
	ADD = 2,

	/// <summary>
	/// Remove operation
	/// </summary>
	[EnumMember(Value = "REMOVE")]
	REMOVE = 3,

	/// <summary>
	/// Edit operation
	/// </summary>
	[EnumMember(Value = "EDIT")]
	EDIT = 4
}
