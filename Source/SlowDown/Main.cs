using Verse;
using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse.Sound;
using Verse.Steam;

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

		public static readonly Texture2D[] SlowButtonTexturesAlternative = new Texture2D[]
		{
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Pause", true),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Superfast", true),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Fast", true),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Normal", true),
			ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Superfast", true)
		};
	}

	public enum RD_TimeSpeed : byte
	{
		Paused,
		Normal,
		Fast,
		Superfast,
		Ultrafast,
		Slow,
		Superslow,
		Ultraslow
	}

	[DefOf]
	public static class RD_SoundDefOf
	{
		public static SoundDef Clock_Slow;
		public static SoundDef Clock_Superslow;
	}

	[HarmonyPatch(typeof(TickManager))]
	[HarmonyPatch("TickRateMultiplier", MethodType.Getter)]
	public class Patch1
	{
		private static float paused() { return 0f; }
		private static float normal() { return 1f; }
		private static float fast() { return 3f; }
		private static float superfast(TickManager __instance)
		{
			if (Find.Maps.Count == 0)
			{
				return 120f;
			}
			if (Traverse.Create(__instance).Method("NothingHappeningInGame").GetValue<bool>())
			{
				return 12f;
			}
			return 6f;
		}
		private static float ultrafast(TickManager __instance)
		{
			if (Find.Maps.Count == 0 || Traverse.Create(__instance).Field("UltraSpeedBoost").GetValue<bool>())
			{
				return 150f;
			}
			return 15f;
		}
		private static float slow() { return 1f / fast(); }
		private static float superslow(TickManager __instance)
		{
			return 1f / superfast(__instance);
		}
		private static float ultraslow(TickManager __instance)
		{
			return 1f / ultrafast(__instance);
		}
		private static float _default() { return -1f; }

		private static float TickSpeedFromTimeSpeed(RD_TimeSpeed ts, TickManager __instance)
		{
			switch(ts)
			{
				case RD_TimeSpeed.Paused: return paused();
				case RD_TimeSpeed.Normal: return normal();
				case RD_TimeSpeed.Fast: return fast();
				case RD_TimeSpeed.Slow: return slow();
				case RD_TimeSpeed.Superfast: return superfast(__instance);
				case RD_TimeSpeed.Superslow: return superslow(__instance);
				case RD_TimeSpeed.Ultrafast: return ultrafast(__instance);
				case RD_TimeSpeed.Ultraslow: return ultraslow(__instance);
				default: return _default();
			}
		}

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
						__result = paused();
						return false;
					case TimeSpeed.Normal:
						__result = normal() ;
						return false;
					case TimeSpeed.Fast:
						__result = Settings.isSlowed ? slow() : fast();
						return false;
					case TimeSpeed.Superfast:
						__result = Settings.isSlowed ?
								superslow(__instance) :
								superfast(__instance);
						return false;
					case TimeSpeed.Ultrafast:
						__result = Settings.isSlowed ?
								ultraslow(__instance) :
								ultrafast(__instance);
						return false;
					default:
						__result = -_default();
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
	[StaticConstructorOnStartup]
	public class Patch3
	{
		public static Traverse tc = Traverse.CreateWithType("TimeControls");
		static Texture2D[] speedButtons;

		static Traverse sound_superfast()
		{
			Traverse method_to_play_sound = null;
			method_to_play_sound = tc.Method("PlaySoundOf", TimeSpeed.Superfast);
			return method_to_play_sound;
		}

		static Traverse sound_fast()
		{
			Traverse method_to_play_sound = null;
			method_to_play_sound = tc.Method("PlaySoundOf", TimeSpeed.Fast);
			return method_to_play_sound;
		}

		static Traverse sound_normal()
		{
			Traverse method_to_play_sound = null;
			method_to_play_sound = tc.Method("PlaySoundOf", TimeSpeed.Normal);
			return method_to_play_sound;
		}

		private static void PlaySoundOf(RD_TimeSpeed ts)
		{
			SoundDef soundDef = null;
			switch (ts)
			{
				case RD_TimeSpeed.Paused: soundDef = SoundDefOf.Clock_Stop; break;
				case RD_TimeSpeed.Normal: soundDef = SoundDefOf.Clock_Normal; break;
				case RD_TimeSpeed.Fast: soundDef = SoundDefOf.Clock_Fast; break;
				case RD_TimeSpeed.Slow: soundDef = RD_SoundDefOf.Clock_Slow; break;
				case RD_TimeSpeed.Superfast: soundDef = SoundDefOf.Clock_Superfast; break;
				case RD_TimeSpeed.Superslow: soundDef = RD_SoundDefOf.Clock_Superslow; break;
				case RD_TimeSpeed.Ultrafast: soundDef = SoundDefOf.Clock_Superfast; break;
				case RD_TimeSpeed.Ultraslow: soundDef = RD_SoundDefOf.Clock_Superslow; break;
			}
			if (soundDef != null)
			{
				soundDef.PlayOneShotOnCamera(null);
			}
			SteamDeck.Vibrate();
		}

		[HarmonyPrefix]
		static bool Prefix(Rect timerRect)
		{
			int start = Settings.isHardcore ? 1 : 0;

			if (Settings.isSlowed)
			{
				if (Settings.SlowestToTheLeft)
				{
					//speedButtons = Main.SlowButtonTexturesAlternative;
					speedButtons = Main.SlowButtonTextures;
				} else
				{
					speedButtons = Main.SlowButtonTextures;
				}
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

			Traverse method_to_play_sound = null;

			if (Event.current.type == EventType.KeyDown)
			{
				if (KeyBindingDefOf.TogglePause.KeyDownEvent)
				{
					Find.TickManager.TogglePaused();
					method_to_play_sound = tc.Method("PlaySoundOf", Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (!Find.WindowStack.WindowsForcePause)
				{
					if (!Settings.SlowestToTheLeft || !Settings.isSlowed)
					{
						// Standard behaviour & default case
						// Closely mirrors vanilla behaviour
						if (KeyBindingDefOf.TimeSpeed_Normal.KeyDownEvent)
						{
							Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
							Event.current.Use();
							method_to_play_sound = sound_normal();
						}
						if (KeyBindingDefOf.TimeSpeed_Fast.KeyDownEvent)
						{
							if (Settings.isDisabledOrSoftcore || (Settings.HardcoreMode && Settings.maxTimeSpeed >= 2 || !Settings.Slowed))
							{
								Find.TickManager.CurTimeSpeed = TimeSpeed.Fast;
								PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
								Event.current.Use();
								method_to_play_sound = sound_fast();
							}
						}
						if (KeyBindingDefOf.TimeSpeed_Superfast.KeyDownEvent)
						{
							if (Settings.isDisabledOrSoftcore || (Settings.HardcoreMode && Settings.maxTimeSpeed >= 3 || !Settings.Slowed))
							{
								Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;
								PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
								Event.current.Use();
								method_to_play_sound = sound_superfast();
							}
						}
					} else
					{
						// Slowest to the left
						// If slowed, normal becomes superslow and superfast becomes normal
						if (KeyBindingDefOf.TimeSpeed_Normal.KeyDownEvent)
						{
							if (Settings.isDisabledOrSoftcore || (Settings.HardcoreMode && Settings.maxTimeSpeed >= 3 || !Settings.Slowed))
							{
								Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;
								PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
								Event.current.Use();
								method_to_play_sound = sound_superfast();
							}
						}
						if (KeyBindingDefOf.TimeSpeed_Fast.KeyDownEvent)
						{
							if (Settings.isDisabledOrSoftcore || (Settings.HardcoreMode && Settings.maxTimeSpeed >= 3 || !Settings.Slowed))
							{
								Find.TickManager.CurTimeSpeed = TimeSpeed.Fast;
								PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
								Event.current.Use();
								method_to_play_sound = sound_fast();
							}

						}
						if (KeyBindingDefOf.TimeSpeed_Superfast.KeyDownEvent)
						{
							Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
							Event.current.Use();
							method_to_play_sound = sound_normal();
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
							method_to_play_sound = tc.Method("PlaySoundOf", Find.TickManager.CurTimeSpeed);
							Event.current.Use();
						}
					}
					if (KeyBindingDefOf.Dev_TickOnce.KeyDownEvent && tickManager.CurTimeSpeed == TimeSpeed.Paused)
					{
						tickManager.DoSingleTick();
						RimWorld.SoundDefOf.Clock_Stop.PlayOneShotOnCamera(null);
					}
				}
			}

			if (method_to_play_sound != null)
			{
				method_to_play_sound.GetValue();
			}

			return false;
		}
	}
}