using Verse;

namespace RD_SlowDown
{
	public class Settings : ModSettings
	{
		internal static bool EnableMod = false;

		public static void Toggle()
		{
			EnableMod = !EnableMod;
			//Log.Message("[RD_SlowDown] toggling mod");
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref EnableMod, "EnableMod", false);
		}
	}
}