using System.Reflection;
using UOAIO;
using Veritas;

namespace UOAIOPlugins.Automation;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class AutomationOptions : PersistableObject
{
	public static readonly PersistableType TypeCode;

	public override PersistableType TypeID => AutomationOptions.TypeCode;

	[Optionable("Auto Heal Range", "Automations", Default = 10)]
	public int AutoHealRange { get; set; }

	[Optionable("Auto Heal Value", "Automations", Default = 62)]
	public int AutoHealValue { get; set; }

	private static PersistableObject Construct()
	{
		return new AutomationOptions();
	}

	public AutomationOptions()
	{
		this.AutoHealRange = 10;
		this.AutoHealValue = 62;
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetInt32("auto-heal-range", this.AutoHealRange);
		op.SetInt32("auto-heal-value", this.AutoHealValue);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.AutoHealRange = ip.GetInt32("auto-heal-range");
		this.AutoHealValue = ip.GetInt32("auto-heal-value");
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
	}

	static AutomationOptions()
	{
		AutomationOptions.TypeCode = new PersistableType("automations", Construct);
	}
}
