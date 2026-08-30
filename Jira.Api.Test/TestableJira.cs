using System.Globalization;
using System.Linq.Expressions;
using Jira.Api.Linq;

namespace Jira.Api.Test;

public class TestableJira : JiraClient
{
	public Mock<IJqlExpressionVisitor> Translator { get; private set; }
	public Mock<IJiraRestClient> RestService { get; private set; }
	public Mock<IFileSystem> FileSystem { get; private set; }
	public Mock<IIssueTypeService> IssueTypeService { get; private set; }
	public Mock<IIssueFieldService> IssueFieldService { get; private set; }
	public Mock<IIssueFilterService> IssueFilterService { get; private set; }
	public Mock<IIssueService> IssueService { get; private set; }
	public Mock<IIssuePriorityService> IssuePriorityService { get; private set; }
	public Mock<IIssueResolutionService> IssueResolutionService { get; private set; }

	internal static readonly CultureInfo TestCulture = CultureInfo.InvariantCulture;

	private TestableJira()
		: base(new ServiceLocator())
	{
		RestService = new Mock<IJiraRestClient>();
		FileSystem = new Mock<IFileSystem>();
		Translator = new Mock<IJqlExpressionVisitor>();
		IssueTypeService = new Mock<IIssueTypeService>();
		IssueFieldService = new Mock<IIssueFieldService>();
		IssueFilterService = new Mock<IIssueFilterService>();
		IssueService = new Mock<IIssueService>();
		IssuePriorityService = new Mock<IIssuePriorityService>();
		IssueResolutionService = new Mock<IIssueResolutionService>();

		Services.Register(() => IssueTypeService.Object);
		Services.Register(() => IssueFieldService.Object);
		Services.Register(() => IssueFilterService.Object);
		Services.Register(() => IssueService.Object);
		Services.Register(() => Translator.Object);
		Services.Register(() => FileSystem.Object);
		Services.Register(() => RestService.Object);
		Services.Register(() => IssuePriorityService.Object);
		Services.Register(() => IssueResolutionService.Object);

		Translator.Setup(t => t.Process(It.IsAny<Expression>())).Returns(new JqlData { Expression = "dummy expression" });
	}

	public static TestableJira Create()
	{
		// switch thread locale to avoid "2016/01/01 does not equal 2016.01.01" errors
		Thread.CurrentThread.CurrentCulture = TestCulture;
		Thread.CurrentThread.CurrentUICulture = TestCulture;

		return new TestableJira();
	}

	public void SetupIssues(params RemoteIssue[] remoteIssues)
	{
		IssueService.SetupIssues(this, remoteIssues);
	}
}

