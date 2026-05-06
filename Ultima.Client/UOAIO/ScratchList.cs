using System.Collections.Generic;

namespace UOAIO;

public class ScratchList<T> : Scratch<List<T>>
{
	protected override void Release(List<T> value)
	{
		base.Release(value);
		value.Clear();
	}

	public static T[] ToArray(IEnumerable<T> collection)
	{
		using ScratchList<T> scratchList = new ScratchList<T>();
		List<T> list = scratchList.Value;
		list.AddRange(collection);
		return list.ToArray();
	}
}
