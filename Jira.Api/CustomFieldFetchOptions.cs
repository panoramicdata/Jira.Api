using System.Collections.Generic;

namespace Jira.Api;

/// <summary>
/// Represents the filters to use when fetching custom fields.
/// </summary>
public class CustomFieldFetchOptions
{
	/// <summary>
	/// The list of projects with which to filter the results.
	/// </summary>
	public IList<string> ProjectKeys { get; } = [];

	/// <summary>
	/// The list of issue type ids with which to filter the results.
	/// </summary>
	public IList<string> IssueTypeIds { get; } = [];

	/// <summary>
	/// The list of issue type names with which to filter the results.
	/// </summary>
	public IList<string> IssueTypeNames { get; } = [];
}
