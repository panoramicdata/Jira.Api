namespace Jira.Api.OAuth;

/// <summary>
/// Possible values for OAuth signature method.
/// </summary>
public enum JiraOAuthSignatureMethod
{
	/// <summary>
	/// Represents an implementation of the HMAC-SHA1 cryptographic algorithm.
	/// </summary>
	/// <remarks>HMAC-SHA1 is a keyed-hash message authentication code (HMAC) that uses the SHA-1 hash function. It
	/// is commonly used to verify the integrity and authenticity of a message.</remarks>
	HmacSha1,

	/// <summary>
	/// Represents an implementation of the HMAC-SHA256 cryptographic algorithm.
	/// </summary>
	/// <remarks>
	/// HMAC-SHA256 is a keyed-hash message authentication code (HMAC) that uses the SHA-256 hash function.
	/// It provides stronger security than HMAC-SHA1 and is supported by some Jira instances for OAuth 1.0a authentication.
	/// </remarks>
	HmacSha256,

	/// <summary>
	/// Represents the use of plain text as the OAuth signature method.
	/// </summary>
	/// <remarks>
	/// This method does not apply any cryptographic signing and is generally not recommended for production use.
	/// It may be used in testing or in environments where security is not a concern.
	/// </remarks>
	PlainText,

	/// <summary>
	/// Represents an implementation of the RSA-SHA1 cryptographic algorithm.
	/// </summary>
	/// <remarks>RSA-SHA1 is a combination of the RSA public-key cryptographic system and the SHA-1 hashing algorithm. It is
	/// commonly used for digital signatures and secure data verification.</remarks>
	RsaSha1
}
