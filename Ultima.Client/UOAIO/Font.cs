using System;
using System.Collections.Generic;
using System.IO;

namespace UOAIO;

public class Font : IFont, IFontFactory
{
	private int m_FontID;

	private FontCache m_Cache;

	private Dictionary<WrapKey, string> m_WrapCache = new Dictionary<WrapKey, string>();

	private const string RelativeApplicationDataPath = "Veritas/Ultima Online/Cache/Fonts";

	private const string RelativeLegacyPath = "data/ultima/cache/fonts.uoi";

	private short[] m_Palette;

	public FontImage[] m_Images;

	private static byte[] m_Buffer;

	public Dictionary<WrapKey, string> WrapCache => this.m_WrapCache;

	public string Name => $"Font[{this.m_FontID}]";

	public override string ToString()
	{
		return $"<ASCII Font #{this.m_FontID}>";
	}

	private static string GetCachePath()
	{
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Veritas/Ultima Online/Cache/Fonts");
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(text));
		if (!directoryInfo.Exists)
		{
			directoryInfo.Create();
		}
		return text;
	}

	public static void Reformat()
	{
		string text = Engine.FileManager.ResolveMUL(Files.Fonts);
		if (!File.Exists(text))
		{
			throw new InvalidOperationException($"Unable to reformat the font file, it doesn't exist. (inputPath={text})");
		}
		string cachePath = Font.GetCachePath();
		using FileStream input = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.Read);
		using BinaryReader binaryReader = new BinaryReader(input);
		using FileStream fileStream = new FileStream(cachePath, FileMode.Create, FileAccess.Write, FileShare.None);
		using BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		FileInfo fileInfo = new FileInfo(text);
		binaryWriter.Write(fileInfo.LastWriteTime.ToFileTime());
		binaryWriter.Write(10);
		binaryWriter.Write(new byte[80]);
		for (int i = 0; i < 10; i++)
		{
			binaryWriter.Flush();
			long length = fileStream.Length;
			fileStream.Seek(12 + i * 8, SeekOrigin.Begin);
			binaryWriter.Write((int)length);
			fileStream.Seek(length, SeekOrigin.Begin);
			binaryReader.ReadByte();
			int num = 0;
			List<short> list = new List<short>();
			list.Add(0);
			for (int j = 0; j < 224; j++)
			{
				int num2 = binaryReader.ReadByte();
				int num3 = binaryReader.ReadByte();
				int num4 = binaryReader.ReadByte();
				byte[,] array = new byte[num2, num3];
				for (int k = 0; k < num3; k++)
				{
					for (int l = 0; l < num2; l++)
					{
						int num5 = binaryReader.ReadInt16() & 0x7FFF;
						int num6 = -1;
						if (num5 != 0)
						{
							num5 |= 0x8000;
						}
						for (int m = 0; m < list.Count; m++)
						{
							if (list[m] == (short)num5)
							{
								num6 = m;
								break;
							}
						}
						if (num6 == -1)
						{
							num6 = list.Count;
							list.Add((short)num5);
						}
						array[l, k] = (byte)num6;
					}
				}
				binaryWriter.Write((byte)num2);
				binaryWriter.Write((byte)num3);
				binaryWriter.Write((byte)num4);
				num += 3;
				for (int n = 0; n < num3; n++)
				{
					for (int num7 = 0; num7 < num2; num7++)
					{
						binaryWriter.Write(array[num7, n]);
					}
				}
				num += num2 * num3;
			}
			binaryWriter.Write(list.Count);
			num += 4;
			for (int num8 = 0; num8 < list.Count; num8++)
			{
				binaryWriter.Write(list[num8]);
			}
			num += list.Count * 2;
			length = fileStream.Length;
			fileStream.Seek(12 + i * 8 + 4, SeekOrigin.Begin);
			binaryWriter.Write(num);
			fileStream.Seek(length, SeekOrigin.Begin);
		}
	}

	public int GetStringWidth(string text)
	{
		if (text == null || text.Length <= 0)
		{
			return 0;
		}
		char[] array = text.ToCharArray();
		int num = 0;
		int num2 = 0;
		foreach (char c in array)
		{
			if (c >= ' ' && c < 'Ā')
			{
				FontImage fontImage = this.m_Images[c - 32];
				num += fontImage.xWidth;
				if (num > num2)
				{
					num2 = num;
				}
			}
			else if (c == '\n')
			{
				num = 0;
			}
		}
		return num2;
	}

	public Texture GetString(string String, IHue Hue)
	{
		return this.m_Cache[String, Hue];
	}

	unsafe Texture IFontFactory.CreateInstance(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return Texture.Empty;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 1;
		char[] array = text.ToCharArray();
		foreach (char c in array)
		{
			if (c >= ' ' && c < 'Ā')
			{
				FontImage fontImage = this.m_Images[c - 32];
				num2 += fontImage.xWidth;
				if (num2 > num3)
				{
					num3 = num2;
				}
				if (fontImage.yHeight > num)
				{
					num = fontImage.yHeight;
				}
			}
			else if (c == '\n')
			{
				num2 = 0;
				num4++;
			}
		}
		num4 *= num;
		if (num3 <= 0 || num4 <= 0)
		{
			return Texture.Empty;
		}
		Texture texture = new Texture(num3, num4, TextureTransparency.Simple);
		if (texture.IsEmpty())
		{
			return Texture.Empty;
		}
		fixed (short* ptr = new short[this.m_Palette.Length])
		{
			fixed (short* palette = this.m_Palette)
			{
				Hues.Default.CopyPixels(palette + 1, ptr + 1, this.m_Palette.Length - 1);
			}
			LockData lockData = texture.Lock(LockFlags.WriteOnly);
			short* ptr2 = (short*)lockData.pvSrc;
			short* ptr3 = ptr2;
			int num5 = lockData.Pitch >> 1;
			int num6 = num5 * num;
			foreach (char c in array)
			{
				if (c >= ' ' && c < 'Ā')
				{
					FontImage fontImage = this.m_Images[c - 32];
					int xWidth = fontImage.xWidth;
					int yHeight = fontImage.yHeight;
					short* ptr4 = ptr3;
					ptr4 += (num - yHeight) * num5;
					int num7 = num5 - xWidth;
					int num8 = fontImage.xDelta - xWidth;
					fixed (byte* xyPixels = fontImage.xyPixels)
					{
						byte* ptr5 = xyPixels;
						int num9 = 0;
						while (num9 < yHeight)
						{
							int num10 = xWidth >> 2;
							int num11 = xWidth & 3;
							while (--num10 >= 0)
							{
								*ptr4 = ptr[(int)(*ptr5)];
								ptr4[1] = ptr[(int)ptr5[1]];
								ptr4[2] = ptr[(int)ptr5[2]];
								ptr4[3] = ptr[(int)ptr5[3]];
								ptr4 += 4;
								ptr5 += 4;
							}
							while (--num11 >= 0)
							{
								*(ptr4++) = ptr[(int)(*(ptr5++))];
							}
							num9++;
							ptr4 += num7;
							ptr5 += num8;
						}
					}
					ptr3 += fontImage.xWidth;
				}
				else if (c == '\n')
				{
					ptr2 += num6;
					ptr3 = ptr2;
				}
			}
			texture.Unlock();
			return texture;
		}
	}

	public void Dispose()
	{
		this.m_Cache.Dispose();
		this.m_Cache = null;
		this.m_Palette = null;
		this.m_Images = null;
		Font.m_Buffer = null;
		this.m_WrapCache.Clear();
		this.m_WrapCache = null;
	}

	public unsafe Font(int fid)
	{
		this.m_FontID = fid;
		this.m_Cache = new FontCache(this);
		this.m_Images = new FontImage[224];
		string cachePath = Font.GetCachePath();
		if (!File.Exists(cachePath))
		{
			string text = Engine.FileManager.BasePath("data/ultima/cache/fonts.uoi");
			if (File.Exists(text))
			{
				try
				{
					File.Move(text, cachePath);
				}
				catch
				{
					File.Copy(text, cachePath, overwrite: false);
				}
			}
			else
			{
				Font.Reformat();
			}
		}
		FileStream fileStream = new FileStream(cachePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		DateTime dateTime = DateTime.FromFileTime(binaryReader.ReadInt64());
		if (dateTime != new FileInfo(Engine.FileManager.ResolveMUL(Files.Fonts)).LastWriteTime)
		{
			binaryReader.Close();
			Font.Reformat();
			fileStream = new FileStream(cachePath, FileMode.Open, FileAccess.Read, FileShare.None);
			binaryReader = new BinaryReader(fileStream);
		}
		fileStream.Seek(12 + fid * 8, SeekOrigin.Begin);
		int num = binaryReader.ReadInt32();
		int num2 = binaryReader.ReadInt32();
		fileStream.Seek(num, SeekOrigin.Begin);
		if (Font.m_Buffer == null || num2 > Font.m_Buffer.Length)
		{
			Font.m_Buffer = new byte[num2];
		}
		fixed (byte* buffer = Font.m_Buffer)
		{
			UnsafeMethods.ReadFile(fileStream, buffer, num2);
			byte* ptr = buffer;
			for (int i = 0; i < 224; i++)
			{
				int num3 = *ptr;
				int num4 = ptr[1];
				ptr += 3;
				FontImage fontImage = new FontImage(num3, num4);
				int xDelta = fontImage.xDelta;
				fixed (byte* xyPixels = fontImage.xyPixels)
				{
					byte* ptr2 = xyPixels;
					int num5 = 0;
					while (num5 < num4)
					{
						int j = 0;
						byte* ptr3 = ptr2;
						for (; j < num3; j++)
						{
							*(ptr3++) = *(ptr++);
						}
						num5++;
						ptr2 += xDelta;
					}
				}
				this.m_Images[i] = fontImage;
			}
			int num6 = *(int*)ptr;
			ptr += 4;
			short* ptr4 = (short*)ptr;
			this.m_Palette = new short[num6];
			for (int k = 0; k < num6; k++)
			{
				this.m_Palette[k] = *(ptr4++);
			}
			ptr = (byte*)ptr4;
		}
		binaryReader.Close();
	}
}
