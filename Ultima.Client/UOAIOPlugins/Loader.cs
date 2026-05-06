using System.Threading;
using UOAIO;
using UOAIOPlugins.Automation;
using UOAIOPlugins.Bars;
using UOAIOPlugins.Display;
using UOAIOPlugins.Items;
using UOAIOPlugins.Targeting;

namespace UOAIOPlugins;

public class Loader
{
	private static readonly ClientFormat _ClientFormat;

	private static Thread m_Load;

	public static void Initialize()
	{
		Console.Print("Starting initialization...", Console.MessageType.Information);
		Loader.m_Load = new Thread(Load);
		Loader.m_Load.IsBackground = true;
		Loader.m_Load.Start();
		Console.Print("Initialization finished.", Console.MessageType.Information);
	}

	private static void Load()
	{
		Loader.InitializeAutomation();
		Loader.InitializeGumps();
		Loader.InitializeItems();
		Loader.InitializeTargeting();
		Loader.InitializeUtilities();
		Macros.Reset();
		Macros.Config = null;
		Macros.Current = Macros.FindCurrent();
		Macros.Slice();
	}

	private static void InitializeAutomation()
	{
		Console.Print("Initializing Automation", Console.MessageType.Information);
		Loader._ClientFormat.Register("heal", Defender.Command_Callback);
		Loader._ClientFormat.Register("healrange", Defender.HealRangeCommand_Callback);
		MacroHandlers.Register(AutoCure.Macro_Callback, "Cheats|Automation|Auto Cure@Auto Cure", ParamNode.Toggle);
		MacroHandlers.Register(Defender.Macro_Callback, "Cheats|Automation|Auto Heal@Auto Heal", ParamNode.Toggle);
		MacroHandlers.Register(AutoBandage.Macro_Callback, "Cheats|Automation|Auto Bandage@Auto Bandage", ParamNode.Toggle);
		MacroHandlers.Register(AutoExploPot.Macro_Callback, "Cheats|Automation|Auto Explo Pot@Auto Explo Pot", ParamNode.Empty);
		MacroHandlers.Register(AutoMeditate.Macro_Callback, "Cheats|Automation|Auto Meditate@Auto Meditate", ParamNode.Toggle);
		MacroHandlers.Register(AutoMount.Macro_Callback, "Cheats|Automation|Auto Mount@Auto Mount", ParamNode.Toggle);
		MacroHandlers.Register(AutoStrPot.Macro_Callback, "Cheats|Automation|Auto StrPot@Auto StrPot", ParamNode.Toggle);
		MacroHandlers.Register(AutoStun.Macro_Callback, "Cheats|Automation|Auto Stun@Auto Stun", ParamNode.Toggle);
		MacroHandlers.Register(AutoPopPouch.Macro_Callback, "Cheats|Automation|Auto Use Once@Auto Use Once", ParamNode.Toggle);
	}

	private static void InitializeGumps()
	{
		Console.Print("Initializing Gumps", Console.MessageType.Information);
		Gumps.Desktop.Children.Add(new GPlayerInfo());
		Gumps.Desktop.Children.Add(new GAutoHeal());
		Gumps.Desktop.Children.Add(new GAutoCure());
		Gumps.Desktop.Children.Add(new GAutoBandage());
		Gumps.Desktop.Children.Add(new GAutoStrPot());
		Gumps.Desktop.Children.Add(new GLootCorpseNG());
		Gumps.Desktop.Children.Add(new GAutoStun());
		Gumps.Desktop.Children.Add(new GAutoPopPouch());
		Gumps.Desktop.Children.Add(new GAutoMed());
		Gumps.Desktop.Children.Add(new GAutoMount());
	}

	private static void InitializeItems()
	{
		Console.Print("Initializing Items", Console.MessageType.Information);
		Loader._ClientFormat.Register("additem", ItemIDs.Command_Callback);
		MacroHandlers.Register(Scrolls.UseScroll_OnAction, "Cheats|Items|UseScroll@Use Scroll", "Chain Lightning", "Energy Field", "Flame Strike", "Gate Travel", "Recall", "Mass Dispel", "Meteor Swarm", "Earthquake", "Resurrection");
		MacroHandlers.Register(LootCorpseNG.Macro_Callback, "Cheats|Items|Loot CorpseNG@Loot CorpseNG", ParamNode.Toggle);
		MacroHandlers.Register(DropStool.Macro_Callback, "Cheats|Items|Drop Stool", ParamNode.Empty);
		MacroHandlers.Register(UseItemByType.UseItemByType_OnAction, "Cheats|Items|UseByType@Use By Type", "Storms Eye", "Gem of Emp", "Urn", "Blood Rose", "Clarity Pot");
	}

	private static void InitializeTargeting()
	{
		Console.Print("Initializing Targeting", Console.MessageType.Information);
		Loader._ClientFormat.Register("getbars", GetBars.GetBars_OnCommand);
		Loader._ClientFormat.Register("clearbars", GetBars.ClearBars_OnCommand);
		MacroHandlers.Register(CleanHouse.Macro_Callback, "Cheats|Targeting|Clean House@Clean House", ParamNode.Empty);
		MacroHandlers.Register(TargetGate.TargetGate_OnMacro, "Cheats|Targeting|Target Gate@Target Gate", ParamNode.Empty);
		MacroHandlers.Register(SmartTeleport.Macro_Callback, "Cheats|Targeting|Smart Teleport@Smart Teleport", ParamNode.Empty);
		SmartAoE.Initialize();
	}

	private static void InitializeVisuals()
	{
		Console.Print("Initializing Visuals", Console.MessageType.Information);
		Renderer.DrawPing = true;
		GItemCounters.Active = true;
		GRadar.Open();
		Engine.OpenPaperdoll();
		Engine.OpenBackpack();
	}

	private static void InitializeUtilities()
	{
	}

	static Loader()
	{
		Loader._ClientFormat = (ClientFormat)SpeechFormat.Client;
	}
}
