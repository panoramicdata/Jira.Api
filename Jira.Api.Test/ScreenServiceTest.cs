namespace Jira.Api.Test;

public class ScreenServiceTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Fact]
	public async Task GetScreensAsync_ReturnsScreens()
	{
		var (client, jira) = MockRest.Create();
		client.Returns(Method.Get, "rest/api/2/screens", new RemotePagedResult<RemoteScreen>
		{
			StartAt = 0,
			MaxResults = 100,
			Total = 2,
			Values =
			[
				new RemoteScreen { Id = 1, Name = "Default Screen", Description = "The default screen" },
				new RemoteScreen { Id = 2, Name = "Resolve Issue Screen", Description = "Screen for resolving issues" }
			]
		});

		var screens = (await jira.Screens.GetScreensAsync(CancellationToken)).ToArray();

		screens.Should().HaveCount(2);
		screens[0].Id.Should().Be(1);
		screens[0].Name.Should().Be("Default Screen");
		screens[0].Description.Should().Be("The default screen");
	}

	[Fact]
	public async Task GetScreenAsync_ReturnsScreen()
	{
		var (client, jira) = MockRest.Create();
		client.Returns(Method.Get, "rest/api/2/screens/1", new RemoteScreen { Id = 1, Name = "Default Screen", Description = "The default screen" });

		var screen = await jira.Screens.GetScreenAsync(1, CancellationToken);

		screen.Id.Should().Be(1);
		screen.Name.Should().Be("Default Screen");
		screen.Description.Should().Be("The default screen");
	}

	[Theory]
	[InlineData(null, "rest/api/2/screens/1/tabs")]
	[InlineData("PRJ", "rest/api/2/screens/1/tabs?projectKey=PRJ")]
	public async Task GetScreenTabsAsync_BuildsResource(string? projectKey, string resource)
	{
		var (client, jira) = MockRest.Create();
		client.Returns<IEnumerable<RemoteScreenTab>>(Method.Get, resource, [new() { id = "1", name = "Field Tab" }, new() { id = "2", name = "Details Tab" }]);

		var tabs = (await jira.Screens.GetScreenTabsAsync("1", projectKey, CancellationToken)).ToArray();

		tabs.Should().HaveCount(2);
		tabs[0].Id.Should().Be("1");
		tabs[0].Name.Should().Be("Field Tab");
	}

	[Theory]
	[InlineData(null, "rest/api/2/screens/1/tabs/10/fields")]
	[InlineData("PRJ", "rest/api/2/screens/1/tabs/10/fields?projectKey=PRJ")]
	public async Task GetScreenTabFieldsAsync_ReturnsFields(string? projectKey, string resource)
	{
		var (client, jira) = MockRest.Create();
		client.Returns<IEnumerable<RemoteScreenField>>(Method.Get, resource,
		[
			new() { id = "summary", name = "Summary", type = "string" },
			new() { id = "customfield_10001", name = "Custom Field", type = "textfield" }
		]);

		var fields = (await jira.Screens.GetScreenTabFieldsAsync("1", "10", projectKey, CancellationToken)).ToArray();

		fields.Should().HaveCount(2);
		fields[0].Id.Should().Be("summary");
		fields[0].Name.Should().Be("Summary");
		fields[0].Type.Should().Be("string");
	}

	[Fact]
	public async Task GetScreenAvailableFieldsAsync_ReturnsAvailableFields()
	{
		var (client, jira) = MockRest.Create();
		client.Returns<IEnumerable<RemoteScreenField>>(Method.Get, "rest/api/2/screens/1/availableFields",
			[new() { id = "priority", name = "Priority", type = "priority" }, new() { id = "labels", name = "Labels", type = "array" }]);

		var fields = (await jira.Screens.GetScreenAvailableFieldsAsync("1", CancellationToken)).ToArray();

		fields.Should().HaveCount(2);
		fields[0].Id.Should().Be("priority");
		fields[0].Name.Should().Be("Priority");
	}

	[Theory]
	[InlineData(1L, "Test Screen", "Test Screen")]
	[InlineData(42L, null, "Screen 42")]
	public async Task Screen_ToString_FallsBackToId(long id, string? name, string expected)
	{
		var (client, jira) = MockRest.Create();
		client.Returns(Method.Get, $"rest/api/2/screens/{id}", new RemoteScreen { Id = id, Name = name, Description = null });

		var screen = await jira.Screens.GetScreenAsync(id, CancellationToken);

		screen.ToString().Should().Be(expected);
	}
}
