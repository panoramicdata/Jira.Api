using Jira.Api.Remote;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Collection of custom fields
/// </summary>
public class CustomFieldValueCollection : ReadOnlyCollection<CustomFieldValue>, IRemoteIssueFieldProvider
{
	private readonly Issue _issue;

	internal CustomFieldValueCollection(Issue issue)
		: this(issue, [])
	{
	}

	internal CustomFieldValueCollection(Issue issue, IList<CustomFieldValue> list)
		: base(list)
	{
		_issue = issue;
	}

	/// <summary>
	/// When Id's are unknown, searches for custom fields by the issue project only.
	/// </summary>
	public bool SearchByProjectOnly { get; set; }

	/// <summary>
	/// Add a custom field by name
	/// </summary>
	/// <param name="fieldName">The name of the custom field as defined in JIRA</param>
	/// <param name="fieldValue">The value of the field</param>
	public CustomFieldValueCollection Add(string fieldName, string fieldValue)
	{
		return AddAsync(fieldName, [fieldValue], null, default).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Add a custom field by name with an array of values
	/// </summary>
	/// <param name="fieldName">The name of the custom field as defined in JIRA</param>
	/// <param name="fieldValues">The values of the field</param>
	/// <param name="cancellationToken"></param>
	public async Task<CustomFieldValueCollection> AddArrayAsync(string fieldName, string[] fieldValues, CancellationToken cancellationToken)
	{
		return await AddAsync(fieldName, fieldValues, new MultiStringCustomFieldValueSerializer(), cancellationToken);
	}

	/// <summary>
	/// Add a cascading select field.
	/// </summary>
	/// <param name="cascadingSelectField">Cascading select field to add.</param>
	/// <param name="cancellationToken"></param>
	public Task<CustomFieldValueCollection> AddCascadingSelectFieldAsync(CascadingSelectCustomField cascadingSelectField, CancellationToken cancellationToken)
	{
		return AddCascadingSelectFieldAsync(cascadingSelectField.Name, cascadingSelectField.ParentOption, cascadingSelectField.ChildOption, cancellationToken);
	}

	/// <summary>
	/// Add a cascading select field.
	/// </summary>
	/// <param name="fieldName">The name of the custom field as defined in JIRA.</param>
	/// <param name="parentOption">The value of the parent option.</param>
	/// <param name="childOption">The value of the child option.</param>
	/// <param name="cancellationToken"></param>
	public Task<CustomFieldValueCollection> AddCascadingSelectFieldAsync(
		string fieldName,
		string parentOption,
		string? childOption,
		CancellationToken cancellationToken)
	{
		var options = new List<string>() { parentOption };

		if (!string.IsNullOrEmpty(childOption))
		{
			options.Add(childOption);
		}

		return AddArrayAsync(fieldName, [.. options], cancellationToken);
	}

	/// <summary>
	/// Add a custom field by name
	/// </summary>
	/// <param name="fieldName">The name of the custom field as defined in JIRA</param>
	/// <param name="fieldValues">The values of the field</param>
	/// <param name="serializer"></param>
	/// <param name="cancellationToken"></param>
	public async Task<CustomFieldValueCollection> AddAsync(
		string fieldName,
		string[] fieldValues,
		ICustomFieldValueSerializer? serializer,
		CancellationToken cancellationToken)
	{
		var fieldId = await GetCustomFieldIdAsync(fieldName, cancellationToken);
		Items.Add(new CustomFieldValue(fieldId, fieldName, _issue) { Values = fieldValues, Serializer = serializer });
		return this;
	}

	/// <summary>
	/// Add a custom field by id with an array of values.
	/// </summary>
	/// <param name="fieldId">The id of the custom field as defined in JIRA.</param>
	/// <param name="fieldValues">The values of the field.</param>
	public CustomFieldValueCollection AddById(string fieldId, params string[] fieldValues)
	{
		Items.Add(new CustomFieldValue(fieldId, _issue) { Values = fieldValues });
		return this;
	}

	/// <summary>
	/// Gets a cascading select custom field by name.
	/// </summary>
	/// <param name="fieldName">Name of the custom field as defined in JIRA.</param>
	/// <returns>CascadingSelectCustomField instance if the field has been set on the issue, null otherwise</returns>
	public CascadingSelectCustomField GetCascadingSelectField(string fieldName)
	{
		CascadingSelectCustomField result = null;
		var fieldValue = this[fieldName];

		if (fieldValue != null && fieldValue.Values != null)
		{
			var parentOption = fieldValue.Values.Length > 0 ? fieldValue.Values[0] : null;
			var childOption = fieldValue.Values.Length > 1 ? fieldValue.Values[1] : null;

			result = new CascadingSelectCustomField(fieldName, parentOption, childOption);
		}

		return result;
	}

	/// <summary>
	/// Deserializes the custom field value to the specified type.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize to.</typeparam>
	/// <param name="fieldName">Name of the custom field as defined in JIRA.</param>
	public T GetAs<T>(string fieldName)
	{
		var result = default(T);
		var fieldValue = this[fieldName];

		if (fieldValue != null && fieldValue.RawValue != null)
		{
			result = JsonConvert.DeserializeObject<T>(fieldValue.RawValue.ToString(), _issue.Jira.RestClient.Settings.JsonSerializerSettings);
		}

		return result;
	}

	/// <summary>
	/// Gets a custom field by name
	/// </summary>
	/// <param name="fieldName">Name of the custom field as defined in JIRA</param>
	/// <returns>CustomField instance if the field has been set on the issue, null otherwise</returns>
	public CustomFieldValue? this[string fieldName]
	{
		get
		{
			var fieldId = GetCustomFieldIdAsync(fieldName, default).GetAwaiter().GetResult();
			return Items.FirstOrDefault(f => f.Id == fieldId);
		}
	}

	private async Task<string> GetCustomFieldIdAsync(string fieldName, CancellationToken cancellationToken)
	{
		var customFieldsOrig = await _issue.Jira.Fields.GetCustomFieldsAsync(cancellationToken);
		var customFields = customFieldsOrig.Where(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
		var searchByProject = (customFields.Count() > 1) || SearchByProjectOnly;

		if (searchByProject)
		{
			// There are multiple custom fields with the same name, need to find it by the project.
			var options = new CustomFieldFetchOptions();
			options.ProjectKeys.Add(_issue.Project);

			if (!string.IsNullOrEmpty(_issue.Type?.Id))
			{
				options.IssueTypeIds.Add(_issue.Type.Id);
			}
			else if (!string.IsNullOrEmpty(_issue.Type?.Name))
			{
				options.IssueTypeNames.Add(_issue.Type.Name);
			}

			customFields = (await _issue.Jira.Fields.GetCustomFieldsAsync(options, cancellationToken)).Where(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
		}

		if (!customFields.Any())
		{
			var errorMessage = $"Could not find custom field with name '{fieldName}' on the JIRA server.";

			if (searchByProject)
			{
				errorMessage += $" The field was only searched for in the project with key '{_issue.Project}'." +
					$" Make sure the custom field is available in the issue create screen for that project.";
			}

			throw new InvalidOperationException(errorMessage);
		}
		else
		{
			return customFields.Single().Id;
		}
	}

	Task<RemoteFieldValue[]> IRemoteIssueFieldProvider.GetRemoteFieldValuesAsync(CancellationToken cancellationToken)
	{
		var fieldValues = Items
			.Where(field => IsCustomFieldNewOrUpdated(field))
			.Select(field => new RemoteFieldValue()
			{
				id = field.Id,
				values = field.Values
			});

		return Task.FromResult(fieldValues.ToArray());
	}

	private bool IsCustomFieldNewOrUpdated(CustomFieldValue customField)
	{
		if (_issue.OriginalRemoteIssue.customFieldValues == null)
		{
			// Original remote issue had no custom fields, this means that a new one has been added by user.
			return true;
		}

		var originalField = _issue.OriginalRemoteIssue.customFieldValues.FirstOrDefault(field => field.customfieldId == customField.Id);

		if (originalField == null)
		{
			// A custom field with this id does not exist on the original remote issue, this means that it was
			//   added by the user
			return true;
		}
		else if (originalField.values == null)
		{
			// The remote custom field was not initialized, include it on the payload.
			return true;
		}
		else if (customField.Values == null)
		{
			// Original field had values, but the new field has been set to null.
			//  User means to clear the value, include it on the payload.
			return true;
		}
		else
		{
			return !originalField.values.SequenceEqual(customField.Values);
		}
	}

}
