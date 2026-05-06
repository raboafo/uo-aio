using System.Collections.Generic;

namespace UOAIO;

public sealed class GraphicTranslation
{
	private int updatedId;

	private int fallbackId;

	private int fallbackData;

	public int UpdatedId => this.updatedId;

	public int FallbackId => this.fallbackId;

	public int FallbackData => this.fallbackData;

	public GraphicTranslation(int updatedId, int fallbackId, int fallbackData)
	{
		this.updatedId = updatedId;
		this.fallbackId = fallbackId;
		this.fallbackData = fallbackData;
	}

	public static IEnumerable<GraphicTranslation> Parse(string line)
	{
		int leftBrace = line.IndexOf('{');
		int rightBrace = line.IndexOf('}', leftBrace);
		if (leftBrace < 0 || rightBrace < 0)
		{
			yield break;
		}
		string updatedIdString = line.Substring(0, leftBrace);
		string[] fallbackIdStrings = line.Substring(leftBrace + 1, rightBrace - leftBrace - 1).Split(',');
		string fallbackContextString = line.Substring(rightBrace + 1);
		if (!int.TryParse(updatedIdString, out var updatedId) || !int.TryParse(fallbackContextString, out var fallbackContext))
		{
			yield break;
		}
		string[] array = fallbackIdStrings;
		foreach (string fallbackIdString in array)
		{
			if (int.TryParse(fallbackIdString, out var fallbackId))
			{
				yield return new GraphicTranslation(updatedId, fallbackId, fallbackContext);
			}
		}
	}
}
