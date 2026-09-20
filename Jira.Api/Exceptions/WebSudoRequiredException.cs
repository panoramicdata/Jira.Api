namespace Jira.Api.Exceptions;

/// <summary>
/// The requested Jira administration operation requires an active WebSudo session.
/// </summary>
public class WebSudoRequiredException : System.Security.Authentication.AuthenticationException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WebSudoRequiredException"/> class.
	/// </summary>
	/// <param name="message">The error message.</param>
	public WebSudoRequiredException(string message) : base(message)
	{
	}
}
