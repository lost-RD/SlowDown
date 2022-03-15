using UnityEngine;
using Verse;

namespace RD_SlowDown
{
	public class Controller : Mod
	{
		string label = "undefined";

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
			switch (Settings.maxTimeSpeed)
			{
				case 1:
					label = "Normal";
					break;
				case 2:
					label = "Slow";
					break;
				case 3:
					label = "Superslow";
					break;
				case 4:
					label = "Ultraslow (dev slow)";
					break;
				default:
					label = "undefined";
					break;
			}
			Listing_Standard list = new Listing_Standard();
			list.Begin(inRect);
			list.verticalSpacing = 10f;
			list.ColumnWidth = inRect.width*(3f/5f);

			Text.Font = GameFont.Medium;
			list.Label("Settings", -1f, null);
			Text.Font = GameFont.Small;

			list.Gap(list.verticalSpacing);

			list.CheckboxLabeled("Enable mod", ref Settings.EnableMod, "Enabled or disable all features of the mod");
			list.CheckboxLabeled("Disable forced normal", ref Settings.DisableForcedNormal, "Forced normal speed happens on some letters");
			float h = list.CurHeight-list.verticalSpacing;
			list.CheckboxLabeled("Hardcore mode: disable pause", ref Settings.HardcoreMode, "Enabled = hardcore, disabled = normal");
			
			list.ColumnWidth = inRect.width * (2f / 5f);
			list.Label("Slowest Hardcore speed: "+label, -1, "This is the slowest speed you'll be allowed to use during Hardcore");

			list.NewColumn();
			list.ColumnWidth = inRect.width * (1f / 5f);

			Text.Font = GameFont.Medium;
			list.Label("", -1f, null);
			Text.Font = GameFont.Small;

			list.Gap(list.verticalSpacing);
			list.Gap(h);
			int v = Mathf.RoundToInt(list.Slider((float)Settings.maxTimeSpeed, 1, 4));
			Settings.maxTimeSpeed = v;

			list.End();
		}

		public override string SettingsCategory()
		{
			return "Slow Down";
		}
	}
}