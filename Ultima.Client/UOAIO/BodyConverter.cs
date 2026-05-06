using System;
using System.IO;
using System.Text.RegularExpressions;

namespace UOAIO;

public class BodyConverter
{
	private static int[] m_Table_LBR;

	private static int[] m_Table_AOS;

	private static int[] m_Table_AOW;

	private static int[] m_Table_ML;

	static BodyConverter()
	{
		string path = Engine.FileManager.ResolveMUL("bodyconv.def");
		if (!File.Exists(path))
		{
			return;
		}
		BodyConverter.m_Table_LBR = new int[2048];
		BodyConverter.m_Table_AOS = new int[2048];
		BodyConverter.m_Table_AOW = new int[2048];
		BodyConverter.m_Table_ML = new int[2048];
		for (int i = 0; i < BodyConverter.m_Table_LBR.Length; i++)
		{
			BodyConverter.m_Table_LBR[i] = -1;
		}
		for (int j = 0; j < BodyConverter.m_Table_AOS.Length; j++)
		{
			BodyConverter.m_Table_AOS[j] = -1;
		}
		for (int k = 0; k < BodyConverter.m_Table_AOW.Length; k++)
		{
			BodyConverter.m_Table_AOW[k] = -1;
		}
		for (int l = 0; l < BodyConverter.m_Table_ML.Length; l++)
		{
			BodyConverter.m_Table_ML[l] = -1;
		}
		using StreamReader streamReader = new StreamReader(path);
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			if ((text = text.Trim()).Length == 0 || text.StartsWith("#"))
			{
				continue;
			}
			int num = text.IndexOf('#');
			if (num >= 0)
			{
				text = text.Substring(0, num);
			}
			text = Regex.Replace(text.Trim(), "\\s+", "\t");
			try
			{
				string[] array = text.Split('\t');
				if (array.Length < 2)
				{
					continue;
				}
				int num2 = System.Convert.ToInt32(array[0]);
				int num3 = System.Convert.ToInt32(array[1]);
				int num4 = -1;
				int num5 = -1;
				int num6 = -1;
				try
				{
					if (array.Length >= 3)
					{
						num4 = System.Convert.ToInt32(array[2]);
					}
				}
				catch
				{
				}
				try
				{
					if (array.Length >= 4)
					{
						num5 = System.Convert.ToInt32(array[3]);
					}
				}
				catch
				{
				}
				try
				{
					if (array.Length >= 5)
					{
						num6 = System.Convert.ToInt32(array[4]);
					}
				}
				catch
				{
				}
				if (num2 >= 0 && num2 < BodyConverter.m_Table_LBR.Length)
				{
					if (num3 == 68)
					{
						num3 = 122;
					}
					BodyConverter.m_Table_LBR[num2] = num3;
				}
				if (num2 >= 0 && num2 < BodyConverter.m_Table_AOS.Length)
				{
					BodyConverter.m_Table_AOS[num2] = num4;
				}
				if (num2 >= 0 && num2 < BodyConverter.m_Table_AOW.Length)
				{
					BodyConverter.m_Table_AOW[num2] = num5;
				}
				if (num2 >= 0 && num2 < BodyConverter.m_Table_ML.Length)
				{
					BodyConverter.m_Table_ML[num2] = num6;
				}
			}
			catch
			{
				Debug.Error("Bad def format");
			}
		}
	}

	public static bool Contains(int bodyID)
	{
		if (BodyConverter.m_Table_LBR != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_LBR.Length && BodyConverter.m_Table_LBR[bodyID] != -1)
		{
			return true;
		}
		if (BodyConverter.m_Table_AOS != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_AOS.Length && BodyConverter.m_Table_AOS[bodyID] != -1)
		{
			return true;
		}
		if (BodyConverter.m_Table_AOW != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_AOW.Length && BodyConverter.m_Table_AOW[bodyID] != -1)
		{
			return true;
		}
		if (BodyConverter.m_Table_ML != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_ML.Length && BodyConverter.m_Table_ML[bodyID] != -1)
		{
			return true;
		}
		return false;
	}

	public static int GetFileSet(int bodyID)
	{
		if (BodyConverter.m_Table_LBR != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_LBR.Length)
		{
			int num = BodyConverter.m_Table_LBR[bodyID];
			if (num != -1)
			{
				return 2;
			}
		}
		if (BodyConverter.m_Table_AOS != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_AOS.Length)
		{
			int num2 = BodyConverter.m_Table_AOS[bodyID];
			if (num2 != -1)
			{
				return 3;
			}
		}
		if (BodyConverter.m_Table_AOW != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_AOW.Length)
		{
			int num3 = BodyConverter.m_Table_AOW[bodyID];
			if (num3 != -1)
			{
				return 4;
			}
		}
		if (BodyConverter.m_Table_ML != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_ML.Length)
		{
			int num4 = BodyConverter.m_Table_ML[bodyID];
			if (num4 != -1)
			{
				return 5;
			}
		}
		return 1;
	}

	public static int Convert(ref int bodyID)
	{
		if (BodyConverter.m_Table_LBR != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_LBR.Length)
		{
			int num = BodyConverter.m_Table_LBR[bodyID];
			if (num != -1)
			{
				bodyID = num;
				return 2;
			}
		}
		if (BodyConverter.m_Table_AOS != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_AOS.Length)
		{
			int num2 = BodyConverter.m_Table_AOS[bodyID];
			if (num2 != -1)
			{
				bodyID = num2;
				return 3;
			}
		}
		if (BodyConverter.m_Table_AOW != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_AOW.Length)
		{
			int num3 = BodyConverter.m_Table_AOW[bodyID];
			if (num3 != -1)
			{
				bodyID = num3;
				return 4;
			}
		}
		if (BodyConverter.m_Table_ML != null && bodyID >= 0 && bodyID < BodyConverter.m_Table_ML.Length)
		{
			int num4 = BodyConverter.m_Table_ML[bodyID];
			if (num4 != -1)
			{
				bodyID = num4;
				return 5;
			}
		}
		return 1;
	}
}
