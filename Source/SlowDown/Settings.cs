using Verse;

namespace RD_SlowDown
{
	public class Settings : ModSettings
	{
		internal static bool EnableMod = true;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref EnableMod, "EnableMod", false);
		}
	}
}