using Jira.Api.OAuth;
using Newtonsoft.Json;
using System;
using Xunit;

namespace Jira.Api.Test;

public class OAuthAccessTokenTest
{
	[Fact]
	public void OAuthAccessToken_CanDeserialize()
	{
		// Arrange
		var accessToken = new OAuthAccessToken(
			"oauth_token",
			"oauth_token_secret",
			DateTimeOffset.Now);
		var json = JsonConvert.SerializeObject(accessToken);

		// Act
		var deserializedAccessToken = JsonConvert.DeserializeObject<OAuthAccessToken>(json);

		// Assert
		Assert.Equal(accessToken.OAuthToken, deserializedAccessToken.OAuthToken);
		Assert.Equal(accessToken.OAuthTokenSecret, deserializedAccessToken.OAuthTokenSecret);
		Assert.Equal(accessToken.OAuthTokenExpiry, deserializedAccessToken.OAuthTokenExpiry);
	}
}
