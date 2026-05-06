using System.Collections.Generic;
using System.IO;

namespace UOAIO;

public static class Speech
{
	private static SpeechEntry[] m_Speech;

	public static SpeechEntry[] GetKeywords(string text)
	{
		if (Speech.m_Speech == null)
		{
			Speech.LoadSpeechTable();
		}
		text = text.ToLower();
		List<SpeechEntry> list = null;
		SpeechEntry[] speech = Speech.m_Speech;
		int num = speech.Length;
		for (int i = 0; i < num; i++)
		{
			SpeechEntry speechEntry = speech[i];
			if (Speech.IsMatch(text, speechEntry.m_Keywords))
			{
				if (list == null)
				{
					list = new List<SpeechEntry>();
				}
				list.Add(speechEntry);
			}
		}
		if (list == null)
		{
			return new SpeechEntry[0];
		}
		list.Sort();
		return list.ToArray();
	}

	public static bool IsMatch(string input, string[] split)
	{
		int num = 0;
		for (int i = 0; i < split.Length; i++)
		{
			if (split[i].Length > 0)
			{
				int num2 = input.IndexOf(split[i], num);
				if (num2 > 0 && i == 0)
				{
					return false;
				}
				if (num2 < 0)
				{
					return false;
				}
				num = num2 + split[i].Length;
			}
		}
		return split[split.Length - 1].Length <= 0 || num == input.Length;
	}

	public unsafe static void LoadSpeechTable()
	{
		string text = Engine.FileManager.ResolveMUL("Speech.mul");
		if (!File.Exists(text))
		{
			Speech.m_Speech = new SpeechEntry[0];
			Debug.Trace("File '{0}' not found, speech will not be encoded.", text);
			return;
		}
		fixed (byte* ptr = new byte[1024])
		{
			List<SpeechEntry> list = new List<SpeechEntry>();
			FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.Read);
			int num = 0;
			while ((num = UnsafeMethods.ReadFile(fileStream, ptr, 4)) > 0)
			{
				int idKeyword = ptr[1] | (*ptr << 8);
				int num2 = ptr[3] | (ptr[2] << 8);
				if (num2 > 0)
				{
					UnsafeMethods.ReadFile(fileStream, ptr, num2);
					list.Add(new SpeechEntry(idKeyword, new string((sbyte*)ptr, 0, num2)));
				}
			}
			fileStream.Close();
			Speech.m_Speech = list.ToArray();
		}
	}

	public static void Dispose()
	{
		Speech.m_Speech = null;
	}
}
