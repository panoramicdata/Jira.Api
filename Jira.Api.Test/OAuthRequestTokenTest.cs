namespace Jira.Api.Test;

public class OAuthRequestTokenTest
{
	[Fact]
	public void OAuthRequestToken_CanDeserialize()
	{
		// Arrange
		var requestToken = new OAuthRequestToken(
			"authorize_uri",
			"oauth_token",
			"oauth_token_secret",
			"oauth_callback_confirmation");
		var json = JsonConvert.SerializeObject(requestToken);

		// Act
		var deserializedRequestToken = JsonConvert.DeserializeObject<OAuthRequestToken>(json);
		deserializedRequestToken.Should().NotBeNull();

		// Assert
		deserializedRequestToken.AuthorizeUri.Should().Be(requestToken.AuthorizeUri);
		deserializedRequestToken.OAuthToken.Should().Be(requestToken.OAuthToken);
		deserializedRequestToken.OAuthTokenSecret.Should().Be(requestToken.OAuthTokenSecret);
		deserializedRequestToken.OAuthCallbackConfirmation.Should().Be(requestToken.OAuthCallbackConfirmation);
	}
}

