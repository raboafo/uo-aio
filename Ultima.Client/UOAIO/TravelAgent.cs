using Veritas;

namespace UOAIO;

public class TravelAgent : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private RunebookInfoCollection m_Runebooks;

	public override PersistableType TypeID => TravelAgent.TypeCode;

	public RunebookInfoCollection Runebooks => this.m_Runebooks;

	public RunebookInfo this[Item runebook]
	{
		get
		{
			RunebookInfo runebookInfo;
			for (int i = 0; i < this.m_Runebooks.Count; i++)
			{
				runebookInfo = this.m_Runebooks[i];
				if (runebookInfo.Serial == runebook.Serial)
				{
					return runebookInfo;
				}
			}
			this.m_Runebooks.Add(runebookInfo = new RunebookInfo(runebook));
			return runebookInfo;
		}
	}

	private static PersistableObject Construct()
	{
		return new TravelAgent();
	}

	public TravelAgent()
	{
		this.m_Runebooks = new RunebookInfoCollection();
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		for (int i = 0; i < this.m_Runebooks.Count; i++)
		{
			this.m_Runebooks[i].Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.m_Runebooks.Add(ip.GetChild() as RunebookInfo);
		}
	}

	static TravelAgent()
	{
		TravelAgent.TypeCode = new PersistableType("travel", Construct, RunebookInfo.TypeCode);
	}
}
