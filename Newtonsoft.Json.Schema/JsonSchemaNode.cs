using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Newtonsoft.Json.Schema
{
	internal class JsonSchemaNode
	{
		public string Id
		{
			get;
			private set;
		}

		public ReadOnlyCollection<JsonSchema> Schemas
		{
			get;
			private set;
		}

		public Dictionary<string, JsonSchemaNode> Properties
		{
			get;
			private set;
		}

		public Dictionary<string, JsonSchemaNode> PatternProperties
		{
			get;
			private set;
		}

		public List<JsonSchemaNode> Items
		{
			get;
			private set;
		}

		public JsonSchemaNode AdditionalProperties
		{
			get;
			set;
		}

		public JsonSchemaNode(JsonSchema schema)
		{
			this.Schemas = new ReadOnlyCollection<JsonSchema>(new JsonSchema[]
			{
				schema
			});
			this.Properties = new Dictionary<string, JsonSchemaNode>();
			this.PatternProperties = new Dictionary<string, JsonSchemaNode>();
			this.Items = new List<JsonSchemaNode>();
			this.Id = JsonSchemaNode.GetId(this.Schemas);
		}

		private JsonSchemaNode(JsonSchemaNode source, JsonSchema schema)
		{
			this.Schemas = new ReadOnlyCollection<JsonSchema>(Enumerable.ToList<JsonSchema>(Enumerable.Union<JsonSchema>(source.Schemas, new JsonSchema[]
			{
				schema
			})));
			this.Properties = new Dictionary<string, JsonSchemaNode>(source.Properties);
			this.PatternProperties = new Dictionary<string, JsonSchemaNode>(source.PatternProperties);
			this.Items = new List<JsonSchemaNode>(source.Items);
			this.AdditionalProperties = source.AdditionalProperties;
			this.Id = JsonSchemaNode.GetId(this.Schemas);
		}

		public JsonSchemaNode Combine(JsonSchema schema)
		{
			return new JsonSchemaNode(this, schema);
		}

		public static string GetId(IEnumerable<JsonSchema> schemata)
		{
			return string.Join("-", Enumerable.ToArray<string>(Enumerable.OrderBy<string, string>(Enumerable.Select<JsonSchema, string>(schemata, (JsonSchema s) => s.InternalId), (string id) => id, StringComparer.get_Ordinal())));
		}
	}
}
