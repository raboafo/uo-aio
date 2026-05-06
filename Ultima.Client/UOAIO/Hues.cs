using System;
using System.IO;
using SharpDX;
using SharpDX.Direct3D9;
using UOAIO.Assets;
using UOAIO.Profiles;

namespace UOAIO;

public class Hues
{
	public class ShadowHue : IHue, IGraphicProvider, IDisposable
	{
		private static ShaderData _shaderData;

		private IGraphicProvider _provider;

		public ShaderData ShaderData => ShadowHue._shaderData;

		public Palette Palette => null;

		public override string ToString()
		{
			return "{ shadow }";
		}

		public ShadowHue()
		{
			this._provider = new DynamicCacheGraphicProvider(new PhysicalGraphicProvider(this));
		}

		public Frames GetAnimation(int RealID)
		{
			return this._provider.GetAnimation(RealID);
		}

		public Texture GetTerrainTexture(int TextureID)
		{
			return this._provider.GetTerrainTexture(TextureID);
		}

		public Texture GetTerrainIsometric(int LandID)
		{
			return this._provider.GetTerrainIsometric(LandID);
		}

		public Texture GetGump(int GumpID)
		{
			return this._provider.GetGump(GumpID);
		}

		public Texture GetItem(int ItemID)
		{
			return this._provider.GetItem(ItemID);
		}

		public Texture GetLight(int lightId)
		{
			return this._provider.GetLight(lightId);
		}

		public void Dispose()
		{
			if (this._provider != null)
			{
				this._provider.Dispose();
				this._provider = null;
			}
		}

		public unsafe void CopyPixels(void* pvSrc, void* pvDest, int count)
		{
			throw new InvalidOperationException();
		}

		public unsafe void CopyEncodedLine(ushort* pSrc, ushort* pSrcEnd, ushort* pDest, ushort* pEnd)
		{
			throw new InvalidOperationException();
		}

		public unsafe void FillLine(void* pSrc, void* pDest, int Count)
		{
			throw new InvalidOperationException();
		}

		public ushort Pixel(ushort input)
		{
			return input;
		}

		public int Pixel32(int input)
		{
			return input;
		}

		public int HueID()
		{
			return 65535;
		}

		static ShadowHue()
		{
			ShadowHue._shaderData = new ShaderData("ShadowShader.cso", null, TextureTransparency.Complex);
		}
	}

	public class ColorFillHue : IHue, IGraphicProvider, IDisposable
	{
		private static ShaderData _baseShaderData;

		private IGraphicProvider _provider;

		private ShaderData _shaderData;

		private Vector4 _colorData;

		private int _color;

		public ShaderData ShaderData => this._shaderData;

		public Palette Palette => null;

		private void RenderCallback()
		{
			Engine.m_Device.SetPixelShaderConstant(0, this._colorData.ToArray());
		}

		public ColorFillHue(int rgb32)
		{
			this._color = rgb32;
			int num = (rgb32 >> 16) & 0xFF;
			int num2 = (rgb32 >> 8) & 0xFF;
			int num3 = rgb32 & 0xFF;
			this._colorData = new Vector4((float)num / 255f, (float)num2 / 255f, (float)num3 / 255f, 1f);
			this._shaderData = new ShaderData(ColorFillHue._baseShaderData.PixelShader, null, TextureTransparency.NotSpecified);
			this._shaderData.RenderCallback = RenderCallback;
			this._provider = new DynamicCacheGraphicProvider(new ShadedGraphicProvider(this._shaderData, Hues.Default));
		}

		public Frames GetAnimation(int RealID)
		{
			return this._provider.GetAnimation(RealID);
		}

		public Texture GetTerrainTexture(int TextureID)
		{
			return this._provider.GetTerrainTexture(TextureID);
		}

		public Texture GetTerrainIsometric(int LandID)
		{
			return this._provider.GetTerrainIsometric(LandID);
		}

		public Texture GetGump(int GumpID)
		{
			return this._provider.GetGump(GumpID);
		}

		public Texture GetItem(int ItemID)
		{
			return this._provider.GetItem(ItemID);
		}

		public Texture GetLight(int lightId)
		{
			return this._provider.GetLight(lightId);
		}

		public void Dispose()
		{
			if (this._provider != null)
			{
				this._provider.Dispose();
				this._provider = null;
			}
		}

		public unsafe void CopyPixels(void* pvSrc, void* pvDest, int count)
		{
			throw new InvalidOperationException();
		}

		public unsafe void CopyEncodedLine(ushort* pSrc, ushort* pSrcEnd, ushort* pDest, ushort* pEnd)
		{
			throw new InvalidOperationException();
		}

		public unsafe void FillLine(void* pSrc, void* pDest, int Count)
		{
			throw new InvalidOperationException();
		}

		public ushort Pixel(ushort input)
		{
			throw new InvalidOperationException();
		}

		public int Pixel32(int input)
		{
			return this._color;
		}

		public int HueID()
		{
			return 65535;
		}

		static ColorFillHue()
		{
			ColorFillHue._baseShaderData = new ShaderData("ColorFillShader.cso", null, TextureTransparency.NotSpecified);
		}
	}

	public class DefaultHue : IHue, IGraphicProvider, IDisposable
	{
		private static ShaderData _shaderData;

		private IGraphicProvider _provider;

		private const int DoubleOpaque = -2147450880;

		public ShaderData ShaderData => DefaultHue._shaderData;

		public Palette Palette => null;

		public bool HintItem(int itemId)
		{
			return false;
		}

		public bool HintLand(int landId)
		{
			return false;
		}

		public override string ToString()
		{
			return "{ default }";
		}

		public DefaultHue()
		{
			this._provider = new StaticCacheGraphicProvider(new PhysicalGraphicProvider(this));
		}

		public Frames GetAnimation(int RealID)
		{
			return this._provider.GetAnimation(RealID);
		}

		public Texture GetTerrainTexture(int TextureID)
		{
			return this._provider.GetTerrainTexture(TextureID);
		}

		public Texture GetTerrainIsometric(int LandID)
		{
			return this._provider.GetTerrainIsometric(LandID);
		}

		public Texture GetGump(int GumpID)
		{
			return this._provider.GetGump(GumpID);
		}

		public Texture GetItem(int ItemID)
		{
			return this._provider.GetItem(ItemID);
		}

		public Texture GetLight(int lightId)
		{
			return this._provider.GetLight(lightId);
		}

		public void Dispose()
		{
			if (this._provider != null)
			{
				this._provider.Dispose();
				this._provider = null;
			}
		}

		public unsafe void CopyEncodedLine(ushort* pSrc, ushort* pSrcEnd, ushort* pDest, ushort* pEnd)
		{
			ushort* ptr = pSrc;
			ushort* ptr2 = pDest;
			while (pDest < pEnd && pSrc + 1 < pSrcEnd)
			{
				ushort num = *pSrc;
				ushort num2 = pSrc[1];
				pSrc += 2;
				ptr2 += (int)num2;
				if (num != 0)
				{
					num |= 0x8000;
					while (pDest < ptr2)
					{
						*(pDest++) = num;
					}
				}
				else
				{
					pDest += (int)num2;
				}
			}
		}

		public unsafe void CopyPixels(void* pvSrc, void* pvDest, int Pixels)
		{
			int* ptr = (int*)pvSrc;
			int* ptr2 = (int*)pvDest;
			for (int* ptr3 = ptr + ((Pixels >> 1) & -4); ptr < ptr3; ptr += 4)
			{
				*ptr2 = *ptr | -2147450880;
				ptr2[1] = ptr[1] | -2147450880;
				ptr2[2] = ptr[2] | -2147450880;
				ptr2[3] = ptr[3] | -2147450880;
				ptr2 += 4;
			}
			int num = (Pixels >> 1) & 3;
			switch (num)
			{
			case 3:
				ptr2[2] = ptr[2] | -2147450880;
				goto case 2;
			case 2:
				ptr2[1] = ptr[1] | -2147450880;
				goto case 1;
			case 1:
				*ptr2 = *ptr | -2147450880;
				break;
			}
			ptr2 += num;
			ptr += num;
			if ((Pixels & 1) != 0)
			{
				*(short*)ptr2 = (short)(0x8000 | *(ushort*)ptr);
			}
		}

		public ushort Pixel(ushort input)
		{
			return input;
		}

		public int Pixel32(int input)
		{
			return input;
		}

		public int HueID()
		{
			return 0;
		}

		static DefaultHue()
		{
			DefaultHue._shaderData = new ShaderData("DefaultShader.cso", null, TextureTransparency.NotSpecified);
		}
	}

	private class GrayscaleHue : IHue, IGraphicProvider, IDisposable
	{
		private static ShaderData _shaderData;

		private IGraphicProvider _provider;

		public ShaderData ShaderData => GrayscaleHue._shaderData;

		public Palette Palette => null;

		public override string ToString()
		{
			return "{ grayscale }";
		}

		public GrayscaleHue()
		{
			this._provider = new StaticCacheGraphicProvider(new ShadedGraphicProvider(GrayscaleHue._shaderData, Hues.Default));
		}

		public Frames GetAnimation(int RealID)
		{
			return this._provider.GetAnimation(RealID);
		}

		public Texture GetTerrainTexture(int TextureID)
		{
			return this._provider.GetTerrainTexture(TextureID);
		}

		public Texture GetTerrainIsometric(int LandID)
		{
			return this._provider.GetTerrainIsometric(LandID);
		}

		public Texture GetGump(int GumpID)
		{
			return this._provider.GetGump(GumpID);
		}

		public Texture GetItem(int ItemID)
		{
			return this._provider.GetItem(ItemID);
		}

		public Texture GetLight(int lightId)
		{
			return this._provider.GetLight(lightId);
		}

		public void Dispose()
		{
			if (this._provider != null)
			{
				this._provider.Dispose();
				this._provider = null;
			}
		}

		public unsafe void CopyPixels(void* pvSrc, void* pvDest, int Pixels)
		{
			throw new NotImplementedException();
		}

		public unsafe void CopyEncodedLine(ushort* pSrc, ushort* pSrcEnd, ushort* pDest, ushort* pEnd)
		{
			throw new NotImplementedException();
		}

		public ushort Pixel(ushort input)
		{
			int num = Engine.GrayScale(Engine.C32216(input)) * 255 / 31;
			return (ushort)(1057 * num);
		}

		public int Pixel32(int input)
		{
			int num = Engine.GrayScale(Engine.C32216(input)) * 255 / 31;
			return 65793 * num;
		}

		public int HueID()
		{
			return 65535;
		}

		static GrayscaleHue()
		{
			GrayscaleHue._shaderData = new ShaderData("GrayscaleShader.cso", null, TextureTransparency.NotSpecified);
		}
	}

	private class RegularHue : IHue, IGraphicProvider, IDisposable
	{
		private static ShaderData _baseShaderData;

		private HueData m_Data;

		private int hue;

		private IGraphicProvider _provider;

		private ShaderData _shaderData;

		private Vector4 _offset;

		public ShaderData ShaderData => this._shaderData;

		public Palette Palette => null;

		public override string ToString()
		{
			return $"{{ regular 0x{this.hue:X4} }}";
		}

		public ushort Pixel(ushort c)
		{
			return this.m_Data.colors[c >> 10];
		}

		public int Pixel32(int input)
		{
			return this.Pixel(Engine.C32216(input));
		}

		public int HueID()
		{
			return this.hue;
		}

		private void RenderCallback()
		{
			Engine.m_Device.SetPixelShaderConstant(1, this._offset.ToArray());
		}

		public RegularHue(HueData hd, int hueId, int hueIndex)
		{
			this.hue = hueId;
			this.m_Data = hd;
			this._shaderData = new ShaderData(RegularHue._baseShaderData.PixelShader, Hues.GetHueTexture(), TextureTransparency.NotSpecified);
			this._shaderData.RenderCallback = RenderCallback;
			int num = (hueIndex & 0xF) * 32;
			int num2 = hueIndex >> 4;
			this._offset = new Vector4((float)(1 + num * 2) / 1024f, (float)(1 + num2 * 2) / 512f, 0f, 0f);
			this._provider = new DynamicCacheGraphicProvider(new ShadedGraphicProvider(this._shaderData, Hues.Default));
		}

		public Frames GetAnimation(int RealID)
		{
			return this._provider.GetAnimation(RealID);
		}

		public Texture GetTerrainTexture(int TextureID)
		{
			return this._provider.GetTerrainTexture(TextureID);
		}

		public Texture GetTerrainIsometric(int LandID)
		{
			return this._provider.GetTerrainIsometric(LandID);
		}

		public Texture GetGump(int GumpID)
		{
			return this._provider.GetGump(GumpID);
		}

		public Texture GetItem(int ItemID)
		{
			return this._provider.GetItem(ItemID);
		}

		public Texture GetLight(int lightId)
		{
			return this._provider.GetLight(lightId);
		}

		public void Dispose()
		{
			if (this._provider != null)
			{
				this._provider.Dispose();
				this._provider = null;
			}
		}

		public unsafe void CopyPixels(void* pvSrc, void* pvDest, int Pixels)
		{
			fixed (ushort* colors = this.m_Data.colors)
			{
				ushort* ptr = (ushort*)pvSrc;
				ushort* ptr2 = (ushort*)pvDest;
				for (ushort* ptr3 = ptr + (Pixels & -4); ptr < ptr3; ptr += 4)
				{
					*ptr2 = colors[(*ptr >> 10) | 0x20];
					ptr2[1] = colors[(ptr[1] >> 10) | 0x20];
					ptr2[2] = colors[(ptr[2] >> 10) | 0x20];
					ptr2[3] = colors[(ptr[3] >> 10) | 0x20];
					ptr2 += 4;
				}
				switch (Pixels & 3)
				{
				default:
					goto end_IL_000e;
				case 3:
					ptr2[2] = colors[(ptr[2] >> 10) | 0x20];
					goto case 2;
				case 2:
					ptr2[1] = colors[(ptr[1] >> 10) | 0x20];
					break;
				case 1:
					break;
				}
				*ptr2 = colors[(*ptr >> 10) | 0x20];
				end_IL_000e:;
			}
		}

		public unsafe void CopyEncodedLine(ushort* pSrc, ushort* pSrcEnd, ushort* pDest, ushort* pEnd)
		{
			ushort* ptr = pDest;
			while (pDest < pEnd)
			{
				ushort num = *pSrc;
				ushort num2 = pSrc[1];
				pSrc += 2;
				ptr += (int)num2;
				if (num != 0)
				{
					num = this.m_Data.colors[(num >> 10) | 0x20];
					while (pDest < ptr)
					{
						*(pDest++) = num;
					}
				}
				else
				{
					pDest += (int)num2;
				}
			}
		}

		static RegularHue()
		{
			RegularHue._baseShaderData = new ShaderData("HueShader.cso", null, TextureTransparency.NotSpecified);
		}
	}

	public class EtherealHue : IHue, IGraphicProvider, IDisposable
	{
		private static ShaderData _shaderData;

		private IGraphicProvider _provider;

		public ShaderData ShaderData => EtherealHue._shaderData;

		public Palette Palette => null;

		public override string ToString()
		{
			return "{ ethereal }";
		}

		public EtherealHue()
		{
			this._provider = new DynamicCacheGraphicProvider(new ShadedGraphicProvider(EtherealHue._shaderData, Hues.Default));
		}

		public Frames GetAnimation(int RealID)
		{
			return this._provider.GetAnimation(RealID);
		}

		public Texture GetTerrainTexture(int TextureID)
		{
			return this._provider.GetTerrainTexture(TextureID);
		}

		public Texture GetTerrainIsometric(int LandID)
		{
			return this._provider.GetTerrainIsometric(LandID);
		}

		public Texture GetGump(int GumpID)
		{
			return this._provider.GetGump(GumpID);
		}

		public Texture GetItem(int ItemID)
		{
			return this._provider.GetItem(ItemID);
		}

		public Texture GetLight(int lightId)
		{
			return this._provider.GetLight(lightId);
		}

		public void Dispose()
		{
			if (this._provider != null)
			{
				this._provider.Dispose();
				this._provider = null;
			}
		}

		public unsafe void CopyPixels(void* pvSrc, void* pvDest, int count)
		{
			throw new InvalidOperationException();
		}

		public unsafe void CopyEncodedLine(ushort* pSrc, ushort* pSrcEnd, ushort* pDest, ushort* pEnd)
		{
			throw new InvalidOperationException();
		}

		public unsafe void FillLine(void* pSrc, void* pDest, int Count)
		{
			throw new InvalidOperationException();
		}

		public ushort Pixel(ushort input)
		{
			return input;
		}

		public int Pixel32(int input)
		{
			return input;
		}

		public int HueID()
		{
			return 16385;
		}

		static EtherealHue()
		{
			EtherealHue._shaderData = new ShaderData("EtherealShader.cso", null, TextureTransparency.Complex);
		}
	}

	private class PartialHue : IHue, IGraphicProvider, IDisposable
	{
		private static ShaderData _baseShaderData;

		private HueData m_Data;

		private int hue;

		private IGraphicProvider _provider;

		private ShaderData _shaderData;

		private Vector4 _offset;

		public ShaderData ShaderData => this._shaderData;

		public Palette Palette => null;

		private void RenderCallback()
		{
			Engine.m_Device.SetPixelShaderConstant(1, this._offset.ToArray());
		}

		public override string ToString()
		{
			return $"{{ partial 0x{this.hue:X4} }}";
		}

		public int HueID()
		{
			return this.hue;
		}

		public ushort Pixel(ushort c)
		{
			if ((c & 0x1F) == ((c >> 10) & 0x1F) && (c & 0x1F) == ((c >> 5) & 0x1F))
			{
				return this.m_Data.colors[c >> 10];
			}
			return c;
		}

		public int Pixel32(int input)
		{
			input = Engine.C32216(input);
			return this.m_Data.colors[32 + ((input >> 10) & 0x1F)];
		}

		public PartialHue(HueData hd, int hueId, int hueIndex)
		{
			this.hue = hueId;
			this.m_Data = hd;
			this._shaderData = new ShaderData(PartialHue._baseShaderData.PixelShader, Hues.GetHueTexture(), TextureTransparency.NotSpecified);
			this._shaderData.RenderCallback = RenderCallback;
			int num = (hueIndex & 0xF) * 32;
			int num2 = hueIndex >> 4;
			this._offset = new Vector4((float)(1 + num * 2) / 1024f, (float)(1 + num2 * 2) / 512f, 0f, 0f);
			this._provider = new DynamicCacheGraphicProvider(new ShadedGraphicProvider(this._shaderData, Hues.Default));
		}

		public Frames GetAnimation(int RealID)
		{
			return this._provider.GetAnimation(RealID);
		}

		public Texture GetTerrainTexture(int TextureID)
		{
			return this._provider.GetTerrainTexture(TextureID);
		}

		public Texture GetTerrainIsometric(int LandID)
		{
			return this._provider.GetTerrainIsometric(LandID);
		}

		public Texture GetGump(int GumpID)
		{
			return this._provider.GetGump(GumpID);
		}

		public Texture GetItem(int ItemID)
		{
			return this._provider.GetItem(ItemID);
		}

		public Texture GetLight(int lightId)
		{
			return this._provider.GetLight(lightId);
		}

		public void Dispose()
		{
			if (this._provider != null)
			{
				this._provider.Dispose();
				this._provider = null;
			}
		}

		public unsafe void CopyPixels(void* pvSrc, void* pvDest, int Pixels)
		{
			fixed (ushort* colors = this.m_Data.colors)
			{
				ushort* ptr = (ushort*)pvSrc;
				ushort* ptr2 = (ushort*)pvDest;
				ushort* ptr3 = ptr + Pixels;
				while (ptr < ptr3)
				{
					int num = *(ptr++);
					if ((num & 0x1F) == ((num >> 5) & 0x1F) && (num & 0x1F) == ((num >> 10) & 0x1F))
					{
						*(ptr2++) = colors[(num >> 10) | 0x20];
					}
					else
					{
						*(ptr2++) = (ushort)(num | 0x8000);
					}
				}
			}
		}

		public unsafe void CopyEncodedLine(ushort* pSrc, ushort* pSrcEnd, ushort* pDest, ushort* pEnd)
		{
			ushort* ptr = pDest;
			while (pDest < pEnd)
			{
				ushort num = *pSrc;
				ushort num2 = pSrc[1];
				pSrc += 2;
				ptr += (int)num2;
				if (num != 0)
				{
					num = (((num & 0x1F) != ((num >> 5) & 0x1F) || (num & 0x1F) != ((num >> 10) & 0x1F)) ? ((ushort)(num | 0x8000)) : this.m_Data.colors[(num >> 10) | 0x20]);
					while (pDest < ptr)
					{
						*(pDest++) = num;
					}
				}
				else
				{
					pDest += (int)num2;
				}
			}
		}

		static PartialHue()
		{
			PartialHue._baseShaderData = new ShaderData("PartialHueShader.cso", null, TextureTransparency.NotSpecified);
		}
	}

	private static HueData[] m_HueData;

	private static DefaultHue _default;

	private static EtherealHue _ethereal;

	private static GrayscaleHue m_Grayscale;

	private static PartialHue[] m_Partial;

	private static RegularHue[] m_Regular;

	private static ShadowHue _shadow;

	private static IHue[,] m_NotorietyHues;

	private static Texture _hueTexture;

	private const string RelativeApplicationDataPath = "Veritas/Ultima Online/Cache/Hues";

	private const string RelativeLegacyPath = "data/ultima/cache/hues.uoi";

	public static IHue Grayscale
	{
		get
		{
			if (Hues.m_Grayscale == null)
			{
				Hues.m_Grayscale = new GrayscaleHue();
			}
			return Hues.m_Grayscale;
		}
	}

	public static ShadowHue Shadow
	{
		get
		{
			if (Hues._shadow == null)
			{
				Hues._shadow = new ShadowHue();
			}
			return Hues._shadow;
		}
	}

	public static EtherealHue Ethereal
	{
		get
		{
			if (Hues._ethereal == null)
			{
				Hues._ethereal = new EtherealHue();
			}
			return Hues._ethereal;
		}
	}

	public static IHue Bright => Hues.Default;

	public static IHue Default => Hues._default;

	public static void ClearNotos()
	{
		for (int i = 0; i < 7; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				Hues.m_NotorietyHues[i, j] = null;
			}
		}
	}

	public static IHue GetNotoriety(Notoriety n)
	{
		return Hues.GetNotoriety(n, full: true);
	}

	public static IHue GetNotoriety(Notoriety n, bool full)
	{
		if ((int)n >= 1 && (int)n <= 7)
		{
			int num = (int)(n - 1);
			int num2 = (full ? 1 : 0);
			IHue hue = Hues.m_NotorietyHues[num, num2];
			if (hue == null)
			{
				hue = (Hues.m_NotorietyHues[num, num2] = Hues.Load(Preferences.Current.NotorietyHues[n] | (num2 << 15)));
			}
			return hue;
		}
		return Hues.Default;
	}

	public static HueData GetNotorietyData(Notoriety n)
	{
		if ((int)n >= 1 && (int)n <= 7)
		{
			return Hues.m_HueData[Preferences.Current.NotorietyHues[n]];
		}
		return default(HueData);
	}

	public unsafe static Texture GetHueTexture()
	{
		if (Hues._hueTexture == null)
		{
			Hues._hueTexture = new Texture(512, 256, Format.A8R8G8B8, TextureTransparency.None);
			LockData lockData = Hues._hueTexture.Lock(LockFlags.WriteOnly);
			for (int i = 0; i < Hues.m_HueData.Length; i++)
			{
				HueData hueData = Hues.m_HueData[i];
				fixed (ushort* colors = hueData.colors)
				{
					ushort* ptr = colors + 32;
					int a = Engine.C16232(*ptr);
					int b = Engine.C16232(ptr[31]);
					int num = Engine.C16232(ptr[16]);
					int* pvSrc = (int*)lockData.pvSrc;
					int num2 = (i & 0xF) * 32;
					int num3 = i >> 4;
					pvSrc += num3 * (lockData.Pitch >> 2);
					pvSrc += num2;
					for (int j = 0; j < 32; j++)
					{
						int num4 = Engine.Blend32(a, b, (j * 255 + 15) / 31);
						num4 = Engine.C16232(ptr[j]);
						pvSrc[j] = num4 | -16777216;
					}
				}
			}
			Hues._hueTexture.Unlock();
		}
		return Hues._hueTexture;
	}

	public static HueData GetData(int HueIndex)
	{
		return Hues.m_HueData[HueIndex];
	}

	public static IHue LoadByRgb(int rgbColor)
	{
		int num = (rgbColor >> 16) & 0xFF;
		int num2 = (rgbColor >> 8) & 0xFF;
		int num3 = rgbColor & 0xFF;
		num >>= 3;
		num2 >>= 3;
		num3 >>= 3;
		int num4 = 1000;
		int hueId = 0;
		for (int i = 0; i < 3000; i++)
		{
			int num5 = Hues.m_HueData[i].colors[56];
			int num6 = (num5 >> 10) & 0x1F;
			int num7 = (num5 >> 5) & 0x1F;
			int num8 = num5 & 0x1F;
			num6 = Math.Abs(num6 - num);
			num7 = Math.Abs(num7 - num2);
			num8 = Math.Abs(num8 - num3);
			int num9 = num6 + num7 + num8;
			if (num9 < num4)
			{
				num4 = num9;
				hueId = i + 1;
			}
		}
		return Hues.Load(hueId);
	}

	private static string GetCachePath()
	{
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Veritas/Ultima Online/Cache/Hues");
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(text));
		if (!directoryInfo.Exists)
		{
			directoryInfo.Create();
		}
		return text;
	}

	unsafe static Hues()
	{
		Hues.m_NotorietyHues = new IHue[7, 2];
		Debug.TimeBlock("Initializing Hues");
		Hues._default = new DefaultHue();
		Hues.m_HueData = new HueData[3000];
		Hues.m_Partial = new PartialHue[3000];
		Hues.m_Regular = new RegularHue[3000];
		string cachePath = Hues.GetCachePath();
		if (!File.Exists(cachePath))
		{
			string text = Engine.FileManager.BasePath("data/ultima/cache/hues.uoi");
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
		}
		FileInfo fileInfo = new FileInfo(Engine.FileManager.ResolveMUL(Files.Hues));
		FileInfo fileInfo2 = new FileInfo(Engine.FileManager.ResolveMUL(Files.Verdata));
		if (File.Exists(cachePath))
		{
			using FileStream input = new FileStream(cachePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using BinaryReader binaryReader = new BinaryReader(input);
			DateTime dateTime = DateTime.FromFileTime(binaryReader.ReadInt64());
			DateTime dateTime2 = DateTime.FromFileTime(binaryReader.ReadInt64());
			if (fileInfo.LastWriteTime == dateTime && fileInfo2.LastWriteTime == dateTime2)
			{
				int num = 3000;
				int num2 = 0;
				byte[] src = binaryReader.ReadBytes(num * 68);
				int num3 = 0;
				while (num2 < num)
				{
					HueData hueData = new HueData
					{
						colors = new ushort[64]
					};
					Buffer.BlockCopy(src, num3, hueData.colors, 64, 64);
					num3 += 68;
					Hues.m_HueData[num2++] = hueData;
				}
				Hues.Patch();
				Debug.EndBlock();
				return;
			}
		}
		int num4 = 265500;
		byte[] array = new byte[num4];
		Stream stream = Engine.FileManager.OpenMUL(Files.Hues);
		stream.Read(array, 0, num4);
		stream.Close();
		fixed (byte* ptr = array)
		{
			int num5 = 0;
			int num6 = 0;
			short* ptr2 = (short*)ptr;
			do
			{
				ptr2 += 2;
				int num7 = 0;
				do
				{
					HueData hueData2 = new HueData
					{
						colors = new ushort[64]
					};
					for (int i = 0; i < 32; i++)
					{
						hueData2.colors[32 + i] = (ushort)(*(ptr2++));
					}
					hueData2.tableStart = *(ptr2++);
					hueData2.tableEnd = *(ptr2++);
					Hues.m_HueData[num5++] = hueData2;
					ptr2 += 10;
				}
				while (++num7 < 8);
			}
			while (++num6 < 375);
		}
		Stream stream2 = Engine.FileManager.OpenMUL(Files.Verdata);
		array = new byte[stream2.Length];
		stream2.Read(array, 0, array.Length);
		stream2.Close();
		fixed (byte* ptr3 = array)
		{
			int* ptr4 = (int*)ptr3;
			int num8 = *(ptr4++);
			int num9 = 0;
			while (num9++ < num8)
			{
				int num10 = *(ptr4++);
				if (num10 == 32)
				{
					int num11 = *(ptr4++);
					int num12 = *(ptr4++);
					int num13 = *(ptr4++);
					int num14 = *(ptr4++);
					short* ptr5 = (short*)(ptr3 + num12) + 2;
					for (int j = 0; j < 8; j++)
					{
						HueData hueData3 = new HueData
						{
							colors = new ushort[64]
						};
						for (int k = 0; k < 32; k++)
						{
							hueData3.colors[32 + k] = (ushort)(*(ptr5++));
						}
						hueData3.tableStart = *(ptr5++);
						hueData3.tableEnd = *(ptr5++);
						Hues.m_HueData[(num11 << 3) + j] = hueData3;
						ptr5 += 10;
					}
				}
				else
				{
					ptr4 += 4;
				}
			}
		}
		using (FileStream output = new FileStream(cachePath, FileMode.Create, FileAccess.Write, FileShare.None))
		{
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write(fileInfo.LastWriteTime.ToFileTime());
			binaryWriter.Write(fileInfo2.LastWriteTime.ToFileTime());
			int num15 = 3000;
			for (int l = 0; l < num15; l++)
			{
				HueData hueData4 = Hues.m_HueData[l];
				for (int m = 0; m < 32; m++)
				{
					hueData4.colors[32 + m] |= 32768;
					binaryWriter.Write(hueData4.colors[32 + m]);
				}
				binaryWriter.Write(hueData4.tableStart);
				binaryWriter.Write(hueData4.tableEnd);
			}
		}
		Hues.Patch();
		Debug.EndBlock();
	}

	public static void Patch()
	{
		HueData hueData = Hues.m_HueData[2999];
		for (int i = 0; i < 32; i++)
		{
			int num = 8 + i * 7 / 8;
			if (num > 31)
			{
				num = 31;
			}
			hueData.colors[32 + i] = (ushort)(0x8000 | (num << 10) | (num << 5) | num);
		}
		hueData = Hues.m_HueData[2801];
		for (int j = 0; j < 32; j++)
		{
			int num2 = 0;
			int num3 = ((j >= 16) ? ((j < 20) ? (1 + (j - 16)) : (6 + (j - 20) * 2)) : 0);
			int num4 = 0;
			hueData.colors[32 + j] = (ushort)(0x8000 | (num2 << 10) | (num3 << 5) | num4);
		}
		hueData = Hues.m_HueData[2802];
		for (int k = 0; k < 32; k++)
		{
			int num5 = (k + 1) / 2;
			int num6 = (k + 1) / 2;
			int num7 = k;
			hueData.colors[32 + k] = (ushort)(0x8000 | (num5 << 10) | (num6 << 5) | num7);
		}
		hueData = Hues.m_HueData[2803];
		for (int l = 0; l < 32; l++)
		{
			int num8 = ((l >= 16) ? ((l < 20) ? (1 + (l - 16)) : (6 + (l - 20) * 2)) : 0);
			int num9 = num8 / 4;
			int num10 = num8 / 8;
			hueData.colors[32 + l] = (ushort)(0x8000 | (num8 << 10) | (num9 << 5) | num10);
		}
		hueData = Hues.m_HueData[2806];
		for (int m = 0; m < 32; m++)
		{
			int num11 = ((m >= 24) ? (1 + (m - 24)) : 0);
			int num12 = 0;
			int num13 = ((m >= 16) ? ((m < 20) ? (1 + (m - 16)) : (6 + (m - 20) * 2)) : 0);
			hueData.colors[32 + m] = (ushort)(0x8000 | (num11 << 10) | (num12 << 5) | num13);
		}
		hueData = Hues.m_HueData[2830];
		int[] array = new int[32]
		{
			0, 1, 2, 3, 4, 6, 8, 11, 14, 17,
			20, 23, 26, 29, 30, 31, 31, 31, 31, 31,
			31, 31, 31, 31, 31, 31, 31, 31, 31, 31,
			31, 31
		};
		for (int n = 0; n < 32; n++)
		{
			int num14 = array[n];
			int num15 = (num14 + 1) / 2;
			int num16 = 0;
			hueData.colors[32 + n] = (ushort)(0x8000 | (num14 << 10) | (num15 << 5) | num16);
		}
		hueData = Hues.m_HueData[2831];
		for (int num17 = 0; num17 < 32; num17++)
		{
			int num18 = ((num17 >= 16) ? ((num17 < 20) ? (1 + (num17 - 16)) : (6 + (num17 - 20) * 2)) : 0);
			int num19 = (num18 + 1) / 2;
			int num20 = 0;
			hueData.colors[32 + num17] = (ushort)(0x8000 | (num18 << 10) | (num19 << 5) | num20);
		}
		hueData = Hues.m_HueData[2840];
		for (int num21 = 0; num21 < 32; num21++)
		{
			int num22 = num21;
			int num23 = 0;
			int num24 = 0;
			hueData.colors[32 + num21] = (ushort)(0x8000 | (num22 << 10) | (num23 << 5) | num24);
		}
		hueData = Hues.m_HueData[2860];
		for (int num25 = 0; num25 < 32; num25++)
		{
			int num26 = ((num25 >= 16) ? ((num25 < 20) ? (1 + (num25 - 16)) : (6 + (num25 - 20) * 2)) : 0);
			int num27 = num26;
			int num28 = 0;
			hueData.colors[32 + num25] = (ushort)(0x8000 | (num26 << 10) | (num27 << 5) | num28);
		}
		hueData = Hues.m_HueData[2861];
		for (int num29 = 0; num29 < 32; num29++)
		{
			int num30 = ((num29 >= 8) ? ((num29 < 16) ? (1 + (num29 - 8) / 2) : ((num29 < 25) ? (5 + (num29 - 16)) : (15 + (num29 - 25) * 2))) : 0);
			int num31 = num30;
			int num32 = 0;
			hueData.colors[32 + num29] = (ushort)(0x8000 | (num30 << 10) | (num31 << 5) | num32);
		}
		hueData = Hues.m_HueData[2862];
		for (int num33 = 0; num33 < 32; num33++)
		{
			int num34 = ((num33 >= 8) ? ((num33 < 16) ? (1 + (num33 - 8) / 2) : ((num33 < 25) ? (5 + (num33 - 16)) : (15 + (num33 - 25) * 2))) : 0);
			int num35 = num34;
			int num36 = num34;
			hueData.colors[32 + num33] = (ushort)(0x8000 | (num34 << 10) | (num35 << 5) | num36);
		}
	}

	public static void Dispose()
	{
		if (Hues._default != null)
		{
			Hues._default.Dispose();
			Hues._default = null;
		}
		if (Hues.m_Grayscale != null)
		{
			Hues.m_Grayscale.Dispose();
			Hues.m_Grayscale = null;
		}
		if (Hues._shadow != null)
		{
			Hues._shadow.Dispose();
			Hues._shadow = null;
		}
		if (Hues._ethereal != null)
		{
			Hues._ethereal.Dispose();
			Hues._ethereal = null;
		}
		for (int i = 0; i < 3000; i++)
		{
			if (Hues.m_Partial[i] != null)
			{
				Hues.m_Partial[i].Dispose();
				Hues.m_Partial[i] = null;
			}
			if (Hues.m_Regular[i] != null)
			{
				Hues.m_Regular[i].Dispose();
				Hues.m_Regular[i] = null;
			}
		}
		if (Hues._hueTexture != null)
		{
			Hues._hueTexture.Dispose();
			Hues._hueTexture = null;
		}
		Hues.m_Partial = null;
		Hues.m_Regular = null;
		Hues.m_HueData = null;
	}

	public static IHue GetItemHue(int itemID, int hue)
	{
		hue ^= (int)(((Map.m_ItemFlags[itemID & 0x3FFF].Value64 >> 3) & 0x8000) ^ 0x8000);
		return Hues.Load(hue);
	}

	public static IHue GetMobileHue(int hue)
	{
		return Hues.Load(hue ^ 0x8000);
	}

	public static IHue Load(int hueId)
	{
		IHue hue;
		if ((hueId & 0x4000) == 0)
		{
			int num = (hueId & 0x3FFF) - 1;
			if (num >= 0 && num < 3000)
			{
				if ((hueId & 0x8000) == 0)
				{
					hue = Hues.m_Partial[num];
					if (hue == null)
					{
						hue = (Hues.m_Partial[num] = new PartialHue(Hues.m_HueData[num], hueId, num));
					}
				}
				else
				{
					hue = Hues.m_Regular[num];
					if (hue == null)
					{
						hue = (Hues.m_Regular[num] = new RegularHue(Hues.m_HueData[num], hueId, num));
					}
				}
			}
			else
			{
				hue = Hues.Default;
			}
		}
		else
		{
			hue = Hues.Ethereal;
		}
		return hue;
	}
}
