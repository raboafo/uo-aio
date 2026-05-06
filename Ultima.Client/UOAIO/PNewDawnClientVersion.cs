using System;
using System.Security.Cryptography;
using System.Text;

namespace UOAIO;

internal class PNewDawnClientVersion : Packet
{
	public PNewDawnClientVersion(string version, string key)
		: base(201)
	{
		string text = GenerateHmac(version, key);
		base.m_Stream.Write(text, text.Length + 1);
	}

	private static string GenerateHmac(string message, string key)
	{
		byte[] keyBytes = Encoding.UTF8.GetBytes(key.ToLowerInvariant());
		byte[] messageBytes = Encoding.UTF8.GetBytes(message);
		using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
		{
			return Convert.ToBase64String(hmac.ComputeHash(messageBytes));
		}
	}
}
