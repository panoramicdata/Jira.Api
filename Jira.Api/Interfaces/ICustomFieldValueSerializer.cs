using Newtonsoft.Json.Linq;

namespace Jira.Api.Interfaces;

/// <summary>
/// Contract to serialize and deserialize a custom field value from JIRA.
/// </summary>
public interface ICustomFieldValueSerializer
{
	/// <summary>
	/// Deserializes values from a custom field.
	/// </summary>
	string[] FromJson(JToken json);

	/// <summary>
	/// Serializes values for a custom field.
	/// </summary>
	JToken ToJson(string[] values);
}
