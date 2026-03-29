using LTAF;

namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class OAuthTokenHelperTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Fact]
	public async Task CanGenerateRequestAndAccessTokens()
	{
		// Arrange
		var oAuthTokenSettings = new OAuthRequestTokenSettings(
			JiraProvider.HOST,
			JiraProvider.OAUTHCONSUMERKEY,
			JiraProvider.OAUTHCONSUMERSECRET);

		// Generate request token
		var oAuthRequestToken = await OAuthTokenHelper.GenerateRequestTokenAsync(oAuthTokenSettings, CancellationToken);

		// Verify request token exists
		oAuthRequestToken.Should().NotBeNull();

		// Attempt to get an access token before it has been authorized.
		var oAuthAccessTokenSettings = new OAuthAccessTokenSettings(oAuthTokenSettings, oAuthRequestToken);
		var accessTokenResult = await OAuthTokenHelper.ObtainOAuthAccessTokenAsync(oAuthAccessTokenSettings, CancellationToken.None);

		// Verify no access token is granted.
		accessTokenResult.Should().BeNull();

		// Login to Jira
		var page = new HtmlPage(new Uri(JiraProvider.HOST));
		page.Navigate("/login.jsp");
		var elements = page.RootElement.ChildElements;
		elements.Find("username").SetText(JiraProvider.USERNAME);
		elements.Find("password").SetText(JiraProvider.PASSWORD);
		elements.Find("submit").Click();

		// Authorize token
		page.Navigate(oAuthRequestToken.AuthorizeUri);
		page.RootElement.ChildElements.Find("approve").Click();

		// Re-Attempt to get an access token
		accessTokenResult = await OAuthTokenHelper.ObtainOAuthAccessTokenAsync(oAuthAccessTokenSettings, CancellationToken.None);

		// Verify access token exists.
		accessTokenResult.Should().NotBeNull();
	}
}



