using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Jira.Api.Remote;

namespace Jira.Api;

/// <summary>
/// An issue field edit metadata as defined in JIRA.
/// </summary>
/// <remarks>
/// Creates a new instance of IssueFieldEditMetadata based on a remote Entity
/// </remarks>
/// <param name="remoteEntity">The remote field entity</param>
public class IssueFieldEditMetadata(RemoteIssueFieldMetadata remoteEntity)
{

	/// <summary>
	/// Whether this is a custom field.
	/// </summary>
	public bool IsCustom
	{
		get
		{
			return Schema.Custom != null;
		}
	}

	/// <summary>
	/// Whether the field is required.
	/// </summary>
	public bool IsRequired { get; private set; } = remoteEntity.Required;

	/// <summary>
	/// Schema of this field.
	/// </summary>
	public IssueFieldEditMetadataSchema Schema { get; private set; } = remoteEntity.Schema == null ? null : new IssueFieldEditMetadataSchema(remoteEntity.Schema);

	/// <summary>
	/// Name of this field.
	/// </summary>
	public string Name { get; private set; } = remoteEntity.name;

	/// <summary>
	/// The url to use in autocompletion.
	/// </summary>
	public string AutoCompleteUrl { get; private set; } = remoteEntity.AutoCompleteUrl;

	/// <summary>
	/// Operations that can be done on this field.
	/// </summary>
	public IList<IssueFieldEditMetadataOperation> Operations { get; private set; } = remoteEntity.Operations;

	/// <summary>
	/// List of available allowed values that can be set. All objects in this array are of the same type.
	/// However there is multiple possible types it could be.
	/// You should decide what the type it is and convert to custom implemented type by yourself.
	/// </summary>
	public JArray AllowedValues { get; private set; } = remoteEntity.AllowedValues;

	/// <summary>
	/// Whether the field has a default value.
	/// </summary>
	public bool HasDefaultValue { get; set; } = remoteEntity.HasDefaultValue;

	/// <summary>
	/// List of field's available allowed values as object of class T which is ought to be implemented by user of this method.
	/// Conversion from serialized JObject to custom class T takes here place.
	/// </summary>
	public IEnumerable<T> AllowedValuesAs<T>()
	{
		return AllowedValues.Values<JObject>().Select(x => x.ToObject<T>());
	}
}
