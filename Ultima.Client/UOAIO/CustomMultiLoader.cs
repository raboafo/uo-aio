using System.Collections.Generic;

namespace UOAIO;

public class CustomMultiLoader
{
	private static Dictionary<int, List<CustomMultiEntry>> m_Table;

	public static CustomMultiEntry GetCustomMulti(int serial, int revision)
	{
		List<CustomMultiEntry> value = null;
		if (!CustomMultiLoader.m_Table.TryGetValue(serial, out value))
		{
			return null;
		}
		for (int i = 0; i < value.Count; i++)
		{
			CustomMultiEntry customMultiEntry = value[i];
			if (customMultiEntry.Revision == revision)
			{
				return customMultiEntry;
			}
		}
		return null;
	}

	public static void SetCustomMulti(int serial, int revision, Multi baseMulti, int compressionType, byte[] buffer)
	{
		List<CustomMultiEntry> value = null;
		if (!CustomMultiLoader.m_Table.TryGetValue(serial, out value))
		{
			value = (CustomMultiLoader.m_Table[serial] = new List<CustomMultiEntry>());
		}
		CustomMultiEntry customMultiEntry = new CustomMultiEntry(serial, revision, baseMulti, compressionType, buffer);
		for (int i = 0; i < value.Count; i++)
		{
			CustomMultiEntry customMultiEntry2 = value[i];
			if (customMultiEntry2.Revision == revision)
			{
				value[i] = customMultiEntry;
				return;
			}
		}
		value.Add(customMultiEntry);
		Map.Invalidate();
		GRadar.Invalidate();
	}

	static CustomMultiLoader()
	{
		CustomMultiLoader.m_Table = new Dictionary<int, List<CustomMultiEntry>>();
	}
}
