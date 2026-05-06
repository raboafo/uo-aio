namespace UOAIO;

public class FontImage
{
	public int xWidth;

	public int yHeight;

	public int xDelta;

	public byte[] xyPixels;

	public FontImage(int xWidth, int yHeight)
	{
		this.xWidth = xWidth;
		this.yHeight = yHeight;
		this.xDelta = xWidth + (-xWidth & 3);
		this.xyPixels = new byte[this.xDelta * yHeight];
	}
}
