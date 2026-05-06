using System.Net;

namespace UOAIO;

public class AuthenticationTicket
{
	public IPAddress _ipAddress;

	public int _port;

	public ulong _key;

	public string _contentArchive;
}
