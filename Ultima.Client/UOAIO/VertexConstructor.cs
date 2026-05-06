namespace UOAIO;

public class VertexConstructor
{
	public static TransformedColoredTextured[] Create()
	{
		TransformedColoredTextured[] array = new TransformedColoredTextured[4];
		array[0].Rhw = 1f;
		array[1].Rhw = 1f;
		array[2].Rhw = 1f;
		array[3].Rhw = 1f;
		return array;
	}

	public static TransformedColoredTextured[] Create(int n)
	{
		TransformedColoredTextured[] array = new TransformedColoredTextured[n];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Rhw = 1f;
		}
		return array;
	}
}
