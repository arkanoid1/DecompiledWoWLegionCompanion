using System;

namespace WowStaticData
{
	public class SpellTooltipRec
	{
		public int ID
		{
			get;
			private set;
		}

		public string Description
		{
			get;
			private set;
		}

		public void Deserialize(string valueLine)
		{
			int num = valueLine.IndexOf('\t', 0);
			string text = valueLine.Substring(0, num).Trim();
			this.ID = Convert.ToInt32(text);
			this.Description = valueLine.Substring(num + 1).Trim();
		}
	}
}
