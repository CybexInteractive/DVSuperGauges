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
    public class Main
    {
		public static bool enabled = true;
		public static UnityModManager.ModEntry mod;
		public static Settings settings = new Settings();

		public static string ModPath => mod.Path;


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

			if (SaveLoadController.carsAndJobsLoadingFinished && WorldStreamingInit.IsLoaded) OnLoadingFinished();
			else WorldStreamingInit.LoadingFinished += OnLoadingFinished;

			return true;
		}

		static void OnLoadingFinished ()
		{
			WorldStreamingInit.LoadingFinished -= OnLoadingFinished;
			SuperGauges.Init();
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
			//var harmony = new Harmony(modEntry.Info.Id);
			//harmony.UnpatchAll(modEntry.Info.Id);

			SuperGauges.Stop();

			return true;
		}
#endif

		public static void Log (string msg)
		{
			mod?.Logger.Log($"[DV Super Gauges] {msg}");
		}

		public class Settings : UnityModManager.ModSettings, IDrawable
		{
			[Draw("Shunter", DrawType.Toggle)] public bool LocoShunterTRUE;
			[Draw("Steamer", DrawType.Toggle)] public bool LocoSteamerHeavyTRUE;
			[Draw("Diesel", DrawType.Toggle)] public bool LocoDieselTRUE;

			override public void Save (UnityModManager.ModEntry entry) { Save<Settings>(this, entry); }

			public void OnChange () { UnityEngine.Debug.LogWarning("Settings OnChange()"); SuperGauges.SetSuperGauges(); }
		}
	}
}
