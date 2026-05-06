using System;
using System.IO;
using SharpDX.Direct3D9;

namespace UOAIO;

public sealed class LightReader
{
	private readonly Entry3D[] index;

	private readonly Stream dataStream;

	public unsafe LightReader()
	{
		this.index = new Entry3D[100];
		fixed (Entry3D* buffer = this.index)
		{
			using FileStream file = (FileStream)Engine.FileManager.OpenMUL(Files.LightIdx);
			UnsafeMethods.ReadFile(file, buffer, 1200);
		}
		this.dataStream = Engine.FileManager.OpenMUL(Files.LightMul);
	}

	public unsafe Texture ReadLight(int lightId)
	{
		if (lightId >= 0 && lightId < this.index.Length)
		{
			Entry3D entry3D = this.index[lightId];
			if (entry3D.m_Lookup >= 0 && entry3D.m_Length > 0)
			{
				ushort num = (ushort)(entry3D.m_Extra & 0xFFFF);
				ushort num2 = (ushort)((entry3D.m_Extra >> 16) & 0xFFFF);
				if (num > 0 && num2 > 0 && entry3D.m_Length == num * num2)
				{
					byte[] array = new byte[entry3D.m_Length];
					int num3 = 0;
					int num4 = array.Length;
					this.dataStream.Seek(entry3D.m_Lookup, SeekOrigin.Begin);
					int num5;
					while (num4 > 0 && (num5 = this.dataStream.Read(array, num3, num4)) > 0)
					{
						num3 += num5;
						num4 -= num5;
					}
					if (num4 == 0)
					{
						Texture texture = new Texture(num, num2, Format.A8R8G8B8, TextureTransparency.Complex);
						LockData lockData = texture.Lock(LockFlags.WriteOnly);
						try
						{
							fixed (byte* ptr = array)
							{
								sbyte* ptr2 = (sbyte*)ptr;
								byte* ptr3 = (byte*)lockData.pvSrc;
								int num6 = lockData.Pitch - num * 4;
								for (int i = 0; i < num2; i++)
								{
									for (int j = 0; j < num; j++)
									{
										*ptr3 = (byte)((Math.Abs(*(ptr2++)) * 255 + 15) / 31);
										ptr3[1] = *ptr3;
										ptr3[2] = *ptr3;
										ptr3[3] = *ptr3;
										ptr3 += 4;
									}
									ptr3 += num6;
								}
							}
							return texture;
						}
						finally
						{
							texture.Unlock();
						}
					}
				}
			}
		}
		return Texture.Empty;
	}
}
