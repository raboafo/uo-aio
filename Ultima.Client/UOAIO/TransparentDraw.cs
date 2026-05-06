using System.Collections.Generic;

namespace UOAIO;

public class TransparentDraw
{
	public Texture m_Texture;

	public int m_X;

	public int m_Y;

	public float m_fAlpha;

	public bool m_Double;

	private static Queue<TransparentDraw> m_Pool;

	public static TransparentDraw PoolInstance(Texture tex, int x, int y, float theAlpha, bool xDouble)
	{
		if (TransparentDraw.m_Pool == null)
		{
			TransparentDraw.m_Pool = new Queue<TransparentDraw>();
		}
		if (TransparentDraw.m_Pool.Count > 0)
		{
			TransparentDraw transparentDraw = TransparentDraw.m_Pool.Dequeue();
			transparentDraw.m_Texture = tex;
			transparentDraw.m_X = x;
			transparentDraw.m_Y = y;
			transparentDraw.m_fAlpha = theAlpha;
			transparentDraw.m_Double = xDouble;
			return transparentDraw;
		}
		return new TransparentDraw(tex, x, y, theAlpha, xDouble);
	}

	public void Dispose()
	{
		TransparentDraw.m_Pool.Enqueue(this);
	}

	private TransparentDraw(Texture tex, int x, int y, float theAlpha, bool xDouble)
	{
		this.m_Texture = tex;
		this.m_X = x;
		this.m_Y = y;
		this.m_fAlpha = theAlpha;
		this.m_Double = xDouble;
	}
}
