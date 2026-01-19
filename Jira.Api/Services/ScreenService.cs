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
		var resource = $"rest/api/2/screens/{screenId}/availableFields";

		var remoteScreenFields = await _jira.RestClient.ExecuteRequestAsync<IEnumerable<RemoteScreenField>>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);

		var screenFields = remoteScreenFields.Select(x => new ScreenField(x));
		return screenFields;
	}

	public async Task<IEnumerable<ScreenTab>> GetScreenTabsAsync(string screenId, string? projectKey, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/screens/{screenId}/tabs";
		if (!string.IsNullOrWhiteSpace(projectKey))
		{
			resource += $"?projectKey={projectKey}";
		}

		var remoteScreenTabs = await _jira.RestClient.ExecuteRequestAsync<IEnumerable<RemoteScreenTab>>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);

		var screenTabs = remoteScreenTabs.Select(x => new ScreenTab(x));
		return screenTabs;
	}

	public async Task<IEnumerable<ScreenField>> GetScreenTabFieldsAsync(string screenId, string tabId, string? projectKey, CancellationToken cancellationToken)
	{
		var resource = $"rest/api/2/screens/{screenId}/tabs/{tabId}/fields";
		if (!string.IsNullOrWhiteSpace(projectKey))
		{
			resource += $"?projectKey={projectKey}";
		}

		var remoteScreenFields = await _jira.RestClient.ExecuteRequestAsync<IEnumerable<RemoteScreenField>>(Method.Get, resource, null, cancellationToken).ConfigureAwait(false);

		var screenFields = remoteScreenFields.Select(x => new ScreenField(x));
		return screenFields;
	}
}
