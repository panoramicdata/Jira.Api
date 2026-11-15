namespace Jira.Api.Services;

internal class ScreenService(JiraClient jira) : IScreenService
{
	private readonly JiraClient _jira = jira;

	public async Task<IEnumerable<Screen>> GetScreensAsync(CancellationToken cancellationToken = default)
	{
		var resource = "rest/api/2/screens";
		var result = await _jira.RestClient.ExecuteRequestAsync<RemotePagedResult<RemoteScreen>>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		return result.Values?.Select(x => new Screen(x)) ?? [];
	}

	public async Task<Screen> GetScreenAsync(long screenId, CancellationToken cancellationToken = default)
	{
		var resource = $"rest/api/2/screens/{screenId}";
		var remoteScreen = await _jira.RestClient.ExecuteRequestAsync<RemoteScreen>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);
		return new Screen(remoteScreen);
	}

	public async Task<IEnumerable<ScreenField>> GetScreenAvailableFieldsAsync(string screenId, CancellationToken cancellationToken)
	{
		var remote = await GetListAsync<RemoteScreenField>($"rest/api/2/screens/{screenId}/availableFields", null, cancellationToken).ConfigureAwait(false);
		return remote.Select(x => new ScreenField(x));
	}

	public async Task<IEnumerable<ScreenTab>> GetScreenTabsAsync(string screenId, string? projectKey, CancellationToken cancellationToken)
	{
		var remote = await GetListAsync<RemoteScreenTab>($"rest/api/2/screens/{screenId}/tabs", projectKey, cancellationToken).ConfigureAwait(false);
		return remote.Select(x => new ScreenTab(x));
	}

	public async Task<IEnumerable<ScreenField>> GetScreenTabFieldsAsync(string screenId, string tabId, string? projectKey, CancellationToken cancellationToken)
	{
		var remote = await GetListAsync<RemoteScreenField>($"rest/api/2/screens/{screenId}/tabs/{tabId}/fields", projectKey, cancellationToken).ConfigureAwait(false);
		return remote.Select(x => new ScreenField(x));
	}

	private Task<IEnumerable<TRemote>> GetListAsync<TRemote>(string resource, string? projectKey, CancellationToken cancellationToken)
	{
		if (!string.IsNullOrWhiteSpace(projectKey))
		{
			resource += $"?projectKey={projectKey}";
		}

		return _jira.RestClient.ExecuteRequestAsync<IEnumerable<TRemote>>(Method.Get, resource, null, cancellationToken);
	}
}