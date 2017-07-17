using UnityEngine;
using Verse;

namespace RD_SlowDown
{
	public class Controller : Mod
	{
		public Controller(ModContentPack content) : base(content)
		{
			GetSettings<Settings>();
		}

		public override void WriteSettings()
		{
			base.WriteSettings();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard list = new Listing_Standard();
			list.ColumnWidth = inRect.width;
			list.Begin(inRect);
			list.Gap();
			list.CheckboxLabeled("Enable mod", ref Settings.EnableMod, "Enabled = slow, disabled = fast");
			list.End();
		}

		public override string SettingsCategory()
		{
			return "Slow Down";
		}
	}
}