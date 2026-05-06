namespace UOAIO;

internal interface INetworkDiagnostic
{
	void Open();

	void PacketSent(Packet packet, byte[] buffer, int offset, int length);

	void PacketReceived(PacketHandler packetHandler, byte[] buffer, int offset, int length);

	void Close();
}
