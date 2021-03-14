using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace Cybex.DVSuperGauges
{
#if DEBUG
	[EnableReloading]
#endif
    public class SuperGauges
    {
		public static bool enabled = true;
		public static UnityModManager.ModEntry mod;
		public static Settings settings = new Settings();

		public static bool Load (UnityModManager.ModEntry modEntry)
		{
			mod = modEntry;
			try { settings = Settings.Load<Settings>(modEntry); } catch { }

			modEntry.OnGUI = OnGui;
			modEntry.OnSaveGUI = OnSaveGui;
			modEntry.OnToggle = OnToggle;

#if DEBUG
			modEntry.OnUnload = Unload;
#endif

			

			return true;
		}

		static bool OnToggle (UnityModManager.ModEntry modEntry, bool value)
		{
			if (value != enabled) enabled = value;
			return true;
		}

		static void OnGui (UnityModManager.ModEntry modEntry)
		{
			settings.Draw(modEntry);
		}

		static void OnSaveGui (UnityModManager.ModEntry modEntry)
		{
			settings.Save(modEntry);
		}

#if DEBUG
		static bool Unload (UnityModManager.ModEntry modEntry)
		{
			var harmony = new Harmony(modEntry.Info.Id);
			harmony.UnpatchAll(modEntry.Info.Id);

			return true;
		}
#endif

		static void Log (string msg)
		{
			mod?.Logger.Log($"[DV Super Gauges] {msg}");
		}

		public class Settings : UnityModManager.ModSettings, IDrawable
		{
			//[Draw("Hardness ", DrawType.Slider, Min = 1, Max = 10, Precision = 1)] public float dmpMul = 4;

			override public void Save (UnityModManager.ModEntry entry) { Save<Settings>(this, entry); }

			public void OnChange () { }
		}
	}
}
