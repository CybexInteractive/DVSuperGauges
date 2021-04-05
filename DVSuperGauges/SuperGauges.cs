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

		private static List<string> steamerCabLightsOn;

		#endregion

		#region Start/Stop

		public static void Init ()
		{
			_ = new SGLib();

			manager = new GameObject().AddComponent<SGManager>();
			PlayerManager.CarChanged += CarChangeCheck;

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

			switch (trainCar.carType)
			{
				case TrainCarType.LocoShunter:
					SGLib.ReloadShunterTextures();
					if (PlayerManager.LastLoco != null)
					{
						if (PlayerManager.LastLoco.carType == TrainCarType.LocoShunter)
						{
							if (!restoreDefaults) CarChangeCheck(PlayerManager.LastLoco);
							if (restoreDefaults) RestoreShunterDefaults(PlayerManager.LastLoco);
						}
					}
					break;

				case TrainCarType.LocoSteamHeavy:
					SGLib.ReloadSteamerTextures();
					if (PlayerManager.LastLoco != null)
					{
						if (PlayerManager.LastLoco.carType == TrainCarType.LocoSteamHeavy)
						{
							if (!restoreDefaults) CarChangeCheck(PlayerManager.LastLoco);
							if (restoreDefaults) RestoreSteamerDefaults(PlayerManager.LastLoco);
						}
					}
					break;

				case TrainCarType.LocoDiesel:
					SGLib.ReloadDieselTextures();
					if (PlayerManager.LastLoco != null)
					{
						if (PlayerManager.LastLoco.carType == TrainCarType.LocoDiesel)
						{
							if (!restoreDefaults) CarChangeCheck(PlayerManager.LastLoco);
							if (restoreDefaults) RestoreDieselDefaults(PlayerManager.LastLoco);
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
						FixShunterGaugeAngles(trainCar);

						var cib = trainCar.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>();
						var args = new ValueChangedEventArgs(0, cib.Value);

						SetShunterLitState(args);

						if (Main.settings.LocoShunterTRUE)
						{
							// lightswitch event sub
							cib.ValueChanged -= SetShunterLitState;
							cib.ValueChanged += SetShunterLitState;

							// set gauges material
							trainCar.interior.Find($"loco_621_interior(Clone)/C dashboard buttons controller/I gauges backlights/lamp emmision indicator/gauge_stickers model")
								.GetComponent<MeshRenderer>().material = SGLib.Mat_LocoShunter_Gauge;

							// set needles material
							SHUNTER_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
								trainCar.interior.Find($"loco_621_interior(Clone)/{t}").GetComponent<MeshRenderer>().material = SGLib.Mat_LocoShunter_Gauge);
						}
					}));
			}
			if (trainCar.carType == TrainCarType.LocoSteamHeavy)
			{
				manager.StartCoroutine(manager.ExecuteAfterInteriorLoaded(
					() =>
					{
						FixSteamerGaugeAngles(trainCar);

						if (steamerCabLightsOn.Contains(trainCar.ID))
							trainCar.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>().SetValue(1);

						var cib = trainCar.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>();
						var args = new ValueChangedEventArgs(0, cib.Value);

						SetSteamerLitState(args);

						if (Main.settings.LocoSteamerHeavyTRUE)
						{
							// lightswitch event sub
							cib.ValueChanged -= SetSteamerLitState;
							cib.ValueChanged += SetSteamerLitState;
							cib.ValueChanged -= RegisterSteamerCabLight;
							cib.ValueChanged += RegisterSteamerCabLight;

							// create and set waterlevel backlight material
							if (trainCar.interior.Find("water level backlight") == null)
								CreateSteamerWaterLevelBacklight(trainCar.interior.gameObject);
							trainCar.interior.Find("water level backlight").GetComponent<MeshRenderer>().material = SGLib.Mat_LocoSteam_Gauge;

							// set gauges material
							trainCar.interior.Find($"loco_steam_H_interior(Clone)/Gauges").GetComponent<MeshRenderer>().material = SGLib.Mat_LocoSteam_Gauge;

							// set needles material
							STEAMER_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
								trainCar.interior.Find($"loco_steam_H_interior(Clone)/{t}").GetComponent<MeshRenderer>().material = SGLib.Mat_LocoSteam_Gauge);

							// set waterlevel material
							trainCar.interior.Find("loco_steam_H_interior(Clone)/I boiler water/water level").GetComponent<MeshRenderer>().material = SGLib.Mat_LocoSteam_WaterLevel;
						}
					}));
			}
			if (trainCar.carType == TrainCarType.LocoDiesel)
			{
				manager.StartCoroutine(manager.ExecuteAfterInteriorLoaded(
					() =>
					{
						FixDieselGaugeAngles(trainCar);

						var cib = trainCar.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>();
						var args = new ValueChangedEventArgs(0, cib.Value);

						SetDieselLitState(args);

						if (Main.settings.LocoDieselTRUE)
						{
							// lightswitch event sub
							cib.ValueChanged -= SetDieselLitState;
							cib.ValueChanged += SetDieselLitState;

							// set gauges material
							trainCar.interior.Find($"LocoDiesel_interior(Clone)/offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels")
								.GetComponent<MeshRenderer>().material = SGLib.Mat_LocoDiesel_Gauge;

							// set needles material
							DIESEL_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
								trainCar.interior.Find($"LocoDiesel_interior(Clone)/{t}").GetComponent<MeshRenderer>().material = SGLib.Mat_LocoDiesel_Gauge);
						}
					}));
			}
		}

		#endregion

		#region Steamer Specific

		private static void FixSteamerGaugeAngles (TrainCar trainCar)
		{
			var airBrake = UnityModManagerNet.UnityModManager.FindMod("AirBrake");
			if (airBrake != null && airBrake.Active)
				return;

			var interior = trainCar.interior;
			var ig = interior.transform.Find("loco_steam_H_interior(Clone)/I brake needle pipe").GetComponent<IndicatorGauge>();
			ig.minAngle = -316;
			ig.maxAngle = 46;
			ig = interior.transform.Find("loco_steam_H_interior(Clone)/I brake needle aux").GetComponent<IndicatorGauge>();
			ig.minAngle = -318;
			ig.maxAngle = 45;
			ig = interior.transform.Find("loco_steam_H_interior(Clone)/I speedometer").GetComponent<IndicatorGauge>();
			ig.minAngle = -223; // -221
			ig.maxAngle = 45; // 49
			ig = interior.transform.Find("loco_steam_H_interior(Clone)/I pressure meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -226; // -221
			ig.maxAngle = 135; // 139
		}

		private static void SetSteamerLitState (ValueChangedEventArgs e)
		{
			SGLib.Mat_LocoSteam_Gauge.SetColor("_EmissionColor", e.newValue > 0 ? Color.white : Color.black);
			SGLib.Mat_LocoSteam_WaterLevel.SetColor("_EmissionColor", e.newValue > 0 ? Color.white : Color.black);
		}

		private static void CreateSteamerWaterLevelBacklight (GameObject targetObject = null)
		{
			GameObject level = targetObject.transform.Find("loco_steam_H_interior(Clone)/I boiler water").gameObject;

			var go = new GameObject("water level backlight", typeof(MeshFilter), typeof(MeshRenderer));

			var mesh = new Mesh();
			mesh.vertices = new Vector3[]
			{
				new Vector3(-0.011f,  0.000f,  0.030f),
				new Vector3( 0.011f,  0.000f,  0.030f),
				new Vector3(-0.011f,  0.000f,  0.211f),
				new Vector3( 0.011f,  0.000f,  0.211f)
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
			// assign material outside
			go.transform.SetParent(targetObject.transform);
			go.transform.position = level.transform.position;
			go.transform.rotation = level.transform.rotation;
			go.transform.localScale = Vector3.one;
		}

		private static void RestoreSteamerDefaults (TrainCar trainCar)
		{
			DestroySteamerWaterLevelBacklight();

			if (!trainCar.IsInteriorLoaded) return;

			var prefab = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab.transform;
			var gaugeMat = prefab.Find("Gauges").GetComponent<MeshRenderer>().material;

			// set gauges material
			trainCar.interior.Find($"loco_steam_H_interior(Clone)/Gauges").GetComponent<MeshRenderer>().material = gaugeMat;
			// set needles material
			STEAMER_NEEDLE_TRANSFORMS.ToList().ForEach(t => trainCar.interior.Find($"loco_steam_H_interior(Clone)/{t}").GetComponent<MeshRenderer>().material = gaugeMat);
			// set waterlevels material
			trainCar.interior.Find("loco_steam_H_interior(Clone)/I boiler water/water level").GetComponent<MeshRenderer>().material = 
				prefab.Find("I boiler water/water level").GetComponent<MeshRenderer>().material;
		}

		private static void DestroySteamerWaterLevelBacklight ()
		{
			if (PlayerManager.LastLoco != null)
				if (PlayerManager.LastLoco.carType == TrainCarType.LocoSteamHeavy && PlayerManager.LastLoco.IsInteriorLoaded)
					if (PlayerManager.LastLoco.interior != null)
						if (PlayerManager.LastLoco.interior.Find("water level backlight") != null)
							GameObject.Destroy(PlayerManager.LastLoco.interior.Find("water level backlight").gameObject);
		}

		private static void RegisterSteamerCabLight (ValueChangedEventArgs e)
		{
			if (e.newValue > 0)
			{
				steamerCabLightsOn.Add(PlayerManager.LastLoco.GetComponent<TrainCar>().ID);
				steamerCabLightsOn = steamerCabLightsOn.Distinct().ToList();
				return;
			}
			steamerCabLightsOn.Remove(PlayerManager.LastLoco.GetComponent<TrainCar>().ID);
		}

		#endregion

		#region Shunter Specifica

		private static void FixShunterGaugeAngles (TrainCar trainCar)
		{
			var airBrake = UnityModManagerNet.UnityModManager.FindMod("AirBrake");
			if (airBrake != null && airBrake.Active)
				return;

			var interiorPrefab = trainCar.interior;
			var ig = interiorPrefab.transform.Find("loco_621_interior(Clone)/I brake_pipe_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -277;
			ig = interiorPrefab.transform.Find("loco_621_interior(Clone)/I brake_aux_res_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -278;
		}

		private static void SetShunterLitState (ValueChangedEventArgs e)
		{
			SGLib.Mat_LocoShunter_Gauge.SetColor("_EmissionColor", e.newValue > 0 ? Color.white : Color.black);
		}

		private static void RestoreShunterDefaults (TrainCar trainCar)
		{
			var prefab = CarTypes.GetCarPrefab(TrainCarType.LocoShunter).GetComponent<TrainCar>().interiorPrefab.transform;
			var gaugeMat = prefab.Find("C dashboard buttons controller/I gauges backlights/lamp emmision indicator/gauge_stickers model").GetComponent<MeshRenderer>().material;
			gaugeMat.SetColor("_EmissionColor", trainCar.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().Value > 0 ? Color.white : Color.black);

			// set gauges material
			trainCar.interior.Find($"loco_621_interior(Clone)/C dashboard buttons controller/I gauges backlights/lamp emmision indicator/gauge_stickers model")
				.GetComponent<MeshRenderer>().material = gaugeMat;
			// set needles material
			SHUNTER_NEEDLE_TRANSFORMS.ToList().ForEach(t => trainCar.interior.Find($"loco_621_interior(Clone)/{t}").GetComponent<MeshRenderer>().material = gaugeMat);
		}

		#endregion

		#region Diesel Specific

		private static void FixDieselGaugeAngles (TrainCar trainCar)
		{
			var airBrake = UnityModManagerNet.UnityModManager.FindMod("AirBrake");
			if (airBrake != null && airBrake.Active)
				return;

			var interior = trainCar.interior;
			var ig = interior.transform.Find("LocoDiesel_interior(Clone)/offset/I Indicator meters/I ind_brake_res_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
			ig = interior.transform.Find("LocoDiesel_interior(Clone)/offset/I Indicator meters/I ind_brake_aux_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
			ig = interior.transform.Find("LocoDiesel_interior(Clone)/offset/I Indicator meters/I brake_aux_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
			ig = interior.transform.Find("LocoDiesel_interior(Clone)/offset/I Indicator meters/I brake_res_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
		}

		private static void SetDieselLitState (ValueChangedEventArgs e)
		{
			SGLib.Mat_LocoDiesel_Gauge.SetColor("_EmissionColor", e.newValue > 0 ? Color.white : Color.black);
		}

		private static void RestoreDieselDefaults (TrainCar trainCar)
		{
			if (!trainCar.IsInteriorLoaded) return;

			var prefab = CarTypes.GetCarPrefab(TrainCarType.LocoDiesel).GetComponent<TrainCar>().interiorPrefab.transform;
			var gaugeMat = prefab.Find("offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().material;
			gaugeMat.SetColor("_EmissionColor", trainCar.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().Value > 0 ? Color.white : Color.black);

			// set gauges material
			trainCar.interior.Find($"LocoDiesel_interior(Clone)/offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().material = gaugeMat;
			// set needles material
			DIESEL_NEEDLE_TRANSFORMS.ToList().ForEach(t => trainCar.interior.Find($"LocoDiesel_interior(Clone)/{t}").GetComponent<MeshRenderer>().material = gaugeMat);
		}

		#endregion
	}
}
