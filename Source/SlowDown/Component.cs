using System;
using Verse;
using RimWorld;
using UnityEngine;

namespace RD_SlowDown
{
	public class SlowDown : GameComponent
	{
		internal KeyBindingDef toggleKey;
		internal KeyBindingDef pauseKey;

		public SlowDown()
		{
			Log.Message("[RD_SlowDown] I never saw this message in testing, lucky you!");
			if (Settings.isHardcore)
			{
				Settings.Slowed = true;
			}
		}

		public SlowDown( Game game )
		{
			toggleKey = KeyBindingDef.Named("SlowDown");
			pauseKey = KeyBindingDefOf.TogglePause;
		}

		public override void GameComponentOnGUI()
		{
			try
			{
				if (Event.current.type == EventType.KeyDown)
				{
					if (toggleKey.KeyDownEvent)
					{
						if (Settings.isEnabled)
						{
							Settings.ToggleSlow();
							if (Find.TickManager.CurTimeSpeed != TimeSpeed.Paused)
							{
								Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
							}
						}
						Event.current.Use();
					}
					else if (pauseKey.KeyDownEvent)
					{
						if (Settings.isHardcore)
						{
							Settings.Slowed = true;
							TimeSpeed ts = Find.TickManager.CurTimeSpeed;
							ts++;
							if (((int)ts) > Settings.maxTimeSpeed)
							{
								ts = TimeSpeed.Normal;
							}
							Find.TickManager.CurTimeSpeed = ts;
							Event.current.Use();
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}
	}
}
