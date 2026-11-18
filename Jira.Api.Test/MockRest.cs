namespace Jira.Api.Test;

internal static class MockRest
{
	public static (Mock<IJiraRestClient> Client, JiraClient Jira) Create()
	{
		var client = new Mock<IJiraRestClient>();
		return (client, JiraClient.CreateRestClient(client.Object));
	}

	public static void Returns<T>(this Mock<IJiraRestClient> client, Method method, string resource, T value, object? body = null)
		=> client.Setup(c => c.ExecuteRequestAsync<T>(method, resource, body, It.IsAny<CancellationToken>())).ReturnsAsync(value);
}
