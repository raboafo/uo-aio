using System;

namespace UOAIO;

public static class Interpolator
{
	public static float Linear(float y1, float y2, float mu)
	{
		return y1 * (1f - mu) + y2 * mu;
	}

	public static float Cosine(float y1, float y2, float mu)
	{
		if (float.IsNaN(mu))
		{
			mu = 0f;
		}
		float num = (1f - (float)Math.Cos((double)mu * Math.PI)) / 2f;
		return y1 * (1f - num) + y2 * num;
	}

	public static float Bezier(float f0, float f1, float f2, float f3, float mu)
	{
		float num = f1 + (f1 - f0) / 1f;
		float num2 = f2 + (f2 - f3) / 1f;
		float num3 = 3f * (num - f1);
		float num4 = 3f * (num2 - num) - num3;
		float num5 = f2 - f1 - num3 - num4;
		float num6 = mu * mu;
		float num7 = mu * num6;
		return num5 * num7 + num4 * num6 + num3 * mu + f1;
	}

	public static float Cubic(float y0, float y1, float y2, float y3, float mu)
	{
		float num = mu * mu;
		float num2 = y3 - y2 - y0 + y1;
		float num3 = y0 - y1 - num2;
		float num4 = y2 - y0;
		return num2 * mu * num + num3 * num + num4 * mu + y1;
	}
}
