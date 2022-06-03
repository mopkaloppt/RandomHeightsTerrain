﻿using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor {
	// Properties ----------
	SerializedProperty randomHeightRange;
	// Fold outs -----------
	bool showRandom = false;
	void OnEnable()
	{
		randomHeightRange = serializedObject.FindProperty("randomHeightRange");
	}
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		CustomTerrain terrain = (CustomTerrain) target;

		showRandom = EditorGUILayout.Foldout(showRandom, "Random");
		if (showRandom)
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(randomHeightRange);
			if (GUILayout.Button("Random Heights"))
			{
				terrain.RandomTerrain();
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}