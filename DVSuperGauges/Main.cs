using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;
using UnityEngine;
using System.IO;

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

			modEntry.OnGUI = OnGUI;
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

		static Vector2 scrollShunter, scrollSteamer, scrollDiesel;
		static string[] dirsShunter, dirsSteamer, dirsDiesel;
		static string optionsMenu = "";
		static Texture2D thmb = new Texture2D(256, 256);
		static GUIStyle boxStyle;
		static Texture2D optionsIcon = new Texture2D(32, 32);
		static void OnGUI (UnityModManager.ModEntry modEntry)
		{
			if (boxStyle == null)
			{
				boxStyle = new GUIStyle(GUI.skin.box);
				boxStyle.normal.background = new Texture2D(1, 1);
				boxStyle.normal.background.SetPixel(0, 0, Color.gray);
				boxStyle.normal.background.Apply();
				boxStyle.margin = new RectOffset(4, 4, 4, 4);
			}

			if (SGLib.Instance == null)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("<b><color=yellow>Waiting for initialization</color></b>", new GUIStyle(GUI.skin.label) { richText = true }, GUILayout.Width(149));
				int x = DateTime.Now.Second % 3;
				GUILayout.Label(x == 0 ? "<b><color=yellow>.</color></b>" : x == 1 ? "<b><color=yellow>..</color></b>" : "<b><color=yellow>...</color></b>", new GUIStyle(GUI.skin.label) { richText = true });
				GUILayout.EndHorizontal();
				return;
			}

			GUILayout.BeginHorizontal();
			bool LocoShunterTRUE = GUILayout.Toggle(settings.LocoShunterTRUE, "", GUILayout.Width(20));
			if (settings.LocoShunterTRUE != LocoShunterTRUE)
			{
				settings.LocoShunterTRUE = LocoShunterTRUE;
				SuperGauges.SetSuperGauges(!LocoShunterTRUE, TrainCarType.LocoShunter);
			}
			GUILayout.Label("Shunter", GUILayout.Width(80));
			if (settings.LocoShunterTRUE)
				if (GUILayout.Button("<size=24>\u25BD</size>", new GUIStyle(GUI.skin.button) { richText = true, padding = new RectOffset(0, 0, -6, 0) }, GUILayout.Width(18), GUILayout.Height(18)))
					optionsMenu = optionsMenu == "" ? "shunter" : optionsMenu == "shunter" ? "" : "shunter";
			GUILayout.EndHorizontal();

			if (settings.LocoShunterTRUE && optionsMenu == "shunter")
			{
				scrollShunter = GUILayout.BeginScrollView(scrollShunter);
				GUILayout.BeginHorizontal();
				dirsShunter = Directory.GetDirectories(SGLib.TexPath_LocoShunter);
				foreach (var d in dirsShunter)
				{
					string n = Path.GetFileName(d);

					GUI.color = settings.currentLocoShunterDirName == n ? Color.green : Color.white;
					GUILayout.BeginVertical(boxStyle, GUILayout.Width(100));
					GUI.color = Color.white;

					if (GUILayout.Button("", GUIStyle.none, GUILayout.Width(100), GUILayout.Height(120)))
					{
						settings.currentLocoShunterDirName = Path.GetFileName(d);
						SGLib.ReloadShunterTextures();
						SuperGauges.SetSuperGauges();
						return;
					}

					var rect = GUILayoutUtility.GetLastRect();
					rect.height -= 20;

					if (File.Exists(d + "/thmb.jpg"))
					{
						ImageConversion.LoadImage(thmb, File.ReadAllBytes(d + "/thmb.jpg"));
						GUI.DrawTexture(rect, thmb);
					}

					rect.y += 100;
					rect.height -= 80;

					GUI.Label(rect, $"<b>{n}</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter, margin = new RectOffset(0, 0, 0, 0) });

					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
				GUILayout.EndScrollView();
			}


			GUILayout.BeginHorizontal();
			bool LocoSteamerHeavyTRUE = GUILayout.Toggle(settings.LocoSteamerHeavyTRUE, "", GUILayout.Width(20));
			if (settings.LocoSteamerHeavyTRUE != LocoSteamerHeavyTRUE)
			{
				settings.LocoSteamerHeavyTRUE = LocoSteamerHeavyTRUE;
				SuperGauges.SetSuperGauges(!LocoSteamerHeavyTRUE, TrainCarType.LocoSteamHeavy);
			}
			GUILayout.Label("Steamer", GUILayout.Width(80));
			if (settings.LocoSteamerHeavyTRUE)
				if (GUILayout.Button("<size=24>\u25BD</size>", new GUIStyle(GUI.skin.button) { richText = true, padding = new RectOffset(0, 0, -6, 0) }, GUILayout.Width(18), GUILayout.Height(18)))
					optionsMenu = optionsMenu == "" ? "steamer" : optionsMenu == "steamer" ? "" : "steamer";
			GUILayout.EndHorizontal();

			if (settings.LocoSteamerHeavyTRUE && optionsMenu == "steamer")
			{
				scrollSteamer = GUILayout.BeginScrollView(scrollSteamer);
				GUILayout.BeginHorizontal();
				dirsSteamer = Directory.GetDirectories(SGLib.TexPath_LocoSteamHeavy);
				foreach (var d in dirsSteamer)
				{
					string n = Path.GetFileName(d);

					GUI.color = settings.currentLocoSteamerDirName == n ? Color.green : Color.white;
					GUILayout.BeginVertical(boxStyle, GUILayout.Width(100));
					GUI.color = Color.white;

					if (GUILayout.Button("", GUIStyle.none, GUILayout.Width(100), GUILayout.Height(120)))
					{
						settings.currentLocoSteamerDirName = Path.GetFileName(d);
						SGLib.ReloadSteamerTextures();
						SuperGauges.SetSuperGauges();
						return;
					}

					var rect = GUILayoutUtility.GetLastRect();
					rect.height -= 20;

					if (File.Exists(d + "/thmb.jpg"))
					{
						ImageConversion.LoadImage(thmb, File.ReadAllBytes(d + "/thmb.jpg"));
						GUI.DrawTexture(rect, thmb);
					}

					rect.y += 100;
					rect.height -= 80;

					GUI.Label(rect, $"<b>{n}</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter, margin = new RectOffset(0, 0, 0, 0) });

					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
				GUILayout.EndScrollView();
			}


			GUILayout.BeginHorizontal();
			bool LocoDieselTRUE = GUILayout.Toggle(settings.LocoDieselTRUE, "", GUILayout.Width(20));
			if (settings.LocoDieselTRUE != LocoDieselTRUE)
			{
				settings.LocoDieselTRUE = LocoDieselTRUE;
				SuperGauges.SetSuperGauges(!LocoDieselTRUE, TrainCarType.LocoDiesel);
			}
			GUILayout.Label("Diesel", GUILayout.Width(80));
			if (settings.LocoDieselTRUE)
				if (GUILayout.Button("<size=24>\u25BD</size>", new GUIStyle(GUI.skin.button) { richText = true, padding = new RectOffset(0, 0, -6, 0) }, GUILayout.Width(18), GUILayout.Height(18)))
					optionsMenu = optionsMenu == "" ? "diesel" : optionsMenu == "diesel" ? "" : "diesel";
			GUILayout.EndHorizontal();

			if (settings.LocoDieselTRUE && optionsMenu == "diesel")
			{
				scrollDiesel = GUILayout.BeginScrollView(scrollDiesel);
				GUILayout.BeginHorizontal();
				dirsDiesel = Directory.GetDirectories(SGLib.TexPath_LocoDiesel);
				foreach (var d in dirsDiesel)
				{
					string n = Path.GetFileName(d);

					GUI.color = settings.currentLocoDieselDirName == n ? Color.green : Color.white;
					GUILayout.BeginVertical(boxStyle, GUILayout.Width(100));
					GUI.color = Color.white;

					if (GUILayout.Button("", GUIStyle.none, GUILayout.Width(100), GUILayout.Height(120)))
					{
						settings.currentLocoDieselDirName = Path.GetFileName(d);
						SGLib.ReloadDieselTextures();
						SuperGauges.SetSuperGauges();
						return;
					}

					var rect = GUILayoutUtility.GetLastRect();
					rect.height -= 20;

					if (File.Exists(d + "/thmb.jpg"))
					{
						ImageConversion.LoadImage(thmb, File.ReadAllBytes(d + "/thmb.jpg"));
						GUI.DrawTexture(rect, thmb);
					}

					rect.y += 100;
					rect.height -= 80;

					GUI.Label(rect, $"<b>{n}</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter, margin = new RectOffset(0, 0, 0, 0) });

					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
				GUILayout.EndScrollView();
			}
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
			public bool LocoShunterTRUE;
			public bool LocoSteamerHeavyTRUE;
			public bool LocoDieselTRUE;

			public string currentLocoShunterDirName;
			public string currentLocoSteamerDirName;
			public string currentLocoDieselDirName;

			override public void Save (UnityModManager.ModEntry entry) { Save<Settings>(this, entry); }

			public void OnChange () { }
		}
	}
}
