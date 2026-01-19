namespace Jira.Api.Test;

public abstract class TestBase(ITestOutputHelper outputHelper)
{
	protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	protected ITestOutputHelper OutputHelper { get; } = outputHelper;
}
