using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Jira.Api;

/// <summary>
/// PagedQueryResult that can be deserialized from default JIRA paging response.
/// </summary>
/// <remarks>
/// Create a new instance of PagedQueryResult with all metadata provided.
/// </remarks>
/// <param name="enumerable">Enumerable to wrap.</param>
/// <param name="startAt">Index within the total items where this page's paged result starts.</param>
/// <param name="itemsPerPage">Number of items returned per page.</param>
/// <param name="totalItems">Number of total items available on the server.</param>
internal class PagedQueryResult<T>(IEnumerable<T> enumerable, int startAt, int itemsPerPage, int totalItems) : IPagedQueryResult<T>
{
	private readonly IEnumerable<T> _enumerable = enumerable;
	private readonly int _startAt = startAt;
	private readonly int _itemsPerPage = itemsPerPage;
	private readonly int _totalItems = totalItems;

	/// <summary>
	/// Create an instance of PagedQueryResult taking metadata from a JSON object.
	/// </summary>
	/// <param name="pagedJson">JSON object with JIRA paged metadata.</param>
	/// <param name="items">Enumerable to wrap.</param>
	public static PagedQueryResult<T> FromJson(JObject pagedJson, IEnumerable<T> items)
	{
		return new PagedQueryResult<T>(
			items,
			GetPropertyOrDefault<int>(pagedJson, "startAt"),
			GetPropertyOrDefault<int>(pagedJson, "maxResults"),
			GetPropertyOrDefault<int>(pagedJson, "total"));
	}

	/// <summary>
	/// Index within the total items where this page's paged result starts.
	/// </summary>
	public int StartAt
	{
		get { return _startAt; }
	}

	/// <summary>
	/// Number of items returned per page.
	/// </summary>
	public int ItemsPerPage
	{
		get { return _itemsPerPage; }
	}

	/// <summary>
	/// Number of total items available on the server.
	/// </summary>
	public int TotalItems
	{
		get { return _totalItems; }
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	public IEnumerator<T> GetEnumerator()
	{
		return _enumerable.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _enumerable.GetEnumerator();
	}

	private static TValue GetPropertyOrDefault<TValue>(JObject json, string property)
	{
		var val = json[property];

		if (val == null || val.Type == JTokenType.Null)
		{
			return default;
		}
		else
		{
			return val.Value<TValue>();
		}
	}
}
