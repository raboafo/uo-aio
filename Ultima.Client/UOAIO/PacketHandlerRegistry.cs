using System;

namespace UOAIO;

internal sealed class PacketHandlerRegistry
{
	private readonly PacketHandler[] _handlers;

	public PacketHandlerRegistry(int capacity)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}

		this._handlers = new PacketHandler[capacity];
	}

	public PacketHandler Get(int packetID)
	{
		if (packetID < 0 || packetID >= this._handlers.Length)
		{
			return null;
		}

		return this._handlers[packetID];
	}

	public void Register(int packetID, int length, PacketCallback callback)
	{
		this.ValidateRegistration(packetID, callback);
		this._handlers[packetID] = new PacketHandler(packetID, length, callback);
	}

	public void RegisterNew(int packetID, int length, PacketCallback callback)
	{
		if (this.Get(packetID) != null)
		{
			throw new InvalidOperationException(string.Format("Packet 0x{0:X2} is already registered.", packetID));
		}

		this.Register(packetID, length, callback);
	}

	public void ReplaceExisting(int packetID, int length, PacketCallback callback)
	{
		if (this.Get(packetID) == null)
		{
			throw new InvalidOperationException(string.Format("Packet 0x{0:X2} is not registered.", packetID));
		}

		this.Register(packetID, length, callback);
	}

	private void ValidateRegistration(int packetID, PacketCallback callback)
	{
		if (packetID < 0 || packetID >= this._handlers.Length)
		{
			throw new ArgumentOutOfRangeException("packetID");
		}

		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
	}
}
