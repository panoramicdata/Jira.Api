using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Jira.Api.Remote;

/// <summary>
/// Serializer for single object custom field values
/// </summary>
public class SingleObjectCustomFieldValueSerializer(string propertyName) : ICustomFieldValueSerializer
{
	private readonly string _propertyName = propertyName;

	/// <summary>
	/// Deserializes the value from JSON
	/// </summary>
	public string[] FromJson(JToken json)
	{
		return [json[_propertyName]?.ToString()];
	}

	/// <summary>
	/// Serializes the value to JSON
	/// </summary>
	public JToken ToJson(string[] values)
	{
		return new JObject(new JProperty(_propertyName, values[0]));
	}
}

/// <summary>
/// Serializer for multi-object custom field values
/// </summary>
public class MultiObjectCustomFieldValueSerializer(string propertyName) : ICustomFieldValueSerializer
{
	private readonly string _propertyName = propertyName;

	/// <summary>
	/// Deserializes the value from JSON
	/// </summary>
	public string[] FromJson(JToken json)
	{
		return [.. ((JArray)json).Select(j => j[_propertyName].ToString())];
	}

	/// <summary>
	/// Serializes the value to JSON
	/// </summary>
	public JToken ToJson(string[] values)
	{
		return JArray.FromObject(values.Select(v => new JObject(new JProperty(_propertyName, v))).ToArray());
	}
}

/// <summary>
/// Serializer for float custom field values
/// </summary>
public class FloatCustomFieldValueSerializer : ICustomFieldValueSerializer
{
	/// <summary>
	/// Deserializes the value from JSON
	/// </summary>
	public string[] FromJson(JToken json)
	{
		return [json.ToObject<string>()];
	}

	/// <summary>
	/// Serializes the value to JSON
	/// </summary>
	public JToken ToJson(string[] values)
	{
		return float.Parse(values[0], CultureInfo.InvariantCulture);
	}
}

/// <summary>
/// Serializer for multi-string custom field values
/// </summary>
public class MultiStringCustomFieldValueSerializer : ICustomFieldValueSerializer
{
	/// <summary>
	/// Deserializes the value from JSON
	/// </summary>
	public string[] FromJson(JToken json)
	{
		return JsonConvert.DeserializeObject<string[]>(json.ToString());
	}

	/// <summary>
	/// Serializes the value to JSON
	/// </summary>
	public JToken ToJson(string[] values)
	{
		return JArray.FromObject(values);
	}
}

/// <summary>
/// Serializer for cascading select custom field values
/// </summary>
public class CascadingSelectCustomFieldValueSerializer : ICustomFieldValueSerializer
{
	/// <summary>
	/// Deserializes the value from JSON
	/// </summary>
	public string[] FromJson(JToken json)
	{
		var parentOption = json["value"];
		var childOption = json["child"];

		if (parentOption == null)
		{
			throw new InvalidOperationException($"Unable to deserialize custom field as a cascading select list. The parent value is required. Json: {json.ToString()}");
		}
		else if (childOption == null || childOption["value"] == null)
		{
			return [parentOption.ToString()];
		}
		else
		{
			return [parentOption.ToString(), childOption["value"].ToString()];
		}
	}

	/// <summary>
	/// Serializes the value to JSON
	/// </summary>
	public JToken ToJson(string[] values)
	{
		if (values == null || values.Length < 1)
		{
			throw new InvalidOperationException("Unable to serialize the custom field as a cascading select list. At least the parent value is required.");
		}
		else if (values.Length == 1)
		{
			return JToken.FromObject(new { value = values[0] });
		}
		else
		{
			return JToken.FromObject(new
			{
				value = values[0],
				child = new
				{
					value = values[1]
				}
			});
		}
	}
}

/// <summary>
/// Serializer for Greenhopper sprint custom field values
/// </summary>
public class GreenhopperSprintCustomFieldValueSerialiser(string propertyName) : ICustomFieldValueSerializer
{
	private readonly string _propertyName = propertyName;

	// Sprint field is malformed
	// See https://ecosystem.atlassian.net/browse/ACJIRA-918 for more information
	/// <summary>
	/// Deserializes the value from JSON
	/// </summary>
	public string[] FromJson(JToken json)
	{
		return [.. json.ToString()
			.Split(['{', '}', '[', ']', ','])
			.Where(x => x.StartsWith(_propertyName))
			.Select(x => x.Split(['='])[1])];
	}

	/// <summary>
	/// Serializes the value to JSON
	/// </summary>
	public JToken ToJson(string[] values)
	{
		string val = values?.FirstOrDefault();

		if (int.TryParse(val, out int id))
		{
			return id;
		}

		return val;
	}
}

/// <summary>
/// Serializer for Greenhopper sprint JSON custom field values
/// </summary>
public class GreenhopperSprintJsonCustomFieldValueSerialiser : ICustomFieldValueSerializer
{
	/// <summary>
	/// Deserializes the value from JSON
	/// </summary>
	public string[] FromJson(JToken json)
	{
		return [.. JsonConvert
			.DeserializeObject<List<Sprint>>(json.ToString())
			.OrderByDescending(x => x.endDate)
			.Select(x => x.name)];
	}

	/// <summary>
	/// Serializes the value to JSON
	/// </summary>
	public JToken ToJson(string[] values)
	{
		var val = values?.FirstOrDefault();

		if (int.TryParse(val, out var id))
		{
			return id;
		}

		return val;
	}
}

internal class Sprint
{
	/// <summary>
	/// Sprint ID
	/// </summary>
	public int id { get; set; }

	/// <summary>
	/// Sprint name
	/// </summary>
	public string name { get; set; }

	/// <summary>
	/// Sprint state
	/// </summary>
	public string state { get; set; }

	/// <summary>
	/// Board ID
	/// </summary>
	public int boardId { get; set; }

	/// <summary>
	/// Sprint goal
	/// </summary>
	public string goal { get; set; }

	/// <summary>
	/// Sprint start date
	/// </summary>
	public DateTime startDate { get; set; }

	/// <summary>
	/// Sprint end date
	/// </summary>
	public DateTime endDate { get; set; }

	/// <summary>
	/// Sprint completion date
	/// </summary>
	public DateTime completeDate { get; set; }
}
