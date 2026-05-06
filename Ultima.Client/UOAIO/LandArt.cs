using System;
using UOAIO.Assets;

namespace UOAIO;

public class LandArt
{
	private class LandFactory : TextureFactory
	{
		private int m_LandID;

		private IHue m_Hue;

		private int[] m_Offset;

		private int[] m_Length;

		private LandArt m_Land;

		private byte[] data;

		public override TextureTransparency Transparency => TextureTransparency.Simple;

		public LandFactory(LandArt land)
		{
			this.m_Land = land;
			this.m_Length = new int[44]
			{
				1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
				11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
				21, 22, 22, 21, 20, 19, 18, 17, 16, 15,
				14, 13, 12, 11, 10, 9, 8, 7, 6, 5,
				4, 3, 2, 1
			};
			this.m_Offset = new int[44]
			{
				21, 20, 19, 18, 17, 16, 15, 14, 13, 12,
				11, 10, 9, 8, 7, 6, 5, 4, 3, 2,
				1, 0, 0, 1, 2, 3, 4, 5, 6, 7,
				8, 9, 10, 11, 12, 13, 14, 15, 16, 17,
				18, 19, 20, 21
			};
		}

		private string GetFilePath(int tileId)
		{
			return $"build/artlegacymul/{tileId:00000000}.tga";
		}

		public Texture Load(int landID, IHue hue)
		{
			this.m_LandID = landID & 0x3FFF;
			this.m_Hue = hue;
			return base.Construct(isReconstruct: false);
		}

		public override Texture Reconstruct(object[] args)
		{
			this.m_LandID = (int)args[0];
			this.m_Hue = (IHue)args[1];
			return base.Construct(isReconstruct: true);
		}

		protected override void CoreAssignArgs(Texture tex)
		{
			tex.m_Factory = this;
			tex.m_FactoryArgs = new object[2] { this.m_LandID, this.m_Hue };
			tex._shaderData = this.m_Hue.ShaderData;
		}

		protected override bool CoreLookup()
		{
			AssetSourceManager.Art.TryReadLand(this.m_LandID, out this.data);
			return this.data != null && this.data.Length == 2048;
		}

		protected override void CoreGetDimensions(out int width, out int height)
		{
			if (this.data == null)
			{
				throw new InvalidOperationException();
			}
			width = (height = 44);
		}

		protected unsafe override void CoreProcessImage(int width, int height, int stride, ushort* pLine, ushort* pLineEnd, ushort* pImageEnd, int lineDelta, int lineEndDelta)
		{
			fixed (byte* ptr = this.data)
			{
				int* ptr2 = (int*)ptr;
				short* ptr3 = (short*)pLine;
				int num = 11;
				fixed (int* offset = this.m_Offset)
				{
					fixed (int* length = this.m_Length)
					{
						int* ptr4 = offset;
						int* ptr5 = length;
						ushort* ptr6 = pLine;
						while (--num >= 0)
						{
							int* pvDest = (int*)(pLine + *ptr4);
							int num2 = *ptr5;
							this.m_Hue.CopyPixels(ptr2, pvDest, num2 << 1);
							ptr2 += num2;
							pLine += lineEndDelta;
							pvDest = (int*)(pLine + ptr4[1]);
							num2 = ptr5[1];
							this.m_Hue.CopyPixels(ptr2, pvDest, num2 << 1);
							ptr2 += num2;
							pLine += lineEndDelta;
							pvDest = (int*)(pLine + ptr4[2]);
							num2 = ptr5[2];
							this.m_Hue.CopyPixels(ptr2, pvDest, num2 << 1);
							ptr2 += num2;
							pLine += lineEndDelta;
							pvDest = (int*)(pLine + ptr4[3]);
							num2 = ptr5[3];
							this.m_Hue.CopyPixels(ptr2, pvDest, num2 << 1);
							ptr2 += num2;
							pLine += lineEndDelta;
							ptr4 += 4;
							ptr5 += 4;
						}
					}
				}
			}
		}
	}

	private LandFactory m_Factory;

	public void Dispose()
	{
	}

	public Texture ReadFromDisk(int LandID, IHue Hue)
	{
		if (this.m_Factory == null)
		{
			this.m_Factory = new LandFactory(this);
		}
		return this.m_Factory.Load(LandID, Hue);
	}
}
