﻿using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Jira.Api;

/// <summary>
/// A custom field associated with an issue
/// </summary>
public class CustomFieldValue
{
	private readonly Issue _issue;
	private string _name;

	internal CustomFieldValue(string id, Issue issue)
	{
		Id = id;
		_issue = issue;
	}

	internal CustomFieldValue(string id, string name, Issue issue)
		: this(id, issue)
	{
		_name = name;
	}

	/// <summary>
	/// The values of the custom field
	/// </summary>
	public string[] Values { get; set; }

	/// <summary>
	/// Id of the custom field as defined in JIRA
	/// </summary>
	public string Id { get; private set; }

	internal JToken RawValue { get; set; }

	internal ICustomFieldValueSerializer? Serializer { get; set; }

	/// <summary>
	/// Name of the custom field as defined in JIRA
	/// </summary>
	public string Name
	{
		get
		{
			if (string.IsNullOrEmpty(_name))
			{
				var customField = _issue.Jira.Fields.GetCustomFieldsAsync(default).Result.FirstOrDefault(f => f.Id == Id) ?? throw new InvalidOperationException($"Custom field with id '{Id}' was not found.");
				_name = customField.Name;
			}

			return _name;
		}
	}
}
