using UOAIO.Targeting;

namespace UOAIO;

public class CursorEntry
{
	public int m_Graphic;

	public int m_Type;

	public int m_xOffset;

	public int m_yOffset;

	public Texture m_Image;

	private bool m_Draw;

	private VertexCache m_vCache;

	private static TransformedColoredTextured[] m_vTargetPool;

	public CursorEntry(int graphic, int type, int xOffset, int yOffset, Texture image)
	{
		this.m_Graphic = graphic;
		this.m_Type = type;
		this.m_xOffset = xOffset;
		this.m_yOffset = yOffset;
		this.m_Image = image;
		this.m_Draw = this.m_Image != null && !this.m_Image.IsEmpty();
		this.m_vCache = new VertexCache();
	}

	public void Draw(int xMouse, int yMouse)
	{
		if (!this.m_Draw)
		{
			return;
		}
		this.m_vCache.Draw(this.m_Image, xMouse - this.m_xOffset, yMouse - this.m_yOffset);
		if (this.m_Graphic != 12)
		{
			return;
		}
		int num = 0;
		BaseTargetHandler active = TargetManager.Active;
		ServerTargetHandler serverTargetHandler = active as ServerTargetHandler;
		if (active != null)
		{
			if (active.IsOffensive)
			{
				num = 13369344;
				if (serverTargetHandler != null && serverTargetHandler.Action == TargetAction.Poison)
				{
					num = 65280;
				}
			}
			else if (active.IsDefensive)
			{
				num = 2285055;
				if (serverTargetHandler != null && serverTargetHandler.Action == TargetAction.Cure)
				{
					num = 65450;
				}
			}
		}
		if (num > 0)
		{
			Texture targetCursorHighlight = Engine.ImageCache.TargetCursorHighlight;
			targetCursorHighlight.Draw(xMouse - this.m_xOffset - 3, yMouse - this.m_yOffset - 11, num);
			if (active is ClientTargetHandler)
			{
				Texture targetCursorLocal = Engine.ImageCache.TargetCursorLocal;
				targetCursorLocal.Draw(xMouse - this.m_xOffset, yMouse - this.m_yOffset, 11184810);
			}
		}
		else if (active is ClientTargetHandler)
		{
			Texture targetCursorLocal2 = Engine.ImageCache.TargetCursorLocal;
			targetCursorLocal2.Draw(xMouse - this.m_xOffset, yMouse - this.m_yOffset, 11184810);
		}
	}

	static CursorEntry()
	{
		CursorEntry.m_vTargetPool = VertexConstructor.Create();
	}
}
