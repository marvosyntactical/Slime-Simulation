﻿using UnityEngine;
using UnityEngine.Experimental.Rendering;
using ComputeShaderUtility;

public class Simulation : MonoBehaviour
{
	public enum SpawnMode { Random, Point, InwardCircle, RandomCircle }

	const int updateKernel = 0;
	const int diffuseMapKernel = 1;

	public ComputeShader compute;
	public ComputeShader drawAgentsCS;

	public SlimeSettings settings;

	[Header("Display Settings")]
	public bool showAgentsOnly;
	public FilterMode filterMode = FilterMode.Point;
	public GraphicsFormat format = ComputeHelper.defaultGraphicsFormat;

	private float VerticalCameraSize; //= Camera.main.orthographicSize;
	private float HorizontalCameraSize; // = Camera.main.orthographicSize * Camera.main.aspect;


	[SerializeField, HideInInspector] protected RenderTexture trailMap;
	[SerializeField, HideInInspector] protected RenderTexture diffusedTrailMap;
	[SerializeField, HideInInspector] protected RenderTexture displayTexture;

	ComputeBuffer agentBuffer;
	ComputeBuffer settingsBuffer;
	Texture2D colourMapTexture;

	protected virtual void Start()
	{
		Init();
		transform.GetComponentInChildren<MeshRenderer>().material.mainTexture = displayTexture;

		VerticalCameraSize = Camera.main.orthographicSize;
		HorizontalCameraSize = Camera.main.orthographicSize * Camera.main.aspect;
	}

	void Init()
	{
		// Create render textures
		ComputeHelper.CreateRenderTexture(ref trailMap, settings.width, settings.height, filterMode, format);
		ComputeHelper.CreateRenderTexture(ref diffusedTrailMap, settings.width, settings.height, filterMode, format);
		ComputeHelper.CreateRenderTexture(ref displayTexture, settings.width, settings.height, filterMode, format);

		Texture2D background = null;

		if (settings.import_png) 
		{
			background = ComputeHelper.PNGToTexture2D(settings.png_path);
			ComputeHelper.CopyRenderTexture(background, trailMap);
		}

		// Create agents with initial positions and angles
		Agent[] agents = new Agent[settings.numAgents];
		for (int i = 0; i < agents.Length; i++)
		{
			Vector2 centre = new Vector2(settings.width / 2, settings.height / 2);
			Vector2 startPos = Vector2.zero;
			float randomAngle = Random.value * Mathf.PI * 2;
			float angle = 0;

			if (settings.spawnMode == SpawnMode.Point)
			{
				startPos = centre;
				angle = randomAngle;
			}
			else if (settings.spawnMode == SpawnMode.Random)
			{
				startPos = new Vector2(Random.Range(0, settings.width), Random.Range(0, settings.height));
				angle = randomAngle;
			}
			else if (settings.spawnMode == SpawnMode.InwardCircle)
			{
				startPos = centre + Random.insideUnitCircle * settings.height * 0.5f;
				angle = Mathf.Atan2((centre - startPos).normalized.y, (centre - startPos).normalized.x);
			}
			else if (settings.spawnMode == SpawnMode.RandomCircle)
			{
				startPos = centre + Random.insideUnitCircle * settings.height * 0.15f;
				angle = randomAngle;
			}

			Vector3Int speciesMask;
			int speciesIndex = 0;
			int numSpecies = settings.speciesSettings.Length;

			if (numSpecies == 1)
			{
				speciesMask = Vector3Int.one;
			}
			else
			{
				int species=1;
				if (settings.import_png & settings.png_is_slimes)
				{
					Color pixelcolor = background.GetPixel((int) startPos.x, (int) startPos.y);

					float max = pixelcolor.r;
					
					if (pixelcolor.g > max)
					{
						max = pixelcolor.g;
						species = 2;
					}
					if (pixelcolor.b > max)
					{
						species = 3;
					}
					
				} else {
					species = Random.Range(1, numSpecies + 1);
				}
				speciesIndex = species - 1;
				speciesMask = new Vector3Int((species == 1) ? 1 : 0, (species == 2) ? 1 : 0, (species == 3) ? 1 : 0);
			}



			agents[i] = new Agent() { position = startPos, angle = angle, speciesMask = speciesMask, speciesIndex = speciesIndex };
		}

		ComputeHelper.CreateAndSetBuffer<Agent>(ref agentBuffer, agents, compute, "agents", updateKernel);
		compute.SetInt("numAgents", settings.numAgents);
		drawAgentsCS.SetBuffer(0, "agents", agentBuffer);
		drawAgentsCS.SetInt("numAgents", settings.numAgents);


		compute.SetInt("width", settings.width);
		compute.SetInt("height", settings.height);

		compute.SetBool("mouseDown", false);
		compute.SetInt("mouseX", (int)settings.width/2);
		compute.SetInt("mouseY",(int)settings.height/2);


	}

	void OnMouseDown()
	{
		// compute.SetBool("mouseDown", true);
		if (Input.GetMouseButton(0))
		{
			compute.SetBool("mouseDown", true);
			Debug.Log("Mouse is down!");
		}
	}

	void OnMouseUp()
	{
		compute.SetBool("mouseDown", false);
		Debug.Log("Mouse is up!");
	}

	void FixedUpdate()
	{
		for (int i = 0; i < settings.stepsPerFrame; i++)
		{
			RunSimulation();
		}
	}

	void LateUpdate()
	{
		if (showAgentsOnly)
		{
			ComputeHelper.ClearRenderTexture(displayTexture);

			drawAgentsCS.SetTexture(0, "TargetTexture", displayTexture);
			ComputeHelper.Dispatch(drawAgentsCS, settings.numAgents, 1, 1, 0);

		}
		else
		{
			ComputeHelper.CopyRenderTexture(trailMap, displayTexture);
		}
	}

	void RunSimulation()
	{
		// adjust these 2 based on results on your screen
		// float dilation_vertical = (float) 1.04; // unity player
		// float dilation_horizontal = (float) 1.2; // unity player
		float dilation_vertical = (float) 1.0; // linux build
		float dilation_horizontal = (float) 1.0; // linux build

		// update mouse position
		Vector3 mousePos;
		mousePos = Input.mousePosition;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);

		float CameraMouseY = VerticalCameraSize + mousePos.y * dilation_vertical;
		float CameraMouseX = HorizontalCameraSize + mousePos.x * dilation_horizontal;

		float fractionUp = (CameraMouseY / (2*VerticalCameraSize));
		float fractionRight = (CameraMouseX / (2*HorizontalCameraSize));

        int mouseY = (int) (fractionUp * settings.height);
        int mouseX = (int) (fractionRight * settings.width);

		compute.SetInt("mouseX", mouseX);
		compute.SetInt("mouseY", mouseY);

		// update settings every frame
		var speciesSettings = settings.speciesSettings;
		ComputeHelper.CreateStructuredBuffer(ref settingsBuffer, speciesSettings);
		compute.SetBuffer(0, "speciesSettings", settingsBuffer);

		// Assign textures
		compute.SetTexture(updateKernel, "TrailMap", trailMap);
		compute.SetTexture(diffuseMapKernel, "TrailMap", trailMap);
		compute.SetTexture(diffuseMapKernel, "DiffusedTrailMap", diffusedTrailMap);

		// Assign settings
		compute.SetFloat("deltaTime", Time.fixedDeltaTime);
		compute.SetFloat("time", Time.fixedTime);

		compute.SetFloat("trailWeight", settings.trailWeight);
		compute.SetFloat("decayRate", settings.decayRate);
		compute.SetFloat("diffuseRate", settings.diffuseRate);

		ComputeHelper.Dispatch(compute, settings.numAgents, 1, 1, kernelIndex: updateKernel);
		ComputeHelper.Dispatch(compute, settings.width, settings.height, 1, kernelIndex: diffuseMapKernel);

		ComputeHelper.CopyRenderTexture(diffusedTrailMap, trailMap);
	}

	void OnDestroy()
	{
		ComputeHelper.Release(agentBuffer, settingsBuffer);
	}

	public struct Agent
	{
		public Vector2 position;
		public float angle;
		public Vector3Int speciesMask;
		int unusedSpeciesChannel;
		public int speciesIndex;
	}


}
