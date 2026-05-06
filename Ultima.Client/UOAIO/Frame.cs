namespace UOAIO;

public class Frame
{
	public Texture Image;

	public int CenterX;

	public int CenterY;

	private static Frame m_Empty;

	public static Frame Empty
	{
		get
		{
			if (Frame.m_Empty == null)
			{
				Frame.m_Empty = new Frame();
				Frame.m_Empty.Image = Texture.Empty;
			}
			return Frame.m_Empty;
		}
	}

	public static Frame Clone(Frame original, ShaderData shaderData)
	{
		if (original == null || original == Frame.m_Empty)
		{
			return original;
		}
		Frame frame = new Frame();
		frame.Image = Texture.Clone(original.Image, shaderData);
		frame.CenterX = original.CenterX;
		frame.CenterY = original.CenterY;
		return frame;
	}
}
