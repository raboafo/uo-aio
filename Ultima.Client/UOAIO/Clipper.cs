namespace UOAIO;

public class Clipper
{
	protected int m_xStart;

	protected int m_yStart;

	protected int m_xEnd;

	protected int m_yEnd;

	private static Clipper m_Clipper;

	public int xStart => this.m_xStart;

	public int yStart => this.m_yStart;

	public int xEnd => this.m_xEnd;

	public int yEnd => this.m_yEnd;

	public static Clipper TemporaryInstance(int x, int y, int width, int height)
	{
		Clipper.m_Clipper.m_xStart = x;
		Clipper.m_Clipper.m_yStart = y;
		Clipper.m_Clipper.m_xEnd = x + width;
		Clipper.m_Clipper.m_yEnd = y + height;
		return Clipper.m_Clipper;
	}

	public Clipper(int xStart, int yStart, int xWidth, int yHeight)
	{
		this.m_xStart = xStart;
		this.m_yStart = yStart;
		this.m_xEnd = xStart + xWidth;
		this.m_yEnd = yStart + yHeight;
	}

	public bool Evaluate(Point p)
	{
		return p.X >= this.m_xStart && p.X < this.m_xEnd && p.Y >= this.m_yStart && p.Y < this.m_yEnd;
	}

	public bool Evaluate(int xPoint, int yPoint)
	{
		return xPoint >= this.m_xStart && yPoint >= this.m_yStart && xPoint < this.m_xEnd && yPoint < this.m_yEnd;
	}

	public ClipType Evaluate(int xStart, int yStart, int xWidth, int yHeight)
	{
		int num = xStart + xWidth;
		int num2 = yStart + yHeight;
		if (num <= this.m_xStart || num2 <= this.m_yStart || xStart >= this.m_xEnd || yStart >= this.m_yEnd)
		{
			return ClipType.Outside;
		}
		if (xStart >= this.m_xStart && yStart >= this.m_yStart && num <= this.m_xEnd && num2 <= this.m_yEnd)
		{
			return ClipType.Inside;
		}
		return ClipType.Partial;
	}

	public bool Clip(int xStart, int yStart, int xWidth, int yHeight, TransformedColoredTextured[] Vertices)
	{
		switch (this.Evaluate(xStart, yStart, xWidth, yHeight))
		{
		case ClipType.Outside:
			return false;
		case ClipType.Inside:
		{
			float num11 = -0.5f + (float)xStart;
			float num12 = -0.5f + (float)yStart;
			float num13 = num11 + (float)xWidth;
			float num14 = num12 + (float)yHeight;
			ref TransformedColoredTextured reference5 = ref Vertices[0];
			float x = (Vertices[1].X = num13);
			reference5.X = x;
			ref TransformedColoredTextured reference6 = ref Vertices[0];
			x = (Vertices[2].Y = num14);
			reference6.Y = x;
			ref TransformedColoredTextured reference7 = ref Vertices[1];
			x = (Vertices[3].Y = num12);
			reference7.Y = x;
			ref TransformedColoredTextured reference8 = ref Vertices[2];
			x = (Vertices[3].X = num11);
			reference8.X = x;
			Vertices[0].Tu = (Vertices[0].Tv = (Vertices[1].Tu = (Vertices[2].Tv = 1f)));
			Vertices[1].Tv = (Vertices[2].Tu = (Vertices[3].Tu = (Vertices[3].Tv = 0f)));
			return true;
		}
		case ClipType.Partial:
		{
			int num = xStart;
			int num2 = yStart;
			int num3 = xStart + xWidth;
			int num4 = yStart + yHeight;
			if (xStart < this.m_xStart)
			{
				num = this.m_xStart;
			}
			if (yStart < this.m_yStart)
			{
				num2 = this.m_yStart;
			}
			if (num3 > this.m_xEnd)
			{
				num3 = this.m_xEnd;
			}
			if (num4 > this.m_yEnd)
			{
				num4 = this.m_yEnd;
			}
			ref TransformedColoredTextured reference = ref Vertices[0];
			float x = (Vertices[1].X = -0.5f + (float)num3);
			reference.X = x;
			ref TransformedColoredTextured reference2 = ref Vertices[0];
			x = (Vertices[2].Y = -0.5f + (float)num4);
			reference2.Y = x;
			ref TransformedColoredTextured reference3 = ref Vertices[1];
			x = (Vertices[3].Y = -0.5f + (float)num2);
			reference3.Y = x;
			ref TransformedColoredTextured reference4 = ref Vertices[2];
			x = (Vertices[3].X = -0.5f + (float)num);
			reference4.X = x;
			double num9 = 1.0 / (double)xWidth;
			double num10 = 1.0 / (double)yHeight;
			Vertices[0].Tu = (Vertices[1].Tu = (float)(num9 * (double)(num3 - xStart)));
			Vertices[0].Tv = (Vertices[2].Tv = (float)(num10 * (double)(num4 - yStart)));
			Vertices[1].Tv = (Vertices[3].Tv = (float)(num10 * (double)(num2 - yStart)));
			Vertices[2].Tu = (Vertices[3].Tu = (float)(num9 * (double)(num - xStart)));
			return true;
		}
		default:
			return false;
		}
	}

	public override bool Equals(object Target)
	{
		if (Target == null || Target.GetType() != typeof(Clipper))
		{
			return false;
		}
		Clipper clipper = (Clipper)Target;
		if (clipper == this)
		{
			return true;
		}
		return this.m_xStart == clipper.m_xStart && this.m_yStart == clipper.m_yStart && this.m_xEnd == clipper.m_xEnd && this.m_yEnd == clipper.m_yEnd;
	}

	public override int GetHashCode()
	{
		return this.m_xStart ^ this.m_yStart ^ this.m_xEnd ^ this.m_yEnd;
	}

	static Clipper()
	{
		Clipper.m_Clipper = new Clipper(0, 0, 0, 0);
	}
}
