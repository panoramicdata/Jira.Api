namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the dashboards of jira.
/// </summary>
public interface IDashboardService
{
	/// <summary>
	/// Returns a page of dashboards visible to the current user
	/// (GET rest/api/2/dashboard).
	/// </summary>
	/// <param name="skip">Index at which to start returning dashboards.</param>
	/// <param name="take">Maximum number of dashboards to return per page.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<JiraDashboardPage> GetDashboardsAsync(int skip, int? take, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the dashboard with the specified id
	/// (GET rest/api/2/dashboard/{id}).
	/// </summary>
	/// <param name="dashboardId">Identifier of the dashboard.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<JiraDashboard> GetDashboardAsync(string dashboardId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the dashboard with the specified id, including its gadgets and their
	/// user preferences (GET rest/dashboards/1.0/{id}).
	/// </summary>
	/// <remarks>
	/// This endpoint is not part of the public JIRA REST API. It is the endpoint the
	/// JIRA Server web UI itself uses to render dashboards and has been verified against
	/// JIRA Server. It may not be available on JIRA Cloud.
	/// </remarks>
	/// <param name="dashboardId">Identifier of the dashboard.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	Task<JiraDashboardDetail> GetDashboardDetailAsync(string dashboardId, CancellationToken cancellationToken = default);
}
