using System;
using System.Runtime.Serialization;

namespace Jira.Api.Remote;

/// <summary>
/// Exception thrown when the server responds with HTTP code 404.
/// </summary>
public class ResourceNotFoundException : InvalidOperationException
{
	/// <summary>
	/// Initializes a new instance of the ResourceNotFoundException class
	/// </summary>
	public ResourceNotFoundException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the ResourceNotFoundException class with a specified error message
	/// </summary>
	public ResourceNotFoundException(string message) : base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the ResourceNotFoundException class with a specified error message and a reference to the inner exception
	/// </summary>
	public ResourceNotFoundException(string message, Exception innerException) : base(message, innerException)
	{
	}

	/// <summary>
	/// Initializes a new instance of the ResourceNotFoundException class with serialized data
	/// </summary>
	protected ResourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}
