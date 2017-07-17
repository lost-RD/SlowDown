using Verse;
using Harmony;

namespace RD_SlowDown
{
	[StaticConstructorOnStartup]
	class Main
	{
		static Main()
		{
			var harmony = HarmonyInstance.Create("org.rd.slowdown");
			harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
		}
	}

	[HarmonyPatch(typeof(TickManager))]
	[HarmonyPatch("TickRateMultiplier", PropertyMethod.Getter)]
	public class Patch1
	{
		[HarmonyPrefix]
		static bool Prefix(TickManager __instance, ref float __result)
		{
			/*if (__instance.slower.ForcedNormalSpeed)
			{
				if (__instance.CurTimeSpeed == TimeSpeed.Paused)
				{
					__result = 0f;
					return false;
				}
				//__result = 1f;
			}
			else*/
			{
				switch (__instance.CurTimeSpeed)
				{
					case TimeSpeed.Paused:
						__result = 0f;
						return false;
					case TimeSpeed.Normal:
						__result = 1f;
						return false;
					case TimeSpeed.Fast:
						__result = 3f;
						//__result = 1f;
						return false;
					case TimeSpeed.Superfast:
						if (Find.VisibleMap == null)
						{
							__result = 150f;
							//__result = 1f;
							return false;
						}
						if (Traverse.Create(__instance).Method("NothingHappeningInGame").GetValue<bool>())
						{
							__result = 12f;
							//__result = 1f;
							return false;
						}
						__result = 6f;
						//__result = 1f;
						return false;
					case TimeSpeed.Ultrafast:
						if (Find.VisibleMap == null)
						{
							__result = 250f;
							//__result = 1f;
							return false;
						}
						__result = 15f;
						//__result = 1f;
						return false;
					default:
						__result = -1f;
						return false;
				}
			}
		}
	}

	[HarmonyPatch(typeof(TickManager))]
	[HarmonyPatch("CurTimePerTick", PropertyMethod.Getter)]
	public class Patch2
	{
		[HarmonyPrefix]
		static bool Prefix(TickManager __instance, ref float __result)
		{
			if (__instance.TickRateMultiplier == 0f)
			{
				__result = 0f;
				return false;
			}
			__result = (60f / __instance.TickRateMultiplier);
			if (Settings.EnableMod)
			{
				__result = 1 / (60f / __instance.TickRateMultiplier);
			} else {
				__result = 1 / (60f * __instance.TickRateMultiplier);
			}
			Log.Message(__result.ToString()+", "+Settings.EnableMod.ToString());
			return false;
		}
	}
}