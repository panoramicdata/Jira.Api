using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents the operations on the filters of jira.
/// </summary>
public interface IIssueFilterService
{
	/// <summary>
	/// Returns a filter with the specified id.
	/// </summary>
	/// <param name="filterId">Identifier of the filter to fetch.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<JiraFilter> GetFilterAsync(string filterId, CancellationToken cancellationToken);

	/// <summary>
	/// Returns the favourite filters for the user.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IEnumerable<JiraFilter>> GetFavouritesAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Returns issues that match the specified favorite filter.
	/// </summary>
	/// <param name="filterName">The name of the filter used for the search</param>
	/// <param name="skip">Index of the first issue to return (0-based)</param>
	/// <param name="take">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	/// <remarks>Includes basic fields.</remarks>
	Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteAsync(
		string filterName,
		int skip,
		int? take,
		CancellationToken cancellationToken);

	/// <summary>
	/// Returns issues that match the specified favorite filter.
	/// </summary>
	/// <param name="filterName">The name of the filter used for the search</param>
	/// <param name="skip">Index of the first issue to return (0-based)</param>
	/// <param name="take">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
	/// <param name="fields">A list of specific fields to fetch. Empty or <see langword="null"/> will fetch all fields.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteWithFieldsAsync(
		string filterName,
		int skip,
		int? take,
		IList<string> fields,
		CancellationToken cancellationToken);

	/// <summary>
	/// Returns issues that match the filter with the specified id.
	/// </summary>
	/// <param name="filterId">Identifier of the filter to fetch.</param>
	/// <param name="skip">Index of the first issue to return (0-based)</param>
	/// <param name="take">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	/// <remarks>Includes basic fields.</remarks>
	Task<IPagedQueryResult<Issue>> GetIssuesFromFilterAsync(
		string filterId,
		int skip,
		int? take,
		CancellationToken cancellationToken);

	/// <summary>
	/// Returns issues that match the filter with the specified id.
	/// </summary>
	/// <param name="filterId">Identifier of the filter to fetch.</param>
	/// <param name="skip">Index of the first issue to return (0-based)</param>
	/// <param name="take">Maximum number of issues to return (defaults to 50). The maximum allowable value is dictated by the JIRA property 'jira.search.views.default.max'. If you specify a value that is higher than this number, your search results will be truncated.</param>
	/// <param name="fields">A list of specific fields to fetch. Empty or <see langword="null"/> will fetch all fields.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<IPagedQueryResult<Issue>> GetIssuesFromFilterWithFieldsAsync(
		string filterId,
		int skip,
		int? take,
		IList<string> fields,
		CancellationToken cancellationToken);
}
