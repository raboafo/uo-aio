using System;
using System.IO;
using System.Text;

namespace UOAIO;

public class LocalizationFile
{
	private bool m_Valid;

	private string m_Name;

	private string[] m_Text;

	private static byte[] m_Buffer;

	public int Count => this.m_Text.Length;

	public string this[int index]
	{
		get
		{
			if (this.m_Valid)
			{
				if (index < 0 || index >= this.m_Text.Length)
				{
					return $"<Index out of bounds: {this.m_Name}:{index} ({this.m_Text.Length})>";
				}
				return this.m_Text[index];
			}
			return $"<Invalid localization file: {this.m_Name}>";
		}
	}

	public LocalizationFile(string path)
	{
		this.m_Name = Path.GetFileName(path);
		if (File.Exists(path))
		{
			this.ReadFromDisk(path);
		}
	}

	private int ReadInt32_BE(Stream stream)
	{
		stream.Read(LocalizationFile.m_Buffer, 0, 4);
		return (LocalizationFile.m_Buffer[0] << 24) | (LocalizationFile.m_Buffer[1] << 16) | (LocalizationFile.m_Buffer[2] << 8) | LocalizationFile.m_Buffer[3];
	}

	private int ReadInt32_LE(Stream stream)
	{
		stream.Read(LocalizationFile.m_Buffer, 0, 4);
		return LocalizationFile.m_Buffer[0] | (LocalizationFile.m_Buffer[1] << 8) | (LocalizationFile.m_Buffer[2] << 16) | (LocalizationFile.m_Buffer[3] << 24);
	}

	private unsafe void ReadFromDisk(string path)
	{
		try
		{
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			fileStream.Seek(28L, SeekOrigin.Begin);
			int num = this.ReadInt32_BE(fileStream);
			long position = fileStream.Position;
			while (fileStream.ReadByte() > 0)
			{
			}
			int num2 = this.ReadInt32_LE(fileStream);
			fileStream.Seek(4L, SeekOrigin.Current);
			while (fileStream.ReadByte() > 0)
			{
			}
			fileStream.Seek(4L, SeekOrigin.Current);
			int num3 = this.ReadInt32_LE(fileStream);
			fileStream.Seek(position + num + (num & 1) + 4, SeekOrigin.Begin);
			int num4 = this.ReadInt32_BE(fileStream);
			if (num4 > LocalizationFile.m_Buffer.Length)
			{
				LocalizationFile.m_Buffer = new byte[num4];
			}
			fileStream.Read(LocalizationFile.m_Buffer, 0, num4);
			fileStream.Close();
			this.m_Valid = true;
			switch (num2)
			{
			case 1:
				this.m_Text = new string[num3];
				fixed (byte* buffer2 = LocalizationFile.m_Buffer)
				{
					byte* ptr4 = buffer2;
					byte* ptr5 = ptr4 + num4;
					for (int j = 0; j < num3; j++)
					{
						byte* ptr6 = ptr4;
						while (ptr4 < ptr5 && *(ptr4++) != 0)
						{
						}
						this.m_Text[j] = new string((sbyte*)ptr6, 0, (int)(ptr4 - ptr6 - 1));
					}
					break;
				}
			case 2:
			{
				this.m_Text = new string[num3];
				Encoding unicode = Encoding.Unicode;
				fixed (byte* buffer = LocalizationFile.m_Buffer)
				{
					byte* ptr = buffer;
					byte* ptr2 = ptr + num4;
					for (int i = 0; i < num3; i++)
					{
						byte* ptr3 = ptr;
						while (ptr < ptr2 && (*(ptr++) | *(ptr++)) != 0)
						{
						}
						this.m_Text[i] = new string((sbyte*)ptr3, 0, (int)(ptr - ptr3 - 2), unicode);
					}
					break;
				}
			}
			default:
				throw new InvalidOperationException($"Character size invalid. (charSize={num2})");
			}
		}
		catch
		{
			this.m_Valid = false;
		}
	}

	static LocalizationFile()
	{
		LocalizationFile.m_Buffer = new byte[4];
	}
}
