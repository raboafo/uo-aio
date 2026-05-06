using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UOAIO;

public class RunebookParser
{
	public static RuneInfoExCollection Parse(int gumpSerial, int dialogID, string layout, string[] textArray)
	{
		RuneInfoExCollection runeInfoExCollection = new RuneInfoExCollection();
		int num = 2;
		int num2 = 23;
		int num3 = 16;
		MatchCollection matchCollection = Regex.Matches(layout, "button \\d+ \\d+ \\d+ \\d+ \\d+ \\d+ (\\d+)");
		List<int> list = new List<int>();
		foreach (Match item in matchCollection)
		{
			list.Add(int.Parse(item.Groups[1].Value));
		}
		for (int i = 0; i < num3; i++)
		{
			string text = textArray[num + i];
			if (!string.IsNullOrEmpty(text) && !text.Equals("Empty", StringComparison.OrdinalIgnoreCase))
			{
				Point3D p = RunebookParser.ParseSextantToPoint(textArray[num2 + i]);
				int recallButtonID = 50 + i;
				int gateButtonID = 100 + i;
				runeInfoExCollection.Add(new RuneInfoEx(text, p, recallButtonID, gateButtonID));
			}
		}
		return runeInfoExCollection;
	}

	private static Point3D ParseSextantToPoint(string sextant)
	{
		Match match = Regex.Match(sextant, "(\\d+)o\\s+(\\d+)'([NSEW]),\\s+(\\d+)o\\s+(\\d+)'([NSEW])");
		if (!match.Success)
		{
			return new Point3D(0, 0, 0);
		}
		int num = int.Parse(match.Groups[1].Value);
		int num2 = int.Parse(match.Groups[2].Value);
		bool flag = match.Groups[3].Value == "N";
		int num3 = int.Parse(match.Groups[4].Value);
		int num4 = int.Parse(match.Groups[5].Value);
		bool num5 = match.Groups[6].Value == "W";
		int num6 = 1323;
		int num7 = 1624;
		int num8 = 5120;
		int num9 = 4096;
		double num10 = ((double)num * 60.0 + (double)num2) * (double)num9 / 21600.0;
		double num11 = ((double)num3 * 60.0 + (double)num4) * (double)num8 / 21600.0;
		int x = (num5 ? ((int)((double)num6 - num11)) : ((int)((double)num6 + num11)));
		int y = (flag ? ((int)((double)num7 - num10)) : ((int)((double)num7 + num10)));
		return new Point3D(x, y, 0);
	}

	public static RuneInfoExCollection Parse(int runebookSerial, int gumpSerial, int dialogID, string layout, string[] textArray)
	{
		RuneInfoExCollection runeInfoExCollection = new RuneInfoExCollection();
		int num = 2;
		int num2 = 23;
		int num3 = 16;
		MatchCollection matchCollection = Regex.Matches(layout, "button \\d+ \\d+ \\d+ \\d+ \\d+ \\d+ (\\d+)");
		List<int> list = new List<int>();
		foreach (Match item in matchCollection)
		{
			list.Add(int.Parse(item.Groups[1].Value));
		}
		for (int i = 0; i < num3; i++)
		{
			string text = textArray[num + i];
			if (!string.IsNullOrEmpty(text) && !text.Equals("Empty", StringComparison.OrdinalIgnoreCase))
			{
				Point3D p = RunebookParser.ParseSextantToPoint(textArray[num2 + i]);
				int recallButtonID = 50 + i;
				int gateButtonID = 100 + i;
				runeInfoExCollection.Add(new RuneInfoEx(runebookSerial, text, p, recallButtonID, gateButtonID));
			}
		}
		return runeInfoExCollection;
	}
}
