using Verse;

namespace RD_SlowDown
{
	public class Settings : ModSettings
	{
		internal static bool EnableMod = false;
		internal static bool HardcoreMode = false;
		internal static bool Slowed = false;
		internal static bool DisableForcedNormal = false;
		internal static int maxTimeSpeed = 3;
		internal static bool SlowestToTheLeft = false;

		public static void ToggleSlow()
		{
			Slowed = !Slowed;
			//Log.Message("[RD_SlowDown] toggling slow");
		}

		// This method saves values to savedata
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref EnableMod, "EnableMod", false);
			Scribe_Values.Look(ref HardcoreMode, "HardcoreMode", false);
			Scribe_Values.Look(ref Slowed, "Slowed", false);
			Scribe_Values.Look(ref DisableForcedNormal, "DisableForcedNormal", false);
			Scribe_Values.Look<int>(ref maxTimeSpeed, "maxTimeSpeed", 3);
			Scribe_Values.Look(ref SlowestToTheLeft, "SlowestToTheLeft", false);
		}

		public static bool isEnabled
		{
			get { return Settings.EnableMod; }
		}

		public static bool isHardcore
		{
			get { return Settings.HardcoreMode && isEnabled; }
		}

		public static bool isSlowed
		{
			get { return Settings.Slowed && isEnabled; }
		}

		public static bool isHardcoreOrSlowed
		{
			get { return isHardcore || isSlowed; }
		}

		public static bool isHardcoreAndSlowed
		{
			get { return isHardcore && isSlowed; }
		}

		public static bool isDisabledOrSoftcore
		{
			get { return !Settings.HardcoreMode || !isEnabled; }
		}

		public static bool isSlowestToTheLeft
		{
			get { return Settings.SlowestToTheLeft; }
		}

		public static bool isDefaultSpeedOrder
		{
			get { return !Settings.SlowestToTheLeft; }
		}
	}
}