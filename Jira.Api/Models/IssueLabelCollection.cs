namespace Jira.Api.Models;

/// <summary>
/// Collection of labels for an issue.
/// </summary>
/// <remarks>
/// Creates a new instance of IssueLabelCollection.
/// </remarks>
/// <param name="labels">Labels to seed into this collection</param>
public class IssueLabelCollection(IList<string> labels) : List<string>(labels), IRemoteIssueFieldProvider
{
	private readonly List<string> _originalLabels = [.. labels];

	/// <summary>
	/// Adds labels to this collection.
	/// </summary>
	/// <param name="labels">The list of labels to add.</param>
	public void Add(params string[] labels)
	{
		AddRange(labels);
	}

	/// <summary>
	/// Equality operator for checking if a value exists in the collection
	/// </summary>
	public static bool operator ==(IssueLabelCollection list, string value)
	{
		return list is null ? value == null : list.Any(v => v == value);
	}

	/// <summary>
	/// Inequality operator for checking if a value does not exist in the collection
	/// </summary>
	public static bool operator !=(IssueLabelCollection list, string value)
	{
		return list is null ? value != null : !list.Any(v => v == value);
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is IssueLabelCollection other && this.SequenceEqual(other);

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var hashCode = 0;
		foreach (var item in this)
		{
			hashCode ^= item?.GetHashCode() ?? 0;
		}

		return hashCode;
	}

	Task<RemoteFieldValue[]> IRemoteIssueFieldProvider.GetRemoteFieldValuesAsync(CancellationToken cancellationToken)
	{
		var fieldValues = new List<RemoteFieldValue>();

		if (_originalLabels.Count != this.Count || this.Except(_originalLabels).Any())
		{
			fieldValues.Add(new RemoteFieldValue()
			{
				id = "labels",
				values = ToArray()
			});
		}

		return Task.FromResult(fieldValues.ToArray());
	}
}
