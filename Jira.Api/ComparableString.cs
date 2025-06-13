using System;

namespace Jira.Api;

/// <summary>
/// Type that wraps a string and exposes operator overloads for
/// easier LINQ queries
/// </summary>
/// <remarks>
/// Allows comparisons in the form of issue.Key > "TST-1"
/// </remarks>
public class ComparableString(string value)
{
	public string Value { get; set; } = value;

	public static implicit operator ComparableString(string value)
	{
		if (value != null)
		{
			return new ComparableString(value);
		}
		else
		{
			return null;
		}
	}

	public static bool operator ==(ComparableString field, string value)
	{
		if (field is null)
		{
			return value == null;
		}
		else
		{
			return field.Value == value;
		}
	}

	public static bool operator !=(ComparableString field, string value)
	{
		if (field is null)
		{
			return value != null;
		}
		else
		{
			return field.Value != value;
		}
	}

	public static bool operator >(ComparableString field, string value)
	{
		return field.Value.CompareTo(value) > 0;
	}

	public static bool operator <(ComparableString field, string value)
	{
		return field.Value.CompareTo(value) < 0;
	}

	public static bool operator <=(ComparableString field, string value)
	{
		return field.Value.CompareTo(value) <= 0;
	}

	public static bool operator >=(ComparableString field, string value)
	{
		return field.Value.CompareTo(value) >= 0;
	}

	public static bool operator ==(ComparableString field, DateTime value)
	{
		if (field is null)
		{
			return value == null;
		}
		else
		{
			return field.Value == JiraClient.FormatDateTimeString(value);
		}
	}

	public static bool operator !=(ComparableString field, DateTime value)
	{
		if (field is null)
		{
			return value != null;
		}
		else
		{
			return field.Value != JiraClient.FormatDateTimeString(value);
		}
	}

	public static bool operator >(ComparableString field, DateTime value)
	{
		return field.Value.CompareTo(JiraClient.FormatDateTimeString(value)) > 0;
	}

	public static bool operator <(ComparableString field, DateTime value)
	{
		return field.Value.CompareTo(JiraClient.FormatDateTimeString(value)) < 0;
	}

	public static bool operator <=(ComparableString field, DateTime value)
	{
		return field.Value.CompareTo(JiraClient.FormatDateTimeString(value)) <= 0;
	}

	public static bool operator >=(ComparableString field, DateTime value)
	{
		return field.Value.CompareTo(JiraClient.FormatDateTimeString(value)) >= 0;
	}

	public override string ToString()
	{
		return Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is ComparableString comparableString)
		{
			return Value.Equals(comparableString.Value);
		}
		else if (obj is string obString)
		{
			return Value.Equals(obString);
		}

		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		if (Value == null)
		{
			return 0;
		}

		return Value.GetHashCode();
	}
}
