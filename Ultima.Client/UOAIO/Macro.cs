using System;
using System.Windows.Forms;
using UOAIO.Profiles;

namespace UOAIO;

public class Macro : IComparable
{
	private MacroData m_Data;

	private int m_Index;

	private static Macro m_Current;

	private DateTime m_DelayEnd;

	public MacroData Data => this.m_Data;

	public Keys Key
	{
		get
		{
			return this.m_Data.Key;
		}
		set
		{
			this.m_Data.Key = value;
		}
	}

	public Keys Mods
	{
		get
		{
			return this.m_Data.Mods;
		}
		set
		{
			this.m_Data.Mods = value;
		}
	}

	public bool Control
	{
		get
		{
			return this.m_Data.Control;
		}
		set
		{
			this.m_Data.Control = value;
		}
	}

	public bool Alt
	{
		get
		{
			return this.m_Data.Alt;
		}
		set
		{
			this.m_Data.Alt = value;
		}
	}

	public bool Shift
	{
		get
		{
			return this.m_Data.Shift;
		}
		set
		{
			this.m_Data.Shift = value;
		}
	}

	public ActionCollection Actions => this.m_Data.Actions;

	public bool Running => this.m_Index >= 0;

	public bool IsWheel => this.m_Data.IsWheel;

	public void AddAction(Action a)
	{
		this.m_Data.Actions.Add(a);
	}

	public bool CheckKey(Keys key)
	{
		Keys modifierKeys = System.Windows.Forms.Control.ModifierKeys;
		return key == this.Key && modifierKeys == this.Mods;
	}

	public void Start()
	{
		this.m_Index = 0;
		if (this.Slice())
		{
			Macros.Running.Add(this);
		}
	}

	public void Stop()
	{
		if (this.m_Index >= 0)
		{
			if (Macros.Running.Contains(this))
			{
				Macros.Running.Remove(this);
			}
			this.m_Index = -1;
		}
	}

	public static void Repeat()
	{
		if (Macro.m_Current != null)
		{
			Macro.m_Current.m_Index = -1;
		}
	}

	public static bool Delay(int ms)
	{
		if (Macro.m_Current != null)
		{
			if (Macro.m_Current.m_DelayEnd == DateTime.MinValue)
			{
				Macro.m_Current.m_DelayEnd = DateTime.Now + TimeSpan.FromMilliseconds(ms);
				return false;
			}
			if (DateTime.Now >= Macro.m_Current.m_DelayEnd)
			{
				Macro.m_Current.m_DelayEnd = DateTime.MinValue;
				return true;
			}
			return false;
		}
		return true;
	}

	public bool Slice()
	{
		Macro.m_Current = this;
		if (this.m_Index < this.Actions.Count)
		{
			Action action = this.Actions[this.m_Index];
			ActionHandler handler = action.Handler;
			if (handler == null || (!Options.Current.HotkeysEnabled && handler.Name != "Toggle Hotkeys") || handler.Callback(action.Param))
			{
				this.m_Index++;
			}
		}
		if (this.m_Index >= this.Actions.Count)
		{
			this.m_Index = -1;
		}
		Macro.m_Current = null;
		return this.Running;
	}

	public Macro(MacroData data)
	{
		this.m_Data = data;
		this.m_Index = -1;
	}

	public int CompareTo(object obj)
	{
		Macro macro = (Macro)obj;
		if (this.IsWheel && !macro.IsWheel)
		{
			return -1;
		}
		if (!this.IsWheel && macro.IsWheel)
		{
			return 1;
		}
		int num = this.m_Data.Key - macro.m_Data.Key;
		if (num == 0)
		{
			num = this.m_Data.Mods - macro.m_Data.Mods;
		}
		return num;
	}
}
