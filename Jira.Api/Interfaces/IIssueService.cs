using Jira.Api.Linq;

using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issues of jira.
/// </summary>
public interface IIssueService
{
	/// <summary>
	/// Query builder for issues in jira.
	/// </summary>
	JiraQueryable<Issue> Queryable { get; }

	/// <summary>
	/// Whether to validate a JQL query
	/// </summary>
	bool ValidateQuery { get; set; }

	/// <summary>
	/// Maximum number of issues to retrieve per request.
	/// </summary>
	int MaxIssuesPerRequest { get; set; }

	/// <summary>
	/// Retrieves an issue by its key.
	/// </summary>
	Task<Issue> GetIssueAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves a list of issues by their keys.
	/// </summary>
	Task<IDictionary<string, Issue>> GetIssuesAsync(IEnumerable<string> issueKeys, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates all fields of an issue.
	/// </summary>
	Task UpdateIssueAsync(Issue issue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates all fields of an issue.
	/// </summary>
	Task UpdateIssueAsync(Issue issue, IssueUpdateOptions options, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates an issue and returns a new instance populated from server.
	/// </summary>
	Task<string> CreateIssueAsync(Issue issue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes the specified issue.
	/// </summary>
	Task DeleteIssueAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Execute a specific JQL query and return the resulting issues.
	/// </summary>
	Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(string jql, int skip, int? maxIssues, CancellationToken cancellationToken = default);

	/// <summary>
	/// Execute a specific JQL query and return the resulting issues.
	/// </summary>
	Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(IssueSearchOptions options, CancellationToken cancellationToken = default);

	/// <summary>
	/// Transition an issue through a workflow action.
	/// </summary>
	Task ExecuteWorkflowActionAsync(Issue issue, string actionNameOrId, WorkflowTransitionUpdates? updates, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets time tracking information for an issue.
	/// </summary>
	Task<IssueTimeTrackingData> GetTimeTrackingDataAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets metadata object containing dictionary with issue fields identifiers as keys and their metadata as values.
	/// </summary>
	Task<IDictionary<string, IssueFieldEditMetadata>> GetFieldsEditMetadataAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a comment to an issue.
	/// </summary>
	Task<Comment> AddCommentAsync(string issueKey, Comment comment, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns all comments of an issue.
	/// </summary>
	Task<IEnumerable<Comment>> GetCommentsAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns all comments of an issue.
	/// </summary>
	Task<IEnumerable<Comment>> GetCommentsAsync(string issueKey, CommentQueryOptions options, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes a comment from an issue.
	/// </summary>
	Task DeleteCommentAsync(string issueKey, string commentId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates a comment in an issue.
	/// </summary>
	Task<Comment> UpdateCommentAsync(string issueKey, Comment comment, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the comments of an issue with paging.
	/// </summary>
	Task<IPagedQueryResult<Comment>> GetPagedCommentsAsync(string issueKey, int skip, int? take, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the workflow actions that an issue can be transitioned to.
	/// </summary>
	Task<IEnumerable<IssueTransition>> GetActionsAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the workflow actions that an issue can be transitioned to.
	/// </summary>
	Task<IEnumerable<IssueTransition>> GetActionsAsync(string issueKey, bool expandTransitionFields, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieve attachment metadata from server for this issue.
	/// </summary>
	Task<IEnumerable<Attachment>> GetAttachmentsAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieve the labels from server for the issue specified.
	/// </summary>
	[Obsolete("Use Issue.Labels instead.")]
	Task<string[]> GetLabelsAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the labels for the issue specified.
	/// </summary>
	[Obsolete("Modify the Issue.Labels collection and call Issue.SaveChanges to update the labels field.")]
	Task SetLabelsAsync(string issueKey, string[] labels, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieve the watchers from server for the issue specified.
	/// </summary>
	Task<IEnumerable<JiraUser>> GetWatchersAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes a user from the watcher list of an issue.
	/// </summary>
	Task DeleteWatcherAsync(string issueKey, string usernameOrAccountId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a user to the watcher list of an issue.
	/// </summary>
	Task AddWatcherAsync(string issueKey, string usernameOrAccountId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieve the change logs from server for the issue specified.
	/// </summary>
	Task<IEnumerable<IssueChangeLog>> GetChangeLogsAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the issues that are marked as sub tasks of this issue.
	/// </summary>
	Task<IPagedQueryResult<Issue>> GetSubTasksAsync(string issueKey, int skip, int? take, CancellationToken cancellationToken = default);

	/// <summary>
	/// Add one or more attachments to an issue.
	/// </summary>
	Task AddAttachmentsAsync(string issueKey, UploadAttachmentInfo[] attachments, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes an attachment from an issue.
	/// </summary>
	Task DeleteAttachmentAsync(string issueKey, string attachmentId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the worklog with the given identifier from an issue.
	/// </summary>
	Task<Worklog> GetWorklogAsync(string issueKey, string worklogId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the worklogs for an issue.
	/// </summary>
	Task<IEnumerable<Worklog>> GetWorklogsAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a work log to an issue.
	/// </summary>
	Task<Worklog> AddWorklogAsync(string issueKey, Worklog worklog, WorklogStrategy worklogStrategy, string? newEstimate, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes a work log from an issue.
	/// </summary>
	Task DeleteWorklogAsync(string issueKey, string worklogId, WorklogStrategy worklogStrategy, string? newEstimate, CancellationToken cancellationToken = default);

	/// <summary>
	/// Assigns an issue to the specified user.
	/// </summary>
	Task AssignIssueAsync(string issueKey, string assignee, CancellationToken cancellationToken = default);

	/// <summary>
	/// Fetch all entity properties attached to the issue.
	/// </summary>
	Task<IEnumerable<string>> GetPropertyKeysAsync(string issueKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Fetch requested entity properties attached to the issue.
	/// </summary>
	Task<ReadOnlyDictionary<string, JToken>> GetPropertiesAsync(string issueKey, IEnumerable<string> propertyKeys, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds an entity property to the specified issue.
	/// </summary>
	Task SetPropertyAsync(string issueKey, string propertyKey, JToken obj, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes the entity property from the specified issue.
	/// </summary>
	Task DeletePropertyAsync(string issueKey, string propertyKey, CancellationToken cancellationToken = default);
}
