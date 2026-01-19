using AwesomeAssertions;
using Jira.Api.OAuth;
using Newtonsoft.Json;

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
		deserializedAccessToken.Should().NotBeNull();

		// Assert
		deserializedAccessToken.OAuthToken.Should().Be(accessToken.OAuthToken);
		deserializedAccessToken.OAuthTokenSecret.Should().Be(accessToken.OAuthTokenSecret);
		deserializedAccessToken.OAuthTokenExpiry.Should().Be(accessToken.OAuthTokenExpiry);
	}
}

