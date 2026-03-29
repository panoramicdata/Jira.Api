namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class JiraTypesTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetFilters(JiraClient jira)
	{
		var filters = await jira.Filters.GetFavouritesAsync(CancellationToken);

		filters.Should().NotBeEmpty();
		filters.Should().Contain(f => f.Name == "One Issue Filter");

		var filter = await jira.Filters.GetFilterAsync(filters.First().Id, CancellationToken);
		filter.Should().NotBeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task RetrieveNamedEntities(JiraClient jira)
	{
		var issue = await jira.Issues.GetIssueAsync("TST-1", CancellationToken);

		issue.Type.Name.Should().Be("Bug");
		issue.Priority.Name.Should().Be("Major");
		issue.Status.Name.Should().Be("Open");
		issue.Resolution.Should().BeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueTypes(JiraClient jira)
	{
		var issueTypes = await jira.IssueTypes.GetIssueTypesAsync(CancellationToken);

		// In addition, rest API contains "Sub-Task" as an issue type.
		(issueTypes.Count() >= 5).Should().BeTrue();
		issueTypes.Should().Contain(i => i.Name == "Bug");
		issueTypes.First().IconUrl.Should().NotBeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuePriorities(JiraClient jira)
	{
		var priorities = await jira.Priorities.GetPrioritiesAsync(CancellationToken);

		priorities.Should().Contain(i => i.Name == "Blocker");
		priorities.First().IconUrl.Should().NotBeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueResolutions(JiraClient jira)
	{
		var resolutions = await jira.Resolutions.GetResolutionsAsync(CancellationToken);

		resolutions.Should().Contain(i => i.Name == "Fixed");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueStatuses(JiraClient jira)
	{
		var statuses = await jira.Statuses.GetStatusesAsync(CancellationToken);

		var status = statuses.FirstOrDefault(i => i.Name == "Open");
		status.Should().NotBeNull();
		status.IconUrl.Should().NotBeNull();
		status.StatusCategory.Should().NotBeNull();
		status.StatusCategory.Id.Should().Be("2");
		status.StatusCategory.Key.Should().Be("new");
		status.StatusCategory.Name.Should().Be("To Do");
		status.StatusCategory.ColorName.Should().Be("blue-gray");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueStatusById(JiraClient jira)
	{
		var status = await jira.Statuses.GetStatusAsync("1", CancellationToken);

		status.Should().NotBeNull();
		status.Id.Should().Be("1");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueStatusByName(JiraClient jira)
	{
		var status = await jira.Statuses.GetStatusAsync("Open", CancellationToken);

		status.Should().NotBeNull();
		status.Name.Should().Be("Open");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueStatusByInvalidNameShouldThrowException(JiraClient jira)
	{
		var act = async () => await jira.Statuses.GetStatusAsync("InvalidName", CancellationToken);
		await act.Should().ThrowExactlyAsync<ResourceNotFoundException>();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetCustomFields(JiraClient jira)
	{
		var fields = await jira.Fields.GetCustomFieldsAsync(CancellationToken);
		(fields.Count() >= 19).Should().BeTrue();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetProjects(JiraClient jira)
	{
		var projects = await jira.Projects.GetProjectsAsync(CancellationToken);
		projects.Should().NotBeEmpty();

		var project = projects.First();
		project.Lead.Should().Be("admin");
		project.LeadUser.DisplayName.Should().Be("admin");
		project.AvatarUrls.Should().NotBeNull();
		project.AvatarUrls.XSmall.Should().NotBeNull();
		project.AvatarUrls.Small.Should().NotBeNull();
		project.AvatarUrls.Medium.Should().NotBeNull();
		project.AvatarUrls.Large.Should().NotBeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetProject(JiraClient jira)
	{
		var project = await jira.Projects.GetProjectAsync("TST", CancellationToken);
		project.Lead.Should().Be("admin");
		project.LeadUser.DisplayName.Should().Be("admin");
		project.Name.Should().Be("Test Project");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetProjectStatusesAsync(JiraClient jira)
	{
		Predicate<IssueType> filter = x => x.Name == "Improvement" && x.Statuses.Any(s => s.Name == "Resolved");

		// Validate that issue types are returned with the valid statuses
		var issueTypes = await jira.IssueTypes.GetIssueTypesForProjectAsync("TST", CancellationToken);
		issueTypes.Should().Contain(it => filter(it));

		// Validate that different projects return different info
		issueTypes = await jira.IssueTypes.GetIssueTypesForProjectAsync("SCRUM", CancellationToken);
		issueTypes.Should().NotContain(it => filter(it));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueLinkTypes(JiraClient jira)
	{
		var linkTypes = await jira.Links.GetLinkTypesAsync(CancellationToken);
		linkTypes.Should().Contain(l => l.Name.Equals("Duplicate"));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueStatusesAsync(JiraClient jira)
	{
		// First request.
		var result1 = await jira.Statuses.GetStatusesAsync(CancellationToken);
       result1.Should().NotBeEmpty();

		// Cached
		var result2 = await jira.Statuses.GetStatusesAsync(CancellationToken);
     result2.Should().HaveCount(result1.Count());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueTypesAsync(JiraClient jira)
	{
		// First request.
		var result1 = await jira.IssueTypes.GetIssueTypesAsync(CancellationToken.None);
       result1.Should().NotBeEmpty();

		// Cached
		var result2 = await jira.IssueTypes.GetIssueTypesAsync(CancellationToken.None);
     result2.Should().HaveCount(result1.Count());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuePrioritiesAsync(JiraClient jira)
	{
		// First request.
		var result1 = await jira.Priorities.GetPrioritiesAsync(CancellationToken);
       result1.Should().NotBeEmpty();

		// Cached
		var result2 = await jira.Priorities.GetPrioritiesAsync(CancellationToken);
     result2.Should().HaveCount(result1.Count());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueResolutionsAsync(JiraClient jira)
	{
		// First request.
		var result1 = await jira.Resolutions.GetResolutionsAsync(CancellationToken);
       result1.Should().NotBeEmpty();

		// Cached
		var result2 = await jira.Resolutions.GetResolutionsAsync(CancellationToken);
     result2.Should().HaveCount(result1.Count());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetFavouriteFiltersAsync(JiraClient jira)
	{
		var result1 = await jira.Filters.GetFavouritesAsync(CancellationToken);
       result1.Should().NotBeEmpty();
	}
}



