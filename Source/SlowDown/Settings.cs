using Verse;

namespace RD_SlowDown
{
	public class Settings : ModSettings
	{
		internal static bool EnableMod = false;
		internal static bool HardcoreMode = false;
		internal static bool Slowed = false;

		public static void ToggleSlow()
		{
			Slowed = !Slowed;
			//Log.Message("[RD_SlowDown] toggling slow");
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref EnableMod, "EnableMod", true);
			Scribe_Values.Look(ref HardcoreMode, "HardcoreMode", true);
			Scribe_Values.Look(ref Slowed, "Slowed", true);
		}
	}
}