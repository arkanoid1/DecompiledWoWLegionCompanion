using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JsonDotNet.Extras.CustomConverters
{
	public class Matrix4x4Converter : JsonConverter
	{
		public override bool CanRead
		{
			get
			{
				return false;
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JToken jToken = JToken.FromObject(value);
			if (jToken.Type != JTokenType.Object)
			{
				jToken.WriteTo(writer, new JsonConverter[0]);
			}
			else
			{
				JObject jObject = (JObject)jToken;
				IList<string> content = Enumerable.ToList<string>(Enumerable.Select<JProperty, string>(Enumerable.Where<JProperty>(jObject.Properties(), (JProperty p) => p.Name != "inverse" && p.Name != "transpose"), (JProperty p) => p.Name));
				jObject.AddFirst(new JProperty("Keys", new JArray(content)));
				jObject.WriteTo(writer, new JsonConverter[0]);
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Matrix4x4);
		}
	}
}
