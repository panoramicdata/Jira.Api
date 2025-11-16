using System.Collections.Concurrent;

namespace Jira.Api;

/// <summary>
/// Cache for frequently retrieved server items from JIRA.
/// </summary>
public class JiraCache
{
	/// <summary>
	/// The current authenticated user
	/// </summary>
	public JiraUser CurrentUser { get; set; }

	/// <summary>
	/// Cached issue types
	/// </summary>
	public JiraEntityDictionary<IssueType> IssueTypes { get; } = new JiraEntityDictionary<IssueType>();

	/// <summary>
	/// Cached project components
	/// </summary>
	public JiraEntityDictionary<ProjectComponent> Components { get; } = new JiraEntityDictionary<ProjectComponent>();

	/// <summary>
	/// Cached project versions
	/// </summary>
	public JiraEntityDictionary<ProjectVersion> Versions { get; } = new JiraEntityDictionary<ProjectVersion>();

	/// <summary>
	/// Cached issue priorities
	/// </summary>
	public JiraEntityDictionary<IssuePriority> Priorities { get; } = new JiraEntityDictionary<IssuePriority>();

	/// <summary>
	/// Cached issue statuses
	/// </summary>
	public JiraEntityDictionary<IssueStatus> Statuses { get; } = new JiraEntityDictionary<IssueStatus>();

	/// <summary>
	/// Cached issue resolutions
	/// </summary>
	public JiraEntityDictionary<IssueResolution> Resolutions { get; } = new JiraEntityDictionary<IssueResolution>();

	/// <summary>
	/// Cached projects
	/// </summary>
	public JiraEntityDictionary<Project> Projects { get; } = new JiraEntityDictionary<Project>();

	/// <summary>
	/// Cached custom fields
	/// </summary>
	public JiraEntityDictionary<CustomField> CustomFields { get; } = new JiraEntityDictionary<CustomField>();

	/// <summary>
	/// Cached issue link types
	/// </summary>
	public JiraEntityDictionary<IssueLinkType> LinkTypes { get; } = new JiraEntityDictionary<IssueLinkType>();

	/// <summary>
	/// Custom fields cached by project key
	/// </summary>
	public ConcurrentDictionary<string, JiraEntityDictionary<CustomField>> ProjectCustomFields { get; } = new ConcurrentDictionary<string, JiraEntityDictionary<CustomField>>();

	/// <summary>
	/// Issue types cached by project key
	/// </summary>
	public ConcurrentDictionary<string, JiraEntityDictionary<IssueType>> ProjectIssueTypes { get; } = new ConcurrentDictionary<string, JiraEntityDictionary<IssueType>>();
}
