using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UOAIO.Videos;

namespace UOAIO;

public sealed class NetworkContext
{
	private bool _isOpen;

	private Socket _socket;

	public BaseCrypto _crypto;

	private InputQueue _inputQueue;

	private OutputQueue _outputQueue;

	private byte[] _receiveBuffer;

	private AsyncCallback _sendCallback;

	private AsyncCallback _receiveCallback;

	private List<INetworkDiagnostic> _networkDiagnostics;

	private Callback _connectionLostCallback;

	public static int prior1;

	public static int prior2;

	public static int prior3;

	public bool IsOpen => this._isOpen;

	public Callback ConnectionLostCallback
	{
		get
		{
			return this._connectionLostCallback;
		}
		set
		{
			this._connectionLostCallback = value;
		}
	}

	public NetworkContext(Socket socket, BaseCrypto crypto)
	{
		if (socket == null)
		{
			throw new ArgumentNullException("socket");
		}
		if (crypto == null)
		{
			throw new ArgumentNullException("crypto");
		}
		this._isOpen = true;
		this._socket = socket;
		this._crypto = crypto;
		this._inputQueue = new InputQueue();
		this._outputQueue = new OutputQueue(new BufferAllocator(512));
		this._receiveBuffer = new byte[2048];
		this._sendCallback = OnSend;
		this._receiveCallback = OnReceive;
		this.Receive();
	}

	internal void RegisterDiagnostic(INetworkDiagnostic networkDiagnostic)
	{
		if (networkDiagnostic == null)
		{
			throw new ArgumentNullException("networkDiagnostic");
		}
		if (this._networkDiagnostics == null)
		{
			this._networkDiagnostics = new List<INetworkDiagnostic>();
		}
		if (!this._networkDiagnostics.Contains(networkDiagnostic))
		{
			networkDiagnostic.Open();
			this._networkDiagnostics.Add(networkDiagnostic);
		}
	}

	internal void UnregisterDiagnostic(INetworkDiagnostic networkDiagnostic)
	{
		if (networkDiagnostic == null)
		{
			throw new ArgumentNullException("networkDiagnostic");
		}
		if (this._networkDiagnostics != null && this._networkDiagnostics.Contains(networkDiagnostic))
		{
			this._networkDiagnostics.Remove(networkDiagnostic);
			networkDiagnostic.Close();
		}
	}

	public void Flush()
	{
		OutputQueue.Gram gram = this._outputQueue.Flush();
		if (gram != null)
		{
			this.Dispatch(gram);
		}
	}

	public void Close()
	{
		this.Close(flush: true);
	}

	public void Close(bool flush)
	{
		if (!this._isOpen)
		{
			return;
		}
		lock (this)
		{
			if (!this._isOpen)
			{
				return;
			}
			this._isOpen = false;
			if (flush)
			{
				try
				{
					this._socket.Shutdown(SocketShutdown.Both);
				}
				catch
				{
				}
				try
				{
					this._socket.Close();
				}
				catch
				{
				}
			}
			if (this._networkDiagnostics == null)
			{
				return;
			}
			foreach (INetworkDiagnostic networkDiagnostic in this._networkDiagnostics)
			{
				networkDiagnostic.Close();
			}
			this._networkDiagnostics = null;
		}
	}

	public void Send(Packet packet)
	{
		if (packet == null)
		{
			throw new ArgumentNullException("packet");
		}
		if (Playback.Active && !(packet is PPing))
		{
			return;
		}
		if (packet is PUseRequest || packet is PPickupItem)
		{
			Engine.PushAction();
		}
		try
		{
			byte[] array = packet.Compile();
			if (array.Length == 0)
			{
				return;
			}
			if (this._networkDiagnostics != null)
			{
				for (int i = 0; i < this._networkDiagnostics.Count; i++)
				{
					this._networkDiagnostics[i].PacketSent(packet, array, 0, array.Length);
				}
			}
			if (packet.Encode)
			{
				this._crypto.Encrypt(array, 0, array.Length, this._outputQueue);
			}
			else
			{
				this._outputQueue.Enqueue(array, 0, array.Length);
			}
			OutputQueue.Gram gram = this._outputQueue.Query();
			if (gram != null)
			{
				this.Dispatch(gram);
			}
		}
		catch (Exception ex)
		{
			this.HandleError(ex);
		}
		finally
		{
			packet.Dispose();
		}
	}

	private void Dispatch(OutputQueue.Gram gram)
	{
		if (this._isOpen)
		{
			try
			{
				this._socket.BeginSend(gram.Buffer, 0, gram.Length, SocketFlags.None, this._sendCallback, null);
			}
			catch (Exception ex)
			{
				this.HandleError(ex);
			}
		}
	}

	private void Receive()
	{
		if (this._isOpen)
		{
			try
			{
				this._socket.BeginReceive(this._receiveBuffer, 0, this._receiveBuffer.Length, SocketFlags.None, this._receiveCallback, null);
			}
			catch (Exception ex)
			{
				this.HandleError(ex);
			}
		}
	}

	private void OnSend(IAsyncResult asyncResult)
	{
		try
		{
			int num = this._socket.EndSend(asyncResult);
			if (num > 0)
			{
				OutputQueue.Gram gram = this._outputQueue.Proceed();
				if (gram != null)
				{
					this.Dispatch(gram);
				}
			}
			else
			{
				this.GracefulShutdown();
			}
		}
		catch (Exception ex)
		{
			this.HandleError(ex);
		}
	}

	private void OnReceive(IAsyncResult asyncResult)
	{
		try
		{
			int num = this._socket.EndReceive(asyncResult);
			if (num > 0)
			{
				lock (this._inputQueue)
				{
					this._crypto.Decrypt(this._receiveBuffer, 0, num, this._inputQueue);
				}
				this.Receive();
			}
			else
			{
				this.GracefulShutdown();
			}
		}
		catch (Exception ex)
		{
			this.HandleError(ex);
		}
	}

	private void HandleError(Exception ex)
	{
		if (!this._isOpen)
		{
			return;
		}
		lock (this)
		{
			if (this._isOpen)
			{
				Debug.Trace("Hard disconnect");
				Debug.Error(ex);
				if (this._connectionLostCallback != null)
				{
					this._connectionLostCallback();
				}
				this.Close(flush: false);
			}
		}
	}

	private void GracefulShutdown()
	{
		if (!this._isOpen)
		{
			return;
		}
		lock (this)
		{
			if (this._isOpen)
			{
				Debug.Trace("Graceful disconnect");
				if (this._connectionLostCallback != null)
				{
					this._connectionLostCallback();
				}
				this.Close(flush: false);
			}
		}
	}

	public void Cycle()
	{
		if (Playback.Video != null)
		{
			Playback.Video.Cycle();
		}
		if (this._inputQueue.Length <= 0)
		{
			return;
		}
		try
		{
			PacketHandlers.BeginSlice();
			lock (this._inputQueue)
			{
				while (this._inputQueue.Length > 0)
				{
					int packetId = this._inputQueue.GetPacketId();
					if (packetId < 0)
					{
						break;
					}
					PacketHandler packetHandler = PacketHandlers.Registry.Get(packetId);
					if (packetHandler != null)
					{
						int num = packetHandler.Length;
						if (num == -1)
						{
							num = this._inputQueue.GetPacketLength();
							if (num < 3)
							{
								if (num >= 0)
								{
									this._inputQueue.Clear();
								}
								break;
							}
						}
						if (this._inputQueue.Length < num)
						{
							break;
						}
						ArraySegment<byte> arraySegment = this._inputQueue.Dequeue(num);
						PacketReader pvSrc = PacketReader.Initialize(arraySegment.Array, arraySegment.Offset, arraySegment.Count, packetHandler.Length != -1, (byte)packetId, packetHandler.Callback.Method.Name);
						if (this._networkDiagnostics != null)
						{
							foreach (INetworkDiagnostic networkDiagnostic in this._networkDiagnostics)
							{
								networkDiagnostic.PacketReceived(packetHandler, arraySegment.Array, arraySegment.Offset, arraySegment.Count);
							}
						}
						if (!Playback.Active || packetHandler.PacketID == 115)
						{
							packetHandler.Handle(pvSrc);
						}
						NetworkContext.prior3 = NetworkContext.prior2;
						NetworkContext.prior2 = NetworkContext.prior1;
						NetworkContext.prior1 = packetId;
						continue;
					}
					ArraySegment<byte> arraySegment2 = this._inputQueue.Dequeue(this._inputQueue.Length);
					PacketReader packetReader = PacketReader.Initialize(arraySegment2.Array, arraySegment2.Offset, arraySegment2.Count, fixedSize: true, (byte)packetId, "Unknown");
					packetReader.Trace();
					NetworkContext.prior3 = NetworkContext.prior2;
					NetworkContext.prior2 = NetworkContext.prior1;
					NetworkContext.prior1 = packetId;
					break;
				}
			}
		}
		finally
		{
			PacketHandlers.FinishSlice();
		}
	}
}
