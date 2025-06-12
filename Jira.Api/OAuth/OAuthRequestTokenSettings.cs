namespace Jira.Api.OAuth;

/// <summary>
/// Request token settings to help generate the request token.
/// </summary>
/// <remarks>
/// Creates a Request token settings to generate a request token.
/// </remarks>
/// <param name="url">The URL of the Jira instance to request to.</param>
/// <param name="consumerKey">The consumer key provided by the Jira application link.</param>
/// <param name="consumerSecret">The consumer private key in XML format.</param>
/// <param name="callbackUrl">The callback url for the request token.</param>
/// <param name="signatureMethod">The signature method used to sign the request.</param>
/// <param name="requestTokenUrl">The relative URL to request the token.</param>
/// <param name="authorizeUrl">The relative URL to authorize the token.</param>
public class OAuthRequestTokenSettings(
	string url,
	string consumerKey,
	string consumerSecret,
	string callbackUrl = null,
	JiraOAuthSignatureMethod signatureMethod = JiraOAuthSignatureMethod.RsaSha1,
	string requestTokenUrl = OAuthRequestTokenSettings.DefaultRequestTokenUrl,
	string authorizeUrl = OAuthRequestTokenSettings.DefaultAuthorizeUrl)
{
	/// <summary>
	/// The default relative URL to request a token.
	/// </summary>
	public const string DefaultRequestTokenUrl = "plugins/servlet/oauth/request-token";

	/// <summary>
	/// The default relative URL to authorize a token.
	/// </summary>
	public const string DefaultAuthorizeUrl = "plugins/servlet/oauth/authorize";

	/// <summary>
	/// Gets the URL of the Jira instance to request to.
	/// </summary>
	public string Url { get; } = url;

	/// <summary>
	/// Gets the consumer key provided by the Jira application link.
	/// </summary>
	public string ConsumerKey { get; } = consumerKey;

	/// <summary>
	/// Gets the consumer private key in XML format.
	/// </summary>
	public string ConsumerSecret { get; } = consumerSecret;

	/// <summary>
	/// Gets the callback URL for the request token.
	/// </summary>
	public string CallbackUrl { get; } = callbackUrl;

	/// <summary>
	/// Gets the signature method used to sign the request.
	/// </summary>
	public JiraOAuthSignatureMethod SignatureMethod { get; } = signatureMethod;

	/// <summary>
	/// Gets the relative URL to request the token.
	/// </summary>
	public string RequestTokenUrl { get; } = requestTokenUrl;

	/// <summary>
	/// Gets the relative URL to authorize the token.
	/// </summary>
	public string AuthorizeUrl { get; } = authorizeUrl;
}
