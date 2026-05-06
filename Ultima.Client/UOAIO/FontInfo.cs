namespace UOAIO;

public class FontInfo
{
	private UnicodeFont m_Font;

	private int m_Color;

	private IHue m_Hue;

	public UnicodeFont Font => this.m_Font;

	public int Color => this.m_Color;

	public IHue Hue => this.m_Hue;

	public FontInfo(UnicodeFont font, int color)
	{
		this.m_Font = font;
		this.m_Color = color;
		this.m_Hue = new Hues.ColorFillHue(color);
	}
}
