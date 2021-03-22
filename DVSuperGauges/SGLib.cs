using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Cybex.DVSuperGauges
{
	public class SGLib
	{
		public SGLib ()
		{
			if (Instance != null) Instance = null;
			Instance = this;

			ExportVanillaTextures();
			StoreVanillaTextures();

			tex_loco_621 = new TextureSet();
			TryLoadTextures(LocoShunter, "shunt_gauges_01", TexPath_LocoShunter + Main.settings.currentLocoShunterDirName, 1024, 1024);

			tex_loco_steam_H = new TextureSet();
			TryLoadTextures(LocoSteamHeavy, "SH_gauges_01", TexPath_LocoSteamHeavy + Main.settings.currentLocoSteamerDirName, 1024, 1024);
			tex_loco_steam_H_WaterLevel = new TextureSet();
			TryLoadTextures(LocoSteamHeavy_WaterLevel, "waterlevel_01", TexPath_LocoSteamHeavy + Main.settings.currentLocoSteamerDirName, 8, 128);

			tex_LocoDiesel = new TextureSet();
			TryLoadTextures(LocoDiesel, "LocoDiesel_gauges_01", TexPath_LocoDiesel + Main.settings.currentLocoDieselDirName, 2048, 1024);
		}

		private TextureSet tex_loco_621, tex_loco_steam_H, tex_LocoDiesel;
		private TextureSet van_loco_621, van_loco_steam_H, van_LocoDiesel;
		private TextureSet tex_loco_steam_H_WaterLevel, van_loco_steam_H_WaterLevel;

		public static SGLib Instance { get; private set; }

		public static TextureSet Van_LocoShunter => Instance.van_loco_621;
		public static TextureSet Van_LocoSteamHeavy => Instance.van_loco_steam_H;
		public static TextureSet Van_LocoSteamHeavy_WaterLevel => Instance.van_loco_steam_H_WaterLevel;
		public static TextureSet Van_LocoDiesel => Instance.van_LocoDiesel;

		public static TextureSet LocoShunter => Instance.tex_loco_621;
		public static TextureSet LocoSteamHeavy => Instance.tex_loco_steam_H;
		public static TextureSet LocoSteamHeavy_WaterLevel => Instance.tex_loco_steam_H_WaterLevel;
		public static TextureSet LocoDiesel => Instance.tex_LocoDiesel;

		public static string ResPath => Main.ModPath + "Resources/";
		public static string TexPath => ResPath + "Textures/";

		public static string TexPath_LocoShunter => TexPath + "loco_621/";
		public static string TexPath_LocoSteamHeavy => TexPath + "loco_steam_H/";
		public static string TexPath_LocoDiesel => TexPath + "LocoDiesel/";

		public static void Destroy ()
		{
			SuperGauges.SetSuperGauges(true);
			Instance = null;
		}

		public static void ReloadShunterTextures ()
		{
			Instance.tex_loco_621 = new TextureSet();
			TryLoadTextures(LocoShunter, "shunt_gauges_01", TexPath_LocoShunter + Main.settings.currentLocoShunterDirName, 1024, 1024);
		}

		public static void ReloadSteamerTextures ()
		{
			Instance.tex_loco_steam_H = new TextureSet();
			TryLoadTextures(LocoSteamHeavy, "SH_gauges_01", TexPath_LocoSteamHeavy + Main.settings.currentLocoSteamerDirName, 1024, 1024);
			Instance.tex_loco_steam_H_WaterLevel = new TextureSet();
			TryLoadTextures(LocoSteamHeavy_WaterLevel, "waterlevel_01", TexPath_LocoSteamHeavy + Main.settings.currentLocoSteamerDirName, 8, 128);
		}

		public static void ReloadDieselTextures ()
		{
			Instance.tex_LocoDiesel = new TextureSet();
			TryLoadTextures(LocoDiesel, "LocoDiesel_gauges_01", TexPath_LocoDiesel + Main.settings.currentLocoDieselDirName, 2048, 1024);
		}

		private static void TryLoadTextures (TextureSet target, string filePrefix, string path, int sizeX, int sizeY)
		{
			if (File.Exists(path + "/" + filePrefix + "d.png"))
			{
				if (target.d == null) target.d = new Texture2D(sizeX, sizeY);
				ImageConversion.LoadImage(target.d, File.ReadAllBytes(path + "/" + filePrefix + "d.png"));
			}
			if (File.Exists(path + "/" + filePrefix + "n.png"))
			{
				if (target.n == null) target.n = new Texture2D(sizeX, sizeY);
				ImageConversion.LoadImage(target.n, File.ReadAllBytes(path + "/" + filePrefix + "n.png"));
			}
			if (File.Exists(path + "/" + filePrefix + "s.png"))
			{
				if (target.s == null) target.s = new Texture2D(sizeX, sizeY);
				ImageConversion.LoadImage(target.s, File.ReadAllBytes(path + "/" + filePrefix + "s.png"));
			}
			if (File.Exists(path + "/" + filePrefix + "e.png"))
			{
				if (target.e == null) target.e = new Texture2D(sizeX, sizeY);
				ImageConversion.LoadImage(target.e, File.ReadAllBytes(path + "/" + filePrefix + "e.png"));
			}
		}

		private void ExportVanillaTextures ()
		{
			if (Directory.Exists(TexPath + "[Vanilla]")) return;
			var vanDir = Directory.CreateDirectory(TexPath + "[Vanilla]");

			string path;
			Material mat;
			// SHUNTER
			path = TexPath + "[Vanilla]/loco_621/";
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			mat = CarTypes.GetCarPrefab(TrainCarType.LocoShunter).GetComponent<TrainCar>().interiorPrefab.transform
				.Find("C dashboard buttons controller/I gauges backlights/lamp emmision indicator/gauge_stickers model").GetComponent<MeshRenderer>().sharedMaterial;
			ExportMaterialTextures(path, mat);
			// STEAMER
			path = TexPath + "[Vanilla]/loco_steam_H/";
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			mat = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab.transform
				.Find("Gauges").GetComponent<MeshRenderer>().sharedMaterial;
			ExportMaterialTextures(path, mat);
			mat = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab.transform
				.Find("I boiler water/water level").GetComponent<MeshRenderer>().sharedMaterial;
			ExportMaterialTextures(path, mat);
			// DIESEL
			path = TexPath + "[Vanilla]/LocoDiesel/";
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			mat = CarTypes.GetCarPrefab(TrainCarType.LocoDiesel).GetComponent<TrainCar>().interiorPrefab.transform
				.Find("offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().sharedMaterial;
			ExportMaterialTextures(path, mat);

			vanDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden | FileAttributes.ReadOnly;

		}

		private void ExportMaterialTextures (string path, Material mat)
		{
			Texture2D[] textures = new Texture2D[]
			{
				mat.GetTexture("_MainTex") as Texture2D,
				mat.GetTexture("_BumpMap") as Texture2D,
				mat.GetTexture("_MetallicGlossMap") as Texture2D,
				mat.GetTexture("_EmissionMap") as Texture2D
			};

			textures.Where(t => t != null).ToList().ForEach(t => ExportTexture(path, t));
		}

		private void ExportTexture (string path, Texture2D texture)
		{
			bool n = texture.name.ToCharArray().Last() == 'n';

			RenderTexture temp = n
				? RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
				: RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default);

			Graphics.Blit(texture, temp);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = temp;
			Texture2D readable = new Texture2D(texture.width, texture.height);
			readable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
			readable.Apply();

			if (n) readable.DTXnm2RGBA();

			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(temp);

			File.WriteAllBytes(path + texture.name + ".png", readable.EncodeToPNG());
		}

		private void StoreVanillaTextures ()
		{
			van_loco_621 = CreateDefaultTextureSet(TrainCarType.LocoShunter);
			van_loco_steam_H = CreateDefaultTextureSet(TrainCarType.LocoSteamHeavy);
			van_LocoDiesel = CreateDefaultTextureSet(TrainCarType.LocoDiesel);

			van_loco_steam_H_WaterLevel = new TextureSet();
			string path = TexPath + "[Vanilla]/loco_steam_H/";
			ImageConversion.LoadImage(van_loco_steam_H_WaterLevel.d = new Texture2D(1024, 1024), File.ReadAllBytes(path + "waterlevel_01d.png"));
			ImageConversion.LoadImage(van_loco_steam_H_WaterLevel.s = new Texture2D(1024, 1024), File.ReadAllBytes(path + "waterlevel_01s.png"));
		}

		private TextureSet CreateDefaultTextureSet (TrainCarType trainCarType)
		{
			var ts = new TextureSet();
			string path;

			switch (trainCarType)
			{
				case TrainCarType.LocoShunter:
					path = TexPath + "[Vanilla]/loco_621/";
					ImageConversion.LoadImage(ts.d = new Texture2D(1024, 1024), File.ReadAllBytes(path + "shunt_gauges_01d.png"));
					ImageConversion.LoadImage(ts.n = new Texture2D(1024, 1024), File.ReadAllBytes(path + "shunt_gauges_01n.png"));
					ImageConversion.LoadImage(ts.s = new Texture2D(1024, 1024), File.ReadAllBytes(path + "shunt_gauges_01s.png"));
					ImageConversion.LoadImage(ts.e = new Texture2D(1024, 1024), File.ReadAllBytes(path + "shunt_gauges_01e.png"));
					break;
				case TrainCarType.LocoSteamHeavy:
					path = TexPath + "[Vanilla]/loco_steam_H/";
					ImageConversion.LoadImage(ts.d = new Texture2D(1024, 1024), File.ReadAllBytes(path + "SH_gauges_01d.png"));
					break;
				case TrainCarType.LocoDiesel:
					path = TexPath + "[Vanilla]/LocoDiesel/";
					ImageConversion.LoadImage(ts.d = new Texture2D(2048, 1024), File.ReadAllBytes(path + "LocoDiesel_gauges_01d.png"));
					ImageConversion.LoadImage(ts.e = new Texture2D(2048, 1024), File.ReadAllBytes(path + "LocoDiesel_gauges_01e.png"));
					break;
				default: break;
			}

			return ts;
		}
	}

	public class TextureSet
	{
		public TextureSet () { }
		public TextureSet (Texture2D d, Texture2D n, Texture2D s, Texture2D e) { this.d = d; this.n = n; this.s = s; this.e = e; }

		public Texture2D d, n, s, e;
	}
}
