namespace UOAIO;

public class VertexCache
{
	public void Draw(Texture t, int x, int y, int color)
	{
		t.Draw(x, y, color);
	}

	public void Draw(Texture t, int x, int y)
	{
		t.Draw(x, y, 16777215);
	}

	public void DrawGame(Texture tex, int x, int y)
	{
		this.DrawGame(tex, x, y, 16777215);
	}

	public void DrawGame(Texture t, int x, int y, int color)
	{
		t.DrawGame(x, y, color);
	}

	public void Invalidate()
	{
	}

	public VertexCache()
		: this(null)
	{
	}

	public VertexCache(TransformedColoredTextured[] v)
	{
	}
}
