using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode] // If we don't have this line, the code runs only when we press play.
public class CustomTerrain : MonoBehaviour 
{
	public Vector2 randomHeightRange = new Vector2(0, 0.1f);
	public Texture2D heightMapImage;
	public Vector3 heightMapScale = new Vector3(1, 1, 1);

	public bool resetTerrain = true;

	// PERLIN NOISE -----------------------------------------------
	public float perlinXScale = 0.01f;
	public float perlinYScale = 0.01f;
	public int perlinOffsetX = 0;
	public int perlinOffsetY = 0;
	public int perlinOctaves = 3;
	public float perlinPersistance = 8; // increasing amplitude each time we loop through the octaves
	public float perlinHeightScale = 0.09f;

	// MULTIPLE PERLIN NOISE ---------------------------------------
	[System.Serializable]
	public class PerlinParameters
	{
		public float mPerlinXScale = 0.01f;
		public float mPerlinYScale = 0.01f;
		public int mPerlinOctaves = 3;
		public float mPerlinPersistance = 8;
		public float mPerlinHeightScale = 0.09f;
		public int mPerlinOffsetX = 0;
		public int mPerlinOffsetY = 0;
		public bool remove = false;
	}
	public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
	{
		new PerlinParameters()
	};

	public Terrain terrain;
	public TerrainData terrainData;

	float[,] GetHeightMap()
	{
		if (!resetTerrain)
		{
			return terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
												terrainData.heightmapHeight);
		}
		else
			return new float[terrainData.heightmapWidth,
							 terrainData.heightmapHeight];
	}

	public void Perlin()
	{
		float[,] heightMap = GetHeightMap();
		for (int x = 0; x < terrainData.heightmapWidth; x++)
		{
			for (int y = 0; y < terrainData.heightmapHeight; y++)
			{
				heightMap[x,y] += Utils.fBM((x + perlinOffsetX) * perlinXScale,
										   (y + perlinOffsetY) * perlinYScale,
										   perlinOctaves,
										   perlinPersistance) * perlinHeightScale;
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
	}

	public void MultiplePerlinTerrain()
	{
		float[,] heightMap = GetHeightMap();
		for (int x = 0; x < terrainData.heightmapWidth; x++)
		{
			for (int y = 0; y < terrainData.heightmapHeight; y++)
			{
				foreach (PerlinParameters p in perlinParameters)
				{
					heightMap[x, y] += Utils.fBM((x + p.mPerlinOffsetX) * p.mPerlinXScale,
										   (y + p.mPerlinOffsetY) * p.mPerlinYScale,
										   p.mPerlinOctaves,
										   p.mPerlinPersistance) * p.mPerlinHeightScale;
				}
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
	}

	public void AddNewPerlin()
	{
		perlinParameters.Add(new PerlinParameters());
	}

	public void RemovePerlin()
	{
		if (perlinParameters.TrueForAll(param => param.remove))
    	{
        	perlinParameters[0] = new PerlinParameters();
    	}
    	perlinParameters.RemoveAll(param => param.remove);
	}

	public void Voranoi()
	{
		float[,] heightMap = GetHeightMap();
		Vector3 peak = new Vector3(256, 0.2f, 256);
		float fallOff = 0.5f;
		// Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth),
		// 						   UnityEngine.Random.Range(0.0f, 1.0f),
		// 						   UnityEngine.Random.Range(0, terrainData.heightmapHeight));

		heightMap[(int)peak.x, (int)peak.z] = peak.y;

		Vector2 peakLocation = new Vector2(peak.x, peak.z);
		float maxDistance = Vector2.Distance(new Vector2(0, 0),
											 new Vector2(terrainData.heightmapWidth, terrainData.heightmapHeight));

		for (int y = 0; y < terrainData.heightmapHeight; y++){
			for (int x = 0; x < terrainData.heightmapWidth; x++)
			{
				if (!(x == peak.x && y == peak.z))
				{
					float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) * fallOff;
					// Creating slope by taking the height off the peak proportionate to the distance away from the peak
					heightMap[x,y] += peak.y - (distanceToPeak / maxDistance);
				}
			}
		}									 
		terrainData.SetHeights(0, 0, heightMap);
	}

	public void RandomTerrain()
	{
		float[,] heightMap = GetHeightMap();
		for (int x = 0; x < terrainData.heightmapWidth; x++)
		{
			for (int z = 0; z < terrainData.heightmapHeight; z++)
			{
				heightMap[x,z] += UnityEngine.Random.Range(randomHeightRange.x,randomHeightRange.y);
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
	}
	public void LoadTexture()
	{
		float[,] heightMap = GetHeightMap();
		for (int x = 0; x < terrainData.heightmapWidth; x++)
		{
			for (int z = 0; z < terrainData.heightmapHeight; z++)
			{
				heightMap[x,z] += heightMapImage.GetPixel((int)(x * heightMapScale.x),
														 (int)(z * heightMapScale.z)).grayscale
														  * heightMapScale.y;
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
	}
	public void ResetTerrain()
	{
		float[,] heightMap;
		heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
		for (int x = 0; x < terrainData.heightmapWidth; x++)
		{
			for (int z = 0; z < terrainData.heightmapHeight; z++)
			{
				heightMap[x,z] = 0;
			}
		}
		terrainData.SetHeights(0, 0, heightMap);
	}
	public void OnEnable()
	{
		Debug.Log("Initialising Terrain Data");
		terrain = this.GetComponent<Terrain>();
		terrainData = Terrain.activeTerrain.terrainData;
	}
	void Awake()
	{
		SerializedObject tagManager = new SerializedObject(
			AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		SerializedProperty tagsProp = tagManager.FindProperty("tags");

		AddTag(tagsProp, "Terrain");
		AddTag(tagsProp, "Cloud");
		AddTag(tagsProp, "Shore");

		// Apply tag changes to tag database
		tagManager.ApplyModifiedProperties();

		// Take this object
		this.gameObject.tag = "Terrain";
	}
    void AddTag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;
		// Ensure tag doesn't exist
		for (int i = 0; i < tagsProp.arraySize; i++)
		{
			SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
			if (t.stringValue.Equals(newTag)) { found = true; break; }
		}
		if (!found)
		{
			tagsProp.InsertArrayElementAtIndex(0);
			SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
			newTagProp.stringValue = newTag;
		}
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
