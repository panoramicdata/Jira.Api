namespace Jira.Api.Test;

public class IndexingServiceTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Fact]
	public async Task GetIndexSummaryAsync_MapsRemoteResponse()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);
		var remote = new RemoteIndexSummary
		{
			issueCountInDatabase = 1000,
			issueCountInIndex = 995,
			currentIndexingStatus = "IDLE",
			lastIssueUpdateTime = new DateTimeOffset(2026, 5, 1, 10, 0, 0, TimeSpan.Zero),
			indexReadable = true,
			indexWriteable = true
		};

		client.Setup(c => c.ExecuteRequestAsync<RemoteIndexSummary>(
				Method.Get,
				"rest/api/2/index/summary",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remote);

		var result = await jira.Indexing.GetIndexSummaryAsync(CancellationToken);

		result.IssueCountInDatabase.Should().Be(1000);
		result.IssueCountInIndex.Should().Be(995);
		result.CurrentIndexingStatus.Should().Be("IDLE");
		result.LastIssueUpdateTime.Should().Be(new DateTimeOffset(2026, 5, 1, 10, 0, 0, TimeSpan.Zero));
		result.IndexReadable.Should().BeTrue();
		result.IndexWriteable.Should().BeTrue();
	}

	[Fact]
	public async Task GetReindexStatusAsync_MapsRemoteResponse()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);
		var remote = new RemoteReindexStatus
		{
			progressPercent = 100,
			currentSubTask = "",
			submittedTime = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
			startTime = new DateTimeOffset(2026, 4, 1, 0, 0, 1, TimeSpan.Zero),
			finishTime = new DateTimeOffset(2026, 4, 1, 0, 5, 0, TimeSpan.Zero),
			success = true,
			failed = false
		};

		client.Setup(c => c.ExecuteRequestAsync<RemoteReindexStatus>(
				Method.Get,
				"rest/api/2/reindex",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remote);

		var result = await jira.Indexing.GetReindexStatusAsync(CancellationToken);

		result.ProgressPercent.Should().Be(100);
		result.CurrentSubTask.Should().BeEmpty();
		result.SubmittedTime.Should().Be(new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero));
		result.StartTime.Should().Be(new DateTimeOffset(2026, 4, 1, 0, 0, 1, TimeSpan.Zero));
		result.FinishTime.Should().Be(new DateTimeOffset(2026, 4, 1, 0, 5, 0, TimeSpan.Zero));
		result.Success.Should().BeTrue();
		result.Failed.Should().BeFalse();
	}

	[Fact]
	public async Task TriggerReindexAsync_PostsToCorrectEndpointAndMapsResponse()
	{
		var client = new Mock<IJiraRestClient>();
		var jira = JiraClient.CreateRestClient(client.Object);
		var remote = new RemoteReindexStatus
		{
			progressPercent = 0,
			currentSubTask = "Flushing index",
			submittedTime = new DateTimeOffset(2026, 5, 4, 12, 0, 0, TimeSpan.Zero),
			startTime = null,
			finishTime = null,
			success = false,
			failed = false
		};

		client.Setup(c => c.ExecuteRequestAsync<RemoteReindexStatus>(
				Method.Post,
				"rest/api/2/reindex",
				null,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(remote);

		var result = await jira.Indexing.TriggerReindexAsync(CancellationToken);

		client.Verify(c => c.ExecuteRequestAsync<RemoteReindexStatus>(
			Method.Post,
			"rest/api/2/reindex",
			null,
			It.IsAny<CancellationToken>()), Times.Once);

		result.ProgressPercent.Should().Be(0);
		result.CurrentSubTask.Should().Be("Flushing index");
		result.SubmittedTime.Should().Be(new DateTimeOffset(2026, 5, 4, 12, 0, 0, TimeSpan.Zero));
		result.StartTime.Should().BeNull();
		result.FinishTime.Should().BeNull();
		result.Success.Should().BeFalse();
		result.Failed.Should().BeFalse();
	}
}
