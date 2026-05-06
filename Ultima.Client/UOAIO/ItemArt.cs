using System;
using UOAIO.Assets;

namespace UOAIO;

public class ItemArt : IDisposable
{
	private class ItemFactory : TextureFactory
	{
		private int m_ItemID;

		private IHue m_Hue;

		private int m_xMin;

		private int m_yMin;

		private int m_xMax;

		private int m_yMax;

		private int _averageColor;

		private ItemArt m_Items;

		private byte[] data;

		private static ushort[] _guassianBlurMatrix;

		public override TextureTransparency Transparency => TextureTransparency.Simple;

		public ItemFactory(ItemArt items)
		{
			this.m_Items = items;
		}

		public Texture Load(int itemID, IHue hue)
		{
			this.m_ItemID = itemID;
			this.m_Hue = hue;
			return base.Construct(isReconstruct: false);
		}

		public override Texture Reconstruct(object[] args)
		{
			this.m_ItemID = (int)args[0];
			this.m_Hue = (IHue)args[1];
			return base.Construct(isReconstruct: true);
		}

		protected override void CoreAssignArgs(Texture tex)
		{
			tex.m_Factory = this;
			tex.m_FactoryArgs = new object[2] { this.m_ItemID, this.m_Hue };
			tex.xMin = this.m_xMin;
			tex.yMin = this.m_yMin;
			tex.xMax = this.m_xMax;
			tex.yMax = this.m_yMax;
			tex._averageColor = this._averageColor;
			tex._shaderData = this.m_Hue.ShaderData;
		}

		private string GetFilePath(int tileId)
		{
			return $"build/artlegacymul/{tileId:00000000}.tga";
		}

		protected override bool CoreLookup()
		{
			AssetSourceManager.Art.TryReadItem(this.m_ItemID, out this.data);
			return this.data != null && this.data.Length > 8;
		}

		protected override void CoreGetDimensions(out int width, out int height)
		{
			if (this.data == null)
			{
				throw new InvalidOperationException();
			}
			width = BitConverter.ToInt16(this.data, 4);
			height = BitConverter.ToInt16(this.data, 6);
			if (this.m_Hue is Hues.ShadowHue)
			{
				width += 17;
				height += 17;
			}
		}

		protected unsafe override void CoreProcessImage(int width, int height, int stride, ushort* pLine, ushort* pLineEnd, ushort* pImageEnd, int lineDelta, int lineEndDelta)
		{
			fixed (byte* ptr = this.data)
			{
				short* ptr2 = (short*)(ptr + (nint)4 * (nint)2 + (nint)height * (nint)2) - ((this.m_Hue is Hues.ShadowHue) ? 17 : 0);
				short* ptr3 = (short*)ptr + 3;
				int num = width;
				int num2 = height;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				int num7 = 0;
				int num8 = 0;
				if (this.m_Hue is Hues.ShadowHue)
				{
					fixed (ushort* guassianBlurMatrix = ItemFactory._guassianBlurMatrix)
					{
						ushort* ptr4 = pLine;
						ushort* ptr5 = ptr4;
						while (ptr5 < pImageEnd)
						{
							*(ptr5++) = 0;
						}
						pLine += 8 * lineEndDelta;
						int num9 = 0;
						int num10 = 0;
						int num11 = 0;
						int num12 = 0;
						while (num6 < height - 17)
						{
							short* ptr6 = ptr2 + *(++ptr3);
							ushort* ptr7 = pLine + 8;
							num5 = 0;
							if (*ptr6 + ptr6[1] != 0 && *ptr6 < num)
							{
								num = *ptr6;
							}
							while ((num7 = *(ptr6++)) + (num8 = *(ptr6++)) != 0)
							{
								ptr7 += num7;
								for (int i = 0; i < num8; i++)
								{
									short num13 = *(ptr6++);
									num9 += (num13 >> 10) & 0x1F;
									num10 += (num13 >> 5) & 0x1F;
									num11 += num13 & 0x1F;
									num12 += 32;
									ushort* ptr8 = guassianBlurMatrix;
									for (int j = -8; j <= 8; j++)
									{
										ushort* ptr9 = ptr7 + j * lineEndDelta + i - 8;
										ushort* ptr10 = ptr9 + 17;
										while (ptr9 < ptr10)
										{
											ushort* intPtr = ptr9++;
											*intPtr += *(ptr8++);
										}
									}
								}
								ptr7 += num8;
							}
							if ((num5 = (int)(ptr7 - pLine)) > 8)
							{
								if (num2 == height)
								{
									num2 = num6;
								}
								num4 = num6;
								num5--;
								if (num5 > num3)
								{
									num3 = num5;
								}
							}
							num6++;
							pLine += lineEndDelta;
						}
						num += 8;
						num2 += 8;
						num4 += 8;
						ptr5 = ptr4;
						while (ptr5 < pImageEnd)
						{
							int num14 = *ptr5 * 31 / 22409;
							*(ptr5++) = (ushort)(0x8000 | (num14 << 10) | (num14 << 5) | num14);
						}
						if (num12 > 0)
						{
							num9 = (num9 * 255 + num12 / 2) / num12;
							num10 = (num10 * 255 + num12 / 2) / num12;
							num11 = (num11 * 255 + num12 / 2) / num12;
							this._averageColor = (num9 << 16) | (num10 << 8) | num11;
						}
					}
				}
				else
				{
					ushort* ptr11 = pLine;
					while (num6 < height)
					{
						short* ptr6 = ptr2 + *(++ptr3);
						ushort* ptr7 = pLine;
						num5 = 0;
						if (*ptr6 + ptr6[1] != 0 && *ptr6 < num)
						{
							num = *ptr6;
						}
						while ((num7 = *(ptr6++)) + (num8 = *(ptr6++)) != 0)
						{
							ptr7 += num7;
							this.m_Hue.CopyPixels(ptr6, ptr7, num8);
							ptr7 += num8;
							ptr6 += num8;
						}
						if ((num5 = (int)(ptr7 - pLine)) > 0)
						{
							if (num2 == height)
							{
								num2 = num6;
							}
							num4 = num6;
							num5--;
							if (num5 > num3)
							{
								num3 = num5;
							}
						}
						num6++;
						pLine += lineEndDelta;
					}
				}
				this.m_xMin = num;
				this.m_yMin = num2;
				this.m_xMax = num3;
				this.m_yMax = num4;
			}
		}

		static ItemFactory()
		{
			ItemFactory._guassianBlurMatrix = new ushort[289]
			{
				0, 11, 21, 31, 38, 45, 50, 53, 53, 53,
				50, 45, 38, 31, 21, 11, 0, 11, 23, 34,
				44, 53, 60, 65, 68, 69, 68, 65, 60, 53,
				44, 34, 23, 11, 21, 34, 46, 57, 66, 74,
				80, 84, 85, 84, 80, 74, 66, 57, 46, 34,
				21, 31, 44, 57, 68, 79, 88, 95, 100, 101,
				100, 95, 88, 79, 68, 57, 44, 31, 38, 53,
				66, 79, 91, 101, 110, 116, 117, 116, 110, 101,
				91, 79, 66, 53, 38, 45, 60, 74, 88, 101,
				114, 124, 131, 133, 131, 124, 114, 101, 88, 74,
				60, 45, 50, 65, 80, 95, 110, 124, 136, 146,
				149, 146, 136, 124, 110, 95, 80, 65, 50, 53,
				68, 84, 100, 116, 131, 146, 159, 165, 159, 146,
				131, 116, 100, 84, 68, 53, 53, 69, 85, 101,
				117, 133, 149, 165, 181, 165, 149, 133, 117, 101,
				85, 69, 53, 53, 68, 84, 100, 116, 131, 146,
				159, 165, 159, 146, 131, 116, 100, 84, 68, 53,
				50, 65, 80, 95, 110, 124, 136, 146, 149, 146,
				136, 124, 110, 95, 80, 65, 50, 45, 60, 74,
				88, 101, 114, 124, 131, 133, 131, 124, 114, 101,
				88, 74, 60, 45, 38, 53, 66, 79, 91, 101,
				110, 116, 117, 116, 110, 101, 91, 79, 66, 53,
				38, 31, 44, 57, 68, 79, 88, 95, 100, 101,
				100, 95, 88, 79, 68, 57, 44, 31, 21, 34,
				46, 57, 66, 74, 80, 84, 85, 84, 80, 74,
				66, 57, 46, 34, 21, 11, 23, 34, 44, 53,
				60, 65, 68, 69, 68, 65, 60, 53, 44, 34,
				23, 11, 0, 11, 21, 31, 38, 45, 50, 53,
				53, 53, 50, 45, 38, 31, 21, 11, 0
			};
		}
	}

	private ItemFactory m_Factory;

	public void Dispose()
	{
	}

	public Texture ReadFromDisk(int ItemID, IHue Hue)
	{
		ItemID &= 0xFFFF;
		if (ItemID >= 13700 && ItemID <= 13729)
		{
			return Hue.GetGump(2331 + (ItemID - 13700));
		}
		if (this.m_Factory == null)
		{
			this.m_Factory = new ItemFactory(this);
		}
		return this.m_Factory.Load(ItemID, Hue);
	}
}
