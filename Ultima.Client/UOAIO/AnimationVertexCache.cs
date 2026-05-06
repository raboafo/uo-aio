namespace UOAIO;

public class AnimationVertexCache
{
	public void Draw(Texture t, int x, int y)
	{
		t.Draw(x, y, 16777215);
	}

	public void DrawGame(Texture t, int x, int y, int color)
	{
		t.DrawGame(x, y, color);
	}

	public void Invalidate()
	{
	}

	public AnimationVertexCache()
		: this(null)
	{
	}

	public AnimationVertexCache(TransformedColoredTextured[] v)
	{
	}
}
