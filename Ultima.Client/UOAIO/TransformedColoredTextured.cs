using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D9;

namespace UOAIO;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TransformedColoredTextured
{
	public Vector4 Position;

	public int Color;

	public float Tu;

	public float Tv;

	public const VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Diffuse | VertexFormat.Texture1;

	public float X
	{
		get
		{
			return this.Position.X;
		}
		set
		{
			this.Position.X = value;
		}
	}

	public float Y
	{
		get
		{
			return this.Position.Y;
		}
		set
		{
			this.Position.Y = value;
		}
	}

	public float Z
	{
		get
		{
			return this.Position.Z;
		}
		set
		{
			this.Position.Z = value;
		}
	}

	public float Rhw
	{
		get
		{
			return this.Position.W;
		}
		set
		{
			this.Position.W = value;
		}
	}

	public static int StrideSize => Marshal.SizeOf(typeof(TransformedColoredTextured));

	public TransformedColoredTextured(Vector4 value, int c, float u, float v)
	{
		this.Position = value;
		this.Tu = u;
		this.Tv = v;
		this.Color = c;
	}

	public TransformedColoredTextured(float xvalue, float yvalue, float zvalue, float rhwvalue, int c, float u, float v)
	{
		this.Position = new Vector4(xvalue, yvalue, zvalue, rhwvalue);
		this.Tu = u;
		this.Tv = v;
		this.Color = c;
	}
}
