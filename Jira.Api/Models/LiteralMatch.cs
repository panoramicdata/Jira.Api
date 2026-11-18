namespace Jira.Api.Models;

/// <summary>
/// Force a CustomField comparison to use the exact match JQL operator.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3875:operator== should not be overloaded on reference types", Justification = "Published public API: these operators are how consumers write LINQ to JQL comparisons, so removing them is a breaking change.")]
public class LiteralMatch(string value)
{
	private readonly string _value = value;

	/// <summary>
	/// Returns the string representation
	/// </summary>
	public override string ToString()
	{
		return _value;
	}

	/// <summary>
	/// Equality operator for exact match comparison
	/// </summary>
	public static bool operator ==(ComparableString comparable, LiteralMatch literal)
	{
		if (comparable is null)
		{
			return literal == null;
		}
		else
		{
			return comparable.Value == literal._value;
		}
	}

	/// <summary>
	/// Inequality operator for exact match comparison
	/// </summary>
	public static bool operator !=(ComparableString comparable, LiteralMatch literal)
	{
		if (comparable is null)
		{
			return literal != null;
		}
		else
		{
			return comparable.Value != literal._value;
		}
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is LiteralMatch other && _value == other._value;

	/// <inheritdoc/>
	public override int GetHashCode() => _value?.GetHashCode() ?? 0;
}
