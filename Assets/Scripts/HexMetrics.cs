using UnityEngine;

public static class HexMetrics
{
	public const float outerRadius = 10f;
	public const float innerRadius = outerRadius * 0.866025404f;
	public const float innerRatio = 0.8f;
	public const float gap = (innerRadius * innerRatio) * 2;
	public const float solidFactor = 0.75f;
	public const float blendFactor = 1f - solidFactor;
	public const float elevationStep = 5.0f;
	public const float chunkWidth = innerRadius * 2.0f * (float)chunkSize;
	public const float chunkHeight = (outerRadius * 1.5f * (float)chunkSize) - (gap / 3f);
	public const int chunkSize = 6;
	public static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius),
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(0f, 0f, outerRadius)
	};
	public static Vector3 GetFirstCorner(HexDirection direction)
	{
		return corners[(int)direction];
	}
	public static Vector3 GetSecondCorner(HexDirection direction)
	{
		return corners[(int)direction + 1];
	}
	public static Vector3 GetFirstSolidCorner(HexDirection direction)
	{
		return corners[(int)direction] * solidFactor;
	}
	public static Vector3 GetSecondSolidCorner(HexDirection direction)
	{
		return corners[(int)direction + 1] * solidFactor;
	}
	public static Vector3 GetBridge(HexDirection direction)
	{
		return (corners[(int)direction] + corners[(int)direction + 1]) *
			blendFactor;
	}
	public static float GetSideLength()
    {
		return 2f * Mathf.Sqrt(Mathf.Pow(outerRadius, 2f) + Mathf.Pow(innerRadius, 2f));
	}
}