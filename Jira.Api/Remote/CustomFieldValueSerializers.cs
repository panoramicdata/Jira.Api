﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Jira.Api.Remote;

public class SingleObjectCustomFieldValueSerializer(string propertyName) : ICustomFieldValueSerializer
{
	private readonly string _propertyName = propertyName;

	public string[] FromJson(JToken json)
	{
		return [json[_propertyName]?.ToString()];
	}

	public JToken ToJson(string[] values)
	{
		return new JObject(new JProperty(_propertyName, values[0]));
	}
}

public class MultiObjectCustomFieldValueSerializer(string propertyName) : ICustomFieldValueSerializer
{
	private readonly string _propertyName = propertyName;

	public string[] FromJson(JToken json)
	{
		return [.. ((JArray)json).Select(j => j[_propertyName].ToString())];
	}

	public JToken ToJson(string[] values)
	{
		return JArray.FromObject(values.Select(v => new JObject(new JProperty(_propertyName, v))).ToArray());
	}
}

public class FloatCustomFieldValueSerializer : ICustomFieldValueSerializer
{
	public string[] FromJson(JToken json)
	{
		return [json.ToObject<string>()];
	}

	public JToken ToJson(string[] values)
	{
		return float.Parse(values[0], CultureInfo.InvariantCulture);
	}
}

public class MultiStringCustomFieldValueSerializer : ICustomFieldValueSerializer
{
	public string[] FromJson(JToken json)
	{
		return JsonConvert.DeserializeObject<string[]>(json.ToString());
	}

	public JToken ToJson(string[] values)
	{
		return JArray.FromObject(values);
	}
}

public class CascadingSelectCustomFieldValueSerializer : ICustomFieldValueSerializer
{
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

public class GreenhopperSprintCustomFieldValueSerialiser(string propertyName) : ICustomFieldValueSerializer
{
	private readonly string _propertyName = propertyName;

	// Sprint field is malformed
	// See https://ecosystem.atlassian.net/browse/ACJIRA-918 for more information
	public string[] FromJson(JToken json)
	{
		return [.. json.ToString()
			.Split(['{', '}', '[', ']', ','])
			.Where(x => x.StartsWith(_propertyName))
			.Select(x => x.Split(['='])[1])];
	}

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

public class GreenhopperSprintJsonCustomFieldValueSerialiser : ICustomFieldValueSerializer
{
	public string[] FromJson(JToken json)
	{
		return [.. JsonConvert
			.DeserializeObject<List<Sprint>>(json.ToString())
			.OrderByDescending(x => x.endDate)
			.Select(x => x.name)];
	}

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
	public int id { get; set; }
	public string name { get; set; }
	public string state { get; set; }
	public int boardId { get; set; }
	public string goal { get; set; }
	public DateTime startDate { get; set; }
	public DateTime endDate { get; set; }
	public DateTime completeDate { get; set; }
}
