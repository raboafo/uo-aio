using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using SharpDX;
using SharpDX.Direct3D9;
using Veritas;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class Texture
{
	protected TextureTransparency _transparency;

	protected bool m_Flip;

	protected int m_TexWidth;

	protected int m_TexHeight;

	public int Width;

	public int Height;

	public int _averageColor;

	public ShaderData _shaderData;

	protected float m_fWidth;

	protected float m_fHeight;

	public int xMin;

	public int yMin;

	public int xMax;

	public int yMax;

	protected float _minTu;

	protected float _maxTu;

	protected float _minTv;

	protected float _maxTv;

	public SharpDX.Direct3D9.Texture m_Surface;

	public const ushort Opaque = 32768;

	public const ushort Transparent = 0;

	protected static int m_MaxTextureWidth;

	protected static int m_MaxTextureHeight;

	protected static int m_MinTextureWidth;

	protected static int m_MinTextureHeight;

	protected static bool m_Pow2;

	protected static bool m_Square;

	protected static bool m_CanSysMem;

	protected static bool m_CanVidMem;

	protected static int m_MaxAspect;

	protected static int[] m_2Pow;

	private const int DoubleOpaque = -2147450880;

	private static Texture m_Empty;

	public TextureFactory m_Factory;

	public object[] m_FactoryArgs;

	public static List<Texture> m_Textures;

	private bool m_FourBPP;

	private bool m_Disposed;

	private DataStream m_LockStream;

	public int m_LastAccess;

	private static TransformedColoredTextured[] m_PoolXYWH;

	private static TransformedColoredTextured[] m_PoolClipped;

	private static TransformedColoredTextured[] m_BadClipperPool;

	public TextureVB[] m_VBs;

	public static int MaxAspect
	{
		get
		{
			return Texture.m_MaxAspect;
		}
		set
		{
			Texture.m_MaxAspect = value;
		}
	}

	public static int MinTextureWidth
	{
		get
		{
			return Texture.m_MinTextureWidth;
		}
		set
		{
			Texture.m_MinTextureWidth = value;
		}
	}

	public static int MinTextureHeight
	{
		get
		{
			return Texture.m_MinTextureHeight;
		}
		set
		{
			Texture.m_MinTextureHeight = value;
		}
	}

	public static int MaxTextureWidth
	{
		get
		{
			return Texture.m_MaxTextureWidth;
		}
		set
		{
			Texture.m_MaxTextureWidth = value;
		}
	}

	public static int MaxTextureHeight
	{
		get
		{
			return Texture.m_MaxTextureHeight;
		}
		set
		{
			Texture.m_MaxTextureHeight = value;
		}
	}

	public static bool CanSysMem
	{
		get
		{
			return Texture.m_CanSysMem;
		}
		set
		{
			Texture.m_CanSysMem = value;
		}
	}

	public static bool CanVidMem
	{
		get
		{
			return Texture.m_CanVidMem;
		}
		set
		{
			Texture.m_CanVidMem = value;
		}
	}

	public static bool Pow2
	{
		get
		{
			return Texture.m_Pow2;
		}
		set
		{
			Texture.m_Pow2 = value;
		}
	}

	public static bool Square
	{
		get
		{
			return Texture.m_Square;
		}
		set
		{
			Texture.m_Square = value;
		}
	}

	public TextureTransparency Transparency
	{
		get
		{
			if (this._shaderData != null && this._shaderData.Transparency > this._transparency)
			{
				return this._shaderData.Transparency;
			}
			return this._transparency;
		}
		set
		{
			this._transparency = value;
		}
	}

	public SharpDX.Direct3D9.Texture Surface
	{
		get
		{
			this.DisposedCheck();
			return this.CoreGetSurface();
		}
	}

	public static Texture Empty
	{
		get
		{
			if (Texture.m_Empty == null)
			{
				Texture.m_Empty = new Texture();
			}
			return Texture.m_Empty;
		}
	}

	public float MaxTU
	{
		get
		{
			this.DisposedCheck();
			return this._maxTu;
		}
	}

	public float MaxTV
	{
		get
		{
			this.DisposedCheck();
			return this._maxTv;
		}
	}

	public bool Flip
	{
		get
		{
			this.DisposedCheck();
			return this.m_Flip;
		}
		set
		{
			this.DisposedCheck();
			this.m_Flip = value;
		}
	}

	public unsafe static void FillPixels(void* pvDest, int Color, int Pixels)
	{
		int num = Pixels >> 1;
		int* ptr = (int*)pvDest;
		int num2 = (Color << 16) | Color | -2147450880;
		while (--num >= 0)
		{
			*(ptr++) = num2;
		}
		if ((Pixels & 1) != 0)
		{
			*(short*)ptr = (short)(Color | 0x8000);
		}
	}

	public unsafe static void ClearPixels(void* pvClear, int Pixels)
	{
		int num = Pixels >> 1;
		int* ptr = (int*)pvClear;
		while (--num >= 0)
		{
			*(ptr++) = 0;
		}
		if ((Pixels & 1) != 0)
		{
			*(short*)ptr = 0;
		}
	}

	public unsafe static void CopyPixels(void* pvSrc, void* pvDest, int Pixels)
	{
		int num = Pixels >> 1;
		int* ptr = (int*)pvSrc;
		int* ptr2 = (int*)pvDest;
		int num2 = num & 7;
		num >>= 3;
		while (--num >= 0)
		{
			*ptr2 = *ptr | -2147450880;
			ptr2[1] = ptr[1] | -2147450880;
			ptr2[2] = ptr[2] | -2147450880;
			ptr2[3] = ptr[3] | -2147450880;
			ptr2[4] = ptr[4] | -2147450880;
			ptr2[5] = ptr[5] | -2147450880;
			ptr2[6] = ptr[6] | -2147450880;
			ptr2[7] = ptr[7] | -2147450880;
			ptr2 += 8;
			ptr += 8;
		}
		while (--num2 >= 0)
		{
			*(ptr2++) = *(ptr++) | -2147450880;
		}
		if ((Pixels & 1) != 0)
		{
			*(short*)ptr2 = (short)(0x8000 | *(ushort*)ptr);
		}
	}

	public unsafe static explicit operator Texture(Bitmap bmp)
	{
		int width = bmp.Width;
		int num = bmp.Height;
		Texture texture = new Texture(width, num, TextureTransparency.Simple);
		LockData lockData = texture.Lock(LockFlags.WriteOnly);
		BitmapData bitmapData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, width, num), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
		short* ptr = (short*)bitmapData.Scan0.ToPointer();
		short* ptr2 = (short*)lockData.pvSrc;
		int num2 = (bitmapData.Stride >> 1) - width;
		int num3 = (lockData.Pitch >> 1) - width;
		while (--num >= 0)
		{
			int num4 = width;
			while (--num4 >= 0)
			{
				*(ptr2++) = *(ptr++);
			}
			ptr += num2;
			ptr2 += num3;
		}
		bmp.UnlockBits(bitmapData);
		texture.Unlock();
		return texture;
	}

	public unsafe Bitmap ToBitmap()
	{
		this.DisposedCheck();
		Bitmap bitmap;
		if (this.m_FourBPP)
		{
			bitmap = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
			LockData lockData = this.Lock(LockFlags.ReadOnly);
			BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, this.Width, this.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			for (int i = 0; i < this.Height; i++)
			{
				int* ptr = (int*)((int)lockData.pvSrc + i * lockData.Pitch);
				int* ptr2 = (int*)(bitmapData.Scan0.ToInt32() + i * bitmapData.Stride);
				int num = 0;
				while (num++ < this.Width)
				{
					int num2 = *(ptr++);
					*(ptr2++) = num2;
				}
			}
			this.Unlock();
			bitmap.UnlockBits(bitmapData);
		}
		else
		{
			bitmap = new Bitmap(this.Width, this.Height, PixelFormat.Format16bppArgb1555);
			LockData lockData2 = this.Lock(LockFlags.ReadOnly);
			BitmapData bitmapData2 = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, this.Width, this.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
			for (int j = 0; j < this.Height; j++)
			{
				ushort* ptr3 = (ushort*)((int)lockData2.pvSrc + j * lockData2.Pitch);
				ushort* ptr4 = (ushort*)(bitmapData2.Scan0.ToInt32() + j * bitmapData2.Stride);
				int num3 = 0;
				while (num3++ < this.Width)
				{
					ushort num4 = *(ptr3++);
					*(ptr4++) = num4;
				}
			}
			this.Unlock();
			bitmap.UnlockBits(bitmapData2);
		}
		return bitmap;
	}

	public static void DisposeAll()
	{
		StreamWriter streamWriter = null;
		Texture[] array = Texture.m_Textures.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Texture texture = array[i];
			if (texture == null)
			{
				continue;
			}
			if (texture.m_Surface != null)
			{
				string text = "Texture leak found";
				Debug.Trace(text);
				if (streamWriter == null)
				{
					streamWriter = new StreamWriter(Engine.FileManager.CreateUnique("data/ultima/logs/textures", ".log"));
				}
				streamWriter.WriteLine(text);
				streamWriter.Flush();
			}
			if (!texture.m_Disposed)
			{
				texture.Dispose();
			}
			array[i] = null;
		}
		Texture.m_Textures.Clear();
		Texture.m_Textures = null;
		Texture.m_2Pow = null;
		streamWriter?.Close();
	}

	static Texture()
	{
		Texture.m_BadClipperPool = VertexConstructor.Create();
		int num = 24;
		Texture.m_Textures = new List<Texture>(1024);
		Texture.m_2Pow = new int[num];
		for (int i = 0; i < num; i++)
		{
			Texture.m_2Pow[i] = 1 << i;
		}
		Texture.m_PoolXYWH = VertexConstructor.Create();
		Texture.m_PoolClipped = VertexConstructor.Create();
	}

	public static Texture Clone(Texture original, ShaderData shaderData)
	{
		if (original == null || original.IsEmpty())
		{
			return original;
		}
		Texture texture = new Texture();
		texture._shaderData = shaderData;
		texture._transparency = original._transparency;
		texture.m_Surface = original.m_Surface;
		texture.m_TexWidth = original.m_TexWidth;
		texture.m_TexHeight = original.m_TexHeight;
		texture.Width = original.Width;
		texture.Height = original.Height;
		texture.m_fWidth = original.m_fWidth;
		texture.m_fHeight = original.m_fHeight;
		texture._minTu = original._minTu;
		texture._maxTu = original._maxTu;
		texture._minTv = original._minTv;
		texture._maxTv = original._maxTv;
		texture.xMin = original.xMin;
		texture.yMin = original.yMin;
		texture.xMax = original.xMax;
		texture.yMax = original.yMax;
		texture.m_Disposed = original.m_Disposed;
		texture.m_Flip = original.m_Flip;
		texture.m_FourBPP = original.m_FourBPP;
		texture.m_LastAccess = original.m_LastAccess;
		return texture;
	}

	protected Texture()
	{
		this._transparency = TextureTransparency.None;
	}

	public bool IsEmpty()
	{
		this.DisposedCheck();
		return this.m_Surface == null;
	}

	public unsafe virtual bool HitTest(int x, int y)
	{
		this.DisposedCheck();
		SharpDX.Direct3D9.Texture texture = this.CoreGetSurface();
		if (texture == null)
		{
			return false;
		}
		x = ((x >= 0) ? (x % this.Width) : (this.Width - 1 - -x % this.Width));
		y = ((y >= 0) ? (y % this.Height) : (this.Height - 1 - -y % this.Height));
		if (x < this.xMin || x > this.xMax || y < this.yMin || y > this.yMax)
		{
			return false;
		}
		LockData lockData = this.Lock(LockFlags.ReadOnly);
		bool result = ((!this.m_FourBPP) ? ((*(short*)((long)lockData.pvSrc + (long)(y * lockData.Pitch) + (x << 1)) & 0x8000) != 0) : (((*(int*)((long)lockData.pvSrc + (long)(y * lockData.Pitch) + (x << 2)) >> 24) & 0xFF) != 0));
		this.Unlock();
		return result;
	}

	public void Clear()
	{
		this.DisposedCheck();
		this.Clear(this.Lock(LockFlags.WriteOnly));
		this.Unlock();
	}

	public unsafe void Clear(LockData ld)
	{
		this.DisposedCheck();
		int num = ld.Pitch * ld.Height;
		int num2 = num >> 2;
		num &= 3;
		int* pvSrc = (int*)ld.pvSrc;
		while (--num2 >= 0)
		{
			*(pvSrc++) = 0;
		}
		if (num != 0)
		{
			byte* pvSrc2 = (byte*)ld.pvSrc;
			while (--num != 0)
			{
				*(pvSrc2++) = 0;
			}
		}
	}

	public unsafe void Clear(ushort Color)
	{
		this.DisposedCheck();
		LockData lockData = this.Lock(LockFlags.WriteOnly);
		ushort* pvSrc = (ushort*)lockData.pvSrc;
		int num = this.m_TexHeight * (lockData.Pitch >> 1);
		while (num-- != 0)
		{
			*(pvSrc++) = Color;
		}
		this.Unlock();
	}

	public static Texture[] FromImageSet(string path, int width, int height, int columns, int rows)
	{
		List<Texture> list = new List<Texture>();
		ArchivedFile archivedFile = Engine.FileManager.GetArchivedFile(path);
		if (archivedFile != null)
		{
			using Stream stream = archivedFile.Download();
			using Bitmap bitmap = new Bitmap(stream);
			BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width * columns, rows * height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			int num = 0;
			int num2 = 0;
			while (num < rows)
			{
				int num3 = 0;
				int num4 = 0;
				while (num3 < columns)
				{
					list.Add(Texture.FromBitmap(bitmapData, num4, num2, width, height));
					num3++;
					num4 += width;
				}
				num++;
				num2 += height;
			}
			bitmap.UnlockBits(bitmapData);
		}
		return list.ToArray();
	}

	public static Texture FromBitmap(Bitmap bitmap)
	{
		return new Texture(bitmap);
	}

	public static Texture FromBitmap(BitmapData bitmapData, int x, int y, int width, int height)
	{
		if (bitmapData == null)
		{
			throw new ArgumentNullException("bitmapData");
		}
		if (x < 0 || y < 0 || width < 0 || height < 0)
		{
			throw new ArgumentException("Position and size must be greater than or equal to zero.");
		}
		if (x + width > bitmapData.Width || y + height > bitmapData.Height)
		{
			throw new ArgumentException("Specified region must be contained entirely within the bitmap data bounds.");
		}
		return Texture.FromBitmapAux(bitmapData, x, y, width, height);
	}

	private unsafe static Texture FromBitmapAux(BitmapData bitmapData, int x, int y, int width, int height)
	{
		Texture texture = new Texture(width, height, Format.A8R8G8B8, TextureTransparency.Complex);
		LockData lockData = texture.Lock(LockFlags.WriteOnly);
		int num = 0;
		while (num < height)
		{
			int* ptr = (int*)((byte*)(void*)bitmapData.Scan0 + y * bitmapData.Stride) + x;
			int* ptr2 = (int*)((byte*)lockData.pvSrc + num * lockData.Pitch);
			int* ptr3 = ptr + width;
			while (ptr < ptr3)
			{
				*(ptr2++) = *(ptr++);
			}
			num++;
			y++;
		}
		texture.Unlock();
		return texture;
	}

	private Texture(Bitmap bmp)
	{
		this.Width = bmp.Width;
		this.Height = bmp.Height;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			bmp.Save(memoryStream, ImageFormat.Bmp);
			this.m_Surface = SharpDX.Direct3D9.Texture.FromMemory(Engine.m_Device, memoryStream.ToArray(), Usage.None, Pool.Managed);
			memoryStream.Close();
		}
		SurfaceDescription levelDescription = this.m_Surface.GetLevelDescription(0);
		this.m_FourBPP = levelDescription.Format == Format.A8R8G8B8;
		this._transparency = (this.m_FourBPP ? TextureTransparency.Complex : TextureTransparency.Simple);
		this.m_TexWidth = levelDescription.Width;
		this.m_TexHeight = levelDescription.Height;
		this._maxTu = (float)this.Width / (float)this.m_TexWidth;
		this._maxTv = (float)this.Height / (float)this.m_TexHeight;
		this.m_fWidth = this.Width;
		this.m_fHeight = this.Height;
		this.xMax = this.Width - 1;
		this.yMax = this.Height - 1;
		Texture.m_Textures.Add(this);
	}

	public Texture(int Width, int Height, TextureTransparency transparency)
		: this(Width, Height, Format.A1R5G5B5, transparency)
	{
	}

	public Texture(int Width, int Height, Format fmt, TextureTransparency transparency)
		: this(Width, Height, fmt, Pool.Managed, transparency)
	{
	}

	public Texture(int Width, int Height, Format fmt, Pool pool, TextureTransparency transparency)
		: this(Width, Height, fmt, pool, isReconstruct: false, transparency)
	{
	}

	public Texture(int Width, int Height, Format fmt, Pool pool, bool isReconstruct, TextureTransparency transparency)
		: this(Width, Height, fmt, pool, isReconstruct, transparency, Usage.None)
	{
	}

	public Texture(int Width, int Height, Format fmt, Pool pool, bool isReconstruct, TextureTransparency transparency, Usage usage)
	{
		this._transparency = transparency;
		int num = 0;
		int num2 = 0;
		if (Texture.m_Pow2)
		{
			int num3 = 0;
			while (num < Width)
			{
				num = Texture.m_2Pow[num3++];
			}
			num3 = 0;
			while (num2 < Height)
			{
				num2 = Texture.m_2Pow[num3++];
			}
		}
		else
		{
			num = Width;
			num2 = Height;
		}
		if (Texture.m_MaxAspect != 0)
		{
			double num4 = 0.0;
			if (((num <= num2) ? ((double)num2 / (double)num) : ((double)num / (double)num2)) > (double)Texture.m_MaxAspect)
			{
				if (num > num2)
				{
					num2 = num / Texture.m_MaxAspect;
				}
				else
				{
					num = num2 / Texture.m_MaxAspect;
				}
			}
		}
		if (num < Texture.m_MinTextureWidth)
		{
			num = Texture.m_MinTextureWidth;
		}
		if (num2 < Texture.m_MinTextureHeight)
		{
			num2 = Texture.m_MinTextureHeight;
		}
		if (Texture.m_Square)
		{
			if (num > num2)
			{
				num2 = num;
			}
			else if (num < num2)
			{
				num = num2;
			}
		}
		if (num <= Texture.m_MaxTextureWidth && num2 <= Texture.m_MaxTextureHeight)
		{
			this.Width = Width;
			this.Height = Height;
			this.m_TexWidth = num;
			this.m_TexHeight = num2;
			this.m_FourBPP = fmt == Format.A8R8G8B8;
			this._minTu = 1f / (float)(this.m_TexWidth * 2);
			this._minTv = 1f / (float)(this.m_TexHeight * 2);
			this._maxTu = (float)(Width * 2 - 1) / (float)(this.m_TexWidth * 2);
			this._maxTv = (float)(Height * 2 - 1) / (float)(this.m_TexHeight * 2);
			this.m_fWidth = Width;
			this.m_fHeight = Height;
			this.m_Surface = new SharpDX.Direct3D9.Texture(Engine.m_Device, this.m_TexWidth, this.m_TexHeight, 1, usage, fmt, pool);
			this.xMax = Width - 1;
			this.yMax = Height - 1;
			if (!isReconstruct)
			{
				Texture.m_Textures.Add(this);
			}
		}
	}

	public void SetPriority(int newPriority)
	{
		this.DisposedCheck();
	}

	protected void DisposedCheck()
	{
		if (this.m_Disposed)
		{
			throw new ObjectDisposedException("Texture", "Cannot access disposed Texture");
		}
	}

	public void Dispose()
	{
		if (!this.m_Disposed)
		{
			this.m_Disposed = true;
			if (this.m_Surface != null)
			{
				this.m_Surface.Dispose();
			}
			this.m_Surface = null;
		}
	}

	protected SharpDX.Direct3D9.Texture CoreGetSurface()
	{
		this.m_LastAccess = Engine.Ticks;
		if (this.m_Surface == null)
		{
			return null;
		}
		if (this.m_Surface.IsDisposed)
		{
			return this.m_Surface = this.CoreReconstruct();
		}
		return this.m_Surface;
	}

	protected SharpDX.Direct3D9.Texture CoreReconstruct()
	{
		if (this.m_Factory == null)
		{
			return null;
		}
		return this.m_Factory.Reconstruct(this.m_FactoryArgs).m_Surface;
	}

	public unsafe virtual LockData Lock(LockFlags flags)
	{
		this.DisposedCheck();
		SharpDX.Direct3D9.Texture texture = this.CoreGetSurface();
		if (texture == null)
		{
			return default(LockData);
		}
		SharpDX.Direct3D9.LockFlags lockFlags = SharpDX.Direct3D9.LockFlags.NoSystemLock;
		if (flags == LockFlags.ReadOnly)
		{
			lockFlags |= SharpDX.Direct3D9.LockFlags.ReadOnly;
		}
		DataStream stream = null;
		int pitch = -1;
		do
		{
			try
			{
				pitch = texture.LockRectangle(0, lockFlags, out stream).Pitch;
			}
			catch
			{
			}
		}
		while (stream == null);
		LockData result = new LockData
		{
			Pitch = pitch,
			pvSrc = (void*)stream.DataPointer,
			Height = this.Height,
			Width = this.Width
		};
		this.m_LockStream = stream;
		return result;
	}

	public void Unlock()
	{
		this.DisposedCheck();
		SharpDX.Direct3D9.Texture texture = this.CoreGetSurface();
		if (texture != null)
		{
			if (this.m_LockStream != null)
			{
				this.m_LockStream.Close();
			}
			this.m_LockStream = null;
			texture.UnlockRectangle(0);
		}
	}

	public void Draw(int X, int Y, int Width, int Height)
	{
		this.Draw(X, Y, Width, Height, 16777215);
	}

	public void Draw(int xScreen, int yScreen, int xWidth, int yHeight, int vColor)
	{
		this.DisposedCheck();
		if (this.m_Surface == null || xWidth <= 0 || yHeight <= 0)
		{
			return;
		}
		TransformedColoredTextured[] poolXYWH = Texture.m_PoolXYWH;
		poolXYWH[0].Color = (poolXYWH[1].Color = (poolXYWH[2].Color = (poolXYWH[3].Color = Renderer.GetQuadColor(vColor))));
		float num = -0.5f + (float)xScreen;
		float num2 = -0.5f + (float)yScreen;
		int num3 = xWidth / this.Width;
		int num4 = yHeight / this.Height;
		int num5 = xWidth % this.Width;
		int num6 = yHeight % this.Height;
		int screenWidth = Engine.ScreenWidth;
		int screenHeight = Engine.ScreenHeight;
		float tu = (float)(num5 * 2 - 1) / (float)(this.m_TexWidth * 2);
		float tv = (float)(num6 * 2 - 1) / (float)(this.m_TexHeight * 2);
		Renderer.SetTexture(this);
		if (num3 > 0 && num4 > 0)
		{
			int num7 = xScreen;
			int num8 = xScreen + this.Width;
			ref TransformedColoredTextured reference = ref poolXYWH[0];
			float x = (poolXYWH[1].X = num + this.m_fWidth);
			reference.X = x;
			ref TransformedColoredTextured reference2 = ref poolXYWH[2];
			x = (poolXYWH[3].X = num);
			reference2.X = x;
			this.ApplyQuadTuTv(poolXYWH);
			int num11 = 0;
			while (num11 < num3)
			{
				ref TransformedColoredTextured reference3 = ref poolXYWH[0];
				x = (poolXYWH[2].Y = num2 + this.m_fHeight);
				reference3.Y = x;
				ref TransformedColoredTextured reference4 = ref poolXYWH[1];
				x = (poolXYWH[3].Y = num2);
				reference4.Y = x;
				int num14 = yScreen;
				int num15 = yScreen + this.Height;
				int num16 = 0;
				while (num16 < num4)
				{
					if (num8 > 0 && num7 <= screenWidth && num15 > 0 && num14 <= screenHeight)
					{
						Renderer.DrawQuadPrecalc(poolXYWH);
					}
					num16++;
					poolXYWH[0].Y += this.m_fHeight;
					poolXYWH[1].Y += this.m_fHeight;
					poolXYWH[2].Y += this.m_fHeight;
					poolXYWH[3].Y += this.m_fHeight;
					num14 += this.Height;
					num15 += this.Height;
				}
				num11++;
				poolXYWH[0].X += this.m_fWidth;
				poolXYWH[1].X += this.m_fWidth;
				poolXYWH[2].X += this.m_fWidth;
				poolXYWH[3].X += this.m_fWidth;
				num7 += this.Width;
				num8 += this.Width;
			}
		}
		if (num3 > 0 && num6 > 0)
		{
			int num7 = xScreen;
			int num8 = xScreen + this.Width;
			int num14 = yScreen + num4 * this.Height;
			int num15 = num14 + num6;
			ref TransformedColoredTextured reference5 = ref poolXYWH[0];
			float x = (poolXYWH[1].X = num + this.m_fWidth);
			reference5.X = x;
			ref TransformedColoredTextured reference6 = ref poolXYWH[0];
			x = (poolXYWH[2].Y = -0.5f + (float)num15);
			reference6.Y = x;
			ref TransformedColoredTextured reference7 = ref poolXYWH[1];
			x = (poolXYWH[3].Y = -0.5f + (float)num14);
			reference7.Y = x;
			ref TransformedColoredTextured reference8 = ref poolXYWH[2];
			x = (poolXYWH[3].X = num);
			reference8.X = x;
			poolXYWH[0].Tu = (poolXYWH[1].Tu = this._maxTu);
			poolXYWH[0].Tv = (poolXYWH[2].Tv = tv);
			int num21 = 0;
			while (num21 < num3)
			{
				if (num8 > 0 && num7 <= screenWidth && num15 > 0 && num14 <= screenHeight)
				{
					Renderer.DrawQuadPrecalc(poolXYWH);
				}
				num21++;
				poolXYWH[0].X += this.m_fWidth;
				poolXYWH[1].X += this.m_fWidth;
				poolXYWH[2].X += this.m_fWidth;
				poolXYWH[3].X += this.m_fWidth;
				num7 += this.Width;
				num8 += this.Width;
			}
		}
		if (num4 > 0 && num5 > 0)
		{
			int num7 = xScreen + num3 * this.Width;
			int num8 = num7 + num5;
			int num14 = yScreen;
			int num15 = yScreen + this.Height;
			ref TransformedColoredTextured reference9 = ref poolXYWH[0];
			float x = (poolXYWH[1].X = -0.5f + (float)num8);
			reference9.X = x;
			ref TransformedColoredTextured reference10 = ref poolXYWH[0];
			x = (poolXYWH[2].Y = num2 + this.m_fHeight);
			reference10.Y = x;
			ref TransformedColoredTextured reference11 = ref poolXYWH[1];
			x = (poolXYWH[3].Y = num2);
			reference11.Y = x;
			ref TransformedColoredTextured reference12 = ref poolXYWH[2];
			x = (poolXYWH[3].X = -0.5f + (float)num7);
			reference12.X = x;
			poolXYWH[0].Tu = (poolXYWH[1].Tu = tu);
			poolXYWH[0].Tv = (poolXYWH[2].Tv = this._maxTv);
			int num26 = 0;
			while (num26 < num4)
			{
				if (num8 > 0 && num7 <= screenWidth && num15 > 0 && num14 <= screenHeight)
				{
					Renderer.DrawQuadPrecalc(poolXYWH);
				}
				num26++;
				poolXYWH[0].Y += this.m_fHeight;
				poolXYWH[1].Y += this.m_fHeight;
				poolXYWH[2].Y += this.m_fHeight;
				poolXYWH[3].Y += this.m_fHeight;
				num14 += this.Height;
				num15 += this.Height;
			}
		}
		if (num5 > 0 && num6 > 0)
		{
			int num7 = xScreen + num3 * this.Width;
			int num8 = num7 + num5;
			int num14 = yScreen + num4 * this.Height;
			int num15 = num14 + num6;
			if (num8 > 0 && num7 <= screenWidth && num15 > 0 && num14 <= screenHeight)
			{
				ref TransformedColoredTextured reference13 = ref poolXYWH[0];
				float x = (poolXYWH[1].X = -0.5f + (float)num8);
				reference13.X = x;
				ref TransformedColoredTextured reference14 = ref poolXYWH[0];
				x = (poolXYWH[2].Y = -0.5f + (float)num15);
				reference14.Y = x;
				ref TransformedColoredTextured reference15 = ref poolXYWH[1];
				x = (poolXYWH[3].Y = -0.5f + (float)num14);
				reference15.Y = x;
				ref TransformedColoredTextured reference16 = ref poolXYWH[2];
				x = (poolXYWH[3].X = -0.5f + (float)num7);
				reference16.X = x;
				poolXYWH[0].Tu = (poolXYWH[1].Tu = tu);
				poolXYWH[0].Tv = (poolXYWH[2].Tv = tv);
				Renderer.DrawQuadPrecalc(poolXYWH);
			}
		}
	}

	public void DrawRotated(int x, int y, double angle, int color)
	{
		this.DrawRotated(x, y, angle, color, (double)(this.xMin + this.xMax) * 0.5, (double)(this.yMin + this.yMax) * 0.5);
	}

	public unsafe void DrawRotated(int x, int y, double angle, int color, double xCenter, double yCenter)
	{
		this.DisposedCheck();
		if (this.m_Surface != null)
		{
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Start();
			}
			color = Renderer.GetQuadColor(color);
			Renderer.SetTexture(this);
			float z;
			IVertexStorage vertexStorage = Renderer.AcquireQuadStorage(out z);
			ArraySegment<byte> arraySegment = vertexStorage.Store(4, 1);
			fixed (byte* array = arraySegment.Array)
			{
				TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
				this.ApplyQuadTuTv(ptr);
				double num = (float)x - 0.5f;
				double num2 = (float)y - 0.5f;
				double num3 = this.Width;
				double num4 = this.Height;
				double num5 = num3 - xCenter;
				double num6 = num4 - yCenter;
				double num7 = Math.Atan2(num6, num5);
				double num8 = Math.Sqrt(num5 * num5 + num6 * num6);
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = (float)(num + num8 * Math.Cos(angle + num7));
				ptr->Y = (float)(num2 + num8 * Math.Sin(angle + num7));
				ptr->Z = z;
				ptr++;
				num5 = num3 - xCenter;
				num6 = 0.0 - yCenter;
				num7 = Math.Atan2(num6, num5);
				num8 = Math.Sqrt(num5 * num5 + num6 * num6);
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = (float)(num + num8 * Math.Cos(angle + num7));
				ptr->Y = (float)(num2 + num8 * Math.Sin(angle + num7));
				ptr->Z = z;
				ptr++;
				num5 = 0.0 - xCenter;
				num6 = num4 - yCenter;
				num7 = Math.Atan2(num6, num5);
				num8 = Math.Sqrt(num5 * num5 + num6 * num6);
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = (float)(num + num8 * Math.Cos(angle + num7));
				ptr->Y = (float)(num2 + num8 * Math.Sin(angle + num7));
				ptr->Z = z;
				ptr++;
				num5 = 0.0 - xCenter;
				num6 = 0.0 - yCenter;
				num7 = Math.Atan2(num6, num5);
				num8 = Math.Sqrt(num5 * num5 + num6 * num6);
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = (float)(num + num8 * Math.Cos(angle + num7));
				ptr->Y = (float)(num2 + num8 * Math.Sin(angle + num7));
				ptr->Z = z;
			}
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Stop();
			}
		}
	}

	private unsafe void ApplyQuadTuTv(TransformedColoredTextured[] vertex)
	{
		fixed (TransformedColoredTextured* pVertex = vertex)
		{
			this.ApplyQuadTuTv(pVertex);
		}
	}

	private unsafe void ApplyQuadTuTv(TransformedColoredTextured* pVertex)
	{
		if (!this.m_Flip)
		{
			pVertex->Tu = this._maxTu;
			pVertex->Tv = this._maxTv;
			pVertex[1].Tu = this._maxTu;
			pVertex[1].Tv = this._minTv;
			pVertex[2].Tu = this._minTu;
			pVertex[2].Tv = this._maxTv;
			pVertex[3].Tu = this._minTu;
			pVertex[3].Tv = this._minTv;
		}
		else
		{
			pVertex->Tu = this._minTu;
			pVertex->Tv = this._maxTv;
			pVertex[1].Tu = this._minTu;
			pVertex[1].Tv = this._minTv;
			pVertex[2].Tu = this._maxTu;
			pVertex[2].Tv = this._maxTv;
			pVertex[3].Tu = this._maxTu;
			pVertex[3].Tv = this._minTv;
		}
	}

	public void DrawShadow(int x, int y, float xCenter, float yCenter)
	{
		this.DrawStretchSkew(x, y, 0, xCenter, yCenter, 0.5f, 0.3f);
	}

	public unsafe void DrawStretchSkew(int x, int y, int color, float xCenter, float yCenter, float skew, float scale)
	{
		this.DisposedCheck();
		if (this.m_Surface != null)
		{
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Start();
			}
			color = Renderer.GetQuadColor(color);
			float num = (float)x - 0.5f;
			float num2 = (float)y - 0.5f;
			float num3 = this.Width;
			float num4 = this.Height;
			Renderer.SetTexture(this);
			float z;
			IVertexStorage vertexStorage = Renderer.AcquireQuadStorage(out z);
			ArraySegment<byte> arraySegment = vertexStorage.Store(4, 1);
			fixed (byte* array = arraySegment.Array)
			{
				TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
				this.ApplyQuadTuTv(ptr);
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = num + num3 + (yCenter - num4) * skew;
				ptr->Y = num2 + num4 + (yCenter - num4) * scale;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = num + num3 + yCenter * skew;
				ptr->Y = num2 + yCenter * scale;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = num + (yCenter - num4) * skew;
				ptr->Y = num2 + num4 + (yCenter - num4) * scale;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = num + yCenter * skew;
				ptr->Y = num2 + yCenter * scale;
				ptr->Z = z;
			}
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Stop();
			}
		}
	}

	public unsafe void DrawScaled(int x, int y, int color, float xCenter, float yCenter, float xScale, float yScale)
	{
		this.DisposedCheck();
		if (this.m_Surface != null)
		{
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Start();
			}
			color = Renderer.GetQuadColor(color);
			float num = (float)x - 0.5f;
			float num2 = (float)y - 0.5f;
			float num3 = this.Width;
			float num4 = this.Height;
			Renderer.SetTexture(this);
			float z;
			IVertexStorage vertexStorage = Renderer.AcquireQuadStorage(out z);
			ArraySegment<byte> arraySegment = vertexStorage.Store(4, 1);
			fixed (byte* array = arraySegment.Array)
			{
				TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
				this.ApplyQuadTuTv(ptr);
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = (float)x + (num3 - xCenter) * xScale;
				ptr->Y = (float)y + (num4 - yCenter) * yScale;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = (float)x + (num3 - xCenter) * xScale;
				ptr->Y = (float)y - yCenter * yScale;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = (float)x - xCenter * xScale;
				ptr->Y = (float)y + (num4 - yCenter) * yScale;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = (float)x - xCenter * xScale;
				ptr->Y = (float)y - yCenter * yScale;
				ptr->Z = z;
			}
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Stop();
			}
		}
	}

	public void DrawRotated(int x, int y, double angle)
	{
		this.DrawRotated(x, y, angle, 16777215);
	}

	public void DrawClipped(int X, int Y, Clipper Clipper)
	{
		this.DisposedCheck();
		if (Clipper == null)
		{
			this.Draw(X, Y);
		}
		else
		{
			if (this.m_Surface == null)
			{
				return;
			}
			TransformedColoredTextured[] poolClipped = Texture.m_PoolClipped;
			if (Clipper.Clip(X, Y, this.Width, this.Height, poolClipped))
			{
				poolClipped[0].Color = (poolClipped[1].Color = (poolClipped[2].Color = (poolClipped[3].Color = Renderer.GetQuadColor(16777215))));
				if (this.m_Flip)
				{
					poolClipped[3].Tu = (poolClipped[2].Tu = 1f - poolClipped[3].Tu);
					poolClipped[1].Tu = (poolClipped[0].Tu = 1f - poolClipped[1].Tu);
				}
				poolClipped[0].Tu *= this._maxTu;
				poolClipped[1].Tu *= this._maxTu;
				poolClipped[2].Tu *= this._maxTu;
				poolClipped[3].Tu *= this._maxTu;
				poolClipped[0].Tv *= this._maxTv;
				poolClipped[1].Tv *= this._maxTv;
				poolClipped[2].Tv *= this._maxTv;
				poolClipped[3].Tv *= this._maxTv;
				Renderer.SetTexture(this);
				Renderer.DrawQuadPrecalc(poolClipped);
			}
		}
	}

	public unsafe void Draw(int x, int y, int color)
	{
		this.DisposedCheck();
		if (this.m_Surface != null && x < Engine.ScreenWidth && x + this.Width > 0 && y < Engine.ScreenHeight && y + this.Height > 0)
		{
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Start();
			}
			float num = (float)x - 0.5f;
			float num2 = (float)y - 0.5f;
			float x2 = num + (float)this.Width;
			float y2 = num2 + (float)this.Height;
			color = Renderer.GetQuadColor(color);
			Renderer.SetTexture(this);
			float z;
			IVertexStorage vertexStorage = Renderer.AcquireQuadStorage(out z);
			ArraySegment<byte> arraySegment = vertexStorage.Store(4, 1);
			fixed (byte* array = arraySegment.Array)
			{
				TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
				this.ApplyQuadTuTv(ptr);
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = x2;
				ptr->Y = y2;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = x2;
				ptr->Y = num2;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = y2;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = num2;
				ptr->Z = z;
			}
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Stop();
			}
		}
	}

	public unsafe void DrawGame(int x, int y, int color)
	{
		this.DisposedCheck();
		if (this.m_Surface != null && x < Engine.GameX + Engine.GameWidth && x + this.Width > Engine.GameX && y < Engine.GameY + Engine.GameHeight && y + this.Height > Engine.GameY)
		{
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Start();
			}
			float num = (float)x - 0.5f;
			float num2 = (float)y - 0.5f;
			float x2 = num + (float)this.Width;
			float y2 = num2 + (float)this.Height;
			color = Renderer.GetQuadColor(color);
			Renderer.SetTexture(this);
			float z;
			IVertexStorage vertexStorage = Renderer.AcquireQuadStorage(out z);
			ArraySegment<byte> arraySegment = vertexStorage.Store(4, 1);
			fixed (byte* array = arraySegment.Array)
			{
				TransformedColoredTextured* ptr = (TransformedColoredTextured*)(array + arraySegment.Offset);
				this.ApplyQuadTuTv(ptr);
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = x2;
				ptr->Y = y2;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = x2;
				ptr->Y = num2;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = y2;
				ptr->Z = z;
				ptr++;
				ptr->Color = color;
				ptr->Rhw = 1f;
				ptr->X = num;
				ptr->Y = num2;
				ptr->Z = z;
			}
			if (Renderer._profile != null)
			{
				Renderer._profile._composeTime.Stop();
			}
		}
	}

	[Obsolete]
	public bool DrawGame(int x, int y, int color, TransformedColoredTextured[] pool)
	{
		this.DisposedCheck();
		if (this.m_Surface != null && y < Engine.GameY + Engine.GameHeight && y + this.Height > Engine.GameY && x < Engine.GameX + Engine.GameWidth && x + this.Width > Engine.GameX)
		{
			float num = -0.5f + (float)x;
			float num2 = -0.5f + (float)y;
			ref TransformedColoredTextured reference = ref pool[0];
			float x2 = (pool[1].X = num + this.m_fWidth);
			reference.X = x2;
			ref TransformedColoredTextured reference2 = ref pool[0];
			x2 = (pool[2].Y = num2 + this.m_fHeight);
			reference2.Y = x2;
			ref TransformedColoredTextured reference3 = ref pool[1];
			x2 = (pool[3].Y = num2);
			reference3.Y = x2;
			ref TransformedColoredTextured reference4 = ref pool[2];
			x2 = (pool[3].X = num);
			reference4.X = x2;
			pool[0].Color = (pool[1].Color = (pool[2].Color = (pool[3].Color = Renderer.GetQuadColor(color))));
			this.ApplyQuadTuTv(pool);
			Renderer.SetTexture(this);
			Renderer.DrawQuadPrecalc(pool);
			return true;
		}
		return false;
	}

	public void Draw(int x, int y)
	{
		this.Draw(x, y, 16777215);
	}

	public void DrawGame(int x, int y)
	{
		this.DrawGame(x, y, 16777215);
	}

	public void Draw(int X, int Y, int Width, int Height, float tltu, float tltv, float trtu, float trtv, float brtu, float brtv, float bltu, float bltv)
	{
		this.DisposedCheck();
		if (this.m_Surface != null)
		{
			TransformedColoredTextured[] array = VertexConstructor.Create();
			float num = -0.5f + (float)X;
			float num2 = -0.5f + (float)Y;
			float num3 = Width;
			float num4 = Height;
			array[3].X = num;
			array[3].Y = num2;
			array[1].X = num + num3;
			array[1].Y = num2;
			array[0].X = num + num3;
			array[0].Y = num2 + num4;
			array[2].X = num;
			array[2].Y = num2 + num4;
			array[0].Color = (array[1].Color = (array[2].Color = (array[3].Color = Renderer.GetQuadColor(16777215))));
			array[3].Tu = tltu;
			array[3].Tv = tltv;
			array[1].Tu = trtu;
			array[1].Tv = trtv;
			array[0].Tu = brtu;
			array[0].Tv = brtv;
			array[2].Tu = bltu;
			array[2].Tv = bltv;
			Renderer.SetTexture(this);
			Renderer.DrawQuadPrecalc(array);
		}
	}

	public TextureVB GetVB(int type, bool alphaTest, bool filter)
	{
		if (this.m_VBs == null)
		{
			this.m_VBs = new TextureVB[16];
		}
		int num = 0;
		if (alphaTest)
		{
			num |= 1;
		}
		if (filter)
		{
			num |= 2;
		}
		num |= type << 2;
		TextureVB textureVB = this.m_VBs[num];
		if (textureVB == null)
		{
			textureVB = (this.m_VBs[num] = new TextureVB());
		}
		return textureVB;
	}
}
