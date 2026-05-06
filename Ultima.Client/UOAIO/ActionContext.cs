using System;
using System.Collections.Generic;

namespace UOAIO;

public abstract class ActionContext : IComparable
{
	private static List<ActionContext> _queue;

	private static Queue<ActionContext> _pending;

	private static Stack<ActionContext> _active;

	private bool _wasDelayed;

	public static List<ActionContext> Queued => ActionContext._queue;

	public static Queue<ActionContext> Pending => ActionContext._pending;

	public bool IsPending => ActionContext._pending.Contains(this) || ActionContext._active.Contains(this);

	public static ActionContext Active => (ActionContext._active.Count > 0) ? ActionContext._active.Peek() : null;

	public bool WasDelayed
	{
		get
		{
			return this._wasDelayed;
		}
		set
		{
			this._wasDelayed = value;
		}
	}

	protected virtual bool IsReady => true;

	protected virtual bool ShouldRepeat => this._wasDelayed;

	public static void Clear()
	{
		ActionContext._pending.Clear();
		ActionContext._active.Clear();
		ActionContext._queue.Clear();
	}

	public static void HandleSignal(bool isBegin)
	{
		if (isBegin)
		{
			if (ActionContext._pending.Count > 0)
			{
				ActionContext._pending.Dequeue().Begin();
			}
		}
		else if (ActionContext._active.Count > 0)
		{
			ActionContext._active.Pop().Finish();
		}
	}

	public ActionContext()
	{
	}

	protected virtual bool CheckDispatch()
	{
		return true;
	}

	protected virtual bool CheckQueue()
	{
		return true;
	}

	public static void InvokeQueue()
	{
		while (ActionContext._queue.Count > 0)
		{
			ActionContext actionContext = ActionContext._queue[0];
			if (ActionContext._pending.Contains(actionContext) || !actionContext.IsReady || actionContext.Dispatch())
			{
				break;
			}
			ActionContext._queue.RemoveAt(0);
		}
	}

	public bool Enqueue()
	{
		if (!this.CheckQueue())
		{
			return false;
		}
		ActionContext._queue.Add(this);
		int num = ActionContext._queue.Count - 1;
		while (num - 1 > 0 && this.CompareTo(ActionContext._queue[num - 1]) < 0)
		{
			ActionContext._queue[num] = ActionContext._queue[num - 1];
			ActionContext._queue[num - 1] = this;
			num--;
		}
		this.OnEnqueue();
		ActionContext.InvokeQueue();
		return true;
	}

	protected virtual void OnEnqueue()
	{
	}

	public bool Dispatch()
	{
		if (!this.CheckDispatch())
		{
			return false;
		}
		ActionContext._pending.Enqueue(this);
		Network.Send(new BeginCriticalRegion());
		this.OnDispatch();
		Network.Send(new LeaveCriticalRegion());
		Network.Flush();
		return true;
	}

	public virtual void OnDispatch()
	{
	}

	public void Begin()
	{
		this._wasDelayed = false;
		ActionContext._active.Push(this);
		this.OnBegin();
	}

	public virtual void OnBegin()
	{
	}

	public void Finish()
	{
		this.OnFinish();
		if (ActionContext._queue.Count > 0 && ActionContext._queue[0] == this)
		{
			if (!this.ShouldRepeat)
			{
				ActionContext._queue.RemoveAt(0);
			}
			ActionContext.InvokeQueue();
		}
	}

	public virtual void OnFinish()
	{
	}

	public virtual bool OnSpeech(string text)
	{
		return true;
	}

	public virtual void OnContextBegin(object owner)
	{
	}

	public virtual bool OnContextItem(object owner, int entryID, int stringID)
	{
		return false;
	}

	public virtual bool OnContextEnd(object owner, bool selected)
	{
		return true;
	}

	int IComparable.CompareTo(object obj)
	{
		return this.CompareTo(obj as ActionContext);
	}

	protected virtual int CompareTo(ActionContext cmp)
	{
		return cmp.IsPending.CompareTo(this.IsPending);
	}

	static ActionContext()
	{
		ActionContext._queue = new List<ActionContext>();
		ActionContext._pending = new Queue<ActionContext>();
		ActionContext._active = new Stack<ActionContext>();
	}
}
