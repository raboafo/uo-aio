using Veritas;

namespace UOAIO;

public class MacroConfig : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private MacroSetCollection macroSets;

	public override PersistableType TypeID => MacroConfig.TypeCode;

	public MacroSetCollection MacroSets => this.macroSets;

	public MacroSet this[int serial, int server]
	{
		get
		{
			foreach (MacroSet macroSet in this.macroSets)
			{
				if (macroSet.Serial == serial && macroSet.Server == server)
				{
					return macroSet;
				}
			}
			return null;
		}
	}

	public MacroConfig()
	{
		this.macroSets = new MacroSetCollection();
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		for (int i = 0; i < this.macroSets.Count; i++)
		{
			this.macroSets[i].Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.macroSets.Add(ip.GetChild() as MacroSet);
		}
	}

	static MacroConfig()
	{
		MacroConfig.TypeCode = new PersistableType("macroConfig", null, MacroSet.TypeCode);
	}
}
