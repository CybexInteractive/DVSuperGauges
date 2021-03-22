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

		#region Start/Stop

		public static void Init ()
		{
			_ = new SGLib();

			manager = new GameObject().AddComponent<SGManager>();
			PlayerManager.CarChanged += CarChangeCheck;

			FixDefaultGaugeAngles();
			ReplaceSteamerMaterials();

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

			var interiour = trainCar.interiorPrefab;
			Material mat = null;

			switch (trainCar.carType)
			{
				case TrainCarType.LocoShunter:
					mat = interiour.transform.Find("C dashboard buttons controller/I gauges backlights/lamp emmision indicator/gauge_stickers model").GetComponent<MeshRenderer>().sharedMaterial;
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
								() => SetShunterNeedleLights(new ValueChangedEventArgs(0, PlayerManager.Car.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().Value)),
								() => PlayerManager.Car.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged -= SetShunterNeedleLights,
								() => PlayerManager.Car.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged += SetShunterNeedleLights
								));
							}

							if (restoreDefaults) SetshunterNeedlesToDefault();
						}
					}
					break;

				case TrainCarType.LocoSteamHeavy:
					mat = interiour.transform.Find("Gauges").GetComponent<MeshRenderer>().sharedMaterial;
					SetMaterialTextures(mat, SGLib.LocoSteamHeavy, SGLib.Van_LocoSteamHeavy, restoreDefaults);

					if (PlayerManager.LastLoco != null)
					{
						if (PlayerManager.LastLoco.carType == TrainCarType.LocoSteamHeavy && PlayerManager.LastLoco.IsInteriorLoaded)
						{
							mat = PlayerManager.LastLoco.interior.Find("loco_steam_H_interior(Clone)/Gauges").GetComponent<MeshRenderer>().material;
							if (mat != null) SetMaterialTextures(mat, SGLib.LocoSteamHeavy, SGLib.Van_LocoSteamHeavy, restoreDefaults);

							if (!restoreDefaults)
							{
								SetSteamerNeedleTextures();
								SetSteamerNeedleLights(new ValueChangedEventArgs(0, PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>().Value));
								SetSteamerGaugesBacklight(new ValueChangedEventArgs(0, PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>().Value));

								manager.StartCoroutine(manager.ExecuteAfterInteriorLoaded(
								() =>
								{
									var args = new ValueChangedEventArgs(0, PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>().Value);
									SetSteamerNeedleLights(args);
									SetSteamerGaugesBacklight(args);
									var cib = PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>();
									cib.ValueChanged -= SetSteamerNeedleLights;
									cib.ValueChanged -= SetSteamerGaugesBacklight;
									cib.ValueChanged += SetSteamerNeedleLights;
									cib.ValueChanged += SetSteamerGaugesBacklight;
								}));
							}

							if (restoreDefaults)
							{
								SetSteamerNeedlesToDefault();
								DestroySteamerWaterLevelBacklight();
							}
						}
					}

					break;

				case TrainCarType.LocoDiesel:
					mat = interiour.transform.Find("offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().sharedMaterial;
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
								() => SetDieselNeedleLights(new ValueChangedEventArgs(0, PlayerManager.Car.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().Value)),
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

		#endregion

		#region Shunter Specific

		private static void FixShunterGaugeAngles ()
		{
			var ig = CarTypes.GetCarPrefab(TrainCarType.LocoShunter).GetComponent<TrainCar>().interiorPrefab.transform.Find("I brake_pipe_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
			ig = CarTypes.GetCarPrefab(TrainCarType.LocoShunter).GetComponent<TrainCar>().interiorPrefab.transform.Find("I brake_aux_res_meter").GetComponent<IndicatorGauge>();
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


		#region Steamer Specific

		// TODO:	update water backlight if in cabin (lit state change event not working)
		//			after skin change water level backlight doesn't update anymore, sub to event again!

		// TODO:	find solution to save steamer light switch state

		// TODO:	after skin set to default, disable emission completely (probably event unsub?)
		//			and remove water level backlight object if in cabin (cabin loaded)

		// TODO:	first skin activation load steamer water level backlight object in
		//			applies to loading game as well

		private static void FixSteamerGaugeAngles ()
		{
			// TODO: fix speedo, steam pressure Needle as well
			var ig = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab.transform.Find("I brake needle pipe").GetComponent<IndicatorGauge>();
			ig.minAngle = -316;
			ig.maxAngle = 46;
			ig = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab.transform.Find("I brake needle aux").GetComponent<IndicatorGauge>();
			ig.minAngle = -318;
			ig.maxAngle = 45;
		}

		private static void ReplaceSteamerMaterials ()
		{
			var interiorPrefab = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab;
			var targetMaterial = new Material(CarTypes.GetCarPrefab(TrainCarType.LocoDiesel).GetComponent<TrainCar>().interiorPrefab.transform.Find("offset/I Indicator lamps/I gauges backlights/lamp emission indicator/gauge_labels").GetComponent<MeshRenderer>().sharedMaterial);
			interiorPrefab.transform.Find("Gauges").GetComponent<MeshRenderer>().sharedMaterial = targetMaterial;
			STEAMER_NEEDLE_TRANSFORMS.ToList().ForEach(t => interiorPrefab.transform.Find(t).GetComponent<MeshRenderer>().sharedMaterial = targetMaterial);

			CreateSteamerWaterLevelBacklight(targetMaterial);
		}

		private static void CreateSteamerWaterLevelBacklight (Material targetMaterial)
		{
			var interior = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab;

			var level = interior.transform.Find("I boiler water/water level").gameObject;
			var go = new GameObject("water level backlight", typeof(MeshFilter), typeof(MeshRenderer));

			var mesh = new Mesh();
			mesh.vertices = new Vector3[]
			{
				new Vector3(-0.02f,  0.0f,  0.02f),
				new Vector3( 0.02f,  0.0f,  0.02f),
				new Vector3(-0.02f,  0.0f,  0.22f),
				new Vector3( 0.02f,  0.0f,  0.22f)
			};
			mesh.triangles = new int[]
			{
				0,2,1,
				1,2,3
			};
			mesh.uv = new Vector2[]
			{
				new Vector2(0.0f, 0.0f), // pixel 0, 0
				new Vector2(0.01953125f, 0.0f), // pixel 20, 0
				new Vector2(0.0f, 0.09765625f), // pixel 0, 100
				new Vector2(0.01953125f, 0.09765625f) // pixel 20, 100
			};

			go.GetComponent<MeshFilter>().mesh = mesh;
			go.GetComponent<MeshRenderer>().material = targetMaterial;
			go.transform.SetParent(interior.transform);
			go.transform.position = level.transform.position;
			go.transform.rotation = level.transform.rotation;
		}

		private static void DestroySteamerWaterLevelBacklight ()
		{
			Debug.LogWarning("DestroySteamerWaterLevelBacklight");
			var interior = CarTypes.GetCarPrefab(TrainCarType.LocoSteamHeavy).GetComponent<TrainCar>().interiorPrefab;
			if (interior.transform.Find("water level backlight")) GameObject.Destroy(interior.transform.Find("water level backlight").gameObject);

			if (PlayerManager.LastLoco != null)
				if (PlayerManager.LastLoco.carType == TrainCarType.LocoSteamHeavy && PlayerManager.LastLoco.IsInteriorLoaded)
					if (PlayerManager.LastLoco.interior.Find("loco_steam_H_interior(Clone)/water level backlight"))
						GameObject.Destroy(PlayerManager.LastLoco.transform.Find("loco_steam_H_interior(Clone)/water level backlight").gameObject);
		}

		private static void SetSteamerNeedleTextures (bool restoreDefaults = false)
		{
			STEAMER_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
			{
				var mat = PlayerManager.Car.interior.Find($"loco_steam_H_interior(Clone)/{t}").GetComponent<MeshRenderer>().material;
				SetMaterialTextures(mat, SGLib.LocoSteamHeavy, SGLib.Van_LocoSteamHeavy, restoreDefaults);
			});
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

		private static void SetSteamerNeedlesToDefault ()
		{
			STEAMER_NEEDLE_TRANSFORMS.ToList().ForEach(t =>
				PlayerManager.Car.interior.Find($"loco_steam_H_interior(Clone)/{t}").GetComponent<MeshRenderer>().sharedMaterial =
				PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/Gauges").GetComponent<MeshRenderer>().sharedMaterial
			);
		}

		#endregion



		#region Diesel Specific

		private static void FixDieselGaugeAngles ()
		{
			var ig = CarTypes.GetCarPrefab(TrainCarType.LocoDiesel).GetComponent<TrainCar>().interiorPrefab.transform.Find("offset/I Indicator meters/I ind_brake_res_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
			ig = CarTypes.GetCarPrefab(TrainCarType.LocoDiesel).GetComponent<TrainCar>().interiorPrefab.transform.Find("offset/I Indicator meters/I ind_brake_aux_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
			ig = CarTypes.GetCarPrefab(TrainCarType.LocoDiesel).GetComponent<TrainCar>().interiorPrefab.transform.Find("offset/I Indicator meters/I brake_aux_meter").GetComponent<IndicatorGauge>();
			ig.minAngle = -280;
			ig = CarTypes.GetCarPrefab(TrainCarType.LocoDiesel).GetComponent<TrainCar>().interiorPrefab.transform.Find("offset/I Indicator meters/I brake_res_meter").GetComponent<IndicatorGauge>();
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

		private static void CarChangeCheck (TrainCar trainCar)
		{
			if (!trainCar || !trainCar.IsLoco) return;

			if (trainCar.carType == TrainCarType.LocoShunter)
			{
				manager.StartCoroutine(manager.ExecuteAfterInteriorLoaded(
					() => SetShunterNeedleLights(new ValueChangedEventArgs(0, PlayerManager.Car.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().Value)),
					() => PlayerManager.Car.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged -= SetShunterNeedleLights,
					() => PlayerManager.Car.interior.GetComponentInChildren<ShunterDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged += SetShunterNeedleLights
					));
			}
			if (trainCar.carType == TrainCarType.LocoSteamHeavy)
			{
				manager.StartCoroutine(manager.ExecuteAfterInteriorLoaded(
					() =>
					{
						var args = new ValueChangedEventArgs(0, PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>().Value);
						SetSteamerNeedleLights(args);
						SetSteamerGaugesBacklight(args);
						var cib = PlayerManager.Car.interior.Find("loco_steam_H_interior(Clone)/C inidactor light switch").GetComponentInChildren<ControlImplBase>();
						cib.ValueChanged -= SetSteamerNeedleLights;
						cib.ValueChanged -= SetSteamerGaugesBacklight;
						cib.ValueChanged += SetSteamerNeedleLights;
						cib.ValueChanged += SetSteamerGaugesBacklight;
					}));
			}
			if (trainCar.carType == TrainCarType.LocoDiesel)
			{
				manager.StartCoroutine(manager.ExecuteAfterInteriorLoaded(
					() => SetDieselNeedleLights(new ValueChangedEventArgs(0, PlayerManager.Car.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().Value)),
					() => PlayerManager.Car.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged -= SetDieselNeedleLights,
					() => PlayerManager.Car.interior.GetComponentInChildren<DieselDashboardControls>().cabLightRotary.GetComponent<ControlImplBase>().ValueChanged += SetDieselNeedleLights
					));
			}
		}

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
