namespace Jira.Api.Test;

public class DashboardServiceTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	private const string DashboardDetailJson = /*lang=json,strict*/ """
		{
			"id": "13521",
			"title": "Ali: Projects",
			"writable": false,
			"layout": "A",
			"gadgets": [
				{
					"id": 17342,
					"title": "Filter Results",
					"column": 0,
					"color": "color1",
					"amdModule": "jira-dashboard-items/filter-results",
					"userPrefs": {
						"action": "https://jira.example.com/rest/dashboards/1.0/13521/gadget/17342/prefs",
						"fields": [
							{ "name": "filterId", "value": "20934", "type": "hidden" },
							{ "name": "isConfigured", "value": "true", "type": "hidden" },
							{ "name": "columnNames", "value": "priority|issuekey|summary|assignee|status|updated", "type": "hidden" },
							{ "name": "num", "value": "10", "type": "hidden" },
							{ "name": "refresh", "value": "15", "type": "hidden" }
						]
					}
				},
				{
					"id": 17343,
					"title": "Assigned to Me",
					"column": 1,
					"amdModule": "jira-dashboard-items/assigned-to-me"
				}
			]
		}
		""";

	[Fact]
	public async Task GetDashboardsAsync_WithTake_AppendsPagingAndMapsResponse()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);
		var page = JsonConvert.DeserializeObject<JiraDashboardPage>(/*lang=json,strict*/ """
			{
				"startAt": 0,
				"maxResults": 50,
				"total": 2,
				"dashboards": [
					{ "id": "10000", "name": "System Dashboard", "self": "https://jira.example.com/rest/api/2/dashboard/10000", "view": "https://jira.example.com/secure/Dashboard.jspa?selectPageId=10000" },
					{ "id": "13521", "name": "Ali: Projects", "self": "https://jira.example.com/rest/api/2/dashboard/13521", "view": "https://jira.example.com/secure/Dashboard.jspa?selectPageId=13521" }
				]
			}
			""")!;

		client.Setup(c => c.ExecuteRequestAsync<JiraDashboardPage>(
				Method.Get,
				"rest/api/2/dashboard?startAt=0&maxResults=50",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(page);

		var result = await jira.Dashboards.GetDashboardsAsync(0, 50, CancellationToken);

		result.Total.Should().Be(2);
		result.Dashboards.Should().HaveCount(2);
		result.Dashboards[0].Id.Should().Be("10000");
		result.Dashboards[1].Name.Should().Be("Ali: Projects");
	}

	[Fact]
	public async Task GetDashboardsAsync_WithoutTake_OmitsMaxResults()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);

		client.Setup(c => c.ExecuteRequestAsync<JiraDashboardPage>(
				Method.Get,
				"rest/api/2/dashboard?startAt=5",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(new JiraDashboardPage());

		var result = await jira.Dashboards.GetDashboardsAsync(5, null, CancellationToken);

		result.Dashboards.Should().BeEmpty();
		client.VerifyAll();
	}

	[Fact]
	public async Task GetDashboardAsync_UsesPublicResource_MapsIdNameView()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);
		var dashboard = JsonConvert.DeserializeObject<JiraDashboard>(/*lang=json,strict*/ """
			{
				"id": "13521",
				"name": "Ali: Projects",
				"self": "https://jira.example.com/rest/api/2/dashboard/13521",
				"view": "https://jira.example.com/secure/Dashboard.jspa?selectPageId=13521"
			}
			""")!;

		client.Setup(c => c.ExecuteRequestAsync<JiraDashboard>(
				Method.Get,
				"rest/api/2/dashboard/13521",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(dashboard);

		var result = await jira.Dashboards.GetDashboardAsync("13521", CancellationToken);

		result.Id.Should().Be("13521");
		result.Name.Should().Be("Ali: Projects");
		result.Self.Should().Be("https://jira.example.com/rest/api/2/dashboard/13521");
		result.View.Should().Be("https://jira.example.com/secure/Dashboard.jspa?selectPageId=13521");
	}

	[Fact]
	public async Task GetDashboardDetailAsync_UsesInternalResource_MapsGadgets()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);
		var detail = JsonConvert.DeserializeObject<JiraDashboardDetail>(DashboardDetailJson)!;

		client.Setup(c => c.ExecuteRequestAsync<JiraDashboardDetail>(
				Method.Get,
				"rest/dashboards/1.0/13521",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(detail);

		var result = await jira.Dashboards.GetDashboardDetailAsync("13521", CancellationToken);

		result.Id.Should().Be("13521");
		result.Title.Should().Be("Ali: Projects");
		result.Writable.Should().BeFalse();
		result.Layout.Should().Be("A");
		result.Gadgets.Should().HaveCount(2);
	}

	[Fact]
	public void JiraDashboardGadget_FilterResultsGadget_ParsesUserPrefs()
	{
		var detail = JsonConvert.DeserializeObject<JiraDashboardDetail>(DashboardDetailJson)!;
		var gadget = detail.Gadgets[0];

		gadget.Id.Should().Be(17342);
		gadget.Title.Should().Be("Filter Results");
		gadget.Column.Should().Be(0);
		gadget.Color.Should().Be("color1");
		gadget.IsFilterResults.Should().BeTrue();
		gadget.FilterId.Should().Be("20934");
		gadget.ColumnNames.Should().Equal("priority", "issuekey", "summary", "assignee", "status", "updated");
		gadget.NumberToShow.Should().Be(10);
		gadget.RefreshMinutes.Should().Be(15);
		gadget.GetUserPrefValue("isConfigured").Should().Be("true");
	}

	[Fact]
	public void JiraDashboardGadget_NonFilterResultsGadget_HasSafeDefaults()
	{
		var detail = JsonConvert.DeserializeObject<JiraDashboardDetail>(DashboardDetailJson)!;
		var gadget = detail.Gadgets[1];

		gadget.IsFilterResults.Should().BeFalse();
		gadget.UserPrefs.Should().BeNull();
		gadget.FilterId.Should().BeNull();
		gadget.ColumnNames.Should().BeEmpty();
		gadget.NumberToShow.Should().BeNull();
		gadget.RefreshMinutes.Should().BeNull();
		gadget.GetUserPrefValue("anything").Should().BeNull();
	}

	[Fact]
	public void JiraDashboardGadget_LegacyGadgetUrlSpec_IsRecognisedAsFilterResults()
	{
		var gadget = JsonConvert.DeserializeObject<JiraDashboardGadget>(/*lang=json,strict*/ """
			{
				"id": 1,
				"title": "Filter Results",
				"column": 0,
				"gadgetUrl": "https://jira.example.com/rest/gadgets/1.0/g/com.atlassian.jira.gadgets:filter-results-gadget/gadgets/filter-results-gadget.xml"
			}
			""")!;

		gadget.IsFilterResults.Should().BeTrue();
	}

	[Fact]
	public void JiraDashboardGadget_UnparseablePrefValues_ReturnNull()
	{
		var gadget = JsonConvert.DeserializeObject<JiraDashboardGadget>(/*lang=json,strict*/ """
			{
				"id": 1,
				"title": "Filter Results",
				"column": 0,
				"amdModule": "jira-dashboard-items/filter-results",
				"userPrefs": {
					"fields": [
						{ "name": "num", "value": "not-a-number" },
						{ "name": "columnNames", "value": "" }
					]
				}
			}
			""")!;

		gadget.NumberToShow.Should().BeNull();
		gadget.RefreshMinutes.Should().BeNull();
		gadget.ColumnNames.Should().BeEmpty();
		gadget.FilterId.Should().BeNull();
	}
}
