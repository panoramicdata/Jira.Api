using AwesomeAssertions;

namespace Jira.Api.Test.Integration;

/// <remarks>
/// Screen URL used in this test: http://localhost:8080/plugins/servlet/project-config/TST/screens/1
/// </remarks>
public class ScreenTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	private const string _sscreenId = "1";
	private const string _screenTabId = "10110";

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetScreenAvailableFields(JiraClient jira)
	{
		var screenAvailableFields = await jira.Screens.GetScreenAvailableFieldsAsync(_sscreenId, CancellationToken);

		// Verify there's at least one available field.
		screenAvailableFields.Should().Contain(x => x.Name.Equals("Development", StringComparison.InvariantCultureIgnoreCase));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetScreenTabs(JiraClient jira)
	{
		var screenTabs = await jira.Screens.GetScreenTabsAsync(_sscreenId, null, CancellationToken);

		// Verify there's two tabs.
		screenTabs.Should().HaveCount(2);

		// Verify there's the "Extra Tab" tab.
		var screenTab = screenTabs.FirstOrDefault(x => x.Name.Equals("Extra Tab", StringComparison.InvariantCultureIgnoreCase));
		screenTab.Should().NotBeNull();
		screenTab.Id.Should().NotBeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetScreenTabFields(JiraClient jira)
	{
		var screenTabFields = await jira.Screens.GetScreenTabFieldsAsync(_sscreenId, _screenTabId, null, CancellationToken);

		// Verify there's two fields in the "Extra Tab" tab.
		screenTabFields.Should().HaveCount(2);

		// Verify the fields have a name and a type.
		var field = screenTabFields.FirstOrDefault(x => x.Name.Equals("Epic Name", StringComparison.InvariantCultureIgnoreCase));
		field.Should().NotBeNull();
		field.Id.Should().NotBeNull();
		field.Type.Should().Be("Name of Epic");
	}
}



