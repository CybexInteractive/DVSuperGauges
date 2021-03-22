using DV.CabControls;
using System;
using System.Collections;
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
		#region Constants

		public static readonly string[] SHUNTER_NEEDLE_TRANSFORMS =
		{
			"I speedometer/speedometer_needle pivot/speedometer_needle model",
			"I brake_pipe_meter/brake_pipe needle05 pivot/brake_pipe needle05 model",
			"I brake_aux_res_meter/brake_aux_res needle04 pivot/needle04",
			"C dashboard indicators controller/I engine_temp_meter/engine_temp_needle pivot/engine_temp_needle model",
			"C dashboard indicators controller/I sand_meter/sand needle03 pivot/sand needle03 model",
			"C dashboard indicators controller/I fuel_meter/fuel needle01 pivot/fuel needle01 model",
			"C dashboard indicators controller/I oil_meter/oil needle02 pivot/oil needle02 model"
		};

		public static readonly string[] STEAMER_NEEDLE_TRANSFORMS =
		{
			"I sand meter/sand meter pivot/Needle Sand",
			"I brake needle pipe/brake needle pipe pivot/Needle Brake01",
			"I speedometer/speedometer pivot/Needle Speedometer",
			"I pressure meter/pressure meter pivot/Needle Pressuremeter",
			"I temperature meter/temperature meter pivot/Needle Temperature",
			"I brake needle aux/brake needle aux pivot/Needle Brake02",
			"I cutoff needle/Needle Cutoff"
		};

		public static readonly string[] DIESEL_NEEDLE_TRANSFORMS =
		{
			"offset/I Indicator meters/I ind_brake_res_meter/ind_brake_res needle pivot/needle_ind_brake_res",
			"offset/I Indicator meters/I speedometer/speedometer needle pivot/needle_speedometer",
			"offset/I Indicator meters/I brake_aux_meter/brake_aux needle pivot/needle_brake_aux",
			"offset/I Indicator meters/I fuel_meter/fuel needle pivot/needle_fuel",
			"offset/I Indicator meters/I oil_meter/oil needle pivot/needle_oil",
			"offset/I Indicator meters/I temperature_meter/temperature needle pivot/needle_temperature",
			"offset/I Indicator meters/I voltage_meter/voltage needle pivot/needle_voltage",
			"offset/I Indicator meters/I rpm_meter/rpm needle pivot/needle_rpm",
			"offset/I Indicator meters/I brake_res_meter/brake_res needle pivot/needle_brake_res",
			"offset/I Indicator meters/I sand_meter/sand needle pivot/needle_sand",
			"offset/I Indicator meters/I ind_brake_aux_meter/ind_brake_aux needle pivot/needle_ind_brake_aux"
		};

		#endregion

		#region public Fields

		public static SGManager manager;

		#endregion

		#region Private Fields

		private static Material steamerGaugesMat;
		private static Material steamerWaterLevelMat;

		private static List<string> steamerCabLightsOn;

		#endregion

		#region Start/Stop

		public static void Init ()
		{
			_ = new SGLib();

			manager = new GameObject().AddComponent<SGManager>();
			PlayerManager.CarChanged += CarChangeCheck;

			FixDefaultGaugeAngles();
			steamerGaugesMat = ReplaceSteamerGaugesMaterials();
			steamerWaterLevelMat = ReplaceSteamerWaterLevelMaterial();

			steamerCabLightsOn = new List<string>();

			SetSuperGauges();
		}

		public static void Stop ()
		{
			manager.Destroy();
			PlayerManager.CarChanged -= CarChangeCheck;
			SGLib.Destroy();

			DestroySteamerWaterLevelBacklight();
		}

		private static void FixDefaultGaugeAngles ()
		{
			FixShunterGaugeAngles();
			FixSteamerGaugeAngles();
			FixDieselGaugeAngles();
		}

		#endregion

		#region Super Gauge

		public static void SetSuperGauges (bool restoreDefaults = false, params TrainCarType[] types)
		{
			if (types == null || types.Length == 0) types = new TrainCarType[] { TrainCarType.LocoShunter, TrainCarType.LocoSteamHeavy, TrainCarType.LocoDiesel };

			if (restoreDefaults)
			{
				if (types.Contains(TrainCarType.LocoShunter)) SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoShunter), true);
				if (types.Contains(TrainCarType.LocoSteamHeavy)) SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy), true);
				if (types.Contains(TrainCarType.LocoDiesel)) SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoDiesel), true);
				return;
			}

			if (types.Contains(TrainCarType.LocoShunter)) SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoShunter), !Main.settings.LocoShunterTRUE);
			if (types.Contains(TrainCarType.LocoSteamHeavy)) SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy), !Main.settings.LocoSteamerHeavyTRUE);
			if (types.Contains(TrainCarType.LocoDiesel)) SetSuperGauge(CarTypes.GetCarPrefab(TrainCarType.LocoDiesel), !Main.settings.LocoDieselTRUE);
		}

		private static void SetSuperGauge (GameObject prefab, bool restoreDefaults = false)
		{
			var trainCar = prefab.GetComponent<TrainCar>();
			if (trainCar.carType != TrainCarType.LocoShunter && trainCar.carType != TrainCarType.LocoSteamHeavy && trainCar.carType != TrainCarType.LocoDiesel) return;

			var interior = trainCar.interiorPrefab;
			Material mat = null;

			switch (trainCar.carType)
			{
				case TrainCarType.LocoShunter:
					mat = interior.transform.Find("C dashboard buttons controller/I gauges backlights/lamp emmision indicator/gauge_stickers model").GetComponent<MeshRenderer>().sharedMaterial;
					SetMaterialTextures(mat, SGLib.LocoShunter, SGLib.Van_LocoShunter, restoreDefaults);

					if (PlayerManager.LastLoco != null)
					{
						if (PlayerManager.LastLoco.carType == TrainCarType.LocoShunter && PlayerManager.LastLoco.IsInteriorLoaded)
						{
							mat = PlayerManager.LastLoco.interior.Find("loco_621_interior(Clone)/C dashboard buttons controller/I gauges backlights/lamp emmision indicator/gauge_stickers model").GetComponent<MeshRenderer>().sharedMaterial;
							if (mat != null) SetMaterialTextures(mat, SGLib.LocoShunter, SGLib.Van_LocoShunter, restoreDefaults);

							if (!restoreDefaults)
							{
								SetShunterNeedleTextures();
								SetShunterNeedleLights(new ValueChangedEventArgs(0, PlayerManager.Car.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().Value));

								manager.StartCoroutine(manager.ExecuteAfterInteriorLoaded(
								() => PlayerManager.Car.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged -= SetShunterNeedleLights,
								() => PlayerManager.Car.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged += SetShunterNeedleLights
								));
							}

							if (restoreDefaults) SetshunterNeedlesToDefault();
						}
					}
					break;

				case TrainCarType.LocoSteamHeavy:
					mat = interior.transform.Find("Gauges").GetComponent<MeshRenderer>().sharedMaterial;
					SetMaterialTextures(mat, SGLib.LocoSteamHeavy, SGLib.Van_LocoSteamHeavy, restoreDefaults);
					mat = interior.transform.Find("I boiler water/water level").GetComponent<MeshRenderer>().sharedMaterial;
					SetMaterialTextures(mat, SGLib.LocoSteamHeavy_WaterLevel, SGLib.Van_LocoSteamHeavy_WaterLevel, restoreDefaults);

					if (PlayerManager.LastLoco != null)
					{
						if (PlayerManager.LastLoco.carType == TrainCarType.LocoSteamHeavy && PlayerManager.LastLoco.IsInteriorLoaded)
						{
							mat = PlayerManager.LastLoco.interior.Find("loco_steam_H_interior(Clone)/Gauges").GetComponent<MeshRenderer>().material;
							if (mat != null) SetMaterialTextures(mat, SGLib.LocoSteamHeavy, SGLib.Van_LocoSteamHeavy, restoreDefaults);

							mat = PlayerManager.LastLoco.interior.Find("loco_steam_H_interior(Clone)/I boiler water/water level").GetComponent<MeshRenderer>().material;
							if (mat != null) SetMaterialTextures(mat, SGLib.LocoSteamHeavy_WaterLevel, SGLib.Van_LocoSteamHeavy_WaterLevel, restoreDefaults);

							if (!restoreDefaults)
							{
								SetSteamerNeedleTextures();
								SetSteamerWaterLevelTexture();

								if (PlayerManager.LastLoco.interior.Find("water level backlight") == null)
									CreateSteamerWaterLevelBacklight(steamerGaugesMat, PlayerManager.LastLoco.interior.gameObject);

								var args = new ValueChangedEventArgs(0, PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>().Value);
								SetSteamerNeedleLights(args);
								SetSteamerGaugesBacklight(args);
								SetSteamerWaterLevelLight(args);

								var cib = PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>();
								cib.ValueChanged -= SetSteamerNeedleLights;
								cib.ValueChanged += SetSteamerNeedleLights;
								cib.ValueChanged -= SetSteamerGaugesBacklight;
								cib.ValueChanged += SetSteamerGaugesBacklight;
								cib.ValueChanged -= SetSteamerWaterLevelLight;
								cib.ValueChanged += SetSteamerWaterLevelLight;
								cib.ValueChanged -= RegisterSteamerCabLight;
								cib.ValueChanged += RegisterSteamerCabLight;
							}

							if (restoreDefaults)
							{
								SetSteamerNeedlesToDefault();
								DestroySteamerWaterLevelBacklight();

								SetSteamerNeedleLights(new ValueChangedEventArgs(0, 0));
								SetSteamerGaugesBacklight(new ValueChangedEventArgs(0, 0));

								ControlImplBase cib;
								try
								{
									cib = PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>();
									cib.ValueChanged -= SetSteamerNeedleLights;
									cib.ValueChanged -= SetSteamerGaugesBacklight;
									cib.ValueChanged -= SetSteamerWaterLevelLight;
								}
								catch { }

								try
								{
									cib = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab.transform.Find("C inidactor light switch").GetComponentInChildren<ControlImplBase>();
									cib.ValueChanged -= SetSteamerNeedleLights;
									cib.ValueChanged -= SetSteamerGaugesBacklight;
									cib.ValueChanged -= SetSteamerWaterLevelLight;
								}
								catch { }
							}
						}
					}

					break;

				case TrainCarType.LocoDiesel:
					mat = interior.transform.Find("offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().sharedMaterial;
					SetMaterialTextures(mat, SGLib.LocoDiesel, SGLib.Van_LocoDiesel, restoreDefaults);

					if (PlayerManager.LastLoco != null)
					{
						if (PlayerManager.LastLoco.carType == TrainCarType.LocoDiesel && PlayerManager.LastLoco.IsInteriorLoaded)
						{
							mat = PlayerManager.LastLoco.interior.Find("LocoDiesel_interior(Clone)/offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().sharedMaterial;
							if (mat != null) SetMaterialTextures(mat, SGLib.LocoDiesel, SGLib.Van_LocoDiesel, restoreDefaults);

							if (!restoreDefaults)
							{
								SetDieselNeedleTextures();
								SetDieselNeedleLights(new ValueChangedEventArgs(0, PlayerManager.Car.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().Value));

								manager.StartCoroutine(manager.ExecuteAfterInteriorLoaded(
								() => PlayerManager.Car.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged -= SetDieselNeedleLights,
								() => PlayerManager.Car.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged += SetDieselNeedleLights
								));
							}

							if (restoreDefaults) SetDieselNeedlesToDefault();
						}
					}
					break;

				default: Debug.LogWarning("Failed to set SuperGauges!"); break;
			}
		}

		private static void CarChangeCheck (TrainCar trainCar)
		{
			if (!trainCar || !trainCar.IsLoco) return;

			if (trainCar.carType == TrainCarType.LocoShunter)
			{
				manager.StartCoroutine(manager.ExecuteAfterInteriorLoaded(
					() =>
					{
						if (Main.settings.LocoShunterTRUE)
						{
							SetShunterNeedleLights(new ValueChangedEventArgs(0, trainCar.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().Value));
							trainCar.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged -= SetShunterNeedleLights;
							trainCar.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged += SetShunterNeedleLights;
						}
					}));
			}
			if (trainCar.carType == TrainCarType.LocoSteamHeavy)
			{
				manager.StartCoroutine(manager.ExecuteAfterInteriorLoaded(
					() =>
					{
						if (steamerCabLightsOn.Contains(trainCar.ID))
							trainCar.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>().SetValue(1);

						if (Main.settings.LocoSteamerHeavyTRUE)
						{
							if (trainCar.interior.Find("water level backlight") == null)
								CreateSteamerWaterLevelBacklight(steamerGaugesMat, trainCar.interior.gameObject);

							var args = new ValueChangedEventArgs(0, trainCar.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>().Value);
							SetSteamerNeedleLights(args);
							SetSteamerGaugesBacklight(args);
							SetSteamerWaterLevelLight(args);

							var cib = trainCar.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>();
							cib.ValueChanged -= SetSteamerNeedleLights;
							cib.ValueChanged += SetSteamerNeedleLights;
							cib.ValueChanged -= SetSteamerGaugesBacklight;
							cib.ValueChanged += SetSteamerGaugesBacklight;
							cib.ValueChanged -= SetSteamerWaterLevelLight;
							cib.ValueChanged += SetSteamerWaterLevelLight;
							cib.ValueChanged -= RegisterSteamerCabLight;
							cib.ValueChanged += RegisterSteamerCabLight;
						}
					}));
			}
			if (trainCar.carType == TrainCarType.LocoDiesel)
			{
				manager.StartCoroutine(manager.ExecuteAfterInteriorLoaded(
					() =>
					{
						if (Main.settings.LocoDieselTRUE)
						{
							SetDieselNeedleLights(new ValueChangedEventArgs(0, trainCar.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().Value));
							trainCar.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged -= SetDieselNeedleLights;
							trainCar.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged += SetDieselNeedleLights;
						}
					}));
			}
		}

		#endregion

		#region Steamer Specific

		private static void FixSteamerGaugeAngles ()
		{
			var interiorPrefab = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab;
			var ig = interiorPrefab.transform.Find("I brake needle pipe").GetComponent<IndicatorGauge>();
			ig.minAngle = -316;
			ig.maxAngle = 46;
			ig = interiorPrefab.transform.Find("I brake needle aux").GetComponent<IndicatorGauge>();
			ig.minAngle = -318;
			ig.maxAngle = 45;
			ig = interiorPrefab.transform.Find("I speedometer").GetComponent<IndicatorGauge>();
			ig.minAngle = -223; // -221
			ig.maxAngle = 45; // 49
			ig = interiorPrefab.transform.Find("I pressure meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -226; // -221
			ig.maxAngle = 135; // 139
		}

		private static Material ReplaceSteamerGaugesMaterials ()
		{
			var interiorPrefab = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab;
			var gaugeMaterial = new Material(CarTypes.GetCarPrefab(TrainCarType.LocoDiesel).GetComponent<TrainCar>().interiorPrefab.transform.Find("offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().sharedMaterial);
			interiorPrefab.transform.Find("Gauges").GetComponent<MeshRenderer>().sharedMaterial = gaugeMaterial;
			STEAMER_NEEDLE_TRANSFORMS.ToList().ForEach(t => interiorPrefab.transform.Find(t).GetComponent<MeshRenderer>().sharedMaterial = gaugeMaterial);

			return gaugeMaterial;
		}

		private static Material ReplaceSteamerWaterLevelMaterial ()
		{
			var interiorPrefab = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab;
			var waterLevelMaterial = new Material(CarTypes.GetCarPrefab(TrainCarType.LocoDiesel).GetComponent<TrainCar>().interiorPrefab.transform.Find("offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().sharedMaterial);
			interiorPrefab.transform.Find("I boiler water/water level").GetComponent<MeshRenderer>().material = waterLevelMaterial;

			return waterLevelMaterial;
		}

		private static void CreateSteamerWaterLevelBacklight (Material targetMaterial, GameObject targetObject = null)
		{
			GameObject level = targetObject.transform.Find("loco_steam_H_interior(Clone)/I boiler water/water level").gameObject;

			var go = new GameObject("water level backlight", typeof(MeshFilter), typeof(MeshRenderer));

			var mesh = new Mesh();
			mesh.vertices = new Vector3[]
			{
				new Vector3(-0.011f,  0.000f, -0.013f),
				new Vector3( 0.011f,  0.000f, -0.013f),
				new Vector3(-0.011f,  0.000f,  0.170f),
				new Vector3( 0.011f,  0.000f,  0.170f)
			};
			mesh.triangles = new int[]
			{
				0,2,1,
				1,2,3
			};
			mesh.uv = new Vector2[]
			{
				new Vector2(0.0f, 0.0f), // pixel 0, 0
				new Vector2(0.03902439f, 0.0f), // pixel 40, 0
				new Vector2(0.0f, 0.37073170f), // pixel 0, 100
				new Vector2(0.03902439f, 0.37073170f) // pixel 40, 100
			};

			go.GetComponent<MeshFilter>().mesh = mesh;
			go.GetComponent<MeshRenderer>().sharedMaterial = targetMaterial;
			go.transform.SetParent(targetObject.transform);
			go.transform.position = level.transform.position;
			go.transform.rotation = level.transform.rotation;
			go.transform.localScale = Vector3.one;
		}

		private static void DestroySteamerWaterLevelBacklight ()
		{
			if (PlayerManager.LastLoco != null)
				if (PlayerManager.LastLoco.carType == TrainCarType.LocoSteamHeavy && PlayerManager.LastLoco.IsInteriorLoaded)
					if (PlayerManager.LastLoco.interior != null)
						if (PlayerManager.LastLoco.interior.Find("water level backlight") != null)
							GameObject.Destroy(PlayerManager.LastLoco.interior.Find("water level backlight").gameObject);
		}

		private static void SetSteamerNeedleTextures (bool restoreDefaults = false)
		{
			STEAMER_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
			{
				var mat = PlayerManager.Car.interior.Find($"loco_steam_H_interior(Clone)/{t}").GetComponent<MeshRenderer>().material;
				SetMaterialTextures(mat, SGLib.LocoSteamHeavy, SGLib.Van_LocoSteamHeavy, restoreDefaults);
			});
		}

		private static void SetSteamerWaterLevelTexture (bool restoreDefaults = false)
		{
			var mat = PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/I boiler water/water level").GetComponent<MeshRenderer>().material;
			SetMaterialTextures(mat, SGLib.LocoSteamHeavy_WaterLevel, SGLib.Van_LocoSteamHeavy_WaterLevel, restoreDefaults);
		}

		private static void SetSteamerGaugesBacklight (ValueChangedEventArgs e)
		{
			PlayerManager.Car.interior.Find($"loco_steam_H_interior(Clone)/Gauges").GetComponent<MeshRenderer>().sharedMaterial.SetColor("_EmissionColor", e.newValue > 0 ? Color.white : Color.black);
		}

		private static void SetSteamerNeedleLights (ValueChangedEventArgs e)
		{
			STEAMER_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
				PlayerManager.Car.interior.Find($"loco_steam_H_interior(Clone)/{t}").GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", e.newValue > 0 ? Color.white : Color.black)
			);
		}

		private static void SetSteamerWaterLevelLight (ValueChangedEventArgs e)
		{
			PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/I boiler water/water level").GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", e.newValue > 0 ? Color.white : Color.black);
		}

		private static void RegisterSteamerCabLight (ValueChangedEventArgs e)
		{
			if (e.newValue > 0)
			{
				steamerCabLightsOn.Add(PlayerManager.LastLoco.GetComponent<TrainCar>().ID);
				steamerCabLightsOn.Distinct();
				return;
			}

			steamerCabLightsOn.Remove(PlayerManager.LastLoco.GetComponent<TrainCar>().ID);
		}

		private static void SetSteamerNeedlesToDefault ()
		{
			STEAMER_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
				PlayerManager.Car.interior.Find($"loco_steam_H_interior(Clone)/{t}").GetComponent<MeshRenderer>().sharedMaterial =
				PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/Gauges").GetComponent<MeshRenderer>().sharedMaterial
			);
		}

		#endregion

		#region Shunter Specific

		private static void FixShunterGaugeAngles ()
		{
			var interiorPrefab = CarTypes.GetCarPrefab(TrainCarType.LocoShunter).GetComponent<TrainCar>().interiorPrefab;
			var ig = interiorPrefab.transform.Find("I brake_pipe_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
			ig = interiorPrefab.transform.Find("I brake_aux_res_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
		}

		private static void SetShunterNeedleTextures (bool restoreDefaults = false)
		{
			SHUNTER_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
			{
				var mat = PlayerManager.Car.interior.Find($"loco_621_interior(Clone)/{t}").GetComponent<MeshRenderer>().material;
				SetMaterialTextures(mat, SGLib.LocoShunter, SGLib.Van_LocoShunter, restoreDefaults);
			});
		}

		private static void SetShunterNeedleLights (ValueChangedEventArgs e)
		{
			SHUNTER_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
				PlayerManager.Car.interior.Find($"loco_621_interior(Clone)/{t}").GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", e.newValue > 0 ? Color.white : Color.black)
			);
		}

		private static void SetshunterNeedlesToDefault ()
		{
			SHUNTER_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
				PlayerManager.Car.interior.Find($"loco_621_interior(Clone)/{t}").GetComponent<MeshRenderer>().sharedMaterial =
				PlayerManager.Car.interior.Find("loco_621_interior(Clone)/C dashboard buttons controller/I gauges backlights/lamp emmision indicator/gauge_stickers model").GetComponent<MeshRenderer>().sharedMaterial
			);
		}

		#endregion

		#region Diesel Specific

		private static void FixDieselGaugeAngles ()
		{
			var interiorPrefab = CarTypes.GetCarPrefab(TrainCarType.LocoDiesel).GetComponent<TrainCar>().interiorPrefab;
			var ig = interiorPrefab.transform.Find("offset/I Indicator meters/I ind_brake_res_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
			ig = interiorPrefab.transform.Find("offset/I Indicator meters/I ind_brake_aux_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
			ig = interiorPrefab.transform.Find("offset/I Indicator meters/I brake_aux_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
			ig = interiorPrefab.transform.Find("offset/I Indicator meters/I brake_res_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
		}

		private static void SetDieselNeedleTextures (bool restoreDefaults = false)
		{
			DIESEL_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
			{
				var mat = PlayerManager.Car.interior.Find($"LocoDiesel_interior(Clone)/{t}").GetComponent<MeshRenderer>().material;
				SetMaterialTextures(mat, SGLib.LocoDiesel, SGLib.Van_LocoDiesel, restoreDefaults);
			});
		}

		private static void SetDieselNeedleLights (ValueChangedEventArgs e)
		{
			DIESEL_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
				PlayerManager.Car.interior.Find($"LocoDiesel_interior(Clone)/{t}").GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", e.newValue > 0 ? Color.white : Color.black)
			);
		}

		private static void SetDieselNeedlesToDefault ()
		{
			DIESEL_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
				PlayerManager.Car.interior.Find($"LocoDiesel_interior(Clone)/{t}").GetComponent<MeshRenderer>().sharedMaterial =
				PlayerManager.Car.interior.Find("LocoDiesel_interior(Clone)/offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().sharedMaterial
			);
		} 

		#endregion

		#region Miscellaneous

		private static void SetMaterialTextures (Material mat, TextureSet textureSet, TextureSet defaultSet, bool restoreDefaults = false)
		{
			mat.SetTexture("_MainTex", restoreDefaults ? defaultSet.d : textureSet.d ?? defaultSet.d);
			mat.SetTexture("_BumpMap", restoreDefaults ? defaultSet.n : textureSet.n ?? defaultSet.n);
			mat.SetTexture("_MetallicGlossMap", restoreDefaults ? defaultSet.s : textureSet.s ?? defaultSet.s);
			mat.SetTexture("_OcclusionMap", restoreDefaults ? defaultSet.s : textureSet.s ?? defaultSet.s);
			mat.SetTexture("_EmissionMap", restoreDefaults ? defaultSet.e : textureSet.e ?? defaultSet.e);
		} 

		#endregion
	}
}
