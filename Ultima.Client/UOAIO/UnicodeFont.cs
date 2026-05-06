using System.Collections.Generic;
using System.IO;

namespace UOAIO;

public class UnicodeFont : IFont
{
	private class UnicodeFontFactory : TextureFactory, IFontFactory
	{
		private class CharacterBits
		{
			public int m_xOffset;

			public int m_yOffset;

			public int m_xWidth;

			public int m_yHeight;

			public byte[] m_Bits;

			private static byte[] m_Header;

			public CharacterBits(FileStream stream, bool needBits)
			{
				int num = stream.Read(CharacterBits.m_Header, 0, 4);
				if (num == 4)
				{
					this.m_xOffset = (sbyte)CharacterBits.m_Header[0];
					this.m_yOffset = (sbyte)CharacterBits.m_Header[1];
					this.m_xWidth = (sbyte)CharacterBits.m_Header[2];
					this.m_yHeight = (sbyte)CharacterBits.m_Header[3];
					num = (this.m_xWidth + 7 >> 3) * this.m_yHeight;
					if (num > 0)
					{
						if (!needBits)
						{
							return;
						}
						this.m_Bits = new byte[(num + 3) & -4];
						if (stream.Read(this.m_Bits, 0, num) == num)
						{
							return;
						}
					}
				}
				this.m_xOffset = 0;
				this.m_yOffset = 4;
				this.m_xWidth = 8;
				this.m_yHeight = 10;
				this.m_Bits = UnicodeFont.m_NullChar;
			}

			static CharacterBits()
			{
				CharacterBits.m_Header = new byte[4];
			}
		}

		private UnicodeFont m_Font;

		private int m_Flags;

		private string m_String;

		private int m_xMin;

		private int m_yMin;

		private int m_xMax;

		private int m_yMax;

		private CharacterBits[] m_LowCharacters;

		private static byte[] m_Buffer;

		public override TextureTransparency Transparency => TextureTransparency.Simple;

		public UnicodeFontFactory(UnicodeFont font, int flags)
		{
			this.m_Font = font;
			this.m_Flags = flags;
		}

		public Texture CreateInstance(string text)
		{
			this.m_String = text;
			return base.Construct(isReconstruct: false);
		}

		public override Texture Reconstruct(object[] args)
		{
			this.m_String = (string)args[0];
			return base.Construct(isReconstruct: true);
		}

		protected override void CoreAssignArgs(Texture tex)
		{
			tex.m_Factory = this;
			tex.m_FactoryArgs = new object[1] { this.m_String };
			tex.xMin = this.m_xMin;
			tex.yMin = this.m_yMin;
			tex.xMax = this.m_xMax;
			tex.yMax = this.m_yMax;
		}

		protected override bool CoreLookup()
		{
			return this.m_String != null && this.m_String.Length > 0;
		}

		public int MeasureWidth(string text)
		{
			this.m_String = text;
			if (!this.CoreLookup())
			{
				return 2;
			}
			this.CoreGetDimensions(out var width, out var _);
			return width;
		}

		private CharacterBits GetCharacter(char c, bool needBits)
		{
			if (c >= '\0' && c < 'Ā')
			{
				if (this.m_LowCharacters == null)
				{
					this.m_LowCharacters = new CharacterBits[256];
				}
				CharacterBits characterBits = this.m_LowCharacters[(uint)c];
				if (characterBits == null)
				{
					characterBits = (this.m_LowCharacters[(uint)c] = this.LoadCharacter(c, needBits: true));
				}
				return characterBits;
			}
			return this.LoadCharacter(c, needBits);
		}

		private CharacterBits LoadCharacter(int index, bool needBits)
		{
			this.m_Font.m_Stream.Seek(this.m_Font.m_Lookup[index], SeekOrigin.Begin);
			return new CharacterBits((FileStream)this.m_Font.m_Stream, needBits);
		}

		protected override void CoreGetDimensions(out int width, out int height)
		{
			string text = this.m_String;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 18;
			int spaceWidth = this.m_Font.SpaceWidth;
			bool flag = false;
			foreach (char c in text)
			{
				switch (c)
				{
				case '\n':
				case '\r':
					if (!flag)
					{
						num = 0;
						num2 += 18;
						num4 += 18;
						flag = true;
					}
					continue;
				case ' ':
					flag = false;
					num += spaceWidth;
					if (num > num3)
					{
						num3 = num;
					}
					continue;
				case '\t':
					flag = false;
					num += 24;
					if (num > num3)
					{
						num3 = num;
					}
					continue;
				}
				flag = false;
				CharacterBits character = this.GetCharacter(c, needBits: false);
				if (num > 0)
				{
					num++;
				}
				num += character.m_xOffset + character.m_xWidth;
				if (num > num3)
				{
					num3 = num;
				}
				if (num2 + character.m_yOffset + character.m_yHeight > num4)
				{
					num4 = num2 + character.m_yOffset + character.m_yHeight;
				}
			}
			width = num3 + 2;
			height = num4 + 2;
		}

		protected unsafe override void CoreProcessImage(int width, int height, int stride, ushort* pLine, ushort* pLineEnd, ushort* pImageEnd, int lineDelta, int lineEndDelta)
		{
			string text = this.m_String;
			int num = 0;
			int num2 = 0;
			int spaceWidth = this.m_Font.SpaceWidth;
			bool flag = false;
			int num3 = width * height;
			byte[] array = UnicodeFontFactory.m_Buffer;
			if (array == null || num3 > array.Length)
			{
				array = (UnicodeFontFactory.m_Buffer = new byte[num3]);
			}
			bool flag2 = (this.m_Flags & 8) != 0;
			fixed (byte* ptr = array)
			{
				UnsafeMethods.ZeroMemory(ptr, num3);
				foreach (char c in text)
				{
					switch (c)
					{
					case '\n':
					case '\r':
						if (!flag)
						{
							num = 0;
							num2 += 18;
							flag = true;
						}
						continue;
					case ' ':
						flag = false;
						num += spaceWidth;
						continue;
					case '\t':
						flag = false;
						num += 24;
						continue;
					}
					flag = false;
					CharacterBits character = this.GetCharacter(c, needBits: true);
					if (num > 0)
					{
						num++;
					}
					num += character.m_xOffset;
					fixed (byte* bits = character.m_Bits)
					{
						byte* ptr2 = bits;
						byte* ptr3 = ptr + (num2 + 1 + character.m_yOffset) * width + (num + 1 + character.m_xWidth - 1);
						int num4 = 32 - character.m_xWidth;
						int num5 = character.m_xWidth + 7 >> 3;
						int num6 = (num2 + 1 + character.m_yOffset) * width + (num + 1 + character.m_xWidth - 1);
						int num7 = 0;
						while (num7 < character.m_yHeight)
						{
							uint num8 = *(uint*)ptr2;
							num8 = ((num8 & 0xFF) << 24) | ((num8 & 0xFF00) << 8) | ((num8 & 0xFF0000) >> 8) | ((num8 >> 24) & 0xFF);
							num8 >>= num4;
							byte* ptr4 = ptr3;
							int num9 = num6;
							int num10 = num + 1 + character.m_xWidth - 1;
							int num11 = num2 + 1 + character.m_yOffset + num7;
							int num12 = num11 * width + num10;
							int num13 = (int)(ptr4 - ptr);
							if (num11 > 0 && num11 + 1 < height)
							{
								if (flag2)
								{
									if (num10 + 1 < width)
									{
										while (num8 != 0 && num10 > 0)
										{
											if ((num8 & 1) != 0)
											{
												byte* num14 = ptr4 - width;
												*num14 |= 0x80;
												byte* num15 = ptr4 - 1;
												*num15 |= 0x80;
												*ptr4 = 2;
												byte* num16 = ptr4 + 1;
												*num16 |= 0x80;
												byte* num17 = ptr4 + width;
												*num17 |= 0x80;
												array[num9 - width] |= 128;
												array[num9 - 1] |= 128;
												array[num9] = 2;
												array[num9 + 1] |= 128;
												array[num9 + width] |= 128;
											}
											ptr4--;
											num9--;
											num10--;
											num8 >>= 1;
										}
									}
								}
								else if (num10 + 1 < width)
								{
									while (num8 != 0 && num10 > 0)
									{
										if ((num8 & 1) != 0)
										{
											*ptr4 = 2;
											array[num9] = 2;
										}
										ptr4--;
										num9--;
										num10--;
										num8 >>= 1;
									}
								}
							}
							num7++;
							ptr3 += width;
							num6 += width;
							ptr2 += num5;
						}
					}
					num += character.m_xWidth;
				}
				int num18 = width;
				int num19 = height;
				int num20 = 0;
				int num21 = 0;
				bool flag3 = (this.m_Flags & 1) != 0;
				byte* ptr5 = ptr;
				fixed (short* colors = UnicodeFont.m_Colors)
				{
					fixed (short* huedColors = UnicodeFont.m_HuedColors)
					{
						ushort* ptr6 = (ushort*)huedColors;
						Hues.Default.CopyPixels(colors + 1, ptr6 + 1, 32);
						Hues.Default.CopyPixels(colors + 129, ptr6 + 129, 32);
						for (int j = 0; j < height; j++)
						{
							for (int k = 0; k < width; k++)
							{
								if (*ptr5 != 0)
								{
									if (k < num18)
									{
										num18 = k;
									}
									if (k > num20)
									{
										num20 = k;
									}
									if (j < num19)
									{
										num19 = j;
									}
									if (j > num21)
									{
										num21 = j;
									}
								}
								if (flag3 && j % 18 == 15)
								{
									*ptr5 = 16;
								}
								*(pLine++) = ptr6[(int)(*(ptr5++))];
							}
							pLine += lineDelta;
						}
					}
				}
				this.m_xMin = num18;
				this.m_yMin = num19;
				this.m_xMax = num20;
				this.m_yMax = num21;
			}
		}
	}

	private int[] m_Lookup;

	private string m_FileName = "";

	private int m_SpaceWidth = 8;

	private int m_FontID;

	private FontCache[] m_Cache;

	private Stream m_Stream;

	private bool m_Underline;

	private bool m_Bold;

	private bool m_Italic;

	private byte[] m_4Bytes;

	private static byte[] m_NullChar;

	private Dictionary<WrapKey, string> m_WrapCache = new Dictionary<WrapKey, string>();

	private const short Border = short.MinValue;

	private UnicodeFontFactory[] m_Factories;

	private static short[] m_Colors;

	private static short[] m_HuedColors;

	public bool Underline
	{
		get
		{
			return this.m_Underline;
		}
		set
		{
			this.m_Underline = value;
		}
	}

	public bool Bold
	{
		get
		{
			return this.m_Bold;
		}
		set
		{
			this.m_Bold = value;
		}
	}

	public bool Italic
	{
		get
		{
			return this.m_Italic;
		}
		set
		{
			this.m_Italic = value;
		}
	}

	public Dictionary<WrapKey, string> WrapCache => this.m_WrapCache;

	public string Name => $"UniFont[{this.m_FontID}]";

	public int SpaceWidth
	{
		get
		{
			return this.m_SpaceWidth;
		}
		set
		{
			this.m_SpaceWidth = value;
		}
	}

	public override string ToString()
	{
		return $"<Unicode Font #{this.m_FontID}>";
	}

	public void Dispose()
	{
		for (int i = 0; i < this.m_Cache.Length; i++)
		{
			this.m_Cache[i].Dispose();
			this.m_Cache[i] = null;
		}
		this.m_Cache = null;
		this.m_Stream.Close();
		this.m_Stream = null;
		UnicodeFont.m_Colors = null;
		UnicodeFont.m_HuedColors = null;
		this.m_FileName = null;
		this.m_Lookup = null;
		this.m_WrapCache.Clear();
		this.m_WrapCache = null;
		this.m_4Bytes = null;
		UnicodeFont.m_NullChar = null;
	}

	public int GetStringWidth(string text)
	{
		if (text == null || text.Length <= 0)
		{
			return 2;
		}
		return this.m_Factories[this.GetFlags(Hues.Default)].MeasureWidth(text);
	}

	private int GetFlags(IHue hue)
	{
		int num = 0;
		if (this.m_Underline)
		{
			num |= 1;
		}
		if (this.m_Bold)
		{
			num |= 2;
		}
		if (this.m_Italic)
		{
			num |= 4;
		}
		if (!(hue is Hues.ColorFillHue) && (hue.HueID() & 0x3FFF) != 1)
		{
			num |= 8;
		}
		return num;
	}

	public Texture GetString(string String, IHue Hue)
	{
		int flags = this.GetFlags(Hue);
		return this.m_Cache[flags][String, Hue];
	}

	static UnicodeFont()
	{
		UnicodeFont.m_Colors = new short[256];
		UnicodeFont.m_HuedColors = new short[256];
		UnicodeFont.m_NullChar = new byte[10];
		UnicodeFont.m_NullChar[0] = byte.MaxValue;
		UnicodeFont.m_NullChar[1] = 129;
		UnicodeFont.m_NullChar[2] = 129;
		UnicodeFont.m_NullChar[3] = 129;
		UnicodeFont.m_NullChar[4] = 129;
		UnicodeFont.m_NullChar[5] = 129;
		UnicodeFont.m_NullChar[6] = 129;
		UnicodeFont.m_NullChar[7] = 129;
		UnicodeFont.m_NullChar[8] = 129;
		UnicodeFont.m_NullChar[9] = byte.MaxValue;
		UnicodeFont.m_HuedColors[0] = 0;
		UnicodeFont.m_HuedColors[128] = -32767;
		short num = -1;
		int num2 = 0;
		int num3 = 1;
		int num4 = 129;
		while (num2 < 32)
		{
			UnicodeFont.m_Colors[num3] = (UnicodeFont.m_Colors[num4] = num);
			num2++;
			num3++;
			num4++;
			num -= 1057;
		}
	}

	public unsafe UnicodeFont(int FontID)
	{
		this.m_FileName = string.Format("UniFont{0}.mul", (FontID != 0) ? FontID.ToString() : "");
		this.m_FontID = FontID;
		this.m_Stream = Engine.FileManager.OpenMUL(this.m_FileName);
		this.m_Lookup = new int[65536];
		fixed (int* lookup = this.m_Lookup)
		{
			UnsafeMethods.ReadFile((FileStream)this.m_Stream, lookup, 262144);
		}
		this.m_4Bytes = new byte[4];
		this.m_Factories = new UnicodeFontFactory[16];
		this.m_Cache = new FontCache[this.m_Factories.Length];
		for (int i = 0; i < this.m_Factories.Length; i++)
		{
			this.m_Factories[i] = new UnicodeFontFactory(this, i);
			this.m_Cache[i] = new FontCache(this.m_Factories[i]);
		}
	}
}
