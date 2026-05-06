using System.Reflection;

namespace UOAIO;

internal class PPE_Login : Packet
{
	public PPE_Login(ulong ticket)
		: base(240)
	{
		base.m_Stream.Write(byte.MaxValue);
		base.m_Stream.Write((int)(ticket >> 32));
		base.m_Stream.Write((int)ticket);
		byte[] array = null;
		try
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			AssemblyName name = executingAssembly.GetName();
			array = name.GetPublicKeyToken();
		}
		catch
		{
		}
		if (array == null)
		{
			array = new byte[0];
		}
		base.m_Stream.Write(array.Length);
		base.m_Stream.Write(array);
	}
}
