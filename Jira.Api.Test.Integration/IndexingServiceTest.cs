namespace Jira.Api.Test.Integration;

public class IndexingServiceTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetIndexSummaryAsync_ReturnsIndexSummary(JiraClient jira)
	{
		var result = await jira.Indexing.GetIndexSummaryAsync(CancellationToken);

		result.Should().NotBeNull();
		result.IssueCountInDatabase.Should().BeGreaterThanOrEqualTo(0);
		result.IssueCountInIndex.Should().BeGreaterThanOrEqualTo(0);
		result.CurrentIndexingStatus.Should().NotBeNullOrEmpty();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task GetReindexStatusAsync_ReturnsReindexStatus(JiraClient jira)
	{
		var result = await jira.Indexing.GetReindexStatusAsync(CancellationToken);

		result.Should().NotBeNull();
		result.ProgressPercent.Should().BeInRange(0, 100);
	}
}
