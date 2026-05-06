using Veritas;

namespace UOAIO;

public class ActionData : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private string command;

	private string param;

	public override PersistableType TypeID => ActionData.TypeCode;

	public string Command
	{
		get
		{
			return this.command;
		}
		set
		{
			this.command = value;
		}
	}

	public string Param
	{
		get
		{
			return this.param;
		}
		set
		{
			this.param = value;
		}
	}

	private static PersistableObject Construct()
	{
		return new ActionData();
	}

	public ActionData()
	{
	}

	public ActionData(string command, string param)
	{
		this.command = command;
		this.param = param;
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetString("command", this.command);
		if (this.param != null && this.param.Length > 0)
		{
			op.SetString("param", this.param);
		}
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.command = ip.GetString("command");
		this.param = ip.GetString("param");
		if (this.param == null)
		{
			this.param = string.Empty;
		}
	}

	static ActionData()
	{
		ActionData.TypeCode = new PersistableType("action", Construct);
	}
}
