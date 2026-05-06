using Veritas;

namespace UOAIO;

public class MacroSet : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private int serial;

	private int server;

	private MacroCollection macros;

	public override PersistableType TypeID => MacroSet.TypeCode;

	public int Serial
	{
		get
		{
			return this.serial;
		}
		set
		{
			this.serial = value;
		}
	}

	public int Server
	{
		get
		{
			return this.server;
		}
		set
		{
			this.server = value;
		}
	}

	public MacroCollection Macros => this.macros;

	private static PersistableObject Construct()
	{
		return new MacroSet();
	}

	public MacroSet()
	{
		this.macros = new MacroCollection();
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetInt32("serial", this.serial);
		op.SetInt32("server", this.server);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.serial = ip.GetInt32("serial");
		this.server = ip.GetInt32("server");
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		for (int i = 0; i < this.macros.Count; i++)
		{
			this.macros[i].Data.Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.macros.Add(new Macro(ip.GetChild() as MacroData));
		}
	}

	static MacroSet()
	{
		MacroSet.TypeCode = new PersistableType("macroSet", Construct, MacroData.TypeCode);
	}
}
