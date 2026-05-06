using System.Windows.Forms;
using Veritas;

namespace UOAIO;

public class MacroData : PersistableObject
{
	public static readonly PersistableType TypeCode;

	public const Keys WheelUp = (Keys)69632;

	public const Keys WheelDown = (Keys)69633;

	public const Keys WheelPress = (Keys)69634;

	private Keys key;

	private Keys mods;

	private ActionCollection actions;

	public override PersistableType TypeID => MacroData.TypeCode;

	public Keys Key
	{
		get
		{
			return this.key;
		}
		set
		{
			this.key = value;
		}
	}

	public Keys Mods
	{
		get
		{
			return this.mods;
		}
		set
		{
			this.mods = value;
		}
	}

	public ActionCollection Actions => this.actions;

	public bool IsWheel => this.key == (Keys)69632 || this.key == (Keys)69633 || this.key == (Keys)69634;

	public bool Control
	{
		get
		{
			return this.GetMod(Keys.Control);
		}
		set
		{
			this.SetMod(Keys.Control, value);
		}
	}

	public bool Alt
	{
		get
		{
			return this.GetMod(Keys.Alt);
		}
		set
		{
			this.SetMod(Keys.Alt, value);
		}
	}

	public bool Shift
	{
		get
		{
			return this.GetMod(Keys.Shift);
		}
		set
		{
			this.SetMod(Keys.Shift, value);
		}
	}

	private static PersistableObject Construct()
	{
		return new MacroData();
	}

	private bool GetMod(Keys key)
	{
		return (this.mods & key) != 0;
	}

	private void SetMod(Keys key, bool value)
	{
		if (value)
		{
			this.mods |= key;
		}
		else
		{
			this.mods &= ~key;
		}
	}

	public MacroData()
	{
		this.actions = new ActionCollection();
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		int num = 0;
		if (this.Control)
		{
			num |= 1;
		}
		if (this.Alt)
		{
			num |= 2;
		}
		if (this.Shift)
		{
			num |= 4;
		}
		op.SetInt32("key", (int)this.key);
		op.SetInt32("modBits", num);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.key = (Keys)ip.GetInt32("key");
		int @int = ip.GetInt32("modBits");
		this.Control = (@int & 1) != 0;
		this.Alt = (@int & 2) != 0;
		this.Shift = (@int & 4) != 0;
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		for (int i = 0; i < this.actions.Count; i++)
		{
			this.actions[i].Data.Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.actions.Add(new Action(ip.GetChild() as ActionData));
		}
	}

	static MacroData()
	{
		MacroData.TypeCode = new PersistableType("macro", Construct, ActionData.TypeCode);
	}
}
