using System.Text;

namespace Ultima.Data;

internal static class __jenkins_lookup3
{
	public static ulong digestOf(string key)
	{
		return __jenkins_lookup3.digestOf(Encoding.UTF8.GetBytes(key));
	}

	public unsafe static ulong digestOf(byte[] key)
	{
		uint c = default(uint);
		uint b = default(uint);
		fixed (byte* p = key)
		{
			__jenkins_lookup3.hashlittle2(p, (uint)key.Length, &c, &b);
		}
		return ((ulong)b << 32) | c;
	}

	private unsafe static void hashlittle2(byte* key, uint length, uint* pc, uint* pb)
	{
		uint b;
		uint c;
		uint a = (b = (c = (uint)(-559038737 + (int)length) + *pc));
		uint* k = (uint*)key;
		while (length > 12)
		{
			a += *k;
			b += k[1];
			c += k[2];
			__jenkins_lookup3.mix(ref a, ref b, ref c);
			length -= 12;
			k += 3;
		}
		byte* k8 = (byte*)k;
		switch (length)
		{
		case 12u:
			c += k[2];
			b += k[1];
			a += *k;
			break;
		case 11u:
			c += (uint)(k8[10] << 16);
			goto case 10u;
		case 10u:
			c += (uint)(k8[9] << 8);
			goto case 9u;
		case 9u:
			c += k8[8];
			goto case 8u;
		case 8u:
			b += k[1];
			a += *k;
			break;
		case 7u:
			b += (uint)(k8[6] << 16);
			goto case 6u;
		case 6u:
			b += (uint)(k8[5] << 8);
			goto case 5u;
		case 5u:
			b += k8[4];
			goto case 4u;
		case 4u:
			a += *k;
			break;
		case 3u:
			a += (uint)(k8[2] << 16);
			goto case 2u;
		case 2u:
			a += (uint)(k8[1] << 8);
			goto case 1u;
		case 1u:
			a += *k8;
			break;
		case 0u:
			*pc = c;
			*pb = b;
			return;
		}
		__jenkins_lookup3.final(ref a, ref b, ref c);
		*pc = c;
		*pb = b;
	}

	private static void mix(ref uint a, ref uint b, ref uint c)
	{
		a -= c;
		a ^= __jenkins_lookup3.rot(c, 4);
		c += b;
		b -= a;
		b ^= __jenkins_lookup3.rot(a, 6);
		a += c;
		c -= b;
		c ^= __jenkins_lookup3.rot(b, 8);
		b += a;
		a -= c;
		a ^= __jenkins_lookup3.rot(c, 16);
		c += b;
		b -= a;
		b ^= __jenkins_lookup3.rot(a, 19);
		a += c;
		c -= b;
		c ^= __jenkins_lookup3.rot(b, 4);
		b += a;
	}

	private static void final(ref uint a, ref uint b, ref uint c)
	{
		c ^= b;
		c -= __jenkins_lookup3.rot(b, 14);
		a ^= c;
		a -= __jenkins_lookup3.rot(c, 11);
		b ^= a;
		b -= __jenkins_lookup3.rot(a, 25);
		c ^= b;
		c -= __jenkins_lookup3.rot(b, 16);
		a ^= c;
		a -= __jenkins_lookup3.rot(c, 4);
		b ^= a;
		b -= __jenkins_lookup3.rot(a, 14);
		c ^= b;
		c -= __jenkins_lookup3.rot(b, 24);
	}

	private static uint rot(uint x, int k)
	{
		return (x << k) | (x >> 32 - k);
	}
}
