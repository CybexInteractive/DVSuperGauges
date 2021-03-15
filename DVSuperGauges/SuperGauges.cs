using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Cybex.DVSuperGauges
{
	public static class SuperGauges
	{
		public static void Init ()
		{
			_ = new SGLib();

			SetSuperGauges();
		}

		public static void Stop ()
		{
			SGLib.Destroy();
		}

		public static void SetSuperGauges (bool restoreDefaults = false)
		{
			if (restoreDefaults)
			{
				SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoShunter), true);
				SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy), true);
				SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoDiesel), true);
				return;
			}

			SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoShunter), Main.settings.LocoShunterTRUE);
			SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy), Main.settings.LocoSteamerHeavyTRUE);
			SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoDiesel), Main.settings.LocoDieselTRUE);
		}

		private static void SetSuperGauge (GameObject prefab, bool setToDefault = false)
		{
			var trainCar = prefab.GetComponent<TrainCar>();
			if (trainCar.carType != TrainCarType.LocoShunter && trainCar.carType != TrainCarType.LocoSteamHeavy && trainCar.carType != TrainCarType.LocoDiesel) return;

			var interiour = trainCar.interiorPrefab;
			Material mat = null;

			switch (trainCar.carType)
			{
				case TrainCarType.LocoShunter:
					mat = interiour.transform.Find("C dashboard buttons controller/I gauges backlights/lamp emmision indicator/gauge_stickers model").GetComponent<MeshRenderer>().sharedMaterial;
					mat.SetTexture("_MainTex", setToDefault ? SGLib.LocoShunter : SGLib.Default_LocoShunter);
					break;
				case TrainCarType.LocoSteamHeavy:
					mat = interiour.transform.Find("Gauges").GetComponent<MeshRenderer>().sharedMaterial;
					mat.SetTexture("_MainTex", setToDefault ? SGLib.LocoSteamHeavy : SGLib.Default_LocoSteamHeavy);
					break;
				case TrainCarType.LocoDiesel:
					mat = interiour.transform.Find("offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().sharedMaterial;
					mat.SetTexture("_MainTex", setToDefault ? SGLib.LocoDiesel : SGLib.Default_LocoDiesel);
					break;
				default:
					break;
			}
		}
	}

	public class SGLib
	{
		// loco_621
		// LocoDiesel
		// loco_steam_H

		public SGLib ()
		{
			if (Instance != null) Instance = null;
			Instance = this;

			StoreDefaultTextures();

			ImageConversion.LoadImage(Instance.tex_loco_621 = new Texture2D(1024, 1024), File.ReadAllBytes(Tex_loco_621 + "shunt_gauges_01d.png"));
			ImageConversion.LoadImage(Instance.tex_loco_steam_H = new Texture2D(1024, 1024), File.ReadAllBytes(Tex_loco_steam_H + "SH_gauges_01d.png"));
			ImageConversion.LoadImage(Instance.tex_LocoDiesel = new Texture2D(2048, 1024), File.ReadAllBytes(Tex_LocoDiesel + "LocoDiesel_gauges_01d.png"));
		}

		private TextureSet default_loco_621, default_loco_steam_H, default_LocoDiesel;

		private Texture2D tex_loco_621;
		private Texture2D tex_loco_steam_H;
		private Texture2D tex_LocoDiesel;

		public static SGLib Instance { get; private set; }

		public static Texture2D Default_LocoShunter => Instance.default_loco_621.texD;
		public static Texture2D Default_LocoSteamHeavy => Instance.default_loco_steam_H.texD;
		public static Texture2D Default_LocoDiesel => Instance.default_LocoDiesel.texD;

		public static Texture2D LocoShunter => Instance.tex_loco_621;
		public static Texture2D LocoSteamHeavy => Instance.tex_loco_steam_H;
		public static Texture2D LocoDiesel => Instance.tex_LocoDiesel;

		public static string ResPath => Main.ModPath + "Resources/";
		public static string TexPath => ResPath + "Textures/";

		private static string Tex_loco_621 => TexPath + "loco_621/";
		private static string Tex_loco_steam_H => TexPath + "loco_steam_H/";
		private static string Tex_LocoDiesel => TexPath + "LocoDiesel/";

		public static void Destroy ()
		{
			SuperGauges.SetSuperGauges(true);
			Instance = null;
		}

		private void StoreDefaultTextures ()
		{
			default_loco_621 = CreateDefaultTextureSet(CarTypes.GetCarPrefab(TrainCarType.LocoShunter));
			default_loco_steam_H = CreateDefaultTextureSet(CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy));
			default_LocoDiesel = CreateDefaultTextureSet(CarTypes.GetCarPrefab(TrainCarType.LocoDiesel));
		}

		private TextureSet CreateDefaultTextureSet (GameObject prefab)
		{
			var trainCar = prefab.GetComponent<TrainCar>();
			if (trainCar.carType != TrainCarType.LocoShunter && trainCar.carType != TrainCarType.LocoSteamHeavy && trainCar.carType != TrainCarType.LocoDiesel) return null;

			var interiour = trainCar.interiorPrefab;
			Material mat = null;

			switch (trainCar.carType)
			{
				case TrainCarType.LocoShunter:
					mat = interiour.transform.Find("C dashboard buttons controller/I gauges backlights/lamp emmision indicator/gauge_stickers model").GetComponent<MeshRenderer>().sharedMaterial;
					break;
				case TrainCarType.LocoSteamHeavy:
					mat = interiour.transform.Find("Gauges").GetComponent<MeshRenderer>().sharedMaterial;
					break;
				case TrainCarType.LocoDiesel:
					mat = interiour.transform.Find("offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().sharedMaterial;
					break;
				default:
					break;
			}

			if (!mat)
			{
				Main.Log("Could not reate default texture set.");
				return null;
			}

			return new TextureSet(
				mat.GetTexture("_MainTex") as Texture2D,
				mat.GetTexture("_BumpMap") as Texture2D,
				mat.GetTexture("_MetallicGlossMap") as Texture2D,
				mat.GetTexture("_EmissionMap") as Texture2D
				);
		}
	}

	public class TextureSet
	{
		public TextureSet (Texture2D d, Texture2D n, Texture2D s, Texture2D e)
		{ texD = d; texN = n; texS = s; texE = e; }

		public Texture2D texD, texN, texS, texE;
	}
}
