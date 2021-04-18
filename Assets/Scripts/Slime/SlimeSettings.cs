using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SlimeSettings : ScriptableObject
{
	[Header("Simulation Settings")]
	[Min(1)] public int stepsPerFrame = 1;
	public int width = 1280;
	public int height = 720;
	public int numAgents = 100;
	public Simulation.SpawnMode spawnMode;


	[Header("Background Settings")]
	public float red_desirability = 0;
	public float green_desirability = 0;
	public float blue_desirability = 1;

	public bool import_png = false;
	public bool png_is_slimes = false;
	public string png_path = "/home/silversurfer42/fun/Slime-Simulation/Images/diamond.jpg";

	[Header("Trail Settings")]
	public float trailWeight = 1;
	public float decayRate = 1;
	public float diffuseRate = 1;

	public SpeciesSettings[] speciesSettings;

	[System.Serializable]
	public struct SpeciesSettings
	{
		[Header("Movement Settings")]
		public float moveSpeed;
		public float turnSpeed;

		[Header("Sensor Settings")]
		public float sensorAngleSpacing;
		public float sensorOffsetDst;
		[Min(1)] public int sensorSize;
	}
}
