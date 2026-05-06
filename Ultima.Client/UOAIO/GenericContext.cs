using System;

namespace UOAIO;

internal class GenericContext : ActionContext
{
	private readonly System.Action callback;

	public GenericContext(System.Action callback)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		this.callback = callback;
	}

	public override void OnDispatch()
	{
		this.callback();
	}
}
