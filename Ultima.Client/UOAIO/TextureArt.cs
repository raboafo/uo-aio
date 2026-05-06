using System.IO;

namespace UOAIO;

public class TextureArt
{
	private class TexMapFactory : TextureFactory
	{
		private int m_TextureID;

		private IHue m_Hue;

		private byte[] m_Buffer = new byte[32768];

		private TextureArt m_Textures;

		public override TextureTransparency Transparency => TextureTransparency.None;

		public TexMapFactory(TextureArt textures)
		{
			this.m_Textures = textures;
		}

		public Texture Load(int textureID, IHue hue)
		{
			this.m_TextureID = textureID & 0x3FFF;
			this.m_Hue = hue;
			return base.Construct(isReconstruct: false);
		}

		public override Texture Reconstruct(object[] args)
		{
			this.m_TextureID = (int)args[0];
			this.m_Hue = (IHue)args[1];
			return base.Construct(isReconstruct: true);
		}

		protected override void CoreAssignArgs(Texture tex)
		{
			tex.m_Factory = this;
			tex.m_FactoryArgs = new object[2] { this.m_TextureID, this.m_Hue };
			tex._shaderData = this.m_Hue.ShaderData;
		}

		protected override bool CoreLookup()
		{
			int num = this.m_Textures.m_Lookup[this.m_TextureID];
			return num != -1;
		}

		protected override void CoreGetDimensions(out int width, out int height)
		{
			int num = this.m_Textures.m_Lookup[this.m_TextureID];
			if (num < 0)
			{
				width = (height = 128);
			}
			else
			{
				width = (height = 64);
			}
		}

		protected unsafe override void CoreProcessImage(int width, int height, int stride, ushort* pLine, ushort* pLineEnd, ushort* pImageEnd, int lineDelta, int lineEndDelta)
		{
			int num = this.m_Textures.m_Lookup[this.m_TextureID];
			if (num != -1)
			{
				int size = width * height * 2;
				this.m_Textures.m_Stream.Seek(num & 0x7FFFFFFF, SeekOrigin.Begin);
				UnsafeMethods.ReadFile((FileStream)this.m_Textures.m_Stream, this.m_Buffer, 0, size);
				fixed (byte* buffer = this.m_Buffer)
				{
					this.m_Hue.CopyPixels(buffer, pLine, width * height);
				}
			}
		}
	}

	private int[] m_Lookup;

	private Stream m_Stream;

	private TexMapFactory m_Factory;

	public unsafe TextureArt()
	{
		this.m_Lookup = new int[16384];
		Stream stream = Engine.FileManager.OpenMUL(Files.TexIdx);
		byte[] array = new byte[196608];
		UnsafeMethods.ReadFile((FileStream)stream, array, 0, array.Length);
		stream.Close();
		fixed (byte* ptr = array)
		{
			int* ptr2 = (int*)ptr;
			int num = 0;
			do
			{
				this.m_Lookup[num] = *ptr2 | (ptr2[2] << 31);
				ptr2 += 3;
			}
			while (++num < 16384);
		}
		foreach (GraphicTranslation value in GraphicTranslators.Textures.Table.Values)
		{
			if (value.FallbackId < 16384 && value.UpdatedId < 16384)
			{
				this.m_Lookup[value.UpdatedId] = this.m_Lookup[value.FallbackId];
			}
		}
		this.m_Stream = Engine.FileManager.OpenMUL(Files.TexMul);
	}

	public void Dispose()
	{
		this.m_Lookup = null;
		this.m_Stream.Close();
		this.m_Stream = null;
	}

	public Texture ReadFromDisk(int TextureID, IHue Hue)
	{
		if (this.m_Factory == null)
		{
			this.m_Factory = new TexMapFactory(this);
		}
		return this.m_Factory.Load(TextureID, Hue);
	}
}
