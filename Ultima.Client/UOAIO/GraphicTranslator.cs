using System;
using System.Collections.Generic;
using System.IO;

namespace UOAIO;

public sealed class GraphicTranslator
{
	private Dictionary<int, GraphicTranslation> table;

	public Dictionary<int, GraphicTranslation> Table => this.table;

	public GraphicTranslation this[int graphicId]
	{
		get
		{
			this.table.TryGetValue(graphicId, out var value);
			return value;
		}
	}

	public GraphicTranslator(string filePath)
	{
		this.table = new Dictionary<int, GraphicTranslation>();
		this.ReadFromFile(filePath);
	}

	private void ReadFromFile(string filePath)
	{
		Debug.Try("Reading {0}...", filePath);
		try
		{
			filePath = Engine.FileManager.ResolveMUL(filePath);
			if (File.Exists(filePath))
			{
				using StreamReader streamReader = new StreamReader(filePath);
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					try
					{
						text = text.Trim();
						if (text.Length == 0 || text.StartsWith("#", StringComparison.Ordinal))
						{
							continue;
						}
						foreach (GraphicTranslation item in GraphicTranslation.Parse(text))
						{
							if (!this.table.ContainsKey(item.UpdatedId))
							{
								this.table.Add(item.UpdatedId, item);
							}
						}
					}
					catch (Exception)
					{
						Debug.Trace("Error: failed to parse line: " + text + " in " + filePath + ". Check the file and fix it. Line has been skipped and may result in unexpected glitches");
					}
				}
			}
			Debug.EndTry();
		}
		catch (Exception ex2)
		{
			Debug.FailTry();
			Debug.Error(ex2);
		}
	}

	public int Convert(int graphicId)
	{
		GraphicTranslation graphicTranslation = this[graphicId];
		if (graphicTranslation != null)
		{
			graphicId = graphicTranslation.FallbackId;
		}
		return graphicId;
	}
}
