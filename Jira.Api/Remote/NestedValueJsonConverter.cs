using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jira.Api.Remote;

/// <summary>
/// JSON converter for nested value properties
/// </summary>
public class NestedValueJsonConverter(string innerProperty) : JsonConverter
{
	private readonly string _innerProperty = innerProperty;

	/// <summary>
	/// Determines whether this instance can convert the specified object type
	/// </summary>
	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	/// <summary>
	/// Writes the JSON representation of the object
	/// </summary>
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		var outerObject = new JObject(new JProperty(_innerProperty, value));
		outerObject.WriteTo(writer);
	}

	/// <summary>
	/// Reads the JSON representation of the object
	/// </summary>
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var outerObject = JObject.Load(reader);
		return outerObject[_innerProperty]?.ToObject(objectType);
	}
}
