using System.Collections.Generic;

namespace UOAIO;

public class ScratchQueue<T> : Scratch<Queue<T>>
{
	protected override void Release(Queue<T> value)
	{
		base.Release(value);
		value.Clear();
	}
}
