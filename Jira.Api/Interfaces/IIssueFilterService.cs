namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the filters of jira.
/// </summary>
public interface IIssueFilterService
{
	/// <summary>
	/// Returns a filter with the specified id.
	/// </summary>
	Task<JiraFilter> GetFilterAsync(string filterId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the favourite filters for the user.
	/// </summary>
	Task<IEnumerable<JiraFilter>> GetFavouritesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns issues that match the specified favorite filter.
	/// </summary>
	Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteAsync(string filterName, int skip, int? take, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns issues that match the specified favorite filter.
	/// </summary>
	Task<IPagedQueryResult<Issue>> GetIssuesFromFavoriteWithFieldsAsync(string filterName, int skip, int? take, IList<string> fields, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns issues that match the filter with the specified id.
	/// </summary>
	Task<IPagedQueryResult<Issue>> GetIssuesFromFilterAsync(string filterId, int skip, int? take, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns issues that match the filter with the specified id.
	/// </summary>
	Task<IPagedQueryResult<Issue>> GetIssuesFromFilterWithFieldsAsync(string filterId, int skip, int? take, IList<string> fields, CancellationToken cancellationToken = default);
}
