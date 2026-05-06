using System;
using Veritas;

namespace UOAIO;

public class RuneInfoEx : PersistableObject, IEquatable<RuneInfoEx>
{
	public static readonly PersistableType TypeCode;

	private string m_Name;

	private Point3D m_Point;

	private int m_ItemID;

	private int m_GumpSerial;

	private int m_RecallButtonID;

	private int m_DialogID;

	private int m_GateButtonID;

	private int m_RunebookSerial;

	public override PersistableType TypeID => RuneInfoEx.TypeCode;

	public string Name => this.m_Name;

	public Point3D Point => this.m_Point;

	public int ItemID => this.m_ItemID;

	public int GumpSerial => this.m_GumpSerial;

	public int RecallButtonID => this.m_RecallButtonID;

	public int DialogID => this.m_DialogID;

	public int GateButtonID => this.m_GateButtonID;

	public int RunebookSerial => this.m_RunebookSerial;

	private static PersistableObject Construct()
	{
		return new RuneInfoEx();
	}

	private RuneInfoEx()
	{
	}

	public RuneInfoEx(string name, Point3D p, int itemID, int gumpSerial, int recallButtonID, int dialogID)
	{
		this.m_Name = name;
		this.m_Point = p;
		this.m_ItemID = itemID;
		this.m_GumpSerial = gumpSerial;
		this.m_RecallButtonID = recallButtonID;
		this.m_DialogID = dialogID;
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetString("name", this.m_Name);
		op.SetInt32("point-x", this.m_Point.X);
		op.SetInt32("point-y", this.m_Point.Y);
		if (this.m_Point.Z != 0)
		{
			op.SetInt32("point-z", this.m_Point.Z);
		}
		op.SetInt32("item-id", this.m_ItemID);
		op.SetInt32("gump-serial", this.m_GumpSerial);
		op.SetInt32("recall-button", this.m_RecallButtonID);
		op.SetInt32("gate-button", this.GateButtonID);
		op.SetInt32("dialog-id", this.m_DialogID);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.m_Name = ip.GetString("name");
		this.m_Point = new Point3D(ip.GetInt32("point-x"), ip.GetInt32("point-y"), ip.GetInt32("point-z"));
		this.m_ItemID = ip.GetInt32("item-id");
		this.m_GumpSerial = ip.GetInt32("gump-serial");
		this.m_RecallButtonID = ip.GetInt32("recall-button");
		this.m_GateButtonID = ip.GetInt32("gate-button");
		this.m_DialogID = ip.GetInt32("dialog-id");
	}

	public bool Equals(RuneInfoEx other)
	{
		if (other != null && this.m_Name == other.m_Name)
		{
			return this.m_Point == other.m_Point;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return this.m_Name.GetHashCode() ^ this.m_Point.GetHashCode() ^ this.m_ItemID.GetHashCode() ^ this.m_GumpSerial.GetHashCode() ^ this.m_RecallButtonID.GetHashCode() ^ this.m_GateButtonID.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return this.Equals(obj as RuneInfoEx);
	}

	static RuneInfoEx()
	{
		RuneInfoEx.TypeCode = new PersistableType("rune-ex", Construct);
	}

	public RuneInfoEx(string name, Point3D p, int recallButtonID, int gateButtonID)
	{
		this.m_Name = name;
		this.m_Point = p;
		this.m_RecallButtonID = recallButtonID;
		this.m_GateButtonID = gateButtonID;
	}

	public RuneInfoEx(int runebookSerial, string name, Point3D p, int recallButtonID, int gateButtonID)
	{
		this.m_RunebookSerial = runebookSerial;
		this.m_Name = name;
		this.m_Point = p;
		this.m_RecallButtonID = recallButtonID;
		this.m_GateButtonID = gateButtonID;
	}
}
