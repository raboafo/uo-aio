using System;
using System.Net;
using System.Net.Sockets;

namespace UOAIO;

internal class EngineEx
{
	private static IPAddress m_LocalIPAddress;

	public static uint LocalIP => BitConverter.ToUInt32(EngineEx.LocalIPAddress.GetAddressBytes(), 0);

	private static IPAddress LocalIPAddress
	{
		get
		{
			if (EngineEx.m_LocalIPAddress == null)
			{
				IPAddress[] hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
				foreach (IPAddress iPAddress in hostAddresses)
				{
					if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
					{
						EngineEx.m_LocalIPAddress = iPAddress;
					}
				}
				if (EngineEx.m_LocalIPAddress == null)
				{
					EngineEx.m_LocalIPAddress = IPAddress.Parse("5.5.5.5");
				}
			}
			return EngineEx.m_LocalIPAddress;
		}
	}
}
