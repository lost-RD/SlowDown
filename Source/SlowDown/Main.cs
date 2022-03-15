using Verse;
using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse.Sound;

namespace RD_SlowDown
{
	[StaticConstructorOnStartup]
	class Main
	{
		static Main()
		{
			Log.Message("[RD_SlowDown] Initialising mod...");
			var harmony = new Harmony("org.rd.slowdown");
			harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
			Log.Message("[RD_SlowDown] Done!");
		}

		public static readonly Texture2D[] SlowButtonTextures = new Texture2D[]
		{
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Pause", true),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Normal", true),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Fast", true),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Superfast", true),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Superfast", true)
		};
	}

	[HarmonyPatch(typeof(TickManager))]
	[HarmonyPatch("TickRateMultiplier", MethodType.Getter)]
	public class Patch1
	{
		[HarmonyPrefix]
		static bool Prefix(TickManager __instance, ref float __result)
		{
			if (__instance.slower.ForcedNormalSpeed && !Settings.DisableForcedNormal)
			{
				if (__instance.CurTimeSpeed == TimeSpeed.Paused)
				{
					__result = 0f;
					return false;
				}
				__result = 1f;
				return false;
			}
			else
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
						return false;
					case TimeSpeed.Superfast:
						if (Find.Maps.Count == 0)
						{
							__result = 120f;
							return false;
						}
						if (Traverse.Create(__instance).Method("NothingHappeningInGame").GetValue<bool>())
						{
							__result = 12f;
							return false;
						}
						__result = 6f;
						return false;
					case TimeSpeed.Ultrafast:
						if (Find.Maps.Count == 0 || Traverse.Create(__instance).Field("UltraSpeedBoost").GetValue<bool>())
						{
							__result = 150f;
							return false;
						}
						__result = 15f;
						return false;
					default:
						__result = -1f;
						return false;
				}
			}
		}
	}

	[HarmonyPatch(typeof(TickManager))]
	[HarmonyPatch("CurTimePerTick", MethodType.Getter)]
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
			if (Settings.isSlowed)
			{
				__result = 1 / (60f / __instance.TickRateMultiplier);
			}
			else
			{
				__result = 1 / (60f * __instance.TickRateMultiplier);
			}
			//Log.Message(__result.ToString()+", "+Settings.EnableMod.ToString());
			return false;
		}
	}


	[HarmonyPatch(typeof(TimeControls))]
	[HarmonyPatch("DoTimeControlsGUI", MethodType.Normal)]
	public class Patch3
	{
		public static Traverse tc = Traverse.CreateWithType("TimeControls");
		static Texture2D[] speedButtons;

		[HarmonyPrefix]
		static bool Prefix(Rect timerRect)
		{
			int start = Settings.isHardcore ? 1 : 0;

			if (Settings.isSlowed)
			{
				speedButtons = Main.SlowButtonTextures;
			}
			else
			{
				speedButtons = TexButton.SpeedButtonTextures;
			}

			TickManager tickManager = Find.TickManager;
			TimeSpeed[] ctsv = tc.Field("CachedTimeSpeedValues").GetValue<TimeSpeed[]>();
			GUI.BeginGroup(timerRect);
			Rect rect = new Rect(0f, 0f, TimeControls.TimeButSize.x, TimeControls.TimeButSize.y);

			int end = (Settings.isHardcoreAndSlowed) ? Settings.maxTimeSpeed + 1 : ctsv.Length;
			for (int i = start; i < end; i++)
			{
				TimeSpeed timeSpeed = ctsv[i];
				if (timeSpeed != TimeSpeed.Ultrafast)
				{
					if (Widgets.ButtonImage(rect, speedButtons[(int)timeSpeed], true))
					{
						if (timeSpeed == TimeSpeed.Paused)
						{
							tickManager.TogglePaused();
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
						}
						else
						{
							tickManager.CurTimeSpeed = timeSpeed;
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
						}
						tc.Method("PlaySoundOf", tickManager.CurTimeSpeed);
					}
					if (tickManager.CurTimeSpeed == timeSpeed)
					{
						GUI.DrawTexture(rect, TexUI.HighlightTex);
					}
					rect.x += rect.width;
				}
			}
			if (Find.TickManager.slower.ForcedNormalSpeed)
			{
				Widgets.DrawLineHorizontal(rect.width * 2f, rect.height / 2f, rect.width * 2f);
			}
			GUI.EndGroup();
			GenUI.AbsorbClicksInRect(timerRect);
			UIHighlighter.HighlightOpportunity(timerRect, "TimeControls");
			if (Event.current.type == EventType.KeyDown)
			{
				if (KeyBindingDefOf.TogglePause.KeyDownEvent)
				{
					Find.TickManager.TogglePaused();
					tc.Method("PlaySoundOf", Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (!Find.WindowStack.WindowsForcePause)
				{
					if (KeyBindingDefOf.TimeSpeed_Normal.KeyDownEvent)
					{
						Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
						tc.Method("PlaySoundOf", Find.TickManager.CurTimeSpeed);
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
						Event.current.Use();
					}
					if (KeyBindingDefOf.TimeSpeed_Fast.KeyDownEvent)
					{
						if (Settings.isDisabledOrSoftcore || (Settings.HardcoreMode && Settings.maxTimeSpeed >= 2 || !Settings.Slowed))
						{
							Find.TickManager.CurTimeSpeed = TimeSpeed.Fast;
							tc.Method("PlaySoundOf", Find.TickManager.CurTimeSpeed);
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
							Event.current.Use();
						}
					}
					if (KeyBindingDefOf.TimeSpeed_Superfast.KeyDownEvent)
					{
						if (Settings.isDisabledOrSoftcore || (Settings.HardcoreMode && Settings.maxTimeSpeed >= 3 || !Settings.Slowed))
						{
							Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;
							tc.Method("PlaySoundOf", Find.TickManager.CurTimeSpeed);
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
							Event.current.Use();
						}
					}
				}
				if (Prefs.DevMode)
				{
					if (KeyBindingDefOf.TimeSpeed_Ultrafast.KeyDownEvent)
					{
						if (Settings.isDisabledOrSoftcore || (Settings.HardcoreMode && Settings.maxTimeSpeed >= 4 || !Settings.Slowed))
						{
							Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
							tc.Method("PlaySoundOf", Find.TickManager.CurTimeSpeed);
							Event.current.Use();
						}
					}
					if (KeyBindingDefOf.Dev_TickOnce.KeyDownEvent && tickManager.CurTimeSpeed == TimeSpeed.Paused)
					{
						tickManager.DoSingleTick();
						SoundDefOf.Clock_Stop.PlayOneShotOnCamera(null);
					}
				}
			}
			return false;
		}
	}
}