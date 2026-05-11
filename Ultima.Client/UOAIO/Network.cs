using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace UOAIO;

public class Network
{
	private static NetworkContext _networkContext;

	public static bool IsDebug;

	private static PacketLogger _packetLogger;

	public static NetworkContext Context => Network._networkContext;

	public static bool Connect(BaseCrypto cryptoProvider, IPEndPoint ipEndPoint)
	{
		if (cryptoProvider == null)
		{
			throw new ArgumentNullException("cryptoProvider");
		}
		if (ipEndPoint == null)
		{
			throw new ArgumentNullException("ipEndPoint");
		}
		Network.Disconnect();
		try
		{
			Socket socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, optionValue: true);
			}
			catch (Exception ex)
			{
				Debug.Trace("SetSocketOption failed.");
				Debug.Error(ex);
			}
			Network.ProcessAsyncConnect(socket, socket.BeginConnect(ipEndPoint, null, null));
			Network._networkContext = new NetworkContext(socket, cryptoProvider);
			if (Network.IsDebug)
			{
				if (Network._packetLogger == null)
				{
					Network._packetLogger = new PacketLogger(ClientRuntimeEnvironment.CreateRuntimeTextWriter("data/ultima/logs/PacketTrace_all.log", append: true));
				}
				Network._networkContext.RegisterDiagnostic(Network._packetLogger);
			}
			Network._networkContext.ConnectionLostCallback = delegate
			{
				Gumps.MessageBoxOk("Connection lost", Modal: true, Engine.DestroyDialogShowAcctLogin_OnClick);
				Network._networkContext = null;
				Cursor.Hourglass = false;
				Engine.amMoving = false;
			};
			return true;
		}
		catch (SocketException)
		{
			return false;
		}
	}

	private static void ProcessAsyncConnect(Socket socket, IAsyncResult asyncResult)
	{
		do
		{
			Engine.DrawNow();
		}
		while (!asyncResult.AsyncWaitHandle.WaitOne(10, exitContext: false));
		socket.EndConnect(asyncResult);
	}

	public static void Disconnect()
	{
		Network.Disconnect(flush: true);
	}

	public static void Disconnect(bool flush)
	{
		Engine.ClearPings();
		ActionContext.Clear();
		if (Network._networkContext != null)
		{
			Network._networkContext.Close(flush);
			Network._networkContext = null;
		}
	}

	public static void Close()
	{
		Network.Disconnect();
	}

	public static void Flush()
	{
		if (Network.VerifyContext())
		{
			Network._networkContext.Flush();
		}
	}

	private static bool VerifyContext()
	{
		if (Network._networkContext != null)
		{
			if (Network._networkContext.IsOpen)
			{
				return true;
			}
			Network._networkContext = null;
		}
		return false;
	}

	public static bool Send(Packet p)
	{
		if (Network.VerifyContext())
		{
			Network._networkContext.Send(p);
		}
		return true;
	}

	public static bool Slice()
	{
		if (Network.VerifyContext())
		{
			Network._networkContext.Cycle();
		}
		return true;
	}
}
