using AwesomeAssertions;
using RestSharp;

namespace Jira.Api.Test;

public class ScreenServiceTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Fact]
	public async Task GetScreensAsync_ReturnsScreens()
	{
		// Arrange
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var pagedResult = new RemotePagedResult<RemoteScreen>
		{
			StartAt = 0,
			MaxResults = 100,
			Total = 2,
			Values =
			[
				new RemoteScreen { Id = 1, Name = "Default Screen", Description = "The default screen" },
				new RemoteScreen { Id = 2, Name = "Resolve Issue Screen", Description = "Screen for resolving issues" }
			]
		};

		client.Setup(c => c.ExecuteRequestAsync<RemotePagedResult<RemoteScreen>>(
				Method.Get,
				"rest/api/2/screens",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(pagedResult);

		// Act
		var screens = await jira.Screens.GetScreensAsync(CancellationToken);

		// Assert
		screens.Should().HaveCount(2);
		screens.First().Id.Should().Be(1);
		screens.First().Name.Should().Be("Default Screen");
		screens.First().Description.Should().Be("The default screen");
	}

	[Fact]
	public async Task GetScreenAsync_ReturnsScreen()
	{
		// Arrange
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var remoteScreen = new RemoteScreen
		{
			Id = 1,
			Name = "Default Screen",
			Description = "The default screen"
		};

		client.Setup(c => c.ExecuteRequestAsync<RemoteScreen>(
				Method.Get,
				"rest/api/2/screens/1",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remoteScreen);

		// Act
		var screen = await jira.Screens.GetScreenAsync(1, CancellationToken);

		// Assert
		screen.Id.Should().Be(1);
		screen.Name.Should().Be("Default Screen");
		screen.Description.Should().Be("The default screen");
	}

	[Fact]
	public async Task GetScreenTabsAsync_ReturnsTabs()
	{
		// Arrange
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var remoteTabs = new List<RemoteScreenTab>
		{
			new() { id = "1", name = "Field Tab" },
			new() { id = "2", name = "Details Tab" }
		};

		client.Setup(c => c.ExecuteRequestAsync<IEnumerable<RemoteScreenTab>>(
				Method.Get,
				"rest/api/2/screens/1/tabs",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remoteTabs);

		// Act
		var tabs = await jira.Screens.GetScreenTabsAsync("1", cancellationToken: CancellationToken);

		// Assert
		tabs.Should().HaveCount(2);
		tabs.First().Id.Should().Be("1");
		tabs.First().Name.Should().Be("Field Tab");
	}

	[Fact]
	public async Task GetScreenTabsAsync_WithProjectKey_IncludesProjectKeyInUrl()
	{
		// Arrange
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var remoteTabs = new List<RemoteScreenTab>
		{
			new() { id = "1", name = "Field Tab" }
		};

		client.Setup(c => c.ExecuteRequestAsync<IEnumerable<RemoteScreenTab>>(
				Method.Get,
				"rest/api/2/screens/1/tabs?projectKey=PRJ",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remoteTabs);

		// Act
		var tabs = await jira.Screens.GetScreenTabsAsync("1", "PRJ", CancellationToken);

		// Assert
		tabs.Should().HaveCount(1);
	}

	[Fact]
	public async Task GetScreenTabFieldsAsync_ReturnsFields()
	{
		// Arrange
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var remoteFields = new List<RemoteScreenField>
		{
			new() { id = "summary", name = "Summary", type = "string" },
			new() { id = "description", name = "Description", type = "string" },
			new() { id = "customfield_10001", name = "Custom Field", type = "com.atlassian.jira.plugin.system.customfieldtypes:textfield" }
		};

		client.Setup(c => c.ExecuteRequestAsync<IEnumerable<RemoteScreenField>>(
				Method.Get,
				"rest/api/2/screens/1/tabs/10/fields",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remoteFields);

		// Act
		var fields = await jira.Screens.GetScreenTabFieldsAsync("1", "10", cancellationToken: CancellationToken);

		// Assert
		fields.Should().HaveCount(3);
		fields.First().Id.Should().Be("summary");
		fields.First().Name.Should().Be("Summary");
		fields.First().Type.Should().Be("string");
	}

	[Fact]
	public async Task GetScreenAvailableFieldsAsync_ReturnsAvailableFields()
	{
		// Arrange
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var remoteFields = new List<RemoteScreenField>
		{
			new() { id = "priority", name = "Priority", type = "priority" },
			new() { id = "labels", name = "Labels", type = "array" }
		};

		client.Setup(c => c.ExecuteRequestAsync<IEnumerable<RemoteScreenField>>(
				Method.Get,
				"rest/api/2/screens/1/availableFields",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remoteFields);

		// Act
		var fields = await jira.Screens.GetScreenAvailableFieldsAsync("1", CancellationToken);

		// Assert
		fields.Should().HaveCount(2);
		fields.First().Id.Should().Be("priority");
		fields.First().Name.Should().Be("Priority");
	}

	[Fact]
	public async Task Screen_ToString_ReturnsName()
	{
		// Arrange
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var remoteScreen = new RemoteScreen { Id = 1, Name = "Test Screen", Description = "Description" };

		client.Setup(c => c.ExecuteRequestAsync<RemoteScreen>(
				Method.Get,
				"rest/api/2/screens/1",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remoteScreen);

		// Act
		var screen = await jira.Screens.GetScreenAsync(1, CancellationToken);

		// Assert
		screen.ToString().Should().Be("Test Screen");
	}

	[Fact]
	public async Task Screen_ToString_WithNullName_ReturnsIdFallback()
	{
		// Arrange
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		var remoteScreen = new RemoteScreen { Id = 42, Name = null, Description = null };

		client.Setup(c => c.ExecuteRequestAsync<RemoteScreen>(
				Method.Get,
				"rest/api/2/screens/42",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remoteScreen);

		// Act
		var screen = await jira.Screens.GetScreenAsync(42, CancellationToken);

		// Assert
		screen.ToString().Should().Be("Screen 42");
	}
}
