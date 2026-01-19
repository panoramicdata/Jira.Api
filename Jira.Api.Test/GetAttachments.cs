using AwesomeAssertions;

namespace Jira.Api.Test;

public partial class IssueTest
{
	public class GetAttachments(ITestOutputHelper outputHelper) : TestBase(outputHelper)
	{
		[Fact]
		public async Task IfIssueNotCreated_ShouldThrowException()
		{
			var issue = CreateIssue();

			var act = () => issue.GetAttachmentsAsync(default);
			await act.Should().ThrowExactlyAsync<InvalidOperationException>();
		}

		[Fact]
		public async Task IfIssueIsCreated_ShouldLoadAttachments()
		{
			//arrange
			var jira = TestableJira.Create();
			var remoteAttachment = new RemoteAttachment() { filename = "attach.txt" };
			jira.IssueService.Setup(j => j.GetAttachmentsAsync("issueKey", It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(Enumerable.Repeat(new Attachment(jira, remoteAttachment), 1)));

			var issue = (new RemoteIssue() { key = "issueKey" }).ToLocal(jira);

			//act
			var attachments = await issue.GetAttachmentsAsync(CancellationToken);

			//assert
			attachments.Should().ContainSingle();
			attachments.First().FileName.Should().Be("attach.txt");
		}
	}
}




