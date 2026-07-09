namespace Jira.Api.Test.Integration;

[Trait("Category", "ReadOnly")]
public class DashboardTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetDashboardsAsync_ReturnsAtLeastTheSystemDashboard(JiraClient jira)
	{
		var page = await jira.Dashboards.GetDashboardsAsync(0, 50, CancellationToken);

		// Every JIRA Server instance has at least the System Dashboard.
		page.Total.Should().BePositive();
		page.Dashboards.Should().NotBeEmpty();
		page.Dashboards.Should().AllSatisfy(dashboard =>
		{
			dashboard.Id.Should().NotBeNullOrWhiteSpace();
			dashboard.Name.Should().NotBeNullOrWhiteSpace();
		});
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetDashboardAsync_ById_MatchesListEntry(JiraClient jira)
	{
		var page = await jira.Dashboards.GetDashboardsAsync(0, 1, CancellationToken);
		var listEntry = page.Dashboards[0];

		var dashboard = await jira.Dashboards.GetDashboardAsync(listEntry.Id!, CancellationToken);

		dashboard.Id.Should().Be(listEntry.Id);
		dashboard.Name.Should().Be(listEntry.Name);
		dashboard.View.Should().NotBeNullOrWhiteSpace();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetDashboardDetailAsync_ReturnsGadgets(JiraClient jira)
	{
		var page = await jira.Dashboards.GetDashboardsAsync(0, 1, CancellationToken);
		var listEntry = page.Dashboards[0];

		var detail = await jira.Dashboards.GetDashboardDetailAsync(listEntry.Id!, CancellationToken);

		detail.Id.Should().Be(listEntry.Id);
		detail.Title.Should().Be(listEntry.Name);
		detail.Gadgets.Should().NotBeNull();

		OutputHelper.WriteLine($"Dashboard '{detail.Title}' (layout {detail.Layout}) has {detail.Gadgets.Count} gadget(s):");
		foreach (var gadget in detail.Gadgets)
		{
			OutputHelper.WriteLine(
				$"- Column {gadget.Column}: '{gadget.Title}' (IsFilterResults={gadget.IsFilterResults}, FilterId={gadget.FilterId ?? "-"}, Columns=[{string.Join(", ", gadget.ColumnNames)}], NumberToShow={gadget.NumberToShow?.ToString() ?? "-"})");
		}
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetDashboardDetailAsync_FilterResultsGadgets_ResolveToFiltersAndIssues(JiraClient jira)
	{
		// Find a dashboard with at least one Filter Results gadget, then resolve that
		// gadget's saved filter and run its JQL - the full chain the ProMagic dashboard
		// feature relies on (MS-24563).
		var page = await jira.Dashboards.GetDashboardsAsync(0, 50, CancellationToken);

		foreach (var listEntry in page.Dashboards)
		{
			var detail = await jira.Dashboards.GetDashboardDetailAsync(listEntry.Id!, CancellationToken);
			var gadget = detail.Gadgets.FirstOrDefault(gadget => gadget.IsFilterResults && gadget.FilterId is not null);
			if (gadget is null)
			{
				continue;
			}

			var filter = await jira.Filters.GetFilterAsync(gadget.FilterId!, CancellationToken);
			filter.Name.Should().NotBeNullOrWhiteSpace();
			filter.Jql.Should().NotBeNullOrWhiteSpace();

			var issues = await jira.Filters.GetIssuesFromFilterWithFieldsAsync(
				gadget.FilterId!,
				0,
				gadget.NumberToShow ?? 10,
				["summary", "status", "priority", "assignee", "updated"],
				CancellationToken);

			OutputHelper.WriteLine(
				$"Dashboard '{detail.Title}', gadget filter '{filter.Name}' ({filter.Jql}): {issues.TotalItems} issue(s) in total.");
			return;
		}

		Assert.Fail(
			$"No dashboard visible to user '{JiraProvider.USERNAME}' on '{JiraProvider.HOST}' has a Filter Results gadget. "
			+ "Add a Filter Results gadget to a dashboard visible to this user and re-run.");
	}
}
