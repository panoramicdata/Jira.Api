﻿using Jira.Api.OAuth;
using LTAF;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UniTestAssert = Xunit.Assert;

namespace Jira.Api.Test.Integration;

public class OAuthTokenHelperTest
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
		var oAuthRequestToken = await OAuthTokenHelper.GenerateRequestTokenAsync(oAuthTokenSettings, default);

		// Verify request token exists
		UniTestAssert.NotNull(oAuthRequestToken);

		// Attempt to get an access token before it has been authorized.
		var oAuthAccessTokenSettings = new OAuthAccessTokenSettings(oAuthTokenSettings, oAuthRequestToken);
		var accessToken = await OAuthTokenHelper.ObtainAccessTokenAsync(oAuthAccessTokenSettings, CancellationToken.None);

		// Verify no access token is granted.
		UniTestAssert.Null(accessToken);

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
		accessToken = await OAuthTokenHelper.ObtainAccessTokenAsync(oAuthAccessTokenSettings, CancellationToken.None);

		// Verify access token exists.
		UniTestAssert.NotNull(accessToken);
	}
}
