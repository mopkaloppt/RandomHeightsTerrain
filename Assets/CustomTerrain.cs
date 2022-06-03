using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode] // If we don't have this line, the code runs only when we press play.
public class CustomTerrain : MonoBehaviour {

	public Vector2 randomHeightRange = new Vector2(0, 0.1f);
	public Terrain terrain;
	public TerrainData terrainData;

	public void RandomTerrain()
	{
		float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
														  terrainData.heightmapHeight);
		for (int x = 0; x < terrainData.heightmapWidth; x++)
		{
			for (int z = 0; z < terrainData.heightmapHeight; z++)
			{
				heightMap[x,z] += UnityEngine.Random.Range(randomHeightRange.x,randomHeightRange.y);
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
