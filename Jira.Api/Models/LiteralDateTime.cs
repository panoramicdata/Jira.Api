namespace Jira.Api.Models;

/// <summary>
/// Force a DateTime field to use a string provided as the JQL query value.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3875:operator== should not be overloaded on reference types", Justification = "Published public API: these operators are how consumers write LINQ to JQL comparisons, so removing them is a breaking change.")]
public class LiteralDateTime(string dateTimeString)
{
	private readonly string _dateTimeString = dateTimeString;

	/// <summary>
	/// Returns the string representation
	/// </summary>
	public override string ToString()
	{
		return _dateTimeString;
	}

	/// <summary>
	/// Equality operator for DateTime comparison
	/// </summary>
	public static bool operator ==(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Inequality operator for DateTime comparison
	/// </summary>
	public static bool operator !=(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Greater than operator for DateTime comparison
	/// </summary>
	public static bool operator >(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Less than operator for DateTime comparison
	/// </summary>
	public static bool operator <(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Greater than or equal operator for DateTime comparison
	/// </summary>
	public static bool operator >=(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Less than or equal operator for DateTime comparison
	/// </summary>
	public static bool operator <=(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Equality operator for nullable DateTime comparison
	/// </summary>
	public static bool operator ==(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Inequality operator for nullable DateTime comparison
	/// </summary>
	public static bool operator !=(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Greater than operator for nullable DateTime comparison
	/// </summary>
	public static bool operator >(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Less than operator for nullable DateTime comparison
	/// </summary>
	public static bool operator <(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Greater than or equal operator for nullable DateTime comparison
	/// </summary>
	public static bool operator >=(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Less than or equal operator for nullable DateTime comparison
	/// </summary>
	public static bool operator <=(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is LiteralDateTime other && _dateTimeString == other._dateTimeString;

	/// <inheritdoc/>
	public override int GetHashCode() => _dateTimeString?.GetHashCode() ?? 0;
}
