using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
	// fBM: Fractal Brownian Motion
	public static float fBM(float x, float y, int octave, float persistence)
	{
		float total = 0;
		float frequency = 1; // how close the waves are together
		float amplitude = 1;
		float maxValue = 0;
		for (int i = 0; i < octave; i++)
		{
			total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
			maxValue += amplitude;
			amplitude *= persistence;
			frequency *= 2; // 2 is an experimenting value 
		}
		return total / maxValue;
	}
}
