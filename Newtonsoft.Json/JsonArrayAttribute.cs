using System;

namespace Newtonsoft.Json
{
	[AttributeUsage]
	public sealed class JsonArrayAttribute : JsonContainerAttribute
	{
		private bool _allowNullItems;

		public bool AllowNullItems
		{
			get
			{
				return this._allowNullItems;
			}
			set
			{
				this._allowNullItems = value;
			}
		}

		public JsonArrayAttribute()
		{
		}

		public JsonArrayAttribute(bool allowNullItems)
		{
			this._allowNullItems = allowNullItems;
		}

		public JsonArrayAttribute(string id) : base(id)
		{
		}
	}
}
