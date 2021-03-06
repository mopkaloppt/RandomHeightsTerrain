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

	// voronoi ---------------------------------------
	public float voronoiFallOff = 0.2f;
	public float voronoiDropOff = 0.6f;
	public float voronoiMinHeight = 0.1f;
	public float voronoiMaxHeight = 0.5f;
	public int voronoiPeaks = 5;
	public enum VoronoiType { Linear = 0, Power = 1, Meringue = 2, Combined = 3 }
	public VoronoiType voronoiType = VoronoiType.Linear;

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

	public void Voronoi()
	{
		float[,] heightMap = GetHeightMap();

		for (int p = 0; p < voronoiPeaks; p++)
		{
			Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth),
								       UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight),
								       UnityEngine.Random.Range(0, terrainData.heightmapHeight));
			
			// If the current location has got no peak (checking that its value is lower than the peak we're about to assign),
			// then assign this new peak, else don't assign because if you do it will try to bring the peak down and create a hole
			if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
			{
				// Assign the peak to the randomly picked location on a height map
				heightMap[(int)peak.x, (int)peak.z] = peak.y;
			}
			else
				continue;
		
			Vector2 peakLocation = new Vector2(peak.x, peak.z);
			// Give you diagonal line from bottom left corner to top right corner of the map
			float maxDistance = Vector2.Distance(new Vector2(0, 0),
											     new Vector2(terrainData.heightmapWidth, terrainData.heightmapHeight));
			
			for (int y = 0; y < terrainData.heightmapHeight; y++)
			{
				for (int x = 0; x < terrainData.heightmapWidth; x++)
				{
					if (!(x == peak.x && y == peak.z))
					{
						float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
						float h;

						if (voronoiType == VoronoiType.Combined)
						{
							h = peak.y - distanceToPeak * voronoiFallOff -
								Mathf.Pow(distanceToPeak, voronoiDropOff);
						}
						else if (voronoiType == VoronoiType.Power)
						{
							h = peak.y - Mathf.Pow(distanceToPeak, voronoiDropOff) * voronoiFallOff;
						}
						else if (voronoiType == VoronoiType.Meringue)
						{
							h = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff) - 
											Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / voronoiDropOff;
						}
						else
						{
							// Non-realistic voronoi with Sin wave extending out of the little lower peak in the middle. 
							// float h = peak.y - Mathf.Sin(distanceToPeak * 100) * 0.1f;
							h = peak.y - distanceToPeak * voronoiFallOff;
						}		
						// If the height isn't there, set it to height h (lift it up), else don't bring it back down
						if (heightMap[x,y] < h)
						{
							heightMap[x,y] = h;
						}						
					}
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
