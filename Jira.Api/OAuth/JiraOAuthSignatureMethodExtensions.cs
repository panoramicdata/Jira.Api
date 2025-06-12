using RestSharp.Authenticators.OAuth;

namespace Jira.Api.OAuth;

/// <summary>
/// Provides extension methods for <see cref="JiraOAuthSignatureMethod"/>.
/// </summary>
public static class JiraOAuthSignatureMethodExtensions
{
	/// <summary>
	/// Converts to <see cref="OAuthSignatureMethod"/>.
	/// </summary>
	/// <param name="signatureMethod">The signature method.</param>
	/// <returns>The RestSharp signature method.</returns>
	public static OAuthSignatureMethod ToOAuthSignatureMethod(this JiraOAuthSignatureMethod signatureMethod) => signatureMethod switch
	{
		JiraOAuthSignatureMethod.HmacSha1 => OAuthSignatureMethod.HmacSha1,
		JiraOAuthSignatureMethod.HmacSha256 => OAuthSignatureMethod.HmacSha256,
		JiraOAuthSignatureMethod.PlainText => OAuthSignatureMethod.PlainText,
		JiraOAuthSignatureMethod.RsaSha1 => OAuthSignatureMethod.RsaSha1,
		_ => OAuthSignatureMethod.RsaSha1,
	};
}
