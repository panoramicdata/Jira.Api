using Jira.Api.Remote;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test.Integration;

public class JiraTypesTest
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetFilters(JiraClient jira)
	{
		var filters = await jira.Filters.GetFavouritesAsync(default);

		Assert.True(filters.Any());
		Assert.Contains(filters, f => f.Name == "One Issue Filter");

		var filter = await jira.Filters.GetFilterAsync(filters.First().Id, default);
		Assert.NotNull(filter);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task RetrieveNamedEntities(JiraClient jira)
	{
		var issue = await jira.Issues.GetIssueAsync("TST-1", default);

		Assert.Equal("Bug", issue.Type.Name);
		Assert.Equal("Major", issue.Priority.Name);
		Assert.Equal("Open", issue.Status.Name);
		Assert.Null(issue.Resolution);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueTypes(JiraClient jira)
	{
		var issueTypes = await jira.IssueTypes.GetIssueTypesAsync(default);

		// In addition, rest API contains "Sub-Task" as an issue type.
		Assert.True(issueTypes.Count() >= 5);
		Assert.Contains(issueTypes, i => i.Name == "Bug");
		Assert.NotNull(issueTypes.First().IconUrl);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuePriorities(JiraClient jira)
	{
		var priorities = await jira.Priorities.GetPrioritiesAsync(default);

		Assert.Contains(priorities, i => i.Name == "Blocker");
		Assert.NotNull(priorities.First().IconUrl);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueResolutions(JiraClient jira)
	{
		var resolutions = await jira.Resolutions.GetResolutionsAsync(default);

		Assert.Contains(resolutions, i => i.Name == "Fixed");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueStatuses(JiraClient jira)
	{
		var statuses = await jira.Statuses.GetStatusesAsync(default);

		var status = statuses.FirstOrDefault(i => i.Name == "Open");
		Assert.NotNull(status);
		Assert.NotNull(status.IconUrl);
		Assert.NotNull(status.StatusCategory);
		Assert.Equal("2", status.StatusCategory.Id);
		Assert.Equal("new", status.StatusCategory.Key);
		Assert.Equal("To Do", status.StatusCategory.Name);
		Assert.Equal("blue-gray", status.StatusCategory.ColorName);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueStatusById(JiraClient jira)
	{
		var status = await jira.Statuses.GetStatusAsync("1", default);

		Assert.NotNull(status);
		Assert.Equal("1", status.Id);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueStatusByName(JiraClient jira)
	{
		var status = await jira.Statuses.GetStatusAsync("Open", default);

		Assert.NotNull(status);
		Assert.Equal("Open", status.Name);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueStatusByInvalidNameShouldThrowException(JiraClient jira)
	{
		await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await jira.Statuses.GetStatusAsync("InvalidName", default));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetCustomFields(JiraClient jira)
	{
		var fields = await jira.Fields.GetCustomFieldsAsync(default);
		Assert.True(fields.Count() >= 19);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetProjects(JiraClient jira)
	{
		var projects = await jira.Projects.GetProjectsAsync(default);
		Assert.True(projects.Any());

		var project = projects.First();
		Assert.Equal("admin", project.Lead);
		Assert.Equal("admin", project.LeadUser.DisplayName);
		Assert.NotNull(project.AvatarUrls);
		Assert.NotNull(project.AvatarUrls.XSmall);
		Assert.NotNull(project.AvatarUrls.Small);
		Assert.NotNull(project.AvatarUrls.Medium);
		Assert.NotNull(project.AvatarUrls.Large);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetProject(JiraClient jira)
	{
		var project = await jira.Projects.GetProjectAsync("TST", default);
		Assert.Equal("admin", project.Lead);
		Assert.Equal("admin", project.LeadUser.DisplayName);
		Assert.Equal("Test Project", project.Name);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetProjectStatusesAsync(JiraClient jira)
	{
		Predicate<IssueType> filter = x => x.Name == "Improvement" && x.Statuses.Any(s => s.Name == "Resolved");

		// Validate that issue types are returned with the valid statuses
		var issueTypes = await jira.IssueTypes.GetIssueTypesForProjectAsync("TST", default);
		Assert.Contains(issueTypes, filter);

		// Validate that different projects return different info
		issueTypes = await jira.IssueTypes.GetIssueTypesForProjectAsync("SCRUM", default);
		Assert.DoesNotContain(issueTypes, filter);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueLinkTypes(JiraClient jira)
	{
		var linkTypes = await jira.Links.GetLinkTypesAsync(default);
		Assert.Contains(linkTypes, l => l.Name.Equals("Duplicate"));
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueStatusesAsync(JiraClient jira)
	{
		// First request.
		var result1 = await jira.Statuses.GetStatusesAsync(default);
		Assert.NotEmpty(result1);

		// Cached
		var result2 = await jira.Statuses.GetStatusesAsync(default);
		Assert.Equal(result1.Count(), result2.Count());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueTypesAsync(JiraClient jira)
	{
		// First request.
		var result1 = await jira.IssueTypes.GetIssueTypesAsync(CancellationToken.None);
		Assert.NotEmpty(result1);

		// Cached
		var result2 = await jira.IssueTypes.GetIssueTypesAsync(CancellationToken.None);
		Assert.Equal(result1.Count(), result2.Count());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssuePrioritiesAsync(JiraClient jira)
	{
		// First request.
		var result1 = await jira.Priorities.GetPrioritiesAsync(default);
		Assert.NotEmpty(result1);

		// Cached
		var result2 = await jira.Priorities.GetPrioritiesAsync(default);
		Assert.Equal(result1.Count(), result2.Count());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIssueResolutionsAsync(JiraClient jira)
	{
		// First request.
		var result1 = await jira.Resolutions.GetResolutionsAsync(default);
		Assert.NotEmpty(result1);

		// Cached
		var result2 = await jira.Resolutions.GetResolutionsAsync(default);
		Assert.Equal(result1.Count(), result2.Count());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetFavouriteFiltersAsync(JiraClient jira)
	{
		var result1 = await jira.Filters.GetFavouritesAsync(default);
		Assert.NotEmpty(result1);
	}
}
