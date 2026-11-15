using Jira.Api.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Jira.Api.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers.")]
public partial class Issue
{
	/// <summary>
	/// Retrieve change logs from server for this issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<IssueChangeLog>> GetChangeLogsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve change logs from server, issue has not been created.");
		}

		return Jira.Issues.GetChangeLogsAsync(OriginalRemoteIssue.key, cancellationToken);
	}

	/// <summary>
	/// Get the comments for this issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<Comment>> GetCommentsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve comments from server, issue has not been created.");
		}

		return Jira.Issues.GetCommentsAsync(OriginalRemoteIssue.key, cancellationToken);
	}

	/// <summary>
	/// Get the comments for this issue.
	/// </summary>
	/// <param name="options">Options to use when querying the comments.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<Comment>> GetCommentsAsync(CommentQueryOptions options, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve comments from server, issue has not been created.");
		}

		return Jira.Issues.GetCommentsAsync(OriginalRemoteIssue.key, options, cancellationToken);
	}

	/// <summary>
	/// Get the comments for this issue.
	/// </summary>
	/// <param name="skip">Index of the first comment to return (0-based).</param>
	/// <param name="take">Maximum number of comments to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IPagedQueryResult<Comment>> GetPagedCommentsAsync(
		int skip,
		int? take,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve comments from server, issue has not been created.");
		}

		return Jira.Issues.GetPagedCommentsAsync(Key.Value, skip, take, cancellationToken);
	}

	/// <summary>
	/// Add a comment to this issue.
	/// </summary>
	/// <param name="comment">Comment text to add.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task<Comment> AddCommentAsync(string comment, CancellationToken cancellationToken)
	{
		var jiraUser = await Jira.Users.GetMyselfAsync(cancellationToken);

		var author = Jira.RestClient.Settings.EnableUserPrivacyMode ? jiraUser.AccountId : jiraUser.Username;

		return await AddCommentAsync(new Comment { Author = author, Body = comment }, cancellationToken);
	}

	/// <summary>
	/// Removes a comment from this issue.
	/// </summary>
	/// <param name="comment">Comment to remove.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task DeleteCommentAsync(Comment comment, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to delete comment from server, issue has not been created.");
		}

		return Jira.Issues.DeleteCommentAsync(OriginalRemoteIssue.key, comment.Id, cancellationToken);
	}

	/// <summary>
	/// Add a comment to this issue.
	/// </summary>
	/// <param name="comment">Comment object to add.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<Comment> AddCommentAsync(Comment comment, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to add comment to issue, issue has not been created.");
		}

		return Jira.Issues.AddCommentAsync(Key.Value, comment, cancellationToken);
	}

	/// <summary>
	/// Update a comment in this issue.
	/// </summary>
	/// <param name="comment">Comment object to update.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<Comment> UpdateCommentAsync(Comment comment, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to update comment to issue, issue has not been created.");
		}

		return Jira.Issues.UpdateCommentAsync(Key.Value, comment, cancellationToken);
	}

	/// <summary>
	/// Retrieve the labels from server for this issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	[Obsolete("Use Issue.Labels instead.")]
	public Task<string[]> GetLabelsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to get labels from issue, issue has not been created.");
		}

		return Jira.Issues.GetLabelsAsync(OriginalRemoteIssue.key, cancellationToken);
	}

	/// <summary>
	///  Adds a worklog to this issue.
	/// </summary>
	/// <param name="timeSpent">Specifies a time duration in JIRA duration format, representing the time spent working on the worklog</param>
	/// <param name="worklogStrategy">How to handle the remaining estimate, recommend using AutoAdjustRemainingEstimate</param>
	/// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Worklog as constructed by server</returns>
	public Task<Worklog> AddWorklogAsync(
		string timeSpent,
		WorklogStrategy worklogStrategy,
		string? newEstimate,
		CancellationToken cancellationToken)
	{
		return AddWorklogAsync(new Worklog(timeSpent, DateTime.Now), worklogStrategy, newEstimate, cancellationToken);
	}

	/// <summary>
	///  Adds a worklog to this issue.
	/// </summary>
	/// <param name="worklog">The worklog instance to add</param>
	/// <param name="worklogStrategy">How to handle the remaining estimate, recommend using AutoAdjustRemainingEstimate</param>
	/// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	/// <returns>Worklog as constructed by server</returns>
	public Task<Worklog> AddWorklogAsync(
		Worklog worklog,
		WorklogStrategy worklogStrategy,
		string? newEstimate,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to add worklog to issue, issue has not been saved to server.");
		}

		return Jira.Issues.AddWorklogAsync(OriginalRemoteIssue.key, worklog, worklogStrategy, newEstimate, cancellationToken);
	}

	/// <summary>
	/// Deletes the given worklog from the issue and updates the remaining estimate field.
	/// </summary>
	/// <param name="worklog">The worklog to remove.</param>
	/// <param name="worklogStrategy">How to handle the remaining estimate, suggest AutoAdjustRemainingEstimate.</param>
	/// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task DeleteWorklogAsync(
		Worklog worklog,
		WorklogStrategy worklogStrategy,
		string? newEstimate,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to delete worklog from issue, issue has not been saved to server.");
		}

		return Jira.Issues.DeleteWorklogAsync(OriginalRemoteIssue.key, worklog.Id, worklogStrategy, newEstimate, cancellationToken);
	}

	/// <summary>
	/// Retrieve worklogs for this issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<Worklog>> GetWorklogsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve worklogs, issue has not been saved to server.");
		}

		return Jira.Issues.GetWorklogsAsync(OriginalRemoteIssue.key, cancellationToken);
	}
}
