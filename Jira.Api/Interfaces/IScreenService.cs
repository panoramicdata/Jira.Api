namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the Jira screens.
/// </summary>
public interface IScreenService
{
	/// <summary>
	/// Gets the screen available fields.
	/// </summary>
	Task<IEnumerable<ScreenField>> GetScreenAvailableFieldsAsync(string screenId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the screen tabs.
	/// </summary>
	Task<IEnumerable<ScreenTab>> GetScreenTabsAsync(string screenId, string? projectKey = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the screen tab fields.
	/// </summary>
	Task<IEnumerable<ScreenField>> GetScreenTabFieldsAsync(string screenId, string tabId, string? projectKey = null, CancellationToken cancellationToken = default);
}
