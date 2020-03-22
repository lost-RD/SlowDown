using System;
using Verse;
using RimWorld;
using UnityEngine;

namespace RD_SlowDown
{
	public class SlowDown : GameComponent
	{
		internal KeyBindingDef toggleKey;

		public SlowDown()
		{
			Log.Message("[RD_SlowDown] I never saw this message in testing, lucky you!");
		}

		public SlowDown( Game game )
		{
			toggleKey = KeyBindingDef.Named("SlowDown");
		}

		public override void GameComponentOnGUI()
		{
			try
			{
				if (Event.current.type == EventType.KeyDown)
				{
					if (toggleKey.KeyDownEvent)
					{
						Settings.Toggle();
						Event.current.Use();
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
