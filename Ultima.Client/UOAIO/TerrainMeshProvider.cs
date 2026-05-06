using System;
using Ultima.Client.Terrain;

namespace UOAIO;

public sealed class TerrainMeshProvider : IMeshProvider
{
	private readonly IMeshProvider source;

	private readonly int[] leftRightIndices;

	private readonly int[] topBottomIndices;

	public int Divisions => this.source.Divisions;

	public int Size => this.source.Size;

	public int Stride => this.source.Stride;

	public TerrainMeshProvider(IMeshProvider source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		this.source = source;
		this.leftRightIndices = this.CreateIndices(leftRight: true);
		this.topBottomIndices = this.CreateIndices(leftRight: false);
	}

	public unsafe void Sample(int* pInput, float* pOutput)
	{
		this.source.Sample(pInput, pOutput);
	}

	public int[] GetIndices(bool leftRight)
	{
		return leftRight ? this.leftRightIndices : this.topBottomIndices;
	}

	private int[] CreateIndices(bool leftRight)
	{
		int divisions = this.Divisions;
		int stride = this.Stride;
		int[] array = new int[divisions * divisions * 6];
		int num = 0;
		for (int i = 0; i < divisions; i++)
		{
			for (int j = 0; j < divisions; j++)
			{
				int num2 = i * stride + j;
				if (divisions > 1)
				{
					leftRight = ((j + i) & 1) == 0;
				}
				if (leftRight)
				{
					array[num++] = num2 + stride;
					array[num++] = num2;
					array[num++] = num2 + stride + 1;
					array[num++] = num2;
					array[num++] = num2 + 1;
					array[num++] = num2 + stride + 1;
				}
				else
				{
					array[num++] = num2 + stride;
					array[num++] = num2;
					array[num++] = num2 + 1;
					array[num++] = num2 + stride;
					array[num++] = num2 + 1;
					array[num++] = num2 + stride + 1;
				}
			}
		}
		return array;
	}
}
