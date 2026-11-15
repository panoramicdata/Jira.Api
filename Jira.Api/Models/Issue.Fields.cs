using Jira.Api.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Jira.Api.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers.")]
public partial class Issue
{
	private void InitializeNamedEntities(RemoteIssue remoteIssue)
	{
		Status = CreateIssueStatus(remoteIssue.status);
		Priority = CreateIssuePriority(remoteIssue.priority);
		Resolution = CreateIssueResolution(remoteIssue.resolution);
		Type = CreateIssueType(remoteIssue.type);
	}

	private void InitializeCollections()
	{
		CustomFields = OriginalRemoteIssue.customFieldValues == null ? new CustomFieldValueCollection(this)
			: new CustomFieldValueCollection(this, [.. OriginalRemoteIssue.customFieldValues.Select(f => new CustomFieldValue(f.customfieldId, this) { Values = f.values, RawValue = f.rawValue })]);

		var affectsVersions = OriginalRemoteIssue.affectsVersions ?? Enumerable.Empty<RemoteVersion>();
		AffectsVersions = new ProjectVersionCollection("versions", Jira, Project, [.. affectsVersions.Select(v =>
		{
			v.ProjectKey = OriginalRemoteIssue.project;
			return new ProjectVersion(Jira, v);
		})]);

		var fixVersions = OriginalRemoteIssue.fixVersions ?? Enumerable.Empty<RemoteVersion>();
		FixVersions = new ProjectVersionCollection("fixVersions", Jira, Project, [.. fixVersions.Select(v =>
		{
			v.ProjectKey = OriginalRemoteIssue.project;
			return new ProjectVersion(Jira, v);
		})]);

		var labels = OriginalRemoteIssue.labels ?? [];
		Labels = new IssueLabelCollection([.. labels]);

		var components = OriginalRemoteIssue.components ?? Enumerable.Empty<RemoteComponent>();
		Components = new ProjectComponentCollection("components", Jira, Project, [.. components.Select(c =>
		{
			c.ProjectKey = OriginalRemoteIssue.project;
			return new ProjectComponent(c);
		})]);
	}

	internal RemoteIssue OriginalRemoteIssue { get; private set; }

	/// <summary>
	/// Fields not represented as properties that were retrieved from the server.
	/// </summary>
	public IssueFields AdditionalFields { get; private set; }

	/// <summary>
	/// The parent key if this issue is a subtask.
	/// </summary>
	/// <remarks>
	/// Only available if issue was retrieved using REST API.
	/// </remarks>
	public string ParentIssueKey { get; private set; }

	/// <summary>
	/// The JIRA server that created this issue
	/// </summary>
	public JiraClient Jira { get; }

	/// <summary>
	/// Gets the security level set on the issue.
	/// </summary>
	public IssueSecurityLevel SecurityLevel { get; private set; }

	/// <summary>
	/// Brief one-line summary of the issue
	/// </summary>
	[JqlContainsEquality]
	public string Summary { get; set; }

	/// <summary>
	/// Detailed description of the issue
	/// </summary>
	[JqlContainsEquality]
	public string Description { get; set; }

	/// <summary>
	/// Hardware or software environment to which the issue relates.
	/// </summary>
	[JqlContainsEquality]
	public string Environment { get; set; }

	/// <summary>
	/// Username or account id of user to whom the issue is currently assigned.
	/// </summary>
	public string Assignee { get; set; }

	/// <summary>
	/// User object of user to whom the issue is currently assigned.
	/// </summary>
	public JiraUser AssigneeUser { get; private set; }

	/// <summary>
	/// Time tracking data for this issue.
	/// </summary>
	public IssueTimeTrackingData TimeTrackingData { get; private set; }

	/// <summary>
	/// Gets the internal identifier assigned by JIRA.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2372:Exceptions should not be thrown from property getters", Justification = "Published public API: turning this property into a method is a breaking change for existing consumers.")]
	public string JiraIdentifier
	{
		get
		{
			if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
			{
				throw new InvalidOperationException("Unable to retrieve JIRA id, issue has not been created.");
			}

			return OriginalRemoteIssue.id;
		}
	}

	/// <summary>
	/// Unique identifier for this issue
	/// </summary>
	public ComparableString Key { get; private set; }

	/// <summary>
	/// Importance of the issue in relation to other issues
	/// </summary>
	public IssuePriority Priority { get; set; }

	/// <summary>
	/// Parent project to which the issue belongs
	/// </summary>
	public string Project { get; private set; }

	/// <summary>
	/// Username or account id of user who created the issue in Jira.
	/// </summary>
	public string Reporter { get; set; }

	/// <summary>
	/// User object of user who created the issue in Jira.
	/// </summary>
	public JiraUser ReporterUser { get; private set; }

	/// <summary>
	/// Record of the issue's resolution, if the issue has been resolved or closed
	/// </summary>
	public IssueResolution Resolution { get; set; }

	/// <summary>
	/// The stage the issue is currently at in its lifecycle.
	/// </summary>
	public IssueStatus Status { get; private set; }

	/// <summary>
	/// The type of the issue
	/// </summary>
	[RemoteFieldName("issuetype")]
	public IssueType Type { get; set
		{
			field = value;

			field?.ProjectKey = Project;
		} }

	/// <summary>
	/// Number of votes the issue has
	/// </summary>
	public long? Votes { get; private set; }

	/// <summary>
	/// Whether the user that retrieved this issue has voted on it.
	/// </summary>
	public bool HasUserVoted { get; private set; }

	/// <summary>
	/// Time and date on which this issue was entered into JIRA
	/// </summary>
	public DateTime? Created { get; private set; }

	/// <summary>
	/// Date by which this issue is scheduled to be completed
	/// </summary>
	public DateTime? DueDate { get; set; }

	/// <summary>
	/// Time and date on which this issue was last edited
	/// </summary>
	public DateTime? Updated { get; private set; }

	/// <summary>
	/// Time and date on which this issue was resolved.
	/// </summary>
	/// <remarks>
	/// Only available if issue was retrieved using REST API, use GetResolutionDate
	/// method for SOAP clients.
	/// </remarks>
	public DateTime? ResolutionDate { get; private set; }

	/// <summary>
	/// The components associated with this issue
	/// </summary>
	[JqlFieldName("component")]
	public ProjectComponentCollection Components { get; private set; }

	/// <summary>
	/// The versions that are affected by this issue
	/// </summary>
	[JqlFieldName("AffectedVersion")]
	public ProjectVersionCollection AffectsVersions { get; private set; }

	/// <summary>
	/// The versions in which this issue is fixed
	/// </summary>
	[JqlFieldName("FixVersion")]
	public ProjectVersionCollection FixVersions { get; private set; }

	/// <summary>
	/// The labels assigned to this issue.
	/// </summary>
	public IssueLabelCollection Labels { get; private set; }

	/// <summary>
	/// The custom fields associated with this issue
	/// </summary>
	public CustomFieldValueCollection CustomFields { get; private set; }

	/// <summary>
	/// Gets or sets the value of a custom field
	/// </summary>
	/// <param name="customFieldName">Custom field name</param>
	/// <returns>Value of the custom field</returns>
	public ComparableString? this[string customFieldName]
	{
		get
		{
			var customField = CustomFields[customFieldName];

			if (customField != null && customField.Values != null && customField.Values.Length > 0)
			{
				return customField.Values[0];
			}

			return null;
		}
		set
		{
			var customField = CustomFields[customFieldName];
			string[] customFieldValue = value == null ? null : [value.Value];

			if (customField != null)
			{
				customField.Values = customFieldValue;
			}
			else
			{
				CustomFields.Add(customFieldName, customFieldValue, null);
			}
		}
	}
}
