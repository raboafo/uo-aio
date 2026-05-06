using System.Collections;
using System.Reflection;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class CategorySorter : IComparer
{
	public int Compare(object x, object y)
	{
		DictionaryEntry dictionaryEntry = (DictionaryEntry)x;
		DictionaryEntry dictionaryEntry2 = (DictionaryEntry)y;
		return ((string)dictionaryEntry.Key).CompareTo((string)dictionaryEntry2.Key);
	}
}
