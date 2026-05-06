using System;
using System.IO;

namespace UOAIO;

internal sealed class PacketLogger : INetworkDiagnostic
{
	private TextWriter _output;

	public PacketLogger(TextWriter output)
	{
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		this._output = output;
	}

	public void Open()
	{
		this._output.WriteLine("####\tOpened on {0}\t####", DateTime.Now);
		this._output.Flush();
	}

	public void Close()
	{
		this._output.WriteLine("####\tClosed on {0}\t####", DateTime.Now);
		this._output.Flush();
	}

	public void PacketSent(Packet packet, byte[] buffer, int offset, int length)
	{
		this._output.WriteLine("Packet Client->Server '{0}' ( {1:N0} bytes ) @ {2} {3}", packet.GetType().Name, length, DateTime.Now.Date.ToShortDateString(), DateTime.Now.TimeOfDay);
		PacketLogger.WriteBuffer(this._output, buffer, offset, length);
		this._output.WriteLine();
		this._output.WriteLine();
	}

	public void PacketReceived(PacketHandler packetHandler, byte[] buffer, int offset, int length)
	{
		this._output.WriteLine("Packet Server->Client '{0}' ( {1:N0} bytes ) @ {2} {3}", packetHandler.Callback.Method.Name, length, DateTime.Now.Date.ToShortDateString(), DateTime.Now.TimeOfDay);
		PacketLogger.WriteBuffer(this._output, buffer, offset, length);
		this._output.WriteLine();
		this._output.WriteLine();
	}

	public static void WriteBuffer(TextWriter output, byte[] buffer, int offset, int length)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset >= buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset", offset, "Offset must be greater than or equal to zero and less than the size of the buffer.");
		}
		if (length < 0 || length > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("length", length, "Length cannot be less than zero or greater than the size of the buffer.");
		}
		if (buffer.Length - offset < length)
		{
			throw new ArgumentException("Offset and length do not point to a valid segment within the buffer.");
		}
		output.WriteLine("        0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F");
		output.WriteLine("       -- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --");
		int num = 0;
		while (num < length)
		{
			output.Write(num.ToString("X4"));
			output.Write("   ");
			for (int i = 0; i < 16; i++)
			{
				if (num + i < length)
				{
					output.Write(buffer[offset + num + i].ToString("X2"));
					if (i == 7)
					{
						output.Write("  ");
					}
					else
					{
						output.Write(' ');
					}
				}
				else if (i == 7)
				{
					output.Write("    ");
				}
				else
				{
					output.Write("   ");
				}
			}
			output.Write("  ");
			int num2 = 0;
			while (num2 < 16 && num < length)
			{
				byte b = buffer[offset + num];
				if (b >= 32 && b < 128)
				{
					output.Write((char)b);
				}
				else
				{
					output.Write('.');
				}
				num2++;
				num++;
			}
			output.WriteLine();
		}
		output.Flush();
	}
}
