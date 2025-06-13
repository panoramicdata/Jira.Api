using Jira.Api.Linq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

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
	/// <param name="issueKey">The issue key to retrieve</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<Issue> GetIssueAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Retrieves a list of issues by their keys.
	/// </summary>
	/// <param name="issueKeys">List of issue keys to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IDictionary<string, Issue>> GetIssuesAsync(IEnumerable<string> issueKeys, CancellationToken cancellationToken);

	/// <summary>
	/// Updates all fields of an issue.
	/// </summary>
	/// <param name="issue">Issue to update.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task UpdateIssueAsync(Issue issue, CancellationToken cancellationToken);

	/// <summary>
	/// Updates all fields of an issue.
	/// </summary>
	/// <param name="issue">Issue to update.</param>
	/// <param name="options">Options for the update</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task UpdateIssueAsync(Issue issue, IssueUpdateOptions options, CancellationToken cancellationToken);

	/// <summary>
	/// Creates an issue and returns a new instance populated from server.
	/// </summary>
	/// <param name="issue">Issue to create.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	/// <returns>Promise that contains the new issue key when resolved.</returns>
	Task<string> CreateIssueAsync(Issue issue, CancellationToken cancellationToken);

	/// <summary>
	/// Deletes the specified issue.
	/// </summary>
	/// <param name="issueKey">Key of issue to delete.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task DeleteIssueAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Execute a specific JQL query and return the resulting issues.
	/// </summary>
	/// <param name="jql">JQL search query</param>
	/// <param name="skip">Index of the first issue to return (0-based)</param>
	/// <param name="maxIssues">Maximum number of issues to return (defaults to 20). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(string jql, int skip, int? maxIssues, CancellationToken cancellationToken);

	/// <summary>
	/// Execute a specific JQL query and return the resulting issues.
	/// </summary>
	/// <param name="options">Options to use when executing the search.</param>
	/// <param name="cancellationToken">Cancellatin token for this operation.</param>
	Task<IPagedQueryResult<Issue>> GetIssuesFromJqlAsync(IssueSearchOptions options, CancellationToken cancellationToken);

	/// <summary>
	/// Transition an issue through a workflow action.
	/// </summary>
	/// <param name="issue">Issue to transition.</param>
	/// <param name="actionNameOrId">The workflow action name or id to transition to.</param>
	/// <param name="updates">Additional updates to perform when transitioning the issue.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task ExecuteWorkflowActionAsync(Issue issue, string actionNameOrId, WorkflowTransitionUpdates? updates, CancellationToken cancellationToken);

	/// <summary>
	/// Gets time tracking information for an issue.
	/// </summary>
	/// <param name="issueKey">The issue key.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IssueTimeTrackingData> GetTimeTrackingDataAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Gets metadata object containing dictionary with issuefields identifiers as keys and their metadata as values
	/// </summary>
	/// <param name="issueKey">The issue key.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IDictionary<string, IssueFieldEditMetadata>> GetFieldsEditMetadataAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Adds a comment to an issue.
	/// </summary>
	/// <param name="issueKey">Issue key to add the comment to.</param>
	/// <param name="comment">Comment object to add.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<Comment> AddCommentAsync(string issueKey, Comment comment, CancellationToken cancellationToken);

	/// <summary>
	/// Returns all comments of an issue.
	/// </summary>
	/// <param name="issueKey">Issue key to retrieve comments from.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<Comment>> GetCommentsAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Returns all comments of an issue.
	/// </summary>
	/// <param name="issueKey">Issue key to retrieve comments from.</param>
	/// <param name="options">Options to configure the values of the query.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<Comment>> GetCommentsAsync(string issueKey, CommentQueryOptions options, CancellationToken cancellationToken);

	/// <summary>
	/// Removes a comment from an issue.
	/// </summary>
	/// <param name="issueKey">Issue key to remove the comment from.</param>
	/// <param name="commentId">Identifier of the comment to remove.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task DeleteCommentAsync(string issueKey, string commentId, CancellationToken cancellationToken);

	/// <summary>
	/// Updates a comment in an issue.
	/// </summary>
	/// <param name="issueKey">Issue key to update the comment to.</param>
	/// <param name="comment">Comment object to update.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<Comment> UpdateCommentAsync(string issueKey, Comment comment, CancellationToken cancellationToken);

	/// <summary>
	/// Returns the comments of an issue with paging.
	/// </summary>
	/// <param name="issueKey">Issue key to retrieve comments from.</param>
	/// <param name="skip">Index of the first comment to return (0-based).</param>
	/// <param name="take">Maximum number of comments to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IPagedQueryResult<Comment>> GetPagedCommentsAsync(string issueKey, int skip, int? take, CancellationToken cancellationToken);

	/// <summary>
	/// Returns the workflow actions that an issue can be transitioned to.
	/// </summary>
	/// <param name="issueKey">The issue key</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<IssueTransition>> GetActionsAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Returns the workflow actions that an issue can be transitioned to.
	/// </summary>
	/// <param name="issueKey">The issue key</param>
	/// <param name="expandTransitionFields">Whether to show the transition fields</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<IssueTransition>> GetActionsAsync(string issueKey, bool expandTransitionFields, CancellationToken cancellationToken);

	/// <summary>
	/// Retrieve attachment metadata from server for this issue
	/// </summary>
	/// <param name="issueKey">The issue key to get attachments from.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<Attachment>> GetAttachmentsAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Retrieve the labels from server for the issue specified.
	/// </summary>
	/// <param name="issueKey">The issue key to get labels from.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	[Obsolete("Use Issue.Labels instead.")]
	Task<string[]> GetLabelsAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Sets the labels for the issue specified.
	/// </summary>
	/// <param name="issueKey">The issue key to set the labels.</param>
	/// <param name="labels">The list of labels to set on the issue.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	[Obsolete("Modify the Issue.Labels collection and call Issue.SaveChanges to update the labels field.")]
	Task SetLabelsAsync(string issueKey, string[] labels, CancellationToken cancellationToken);

	/// <summary>
	/// Retrieve the watchers from server for the issue specified.
	/// </summary>
	/// <param name="issueKey">The issue key to get watchers from.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<JiraUser>> GetWatchersAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Removes a user from the watcher list of an issue.
	/// </summary>
	/// <param name="issueKey">The issue key to remove the watcher from.</param>
	/// <param name="usernameOrAccountId">User name or account id of user to remove.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task DeleteWatcherAsync(string issueKey, string usernameOrAccountId, CancellationToken cancellationToken);

	/// <summary>
	/// Adds a user to the watcher list of an issue.
	/// </summary>
	/// <param name="issueKey">The issue key to add the watcher to.</param>
	/// <param name="usernameOrAccountId">User name or account id of user to add.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task AddWatcherAsync(string issueKey, string usernameOrAccountId, CancellationToken cancellationToken);

	/// <summary>
	/// Retrieve the change logs from server for the issue specified.
	/// </summary>
	/// <param name="issueKey">The issue key to get watchers from.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<IssueChangeLog>> GetChangeLogsAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Returns the issues that are marked as sub tasks of this issue.
	/// </summary>
	/// <param name="issueKey">The issue key to get sub tasks from.</param>
	/// <param name="skip">Index of the first issue to return (0-based).</param>
	/// <param name="take">Maximum number of issues to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IPagedQueryResult<Issue>> GetSubTasksAsync(string issueKey, int skip, int? take, CancellationToken cancellationToken);

	/// <summary>
	/// Add one or more attachments to an issue.
	/// </summary>
	/// <param name="issueKey">Issue key to add attachments to.</param>
	/// <param name="attachments">Attachments to add.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task AddAttachmentsAsync(string issueKey, UploadAttachmentInfo[] attachments, CancellationToken cancellationToken);

	/// <summary>
	/// Removes an attachment from an issue.
	/// </summary>
	/// <param name="issueKey">Issue key to remove the attachment from.</param>
	/// <param name="attachmentId">Identifier of the attachment to remove.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task DeleteAttachmentAsync(string issueKey, string attachmentId, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the worklog with the given identifier from an issue.
	/// </summary>
	/// <param name="issueKey">The issue key to retrieve the worklog from.</param>
	/// <param name="worklogId">The worklog identifier.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	/// <returns></returns>
	Task<Worklog> GetWorklogAsync(string issueKey, string worklogId, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the worklogs for an issue.
	/// </summary>
	/// <param name="issueKey">Issue key to retrieve the worklogs from.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<Worklog>> GetWorklogsAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Adds a work log to an issue.
	/// </summary>
	/// <param name="issueKey">Issue key to add the worklog to.</param>
	/// <param name="worklog">The worklog instance to add.</param>
	/// <param name="worklogStrategy">How to handle the remaining estimate, suggest AutoAdjustRemainingEstimate.</param>
	/// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<Worklog> AddWorklogAsync(
		string issueKey,
		Worklog worklog,
		WorklogStrategy worklogStrategy,
		string? newEstimate,
		CancellationToken cancellationToken);

	/// <summary>
	/// Removes a work log from an issue.
	/// </summary>
	/// <param name="issueKey">Issue key to remove the work log from.</param>
	/// <param name="worklogId">The identifier of the work log to remove.</param>
	/// <param name="worklogStrategy">How to handle the remaining estimate, suggest AutoAdjustRemainingEstimate.</param>
	/// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task DeleteWorklogAsync(
		string issueKey,
		string worklogId,
		WorklogStrategy worklogStrategy,
		string? newEstimate,
		CancellationToken cancellationToken);

	/// <summary>
	/// Assigns an issue to the specified user.
	/// </summary>
	/// <param name="issueKey">Identifier of the issue to assign.</param>
	/// <param name="assignee">The username or account id of the user to assign the issue to.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task AssignIssueAsync(string issueKey, string assignee, CancellationToken cancellationToken);

	/// <summary>
	/// Fetch all entity properties attached to the issue.
	/// </summary>
	/// <param name="issueKey">Identifier of the issue.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<string>> GetPropertyKeysAsync(string issueKey, CancellationToken cancellationToken);

	/// <summary>
	/// Fetch requested entity properties attached to the issue.
	/// </summary>
	/// <param name="issueKey">Identifier of the issue.</param>
	/// <param name="propertyKeys">The property keys to fetch.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	/// <returns>A mapping between requested property keys and stored values.</returns>
	Task<ReadOnlyDictionary<string, JToken>> GetPropertiesAsync(string issueKey, IEnumerable<string> propertyKeys, CancellationToken cancellationToken);

	/// <summary>
	/// Adds an entity property to the specified issue.
	/// </summary>
	/// <remarks>
	/// This method overwrites any already existing property values with the same key!
	/// </remarks>
	/// <param name="issueKey">Identifier of the issue.</param>
	/// <param name="propertyKey">The property key to identify the value.</param>
	/// <param name="obj">The JSON construct to store as property value.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task SetPropertyAsync(string issueKey, string propertyKey, JToken obj, CancellationToken cancellationToken);

	/// <summary>
	/// Removes the entity property from the specified issue.
	/// </summary>
	/// <param name="issueKey">Identifier of the issue.</param>
	/// <param name="propertyKey">The property key to identify the value.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task DeletePropertyAsync(string issueKey, string propertyKey, CancellationToken cancellationToken);
}
