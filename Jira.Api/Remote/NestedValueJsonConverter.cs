using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jira.Api.Remote;

public class NestedValueJsonConverter(string innerProperty) : JsonConverter
{
	private readonly string _innerProperty = innerProperty;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		var outerObject = new JObject(new JProperty(this._innerProperty, value));
		outerObject.WriteTo(writer);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var outerObject = JObject.Load(reader);
		return outerObject[_innerProperty]?.ToObject(objectType);
	}
}
