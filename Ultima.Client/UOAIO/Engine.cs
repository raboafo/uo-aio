using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using SharpDX.Direct3D9;
using Ultima.Data;
using UOAIO.Assets;
using UOAIO.Events;
using UOAIO.Profiles;
using UOAIO.Prompts;
using UOAIO.ShardRuntime;
using UOAIO.Targeting;
using UOAIO.Videos;
using Veritas;

namespace UOAIO;

public class Engine
{
	private class BandageContext : TargetContext
	{
		public BandageContext(Item bandage, Mobile toHeal)
			: base(toHeal)
		{
			base.toUse = bandage;
			base.isManual = true;
		}
	}

	public struct FLASHWINFO
	{
		public uint cbSize;

		public IntPtr hwnd;

		public uint dwFlags;

		public uint uCount;

		public uint dwTimeout;
	}

	private class DictionaryComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)x;
			return (int)((DictionaryEntry)y).Key - (int)dictionaryEntry.Key;
		}
	}

	private class DisplayModeComparer : IComparer<DisplayMode>
	{
		private int m_WantWidth;

		private int m_WantHeight;

		private Format m_WantFormat;

		public DisplayModeComparer(int w, int h, Format f)
		{
			this.m_WantWidth = w;
			this.m_WantHeight = h;
			this.m_WantFormat = f;
		}

		public int Compare(DisplayMode a, DisplayMode b)
		{
			int num = Math.Abs(a.Width * a.Height - this.m_WantWidth * this.m_WantHeight);
			int num2 = Math.Abs(b.Width * b.Height - this.m_WantWidth * this.m_WantHeight);
			int num3 = num - num2;
			if (num3 != 0)
			{
				return num3;
			}
			num = ((a.Format != this.m_WantFormat) ? ((a.Format == Format.A1R5G5B5) ? 1 : ((a.Format != Format.R5G6B5) ? 3 : 2)) : 0);
			num2 = ((b.Format != this.m_WantFormat) ? ((b.Format == Format.A1R5G5B5) ? 1 : ((b.Format != Format.R5G6B5) ? 3 : 2)) : 0);
			return num - num2;
		}
	}

	private string cheese = "delicious";

	public static bool m_GMFollow;

	internal static int m_Ticks;

	internal static double m_dTicks;

	public static bool m_SetTicks;

	private static Stopwatch _sw;

	public static Direct3D m_Direct3D;

	public static Device m_Device;

	public const double HalfPI = Math.PI / 2.0;

	public const double FlipIt = 3.9269908169872414;

	public static Rectangle m_rRender;

	public static Multis m_Multis;

	public static Sounds m_Sounds;

	public static Gumps m_Gumps;

	public static LandArt m_LandArt;

	public static TextureArt m_TextureArt;

	public static ItemArt m_ItemArt;

	public static Animations m_Animations;

	public static Skills m_Skills;

	public static Effects m_Effects;

	public static Features m_Features;

	private static ServerFeatures m_ServerFeatures;

	private static Queue<ArrayList> m_DataStores;

	public static int ScreenWidth;

	public static int ScreenHeight;

	public static int GameWidth;

	public static int GameHeight;

	public static int GameX;

	public static int GameY;

	public const int FALSE = 0;

	public const int TRUE = 1;

	public static bool exiting;

	public static int m_Sequence;

	public static int m_OkSequence;

	public static int m_WalkReq;

	public static int m_WalkAck;

	public static int m_xMouse;

	public static int m_yMouse;

	public static int m_dMouse;

	public static Display m_Display;

	public static TimeDelay m_MoveDelay;

	public static bool amMoving;

	public static Direction movingDir;

	public static Direction pointingDir;

	public static bool m_regMap;

	public static bool m_Redraw;

	public static string m_Text;

	public static Font[] m_Font;

	public static UnicodeFont[] m_UniFont;

	public static TimeDelay m_NewFrame;

	public static TimeDelay m_SleepMode;

	public static bool m_Ingame;

	public static bool m_Loading;

	public static List<Timer> m_Timers;

	public static bool m_Fullscreen;

	public static bool m_Locked;

	public static int m_OpenedGumps;

	public static bool m_SkillsOpen;

	public static GSkills m_SkillsGump;

	public static bool m_JournalOpen;

	public static GJournal m_JournalGump;

	public static List<JournalEntry> m_Journal;

	public static FileManager m_FileManager;

	public const int WalkAckSync = 4;

	public static DateTime m_LastAction;

	private static DateTime m_LastStealthUse;

	private static DateTime m_LastLeapfrogPickup;

	public const float C528Scale = 8.225806f;

	public const float C825Scale = 0.12156863f;

	public static Regex m_Encoder;

	public const string CommandPrefix = ". ";

	public static bool m_HideTrees;

	public static Mobile m_BuyHorse;

	public static string m_LastCommand;

	public static Item m_LeapFrog;

	public static bool m_SayMacro;

	private static string m_LastSpeech;

	public static IFont m_DefaultFont;

	public static IHue m_DefaultHue;

	private static float m_MobileDuration;

	private static float m_ItemDuration;

	private static float m_SystemDuration;

	private static DateTime m_NextAction;

	public static Random m_Random;

	public static Stack<uint> _movementKeys;

	public static IPrompt m_Prompt;

	public const string Category = "UONET19";

	public static Texture m_Rain;

	public static Texture[] m_Snow;

	public static Texture m_FormX;

	public static Texture m_Slider;

	public static Texture[] m_Edge;

	public static Texture[] m_WinScrolls;

	public static Texture m_SkillUp;

	public static Texture m_SkillDown;

	public static Texture m_SkillLocked;

	public static ImageCache _imageCache;

	public static AuthenticationTicket _ticket;

	public static bool m_EventOk;

	public static Queue<ILoader> m_LoadQueue;

	public static Queue<Worker> m_MapLoadQueue;

	public static bool m_PumpFPS;

	private static MapBlock[] m_KeepAliveBlocks;

	private static int m_KeepAliveBlockIndex;

	public static MidiTable m_MidiTable;

	public static ContainerBoundsTable m_ContainerBoundsTable;

	private static object m_ClickSender;

	private static EventArgs m_ClickArgs;

	private static object[] m_ClickList;

	private const int QS_KEY = 1;

	private const int QS_MOUSEMOVE = 2;

	private const int QS_MOUSEBUTTON = 4;

	private const int QS_POSTMESSAGE = 8;

	private const int QS_TIMER = 16;

	private const int QS_PAINT = 32;

	private const int QS_SENDMESSAGE = 64;

	private const int QS_HOTKEY = 128;

	private const int QS_ANYTHING = 255;

	public const uint FLASHW_ALL = 3u;

	public const uint FLASHW_TIMERNOFG = 12u;

	public static bool m_Stealth;

	public static int m_StealthSteps;

	public static bool m_InResync;

	private static IPAddress[] _allowedIP;

	private static int m_PingID;

	public static Queue<int[]> m_Pings;

	private static int m_Ping;

	public static Timer m_PingTimer;

	public static bool m_MultiPreview;

	public static int m_MultiSerial;

	public static int m_MultiID;

	public static int m_MultiMinX;

	public static int m_MultiMinY;

	public static int m_MultiMaxX;

	public static int m_MultiMaxY;

	public static int m_xMultiOffset;

	public static int m_yMultiOffset;

	public static int m_zMultiOffset;

	public static List<MultiItem> m_MultiList;

	public static Timer m_ClickTimer;

	public static int m_xClick;

	public static int m_yClick;

	public static object m_Highlight;

	public static string m_ServerName;

	public static TimeDelay m_AllNames;

	private static DateTime m_HealSpam;

	public static DateTime m_NextHealPotion;

	public static int m_World;

	public static TimeDelay m_LastOverCheck;

	public static MouseEventArgs m_LastMouseArgs;

	public static bool m_MouseMoved;

	private static Point m_LastDownPoint;

	private static Timer m_PopupDelay;

	private static int m_LastDown;

	public static Mobile m_LastAttacker;

	public static PresentParameters m_PresentParams;

	public static VertexBuffer m_VertexBuffer;

	private static bool m_RecallActive;

	private static string m_LastRecallRune;

	public static readonly EventBus EventBus;

	public static double dTicks
	{
		get
		{
			if (!Engine.m_SetTicks)
			{
				Engine.UpdateTicks();
			}
			return Engine.m_dTicks;
		}
	}

	public static int Ticks
	{
		get
		{
			if (!Engine.m_SetTicks)
			{
				Engine.UpdateTicks();
			}
			return Engine.m_Ticks;
		}
	}

	public static Multis Multis
	{
		get
		{
			if (Engine.m_Multis == null)
			{
				Engine.m_Multis = new Multis();
			}
			return Engine.m_Multis;
		}
	}

	public static ItemArt ItemArt
	{
		get
		{
			if (Engine.m_ItemArt == null)
			{
				Engine.m_ItemArt = new ItemArt();
			}
			return Engine.m_ItemArt;
		}
	}

	public static LandArt LandArt
	{
		get
		{
			if (Engine.m_LandArt == null)
			{
				Engine.m_LandArt = new LandArt();
			}
			return Engine.m_LandArt;
		}
	}

	public static TextureArt TextureArt
	{
		get
		{
			if (Engine.m_TextureArt == null)
			{
				Engine.m_TextureArt = new TextureArt();
			}
			return Engine.m_TextureArt;
		}
	}

	public static ServerFeatures ServerFeatures
	{
		get
		{
			if (Engine.m_ServerFeatures == null)
			{
				Engine.m_ServerFeatures = new ServerFeatures();
			}
			return Engine.m_ServerFeatures;
		}
	}

	public static Features Features
	{
		get
		{
			if (Engine.m_Features == null)
			{
				Engine.m_Features = new Features();
			}
			return Engine.m_Features;
		}
	}

	public static Sounds Sounds
	{
		get
		{
			if (Engine.m_Sounds == null)
			{
				Debug.TimeBlock("Initializing Sounds");
				Engine.m_Sounds = new Sounds();
				Debug.EndBlock();
			}
			return Engine.m_Sounds;
		}
	}

	public static Skills Skills
	{
		get
		{
			if (Engine.m_Skills == null)
			{
				Debug.TimeBlock("Initializing Skills");
				Engine.m_Skills = new Skills();
				Debug.EndBlock();
			}
			return Engine.m_Skills;
		}
	}

	public static bool RealGMPrivs
	{
		get
		{
			Mobile player = World.Player;
			if (player == null)
			{
				return false;
			}
			if (player.Body == 987)
			{
				return true;
			}
			return player.Flags[MobileFlag.YellowHits];
		}
	}

	public static bool GMPrivs
	{
		get
		{
			if (World.Player != null)
			{
				return World.Player.Body == 987;
			}
			return false;
		}
	}

	public static FileManager FileManager => Engine.m_FileManager;

	public static float MobileDuration => Engine.m_MobileDuration;

	public static float ItemDuration => Engine.m_ItemDuration;

	public static float SystemDuration => Engine.m_SystemDuration;

	public static IFont DefaultFont => Engine.m_DefaultFont;

	public static IHue DefaultHue => Engine.m_DefaultHue;

	public static TimeSpan TripTime => TimeSpan.FromMilliseconds(Engine.Ping / 2);

	public static bool IsActionReady => DateTime.Now + Engine.TripTime > Engine.m_NextAction;

	public static Random Random
	{
		get
		{
			if (Engine.m_Random == null)
			{
				Engine.m_Random = new Random();
			}
			return Engine.m_Random;
		}
	}

	public static IPrompt Prompt
	{
		get
		{
			return Engine.m_Prompt;
		}
		set
		{
			if (Engine.m_Prompt != value)
			{
				if (Engine.m_Prompt != null && value != null)
				{
					Engine.m_Prompt.OnCancel(PromptCancelType.NewPrompt);
				}
				Engine.m_Prompt = value;
			}
		}
	}

	public static Effects Effects => Engine.m_Effects;

	internal static ImageCache ImageCache
	{
		get
		{
			if (Engine._imageCache == null)
			{
				Engine._imageCache = new ImageCache();
			}
			return Engine._imageCache;
		}
	}

	public static MidiTable MidiTable
	{
		get
		{
			if (Engine.m_MidiTable == null)
			{
				Engine.m_MidiTable = new MidiTable();
			}
			return Engine.m_MidiTable;
		}
	}

	public static ContainerBoundsTable ContainerBoundsTable
	{
		get
		{
			if (Engine.m_ContainerBoundsTable == null)
			{
				Engine.m_ContainerBoundsTable = new ContainerBoundsTable();
			}
			return Engine.m_ContainerBoundsTable;
		}
	}

	private static PacketHandlers Initializer { get; set; }

	private static ClientShardHandlerRegistry ShardHandlers { get; } = new ClientShardHandlerRegistry(new IClientBootstrapHandler[]
	{
		new ClassicClientShardHandler(),
		new NewDawnClientShardHandler()
	});

	internal static ClientBootstrapDefinition ClientDef { get; private set; }
	internal static ShardDefinition Shard { get; private set; }
	internal static IClientBootstrapHandler ShardHandler { get; private set; }

	internal static ShardDefinition ActiveShardRuntime { get; private set; }

	public static int Ping => Engine.m_Ping;

	public static bool Grid
	{
		get
		{
			return Renderer.DrawGrid;
		}
		set
		{
			Renderer.DrawGrid = value;
		}
	}

	public static bool FPS
	{
		get
		{
			return Renderer.DrawFPS;
		}
		set
		{
			Renderer.DrawFPS = value;
		}
	}

	public static bool Warmode
	{
		get
		{
			return World.Player?.Flags[MobileFlag.Warmode] ?? false;
		}
		set
		{
			Mobile player = World.Player;
			if (player != null)
			{
				Network.Send(new PSetWarMode(value, 32, 0));
				if (!value)
				{
					Engine.m_Highlight = null;
				}
			}
		}
	}

	public static bool IsRecallActive
	{
		get
		{
			return Engine.m_RecallActive;
		}
		set
		{
			Engine.m_RecallActive = value;
		}
	}

	public static string LastRecallRune
	{
		get
		{
			return Engine.m_LastRecallRune;
		}
		set
		{
			Engine.m_LastRecallRune = value;
		}
	}

	public static void ClearScreen()
	{
		Gumps.Desktop.Children.Clear();
		int gameWidth = Engine.GameWidth;
		int gameHeight = Engine.GameHeight;
		int num = gameWidth / 48;
		int num2 = gameHeight / 48;
		int num3 = num * 48 - 4;
		int num4 = num2 * 48 - 4;
		int num5 = (gameWidth - num3) / 2;
		int num6 = (gameHeight - num4) / 2;
		for (int i = 0; i < num; i++)
		{
			Gumps.Desktop.Children.Add(new GSpellPlaceholder(num5 + i * 48, -54));
			Gumps.Desktop.Children.Add(new GSpellPlaceholder(num5 + i * 48, gameHeight + 6 + 4));
		}
		for (int j = 0; j < num2; j++)
		{
			Gumps.Desktop.Children.Add(new GSpellPlaceholder(-54, num6 + j * 48));
			Gumps.Desktop.Children.Add(new GSpellPlaceholder(gameWidth + 6 + 4, num6 + j * 48));
		}
		Gumps.Desktop.Children.Add(new GSpellPlaceholder(-54, -54));
		Gumps.Desktop.Children.Add(new GSpellPlaceholder(gameWidth + 6 + 4, -54));
		Gumps.Desktop.Children.Add(new GSpellPlaceholder(-54, gameHeight + 6 + 4));
		Gumps.Desktop.Children.Add(new GSpellPlaceholder(gameWidth + 6 + 4, gameHeight + 6 + 4));
		Gumps.Desktop.Children.Add(new GDesktopBorder());
		Gumps.Desktop.Children.Add(new GBandageTimer());
		Gumps.Desktop.Children.Add(new GCriminalTimer());
		Gumps.Desktop.Children.Add(new GMapTracker());
		Gumps.Desktop.Children.Add(new GQuestArrow());
		Gumps.Desktop.Children.Add(new GPingDisplay());
		Gumps.Desktop.Children.Add(new GTransparencyGump());
		Gumps.Desktop.Children.Add(new GQueueStatus());
		Reagent[] reagents = Spells.Reagents;
		int num7 = reagents.Length;
		if (Engine.ServerFeatures == null || !Engine.ServerFeatures.AOS)
		{
			num7 = 8;
		}
		PotionType[] array = new PotionType[6]
		{
			PotionType.Yellow,
			PotionType.Orange,
			PotionType.Red,
			PotionType.Purple,
			PotionType.White,
			PotionType.Blue
		};
		ItemIDValidator[] array2 = new ItemIDValidator[num7 + 1 + array.Length];
		for (int k = 0; k < num7; k++)
		{
			array2[k] = new ItemIDValidator(reagents[k].ItemID);
		}
		for (int l = 0; l < array.Length; l++)
		{
			array2[num7 + l] = new ItemIDValidator((int)(3846 + array[l]));
		}
		array2[num7 + array.Length] = new ItemIDValidator(3617, 3817);
		Gumps.Desktop.Children.Add(new GItemCounters(array2));
	}

	public static void UseItemByType(int[] itemIDs)
	{
		Engine.FindItem(itemIDs)?.Use();
	}

	public static void UseItemByTypeAndHue(int[] itemIDs, int hue)
	{
		Item[] array = Engine.FindItems(itemIDs);
		if (array == null || array.Length == 0)
		{
			return;
		}
		Item[] array2 = array;
		foreach (Item item in array2)
		{
			if (item.Hue == hue)
			{
				item.Use();
				break;
			}
		}
	}

	public static void Remount()
	{
		Mobile player = World.Player;
		if (player == null || player.FindEquip(Layer.Mount) != null)
		{
			return;
		}
		ItemRef mount = Player.Current.EquipAgent.Mount;
		if (mount != null)
		{
			Item item = mount.FindOnPlayer();
			if (item != null)
			{
				item.Use();
				return;
			}
		}
		MountTable mountTable = Engine.m_Animations.MountTable;
		foreach (Mobile value in World.Mobiles.Values)
		{
			if (player.InRange(value.XReal, value.YReal, 1) && !value.IsDeadPet && mountTable.IsMount(value.Body))
			{
				if (value.Name == null || value.Name.Length == 0)
				{
					value.QueryStats();
				}
				else if (value.IsPet)
				{
					value.Use();
					break;
				}
			}
		}
	}

	public static void Dismount()
	{
		Mobile player = World.Player;
		if (player != null && player.FindEquip(Layer.Mount) != null)
		{
			player.Use();
		}
	}

	public static void Paste()
	{
		Debug.Trace("Paste();");
		string text = Clipboard.GetText();
		if (!string.IsNullOrEmpty(text))
		{
			Engine.Paste(text);
		}
	}

	public static void Paste(string ToPaste)
	{
		Debug.Trace("Paste( {0} );", ToPaste);
		string[] array = (Engine.m_Text + ToPaste.Replace("\r\n", "\n")).Split('\n');
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			array[i] = array[i].Trim();
			if (i < num - 1)
			{
				Debug.Trace("commandEntered( {0} );", array[i]);
				Engine.commandEntered(array[i]);
			}
			else
			{
				Debug.Trace("SetText( {0} );", array[i]);
				Engine.m_Text = array[i];
				Renderer.SetText(array[i]);
			}
		}
	}

	public static void Dequip()
	{
		Player.Current.EquipAgent.Arms.Dequip();
	}

	public static void Dequip(bool message)
	{
		Player.Current.EquipAgent.Arms.Dequip(message);
	}

	public static void Equip(int index)
	{
		Player.Current.EquipAgent.Arms.Equip(index);
	}

	public static void Dress()
	{
		Player.Current.EquipAgent.Dress.EnsureDressed();
	}

	public static void SetEquip(int index)
	{
		TargetManager.Client = new SetEquipTargetHandler(index);
	}

	public static void SetAutoUse()
	{
		TargetManager.Client = new AddAutoUseTargetHandler();
	}

	public static void AutoUse()
	{
		Player.Current.UseOnceAgent.Use();
	}

	public static double UpdateTicks()
	{
		if (Engine._sw == null)
		{
			Engine._sw = Stopwatch.StartNew();
		}
		Engine.m_Ticks = (int)Engine._sw.ElapsedMilliseconds;
		Engine.m_dTicks = Engine._sw.ElapsedMilliseconds;
		Engine.m_SetTicks = true;
		return Engine.m_dTicks;
	}

	public static void ResetTicks()
	{
		Engine.m_SetTicks = false;
	}

	public static ArrayList GetDataStore()
	{
		if (Engine.m_DataStores.Count > 0)
		{
			return Engine.m_DataStores.Dequeue();
		}
		return new ArrayList();
	}

	public static void ReleaseDataStore(ArrayList list)
	{
		if (list.Count > 0)
		{
			list.Clear();
		}
		Engine.m_DataStores.Enqueue(list);
	}

	public static void Resurrect_OnAnimationEnd(Animation a, Mobile m)
	{
		m?.Update();
	}

	public static int GetAnimDirection(byte dir)
	{
		return (((dir & 7) + 5) % 8) | (dir & 0x80);
	}

	public static byte GetWalkDirection(Direction d)
	{
		return (byte)(Direction.West & (d - 1));
	}

	public static void DoWalk(Direction d, bool fromRenderer)
	{
		if (Playback.Active)
		{
			return;
		}
		fromRenderer = false;
		if (Engine.m_InResync)
		{
			return;
		}
		Mobile player = World.Player;
		if (player == null || player.Walking.Count > 0 || (player.Body != 987 && Engine.m_WalkReq >= Engine.m_WalkAck + 4))
		{
			return;
		}
		if (Engine.m_Stealth && Engine.m_StealthSteps == 0)
		{
			if (DateTime.Now >= Engine.m_LastStealthUse + TimeSpan.FromSeconds(2.0))
			{
				Engine.Skills[SkillName.Stealth].Use();
				Engine.m_LastStealthUse = DateTime.Now;
			}
			return;
		}
		GContextMenu.Close();
		int x = player.X;
		int y = player.Y;
		int z = player.Z;
		bool ghost = player.Ghost;
		bool flag = !ghost && player.CurrentStamina <= 0 && player.MaximumStamina > 0;
		bool flag2 = !flag && player.CurrentStamina == 1 && player.MaximumStamina > 0;
		if (flag || flag2)
		{
			flag = (flag2 = !Engine.UsePotion(PotionType.Red));
		}
		if (flag)
		{
			return;
		}
		if (Engine.m_Stealth)
		{
			flag2 = true;
		}
		if (!Walking.Calculate(x, y, z, d, out var newZ, out var newDir))
		{
			if ((player.Direction & 7) != (newDir & 7))
			{
				WalkAnimation walkAnimation = WalkAnimation.PoolInstance(player, player.X, player.Y, player.Z, newDir);
				player.Walking.Enqueue(walkAnimation);
				player.IsMoving = true;
				walkAnimation.Start();
				Engine.SendMovementRequest(newDir, player.X, player.Y, player.Z, TimeSpan.FromSeconds(0.1));
				player.Direction = (byte)newDir;
			}
			else
			{
				player.MovedTiles = 0;
				player.HorseFootsteps = 0;
				player.IsMoving = false;
			}
			return;
		}
		newDir &= 7;
		newDir |= ((!flag2 && Engine.m_dMouse > Engine.GameWidth / 6) ? 128 : 0);
		if (!flag2 && Options.Current.AlwaysRun)
		{
			newDir |= 0x80;
		}
		int x2 = x;
		int y2 = y;
		if (fromRenderer || (newDir & 7) == (player.Direction & 7))
		{
			Walking.Offset(newDir, ref x2, ref y2);
		}
		else
		{
			newZ = player.Z;
		}
		if (Engine.m_LeapFrog != null && !Engine.m_LeapFrog.InRange(new Point(x2, y2), 2) && Engine.m_LeapFrog.InRange(x, y, 2))
		{
			if (Engine.m_LastLeapfrogPickup + TimeSpan.FromSeconds(0.1) < DateTime.Now)
			{
				Engine.m_LastLeapfrogPickup = DateTime.Now;
				Walking.Offset(newDir, ref x2, ref y2);
				Network.Send(new PPickupItem(Engine.m_LeapFrog, Engine.m_LeapFrog.Amount));
				Network.Send(new PDropItem(Engine.m_LeapFrog.Serial, (short)x2, (short)y2, (sbyte)newZ, -1));
			}
			return;
		}
		WalkAnimation walkAnimation2 = WalkAnimation.PoolInstance(player, x2, y2, newZ, newDir);
		player.Walking.Enqueue(walkAnimation2);
		bool isMoving = player.IsMoving;
		player.IsMoving = true;
		walkAnimation2.Start();
		player.SetReal(x2, y2, newZ, newDir);
		if (!isMoving && walkAnimation2.Advance)
		{
			World.Offset(walkAnimation2.xOffset, walkAnimation2.yOffset);
			Engine.Effects.Offset(walkAnimation2.xOffset, walkAnimation2.yOffset);
		}
		Engine.Redraw();
		if ((newDir & 7) != (player.Direction & 7))
		{
			Engine.SendMovementRequest(newDir, player.X, player.Y, player.Z, TimeSpan.FromSeconds(0.1));
			if (!fromRenderer)
			{
				player.Direction = (byte)newDir;
				return;
			}
		}
		player.ChairData = Chairs.ChairData.Null;
		if (!ghost && player.Body != 987)
		{
			MapPackage cache = Map.GetCache();
			List<ICell> list = cache.cells[x2 - cache.CellX, y2 - cache.CellY];
			for (int i = 0; i < list.Count; i++)
			{
				ICell cell = list[i];
				if (cell is DynamicItem)
				{
					Item item = ((DynamicItem)cell).m_Item;
					player.ChairData = Chairs.ChairData.Null;
					if (item.IsDoor)
					{
						Network.Send(new POpenDoor());
						break;
					}
				}
			}
		}
		if (Engine.m_Stealth)
		{
			Engine.m_StealthSteps--;
		}
		Engine.SendMovementRequest(newDir, x2, y2, newZ, TimeSpan.FromSeconds(player.IsMounted ? 0.1 : 0.2));
		player.Direction = (byte)newDir;
	}

	public static void AddTimer(Timer t)
	{
		Engine.m_Timers.Add(t);
	}

	public static void RemoveTimer(Timer t)
	{
		Engine.m_Timers.Remove(t);
	}

	public static bool IsMoving()
	{
		Mobile player = World.Player;
		return player != null && player.Walking.Count > 0;
	}

	public static byte ByteCap(int Value)
	{
		if (Value < 0)
		{
			return 0;
		}
		if (Value > 255)
		{
			return byte.MaxValue;
		}
		return (byte)Value;
	}

	public static int Blend32(int a32, int b32, int n)
	{
		int num = (a32 >> 16) & 0xFF;
		int num2 = (a32 >> 8) & 0xFF;
		int num3 = a32 & 0xFF;
		int num4 = (b32 >> 16) & 0xFF;
		int num5 = (b32 >> 8) & 0xFF;
		int num6 = b32 & 0xFF;
		int num7 = (num * (255 - n) + num4 * n + 127) / 255;
		int num8 = (num2 * (255 - n) + num5 * n + 127) / 255;
		int num9 = (num3 * (255 - n) + num6 * n + 127) / 255;
		return (num7 << 16) | (num8 << 8) | num9;
	}

	public static ushort C32216(int c32)
	{
		int num = (c32 >> 16) & 0xFF;
		int num2 = (c32 >> 8) & 0xFF;
		int num3 = c32 & 0xFF;
		num *= 31;
		num2 *= 31;
		num3 *= 31;
		num += 127;
		num2 += 127;
		num3 += 127;
		num /= 255;
		num2 /= 255;
		num3 /= 255;
		return (ushort)(0x8000 | (num << 10) | (num2 << 5) | num3);
	}

	public static int C16232(int C16)
	{
		float num = (C16 >> 10) & 0x1F;
		float num2 = (C16 >> 5) & 0x1F;
		float num3 = C16 & 0x1F;
		num *= 8.225806f;
		num2 *= 8.225806f;
		num3 *= 8.225806f;
		int num4 = Engine.ByteCap((int)num);
		int num5 = Engine.ByteCap((int)num2);
		int num6 = Engine.ByteCap((int)num3);
		return (num4 << 16) | (num5 << 8) | num6;
	}

	public static int GrayScale(int Color)
	{
		float num = (Color >> 10) & 0x1F;
		float num2 = (Color >> 5) & 0x1F;
		float num3 = Color & 0x1F;
		num *= 8.225806f;
		num2 *= 8.225806f;
		num3 *= 8.225806f;
		float num4 = num * 0.299f + num2 * 0.587f + num3 * 0.114f;
		num4 *= 0.12156863f;
		int num5 = (int)num4;
		if (num5 < 0)
		{
			num5 = 0;
		}
		else if (num5 > 31)
		{
			num5 = 31;
		}
		return num5;
	}

	public static float GrayScale(int r, int g, int b)
	{
		return (float)r * 0.299f + (float)g * 0.587f + (float)b * 0.114f;
	}

	public static string Encode(string Input)
	{
		return Engine.m_Encoder.Replace(Input.ToString(), CharEntity_Match);
	}

	private static string CharEntity_Match(Match m)
	{
		try
		{
			int num = Convert.ToInt32(m.Groups[1].Value, 16);
			if (num == 10 || num == 13)
			{
				return m.Groups[0].Value;
			}
			return ((char)num).ToString();
		}
		catch
		{
			return m.Groups[0].Value;
		}
	}

	public static string FormatByteLength(int bytes)
	{
		if (bytes < 1000000)
		{
			return string.Format("{0:N2} KB", (double)bytes / 1024.0, bytes);
		}
		if (bytes < 1000000000)
		{
			return string.Format("{0:N2} MB", (double)bytes / 1024.0 / 1024.0, bytes);
		}
		return string.Format("{0:N2} GB", (double)bytes / 1024.0 / 1024.0 / 1024.0, bytes);
	}

	public static void TimeRefresh_OnTick(Timer t)
	{
		int num = (int)t.GetTag("Frames");
		double num2 = num;
		Cursor.Hourglass = true;
		Engine.m_SetTicks = false;
		double num3 = Engine.dTicks;
		Renderer._timeRefresh = true;
		while (--num >= 0)
		{
			Renderer.Draw();
		}
		if (Renderer._profile != null)
		{
			Engine.AddTextMessage(Renderer._profile.ToString());
		}
		Renderer._timeRefresh = false;
		Engine.m_SetTicks = false;
		double num4 = Engine.dTicks;
		Cursor.Hourglass = false;
		Engine.AddTextMessage($"Time Refresh: {num2} frames in {(num4 - num3) * 0.001:F2} seconds: {num2 / ((num4 - num3) * 0.001):F2} FPS");
	}

	public static void RestoreSpeech()
	{
		if (Engine.m_LastSpeech != null)
		{
			Renderer.SetText(Engine.m_LastSpeech);
			Engine.m_Text = Engine.m_LastSpeech;
		}
	}

	public static void commandEntered(string cmd)
	{
		if (Playback.Active)
		{
			return;
		}
		if (cmd.Length > 0 && !Engine.m_SayMacro)
		{
			Engine.m_LastCommand = cmd;
		}
		if (Engine.m_Prompt != null)
		{
			Engine.m_Prompt.OnReturn(cmd);
			Engine.m_Prompt = null;
			return;
		}
		cmd = cmd.Trim();
		if (cmd.Length > 0)
		{
			if (!Engine.m_SayMacro)
			{
				Engine.m_LastSpeech = cmd;
			}
			SpeechFormat speechFormat = SpeechFormat.Find(cmd);
			speechFormat.OnSpeech(cmd);
		}
	}

	public static void AddTextMessage(string Message)
	{
		Engine.AddTextMessage(Message, Engine.m_SystemDuration, Engine.m_DefaultFont, Engine.m_DefaultHue);
	}

	public static string MakeProperCase(string text)
	{
		StringBuilder stringBuilder = new StringBuilder(text);
		for (int i = 0; i < stringBuilder.Length; i++)
		{
			if (i == 0 || stringBuilder[i - 1] == ' ')
			{
				stringBuilder[i] = char.ToUpper(stringBuilder[i]);
			}
		}
		return stringBuilder.ToString();
	}

	public static void AddTextMessage(string Message, IFont Font)
	{
		Engine.AddTextMessage(Message, Engine.m_SystemDuration, Font, Engine.m_DefaultHue);
	}

	public static void AddTextMessage(string Message, IFont Font, IHue Hue)
	{
		Engine.AddTextMessage(Message, Engine.m_SystemDuration, Font, Hue);
	}

	public static void AddTextMessage(string Message, float Delay)
	{
		Engine.AddTextMessage(Message, Delay, Engine.m_DefaultFont, Engine.m_DefaultHue);
	}

	public static void AddTextMessage(string Message, float Delay, IFont Font)
	{
		Engine.AddTextMessage(Message, Delay, Font, Engine.m_DefaultHue);
	}

	public static void AddTextMessage(string Message, float Delay, IFont Font, IHue Hue)
	{
		if (!Engine.m_Ingame)
		{
			return;
		}
		Message = Message.TrimEnd();
		if (Message.Length > 0)
		{
			Engine.AddToJournal(new JournalEntry(Message, Hue, -1));
			Message = Engine.WrapText(Message, Engine.GameWidth / 2, Font).TrimEnd();
			if (Message.Length > 0)
			{
				MessageManager.AddMessage(new GSystemMessage(Message, Font, Hue, Delay));
			}
		}
	}

	public static string WrapText(string text, int width, IFont f)
	{
		WrapKey key = new WrapKey(text, width);
		if (f.WrapCache.TryGetValue(key, out var value))
		{
			return value;
		}
		if (f.GetStringWidth(text) <= width)
		{
			f.WrapCache.Add(key, text);
			return text;
		}
		string[] array = text.Split(' ');
		StringBuilder stringBuilder = new StringBuilder();
		ArrayList dataStore = Engine.GetDataStore();
		for (int i = 0; i < array.Length; i++)
		{
			if (f.GetStringWidth(stringBuilder.ToString() + array[i]) > width)
			{
				if (f.GetStringWidth(array[i]) > width)
				{
					stringBuilder.Append(array[i]);
					while (stringBuilder.Length > 1 && f.GetStringWidth(stringBuilder.ToString()) > width)
					{
						StringBuilder stringBuilder2 = new StringBuilder();
						stringBuilder2.Append(stringBuilder[0]);
						for (int j = 1; j < stringBuilder.Length; j++)
						{
							if (f.GetStringWidth(stringBuilder2.ToString() + stringBuilder[j]) > width)
							{
								dataStore.Add(stringBuilder2);
								stringBuilder = new StringBuilder(stringBuilder.ToString().Substring(stringBuilder2.Length));
								break;
							}
							stringBuilder2.Append(stringBuilder[j]);
						}
					}
					if (i < array.Length - 1)
					{
						stringBuilder.Append(' ');
					}
				}
				else
				{
					if (stringBuilder.Length > 0)
					{
						dataStore.Add(stringBuilder);
					}
					stringBuilder = new StringBuilder(array[i]);
					if (i < array.Length - 1)
					{
						stringBuilder.Append(' ');
					}
				}
			}
			else
			{
				stringBuilder.Append(array[i]);
				if (i < array.Length - 1)
				{
					stringBuilder.Append(' ');
				}
			}
		}
		if (stringBuilder.Length > 0)
		{
			while (stringBuilder.Length > 1 && f.GetStringWidth(stringBuilder.ToString()) > width)
			{
				StringBuilder stringBuilder3 = new StringBuilder();
				stringBuilder3.Append(stringBuilder[0]);
				for (int k = 1; k < stringBuilder.Length; k++)
				{
					if (f.GetStringWidth(stringBuilder3.ToString() + stringBuilder[k]) > width)
					{
						dataStore.Add(stringBuilder3);
						stringBuilder = new StringBuilder(stringBuilder.ToString().Substring(stringBuilder3.Length));
						break;
					}
					stringBuilder3.Append(stringBuilder[k]);
				}
			}
			if (stringBuilder.Length > 0)
			{
				dataStore.Add(stringBuilder);
			}
		}
		StringBuilder stringBuilder4 = new StringBuilder();
		int count = dataStore.Count;
		for (int l = 0; l < count; l++)
		{
			stringBuilder4.Append(((StringBuilder)dataStore[l]).ToString());
			if (l < count - 1)
			{
				stringBuilder4.Append('\n');
			}
		}
		string text2 = stringBuilder4.ToString();
		f.WrapCache.Add(key, text2);
		Engine.ReleaseDataStore(dataStore);
		return text2;
	}

	public static void DrawNow()
	{
		Engine.DoEvents();
		Renderer.Draw();
		Engine.DoEvents();
	}

	public static void WantDirectory(string Target)
	{
		string path = Engine.FileManager.BasePath(Target);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	public static void MakeDirectory(string Target)
	{
		Directory.CreateDirectory(Engine.FileManager.BasePath(Target));
	}

	public static void Unlock()
	{
		Engine.m_Locked = false;
	}

	public static void Exception_Unhandled(object Sender, UnhandledExceptionEventArgs e)
	{
		Debug.Trace("Unhandled exception");
		Debug.Trace("Object -> {0}", Sender);
		object exceptionObject = e.ExceptionObject;
		if (exceptionObject is Exception)
		{
			Debug.Error((Exception)exceptionObject);
		}
		else
		{
			Debug.Trace("Exception -> {0}", exceptionObject);
		}
	}

	public static void DelayedAction()
	{
		DateTime dateTime = DateTime.Now + TimeSpan.FromSeconds(0.2) - Engine.TripTime;
		if (dateTime < Engine.m_NextAction)
		{
			Engine.m_NextAction = dateTime;
		}
	}

	public static void PushAction()
	{
		Engine.m_NextAction = DateTime.Now + TimeSpan.FromSeconds(0.6) + Engine.TripTime;
	}

	public static void Options_OnClick(Gump g)
	{
		Engine.OpenOptions();
	}

	public static void OpenOptions()
	{
		GObjectEditor.Open(Preferences.Current);
	}

	public static void OpenOptionsMacros()
	{
	}

	public static int RandomRange(int start, int count)
	{
		return start + Engine.Random.Next(count);
	}

	public static int GetRandomHue()
	{
		return Engine.RandomRange(2, 1000);
	}

	public static int GetRandomNeutralHue()
	{
		return Engine.RandomRange(1801, 108);
	}

	public static int GetRandomMetalHue()
	{
		return Engine.RandomRange(2401, 30);
	}

	public static IHue GetRandomBlueHue()
	{
		return Hues.Load(Engine.RandomRange(1301, 54));
	}

	public static IHue GetRandomRedHue()
	{
		return Hues.Load(Engine.RandomRange(1601, 54));
	}

	public static int GetRandomHairHue()
	{
		return Engine.RandomRange(1102, 48);
	}

	public static int GetRandomSkinHue()
	{
		return Engine.RandomRange(1002, 56);
	}

	public static int GetRandomYellowHue()
	{
		return Engine.RandomRange(1701, 54);
	}

	public static int Smallest(int x, int y)
	{
		if (x < y)
		{
			return x;
		}
		return y;
	}

	public static int Biggest(int x, int y)
	{
		if (x > y)
		{
			return x;
		}
		return y;
	}

	public static void ListView_OnValueChange(double Value, double Old, Gump Sender)
	{
		if (Sender.HasTag("ListBox"))
		{
			GListBox gListBox = (GListBox)Sender.GetTag("ListBox");
			if (gListBox != null)
			{
				gListBox.StartIndex = (int)Value;
			}
		}
	}

	public static void ScrollUp_OnClick(Gump Sender)
	{
		if (Sender.HasTag("Scroller"))
		{
			GVSlider gVSlider = (GVSlider)Sender.GetTag("Scroller");
			gVSlider?.SetValue(gVSlider.GetValue() - gVSlider.Increase, CallOnChange: true);
		}
	}

	public static void ScrollDown_OnClick(Gump Sender)
	{
		if (Sender.HasTag("Scroller"))
		{
			GVSlider gVSlider = (GVSlider)Sender.GetTag("Scroller");
			gVSlider?.SetValue(gVSlider.GetValue() + gVSlider.Increase, CallOnChange: true);
		}
	}

	public static void URLButton_OnClick(Gump Sender)
	{
		if (Sender.HasTag("URL"))
		{
			Engine.OpenBrowser((string)Sender.GetTag("URL"));
		}
	}

	public static void OpenBrowser(string url)
	{
		if (Uri.TryCreate(url, UriKind.Absolute, out var result) && !result.IsFile && !result.IsLoopback)
		{
			string absolutePath = result.AbsolutePath;
			if (string.Equals(Path.GetExtension(absolutePath), ".rpv", StringComparison.OrdinalIgnoreCase))
			{
				Playback.Download(result);
			}
			else
			{
				Process.Start(result.ToString());
			}
		}
	}

	public static Version GetVersion()
	{
		return new Version(7, 0, 15, 1);
	}

	public static string GetVersionString()
	{
		return Engine.GetVersion().ToString();
	}

	public static void Setup()
	{
		Cursor.Gold = false;
		Engine.m_LastAttacker = null;
		Renderer.AlwaysHighlight = 0;
		Engine._movementKeys = new Stack<uint>();
		for (int i = 0; i < 5; i++)
		{
			Engine._movementKeys.Push(3131961357u);
		}
		Engine.m_Journal.Clear();
		Renderer.DrawFPS = false;
		Engine.m_Ingame = false;
		Engine.m_WalkAck = 0;
		Engine.m_WalkReq = 0;
		Macros.StopAll();
		World.Clear();
		Cursor.Hourglass = true;
		if (TargetManager.IsActive)
		{
			TargetManager.Active.Clear();
			if (TargetManager.IsActive)
			{
				TargetManager.Active.Clear();
			}
		}
		Gumps.Desktop.Children.Clear();
	}

	public static void ShowAcctLogin()
	{
		Engine.exiting = true;
	}

	private static void PlayRandomMidi()
	{
		if (!Preferences.Current.Music.Volume.IsMuted)
		{
			int[] array = new int[5] { 1, 2, 3, 5, 7 };
			string text = Engine.MidiTable.Translate(array[Engine.Random.Next(array.Length)]);
			if (text != null)
			{
				Music.Play(text);
			}
		}
	}

	public static void TickTimers()
	{
		int count = Engine.m_Timers.Count;
		int num = 0;
		while (num < count)
		{
			Timer timer = Engine.m_Timers[num];
			if (!timer.Tick())
			{
				Engine.m_Timers.RemoveAt(num);
				count = Engine.m_Timers.Count;
			}
			else
			{
				num++;
			}
		}
	}

	public static void DoEvents()
	{
		Application.DoEvents();
	}

	public static Texture LoadImageAsAlpha(string path)
	{
		ArchivedFile archivedFile = Engine.FileManager.GetArchivedFile($"play/images/{path}");
		if (archivedFile != null)
		{
			using Stream stream = archivedFile.Download();
			using Bitmap bitmap = new Bitmap(stream);
			bitmap.MakeTransparent(Color.Black);
			for (int i = 0; i < bitmap.Height; i++)
			{
				for (int j = 0; j < bitmap.Width; j++)
				{
					bitmap.SetPixel(j, i, Color.FromArgb(bitmap.GetPixel(j, i).B, 255, 255, 255));
				}
			}
			try
			{
				return Texture.FromBitmap(bitmap);
			}
			catch
			{
			}
		}
		Debug.Trace("LoadImageAsAlpha( \"{0}\" ) failed", path);
		return Texture.Empty;
	}

	internal static Bitmap LoadArchivedBitmap(string path)
	{
		ArchivedFile archivedFile = Engine.FileManager.GetArchivedFile("play/images/" + path);
		if (archivedFile != null)
		{
			using (Stream stream = archivedFile.Download())
			{
				return new Bitmap(stream);
			}
		}
		return null;
	}

	internal static Bitmap LoadArchivedBitmap(Veritas.Archive archive, string path)
	{
		if (archive != null)
		{
			ArchivedFile archivedFile = archive.FindFile(path);
			if (archivedFile != null)
			{
				using (Stream stream = archivedFile.Download())
				{
					return new Bitmap(stream);
				}
			}
		}
		return null;
	}

	internal static Texture LoadArchivedTexture(string path)
	{
		try
		{
			using Bitmap bitmap = Engine.LoadArchivedBitmap(path);
			if (bitmap != null)
			{
				return Texture.FromBitmap(bitmap);
			}
		}
		catch
		{
		}
		Debug.Trace("LoadArchivedTexture( \"{0}\" ) failed", path);
		return Texture.Empty;
	}

	internal static Texture LoadArchivedTexture(Veritas.Archive archive, string path)
	{
		try
		{
			using Bitmap bitmap = Engine.LoadArchivedBitmap(archive, path);
			if (bitmap != null)
			{
				return Texture.FromBitmap(bitmap);
			}
		}
		catch
		{
		}
		Debug.Trace("LoadArchivedTexture( \"{0}\" ) failed", path);
		return Texture.Empty;
	}

	public static void LoadParticles()
	{
		Engine.m_Rain = Engine.LoadArchivedTexture("rain.png");
		Engine.m_FormX = Engine.LoadImageAsAlpha("Form_X.bmp");
		Engine.m_Slider = Engine.LoadImageAsAlpha("Slider.bmp");
		Engine.m_SkillUp = Engine.LoadImageAsAlpha("Skill_Up.bmp");
		Engine.m_SkillDown = Engine.LoadImageAsAlpha("Skill_Down.bmp");
		Engine.m_SkillLocked = Engine.LoadImageAsAlpha("Skill_Locked.bmp");
		Engine.m_Snow = new Texture[12];
		for (int i = 0; i < 12; i++)
		{
			Engine.m_Snow[i] = Engine.LoadImageAsAlpha($"Snow_{i + 1}.bmp");
		}
		Engine.m_Edge = new Texture[8];
		for (int j = 0; j < 8; j++)
		{
			Engine.m_Edge[j] = Engine.LoadImageAsAlpha($"Edge_{j + 1}.bmp");
		}
		Engine.m_WinScrolls = new Texture[4];
		Engine.m_WinScrolls[0] = Engine.LoadImageAsAlpha("WinScroll_Up.bmp");
		Engine.m_WinScrolls[1] = Engine.LoadImageAsAlpha("WinScroll_Down.bmp");
		Engine.m_WinScrolls[2] = Engine.LoadImageAsAlpha("WinScroll_Left.bmp");
		Engine.m_WinScrolls[3] = Engine.LoadImageAsAlpha("WinScroll_Right.bmp");
	}

	public static DateTime GetTimeStamp(string path)
	{
		return new FileInfo(path).LastWriteTime;
	}

	public static void QueueMapLoad(int xBlock, int yBlock, TileMatrix matrix)
	{
		if (xBlock >= 0 && yBlock >= 0 && xBlock < matrix.BlockWidth && yBlock < matrix.BlockHeight)
		{
			int num = xBlock * 512 + yBlock;
			bool flag = false;
			Mobile player = World.Player;
			if (player != null)
			{
				flag = player.Ghost;
			}
			if (!matrix.CheckLoaded(xBlock, yBlock))
			{
				Engine.m_MapLoadQueue.Enqueue(new Worker(xBlock, yBlock, matrix));
			}
		}
	}

	public static void Preload(Worker w)
	{
		TileMatrix matrix = w.Matrix;
		int blockWidth = matrix.BlockWidth;
		int blockHeight = matrix.BlockHeight;
		if (w.X < 0 || w.Y < 0 || w.X >= blockWidth || w.Y >= blockHeight)
		{
			return;
		}
		Mobile player = World.Player;
		Hues.DefaultHue defaultHue = (Hues.DefaultHue)Hues.Default;
		MapBlock block = matrix.GetBlock(w.X, w.Y);
		bool flag = false;
		int num = 0;
		while (!flag && num < Engine.m_KeepAliveBlocks.Length)
		{
			flag = Engine.m_KeepAliveBlocks[num] == block;
			num++;
		}
		if (!flag)
		{
			Engine.m_KeepAliveBlocks[Engine.m_KeepAliveBlockIndex % Engine.m_KeepAliveBlocks.Length] = block;
			Engine.m_KeepAliveBlockIndex++;
		}
		Tile[] array = ((block == null) ? matrix.InvalidLandBlock : block.m_LandTiles);
		for (int i = 0; i < array.Length; i++)
		{
			if (!defaultHue.HintLand((int)array[i].landId))
			{
				Engine.m_LoadQueue.Enqueue(new LandLoader((int)array[i].landId));
			}
		}
		HuedTile[][][] array2 = ((block == null) ? matrix.EmptyStaticBlock : block.m_StaticTiles);
		for (int j = 0; j < 8; j++)
		{
			for (int k = 0; k < 8; k++)
			{
				HuedTile[] array3 = array2[j][k];
				for (int l = 0; l < array3.Length; l++)
				{
					if (array3[l].hueId == 0 && !defaultHue.HintItem((int)array3[l].itemId))
					{
						Engine.m_LoadQueue.Enqueue(new ItemLoader((int)array3[l].itemId));
					}
				}
			}
		}
	}

	public static void ClickTimer_OnTick(Timer t)
	{
		if (Engine.m_ClickList != null)
		{
			Gump gump = (Gump)Engine.m_ClickList[0];
			Point point = (Point)Engine.m_ClickList[1];
			gump.OnSingleClick(point.X, point.Y);
		}
		else
		{
			Engine.Click(Engine.m_ClickSender, Engine.m_ClickArgs);
		}
		Engine.m_ClickTimer.Stop();
	}

	public static void Ignore()
	{
		TargetManager.Client = new IgnoreTargetHandler();
		Engine.AddTextMessage("Who do you wish to ignore?");
	}

	[DllImport("User32")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

	[DllImport("User32")]
	private static extern int GetQueueStatus(int flags);

	public static bool FlashWindowEx(Form form)
	{
		IntPtr handle = form.Handle;
		FLASHWINFO pwfi = default(FLASHWINFO);
		pwfi.cbSize = Convert.ToUInt32(Marshal.SizeOf(pwfi));
		pwfi.hwnd = handle;
		pwfi.dwFlags = 15u;
		pwfi.uCount = uint.MaxValue;
		pwfi.dwTimeout = 0u;
		return Engine.FlashWindowEx(ref pwfi);
	}

	public static bool Resync()
	{
		if (!Engine.m_InResync)
		{
			Engine.m_InResync = true;
			Engine.AddTextMessage("Please wait, resynchronizing.");
			return Network.Send(new PResyncRequest());
		}
		return false;
	}

	public static bool HandleException(Exception ex)
	{
		try
		{
			GenericExceptionDialog genericExceptionDialog = new GenericExceptionDialog();
			string fmt = ((ex is NetworkException) ? "VertiasUO has encountered a network error and must close." : ((!(ex is StorageException)) ? "VertiasUO has encountered an error and must close." : "VertiasUO has encountered a storage error and must close."));
			genericExceptionDialog.SetExceptionInfo(ex, fmt);
			if (Engine.m_Display != null && !Engine.m_Display.IsDisposed)
			{
				genericExceptionDialog.ShowDialog(Engine.m_Display);
			}
			else
			{
				genericExceptionDialog.ShowDialog();
			}
		}
		catch
		{
		}
		try
		{
			Debug.Error(ex);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static bool Verify()
	{
		MessageBox.Show("QA Mode");
		return true;
	}

	private static bool Verify(string[] args)
	{
		if (args == null || args.Length != 5)
		{
			throw new ArgumentException("Invalid Arguments");
		}
		return true;
	}

	static Engine()
	{
		Engine.m_GMFollow = false;
		Engine.m_DataStores = new Queue<ArrayList>();
		Engine.GameWidth = 640;
		Engine.GameHeight = 480;
		Engine.GameX = 20;
		Engine.GameY = 20;
		Engine.m_regMap = true;
		Engine.m_Text = "";
		Engine.m_Locked = true;
		Engine.m_Encoder = new Regex("&#(?<1>[0-9a-fA-F]+);", RegexOptions.None);
		Engine.m_HideTrees = true;
		Engine.m_MobileDuration = 7.5f;
		Engine.m_ItemDuration = 5f;
		Engine.m_SystemDuration = 7.5f;
		Engine._movementKeys = new Stack<uint>();
		Engine.m_KeepAliveBlocks = new MapBlock[128];
		Engine._allowedIP = new IPAddress[4]
		{
			IPAddress.Parse("191.237.132.72"),
			IPAddress.Parse("69.175.84.106"),
			IPAddress.Parse("54.187.52.191"),
			IPAddress.Parse("34.234.13.101")
		};
		Engine.m_LastDown = -1;
		Engine.Initializer = new PacketHandlers();
		Engine.EventBus = new EventBus();
	}

	public static void Run(ClientBootstrapDefinition bootstrap)
	{
		Console.WriteLine("Initializing engine from bootstrap...");
		Engine.ApplyClientBootstrap(bootstrap);
		Engine.Initialize();
	}

	private static void Initialize()
	{
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		AppDomain.CurrentDomain.DomainUnload += delegate
		{
			Music.Destroy();
		};
		try
		{
			Engine.m_FileManager = new FileManager();
			if (Engine.m_FileManager.Error)
			{
				Engine.m_FileManager = null;
				GC.Collect();
				throw new InvalidOperationException("Unable to initialize file manager.");
			}
			Engine.MainA();
		}
		catch (Exception ex)
		{
			Engine.HandleException(ex);
			throw;
		}
	}

	public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		if (e.IsTerminating && e.ExceptionObject is Exception)
		{
			Engine.HandleException((Exception)e.ExceptionObject);
		}
	}

	public static void UseMoongate()
	{
		Item item = World.FindItem(new PlayerDistanceValidator(new ItemIDValidator(3546, 3948, 8148, 19403), 1));
		if (item == null)
		{
			Engine.AddTextMessage("Moongate not found.", Engine.DefaultFont, Hues.Load(38));
		}
		else if (new UseContext(item, isManual: true).Enqueue())
		{
			item.AddTextMessage("Moongate", "- using -", Engine.DefaultFont, Hues.Load(53), unremovable: true);
		}
	}

	public static void Disturb()
	{
		if (TargetManager.Server != null)
		{
			TargetManager.Server.Cancel();
		}
		else
		{
			World.Player.Disturb();
		}
	}

	[STAThread]
	public static void MainA()
	{
		if (Engine._ticket == null)
		{
			return;
		}

		Engine.WantDirectory("data/");
		Engine.WantDirectory("data/ultima/");
		Engine.WantDirectory("data/ultima/logs/");
		Debug.Block("Environment");
		Debug.Trace("Operating System = '{0}'", Environment.OSVersion);
		Debug.Trace(".NET Framework   = '{0}'", Environment.Version);
		Debug.Trace("Base Directory   = '{0}'", Engine.m_FileManager.BasePath(""));
		Debug.Trace("Data Directory   = '{0}'", Engine.m_FileManager.ResolveMUL(""));
		Debug.EndBlock();
		Engine.m_Timers = new List<Timer>();
		Engine.m_Journal = new List<JournalEntry>();
		Engine.m_Pings = new Queue<int[]>();
		Engine.m_LoadQueue = new Queue<ILoader>();
		Engine.m_MapLoadQueue = new Queue<Worker>();
		MacroHandlers.Setup();
		Debug.Block("Main()");
		Engine.m_ClickTimer = new Timer(ClickTimer_OnTick, SystemInformation.DoubleClickTime);
		Debug.Try("Initializing Display");
		Engine.m_Display = new Display();
		Preferences.Current.Layout.Apply(applyGumps: false);
		Engine.m_Display.KeyPreview = true;
		Engine.m_Display.Show();
		Preferences.Current.Layout.Apply(applyGumps: false);
		Preferences.Current.Layout.Update();
		Debug.EndTry();
		Application.DoEvents();
		Debug.Block("Initializing DirectX");
		Engine.InitDX();
		Debug.EndBlock();
		Engine.m_Loading = true;
		Engine.m_Ingame = false;
		Cursor.Hourglass = true;
		Engine.DrawNow();
		Debug.TimeBlock("Initializing Animations");
		Engine.m_Animations = new Animations();
		Debug.EndBlock();
		Engine.m_Font = new Font[10];
		Engine.m_UniFont = new UnicodeFont[3];
		Debug.TimeBlock("Initializing Gumps");
		Engine.m_Gumps = new Gumps();
		Debug.EndBlock();
		Engine.m_DefaultFont = Engine.GetUniFont(3);
		Engine.m_DefaultHue = Hues.Load(946);
		Renderer.SetText("");
		Macros.Reset();
		Engine.LoadParticles();
		Renderer.FilterEnable = false;
		Renderer.SetTexture(Engine.m_Rain);
		try
		{
			Engine.m_Device.ValidateDevice(1);
		}
		catch (Exception ex)
		{
			Engine.m_Rain.Dispose();
			Engine.m_Rain = Texture.Empty;
			Engine.m_SkillUp.Dispose();
			Engine.m_SkillUp = Hues.Default.GetGump(2435);
			Engine.m_SkillDown.Dispose();
			Engine.m_SkillDown = Hues.Default.GetGump(2437);
			Engine.m_SkillLocked.Dispose();
			Engine.m_SkillLocked = Hues.Default.GetGump(2092);
			Engine.m_Slider.Dispose();
			Engine.m_Slider = Hues.Default.GetGump(2117);
			for (int i = 0; i < Engine.m_Snow.Length; i++)
			{
				Engine.m_Snow[i].Dispose();
				Engine.m_Snow[i] = Texture.Empty;
			}
			for (int j = 0; j < Engine.m_Edge.Length; j++)
			{
				Engine.m_Edge[j].Dispose();
				Engine.m_Edge[j] = Texture.Empty;
			}
			Debug.Trace("ValidateDevice() failed on 32-bit textures");
			Debug.Error(ex);
		}
		Renderer.SetTexture(null);
		Engine.m_Effects = new Effects();
		Engine.m_Loading = false;
		Point point = Engine.m_Display.PointToClient(System.Windows.Forms.Cursor.Position);
		Engine.m_EventOk = true;
		Engine.MouseMove(Engine.m_Display, new MouseEventArgs(Control.MouseButtons, 0, point.X, point.Y, 0));
		Compression.CheckCache();
		Engine.Setup();
		Engine.MouseMoveQueue();
		Engine.m_EventOk = false;
		Preferences.Current.Layout.Update();
		Engine.DrawNow();
		Engine.m_MoveDelay = new TimeDelay(0f);
		Engine.m_LastOverCheck = new TimeDelay(0.1f);
		Engine.m_NewFrame = new TimeDelay(0.05f);
		Engine.m_SleepMode = new TimeDelay(7.5f);
		Engine.m_EventOk = true;
		bool flag = false;
		Animations.StartLoading();
		Engine.Unlock();
		DateTime dateTime = DateTime.Now;
		_ = Engine.Ticks;
		bool flag2 = true;
		if (Animations.IsLoading)
		{
			do
			{
				Engine.DrawNow();
			}
			while (!Animations.WaitLoading());
		}
		Hash.Start(out var crc);
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int k = 0; k < assemblies.Length; k++)
		{
			Hash.Append(assemblies[k].FullName.ToString().GetHashCode(), ref crc);
		}
		Hash.Finish(ref crc);

		Debug.Trace("Connecting to {0}:{1}", Shard.ServerIP, Shard.ServerPort);
		if (!Network.Connect(new LoginCrypto(0u), new IPEndPoint(Shard.ServerIP, Shard.ServerPort)))
		{
			throw new InvalidOperationException("Failed to connect.");
		}

		m_ServerName = Shard.Host;
		ShardHandler.SendFirstLogin();
		PingRequest(sendPing: false);
		Compression.CheckCache();
		Stopwatch stopwatch = new Stopwatch();

		while (!Engine.exiting)
		{
			stopwatch.Start();
			_ = World.Player;
			if (Engine.m_Ingame)
			{
				if (Engine.m_Journal.Count >= 1 && (Engine.m_Journal[Engine.m_Journal.Count - 1].Text.Contains("[Guild][") || Engine.m_Journal[Engine.m_Journal.Count - 1].Text.Contains("[Alliance][")))
				{
					DateTime now = DateTime.Now;
					now = Engine.m_Journal[Engine.m_Journal.Count - 1].Time;
					if (DateTime.Now == now)
					{
						Engine.FlashWindowEx(Engine.m_Display);
					}
				}
				if (World.Player.Notoriety == Notoriety.Criminal && !GCriminalTimer.Active)
				{
					GCriminalTimer.Start();
				}
				else if (World.Player.Notoriety != Notoriety.Criminal && GCriminalTimer.Active)
				{
					GCriminalTimer.Stop();
				}
				if (Options.Current.Scavenger)
				{
					Preferences.Current.Scavenger.Scavenge(isManual: false);
				}
			}
			Engine.m_SetTicks = false;
			_ = Engine.Ticks;
			Macros.Slice();
			ActionContext.InvokeQueue();
			if (Gumps.Invalidated)
			{
				if (Engine.m_LastMouseArgs != null)
				{
					Engine.MouseMove(Engine.m_Display, Engine.m_LastMouseArgs);
				}
				Gumps.Invalidated = false;
			}
			if (Engine.m_MouseMoved)
			{
				Engine.MouseMoveQueue();
			}
			if (Engine.m_NewFrame.ElapsedReset())
			{
				Renderer.m_Frames++;
				Engine.m_Redraw = false;
				Renderer.Draw();
				stopwatch.Reset();
				stopwatch.Start();
			}
			else if (Engine.m_Redraw || Engine.m_PumpFPS || (Engine.amMoving && Options.Current.SmoothWalk && Engine.IsMoving()))
			{
				Engine.m_Redraw = false;
				Renderer.Draw();
				stopwatch.Reset();
				stopwatch.Start();
			}
			if (flag2 && Engine.m_Ingame && DateTime.Now >= dateTime)
			{
				dateTime = DateTime.Now + TimeSpan.FromSeconds(0.5);
				if (Party.State == PartyState.Joined)
				{
					Network.Send(new PPE_QueryPartyLocsEx());
				}
				if (World.Player.Guild != null)
				{
					Network.Send(new PPE_QueryGuildPosition());
				}
			}
			Thread.Sleep((World.Player == null || !World.Player.IsMoving) ? 1 : 0);
			Engine.DoEvents();
			if (Engine.m_Ingame && !World.HasIdentified)
			{
				Mobile player = World.Player;
				if (player != null && !player.Flags[MobileFlag.Hidden])
				{
					player.OnSingleClick();
					World.HasIdentified = true;
				}
			}
			if (!Network.Slice())
			{
				Debug.Trace("network slice failed");
				flag = true;
				break;
			}
			Engine.EventBus.Publish(new EngineTickEvent());
			Network.Flush();
			Engine.TickTimers();
			if (Engine.amMoving && Engine.m_Ingame)
			{
				Engine.DoWalk(Engine.movingDir, fromRenderer: false);
			}
			if (Engine.m_LoadQueue.Count > 0)
			{
				int num = 0;
				while (Engine.m_LoadQueue.Count > 0 && num < 6)
				{
					Engine.m_LoadQueue.Dequeue().Load();
					num++;
				}
			}
			if (Engine.m_MapLoadQueue.Count > 0)
			{
				Engine.Preload(Engine.m_MapLoadQueue.Dequeue());
			}
		}
		Debug.Trace("Exiting while loop");
		UOAIO.Profiles.Config.Current.Save();
		Thread.Sleep(5);
		if (Engine.m_Display != null && !Engine.m_Display.IsDisposed)
		{
			Engine.m_Display.Hide();
		}
		Thread.Sleep(5);
		Application.DoEvents();
		Thread.Sleep(5);
		Application.DoEvents();
		Engine.m_Animations.Dispose();
		if (Engine.m_ItemArt != null)
		{
			Engine.m_ItemArt.Dispose();
		}
		if (Engine.m_LandArt != null)
		{
			Engine.m_LandArt.Dispose();
		}
		if (Engine.m_TextureArt != null)
		{
			Engine.m_TextureArt.Dispose();
		}
		Engine.m_Gumps.Dispose();
		if (Engine.m_Sounds != null)
		{
			Engine.m_Sounds.Dispose();
		}
		if (Engine.m_Multis != null)
		{
			Engine.m_Multis.Dispose();
		}
		Engine.m_FileManager.Dispose();
		Cursor.Dispose();
		Music.Dispose();
		Hues.Dispose();
		GRadar.Dispose();
		if (Engine._imageCache != null)
		{
			Engine._imageCache.Dispose();
			Engine._imageCache = null;
		}
		if (Engine.m_Rain != null)
		{
			Engine.m_Rain.Dispose();
			Engine.m_Rain = null;
		}
		if (Engine.m_Slider != null)
		{
			Engine.m_Slider.Dispose();
			Engine.m_Slider = null;
		}
		if (Engine.m_SkillUp != null)
		{
			Engine.m_SkillUp.Dispose();
			Engine.m_SkillUp = null;
		}
		if (Engine.m_SkillDown != null)
		{
			Engine.m_SkillDown.Dispose();
			Engine.m_SkillDown = null;
		}
		if (Engine.m_SkillLocked != null)
		{
			Engine.m_SkillLocked.Dispose();
			Engine.m_SkillLocked = null;
		}
		if (Engine.m_Snow != null)
		{
			for (int l = 0; l < 12; l++)
			{
				if (Engine.m_Snow[l] != null)
				{
					Engine.m_Snow[l].Dispose();
					Engine.m_Snow[l] = null;
				}
			}
			Engine.m_Snow = null;
		}
		if (Engine.m_Edge != null)
		{
			for (int m = 0; m < 8; m++)
			{
				if (Engine.m_Edge[m] != null)
				{
					Engine.m_Edge[m].Dispose();
					Engine.m_Edge[m] = null;
				}
			}
			Engine.m_Edge = null;
		}
		if (Engine.m_WinScrolls != null)
		{
			for (int n = 0; n < Engine.m_WinScrolls.Length; n++)
			{
				if (Engine.m_WinScrolls[n] != null)
				{
					Engine.m_WinScrolls[n].Dispose();
					Engine.m_WinScrolls[n] = null;
				}
			}
			Engine.m_WinScrolls = null;
		}
		if (Engine.m_FormX != null)
		{
			Engine.m_FormX.Dispose();
			Engine.m_FormX = null;
		}
		if (Engine.m_Font != null)
		{
			for (int num2 = 0; num2 < 10; num2++)
			{
				if (Engine.m_Font[num2] != null)
				{
					Engine.m_Font[num2].Dispose();
					Engine.m_Font[num2] = null;
				}
			}
			Engine.m_Font = null;
		}
		if (Engine.m_UniFont != null)
		{
			int num3 = Engine.m_UniFont.Length;
			for (int num4 = 0; num4 < num3; num4++)
			{
				if (Engine.m_UniFont[num4] != null)
				{
					Engine.m_UniFont[num4].Dispose();
					Engine.m_UniFont[num4] = null;
				}
			}
			Engine.m_UniFont = null;
		}
		if (Engine.m_MidiTable != null)
		{
			Engine.m_MidiTable.Dispose();
			Engine.m_MidiTable = null;
		}
		if (Engine.m_ContainerBoundsTable != null)
		{
			Engine.m_ContainerBoundsTable.Dispose();
			Engine.m_ContainerBoundsTable = null;
		}
		Texture.DisposeAll();
		Debug.EndBlock();
		if (flag)
		{
			Debug.Trace("Network error caused termination");
		}
		Network.Close();
		Debug.Dispose();
		Speech.Dispose();
		Map.Shutdown();
		Ultima.Data.Archives.Shutdown();
		AssetSourceManager.Shutdown();
		Engine.m_LoadQueue = null;
		Engine.m_MapLoadQueue = null;
		Engine.m_DefaultFont = null;
		Engine.m_DefaultHue = null;
		Engine.m_Display = null;
		Engine.m_Encoder = null;
		Engine.m_Effects = null;
		Engine.m_Skills = null;
		Engine.m_Features = null;
		Engine.m_Animations = null;
		Engine.m_LandArt = null;
		Engine.m_TextureArt = null;
		Engine.m_ItemArt = null;
		Engine.m_Gumps = null;
		Engine.m_Sounds = null;
		Engine.m_Multis = null;
		Engine.m_FileManager = null;
		Engine.m_Display = null;
		Engine.m_Font = null;
		Engine.m_UniFont = null;
		Engine.m_Device = null;
		Engine.m_MoveDelay = null;
		Engine.m_Text = null;
		Engine.m_Font = null;
		Engine.m_UniFont = null;
		Engine.m_NewFrame = null;
		Engine.m_SleepMode = null;
		Engine.m_Timers = null;
		Engine.m_SkillsGump = null;
		Engine.m_JournalGump = null;
		Engine.m_Journal = null;
		Engine.m_FileManager = null;
		Engine.m_Encoder = null;
		Engine.m_DefaultFont = null;
		Engine.m_DefaultHue = null;
		Engine.m_Random = null;
		Engine._movementKeys = null;
		Engine.m_Prompt = null;
		Engine.m_Pings = null;
		Engine.m_PingTimer = null;
		Engine.m_MultiList = null;
		Engine.m_AllNames = null;
		Engine.m_LastOverCheck = null;
		Engine.m_LastMouseArgs = null;
		Engine.m_LastAttacker = null;
	}

	private static void ApplyClientBootstrap(ClientBootstrapDefinition bootstrap)
	{
		if (bootstrap == null)
		{
			throw new ArgumentNullException("bootstrap");
		}

		if (bootstrap.Shard == null)
		{
			throw new InvalidOperationException("Client bootstrap is missing shard details.");
		}

		if (bootstrap.Shard.ServerIP == null)
		{
			throw new InvalidOperationException("Client bootstrap is missing a resolved IP address.");
		}

		Engine.ClientDef = bootstrap;
		Engine.Shard = bootstrap.Shard;
		Engine.ActiveShardRuntime = bootstrap.Shard;
		PacketHandlers.ResetHandlers();
		Engine.ShardHandler = Engine.ShardHandlers.Resolve(Engine.ActiveShardRuntime.Id);
		Engine.ShardHandler.InitializeBootstrap(bootstrap);
		Engine._ticket = new AuthenticationTicket();
	}

	public static void MouseWheel(object sender, MouseEventArgs e)
	{
		if (Engine.m_EventOk && (Playback.Active || ((e.Delta <= 0 || !Engine.m_Ingame || !Macros.Start((Keys)69632)) && (e.Delta >= 0 || !Engine.m_Ingame || !Macros.Start((Keys)69633)))))
		{
			Gumps.MouseWheel(e.X, e.Y, e.Delta);
		}
	}

	public static void ClearPings()
	{
		Engine.m_Pings.Clear();
		Engine.m_PingID = 0;
		Engine.m_Ping = 0;
		if (Engine.m_PingTimer != null)
		{
			Engine.m_PingTimer.Delete();
		}
	}

	public static void StartPings()
	{
		Engine.ClearPings();
		Engine.m_PingTimer = new Timer(Ping_OnTick, 5000);
		Engine.m_PingTimer.Start(Now: false);
	}

	public static void Ping_OnTick(Timer t)
	{
		Engine.PingRequest(sendPing: true);
	}

	public static void PingRequest(bool sendPing)
	{
		if (!Engine.m_Ingame || World.Player == null || Engine.m_WalkAck < Engine.m_WalkReq)
		{
			return;
		}
		int tickCount = Environment.TickCount;
		int num = 2 + Engine.m_PingID % 254;
		if (!sendPing || Network.Send(new PPing(num)))
		{
			Engine.m_Pings.Clear();
			Engine.m_Pings.Enqueue(new int[2]
			{
				tickCount,
				sendPing ? num : (-1)
			});
			if (sendPing)
			{
				Engine.m_PingID++;
			}
		}
	}

	public static void PingReply(int val)
	{
		switch (val)
		{
		case 0:
			ActionContext.HandleSignal(isBegin: true);
			return;
		case 1:
			ActionContext.HandleSignal(isBegin: false);
			return;
		}
		try
		{
			int[] array = Engine.m_Pings.Dequeue();
			int tickCount = Environment.TickCount;
			int num = array[0];
			if (array[1] == val)
			{
				Engine.m_Ping = tickCount - num;
			}
		}
		catch
		{
			Engine.StartPings();
		}
	}

	private static void AttackDialog_YesNo(bool yes)
	{
	}

	public static void DoubleClick(object sender, EventArgs e)
	{
		if (!Engine.m_EventOk || Engine.m_Locked || Engine.amMoving || !Engine.m_ClickTimer.Enabled)
		{
			return;
		}
		Engine.m_ClickTimer.Stop();
		if (Gumps.DoubleClick(Engine.m_xMouse, Engine.m_yMouse))
		{
			return;
		}
		int TileX = 0;
		int TileY = 0;
		Renderer.ResetHitTest();
		ICell cell = Renderer.FindTileFromXY(Engine.m_xMouse, Engine.m_yMouse, ref TileX, ref TileY);
		if (cell != null && !TargetManager.IsActive)
		{
			if (cell.GetType() == typeof(DynamicItem))
			{
				((DynamicItem)cell).m_Item?.OnDoubleClick();
			}
			else if (cell.GetType() == typeof(MobileCell))
			{
				((MobileCell)cell).m_Mobile?.OnDoubleClick();
			}
		}
	}

	public static void ClickMessage(object sender, EventArgs e)
	{
		if (Engine.m_EventOk && !Engine.m_Locked && !Engine.amMoving)
		{
			Engine.m_ClickSender = sender;
			Engine.m_ClickArgs = e;
			Engine.m_xClick = Engine.m_xMouse;
			Engine.m_yClick = Engine.m_yMouse;
			Engine.m_ClickList = Gumps.FindListForSingleClick(Engine.m_xMouse, Engine.m_yMouse);
			Engine.m_ClickTimer.Stop();
			Engine.m_ClickTimer.Start(Now: false);
		}
	}

	public static void Click(object sender, EventArgs e)
	{
		if (!Engine.m_EventOk || Engine.m_Locked || Engine.amMoving)
		{
			return;
		}
		Engine.m_LastDown = -1;
		int TileX = 0;
		int TileY = 0;
		Renderer.ResetHitTest();
		ICell cell = Renderer.FindTileFromXY(Engine.m_xClick, Engine.m_yClick, ref TileX, ref TileY);
		if (cell == null || TargetManager.IsActive)
		{
			return;
		}
		if (cell.GetType() == typeof(DynamicItem))
		{
			((DynamicItem)cell).m_Item.OnSingleClick();
		}
		else if (cell.GetType() == typeof(MobileCell))
		{
			((MobileCell)cell).m_Mobile.OnSingleClick();
		}
		else if (cell.GetType() == typeof(StaticItem))
		{
			string text = Localization.GetString(1020000 + ((StaticItem)cell).ID).Trim();
			if (text.Length > 0)
			{
				World.AddStaticMessage(((StaticItem)cell).Serial, text);
			}
		}
		else if (cell.GetType() == typeof(LandTile) && Gumps.Drag != null && Gumps.Drag.GetType() == typeof(GDraggedItem))
		{
			GDraggedItem gDraggedItem = (GDraggedItem)Gumps.Drag;
			Item item = gDraggedItem.Item;
			Network.Send(new PDropItem(item.Serial, TileX, TileY, (sbyte)(cell.Z + cell.Height), -1));
			Gumps.Destroy(gDraggedItem);
		}
	}

	public static void SendMovementRequest(int dir, int x, int y, int z, TimeSpan speed)
	{
		uint key = ((Engine._movementKeys.Count > 0) ? Engine._movementKeys.Pop() : 0u);
		Network.Send(new PMoveRequest((byte)dir, (byte)Engine.m_Sequence, key, x, y, z, speed));
		Engine.m_WalkReq++;
		Engine.m_Sequence++;
		if (Engine.m_Sequence == 256)
		{
			Engine.m_Sequence = 1;
		}
	}

	public static void ChangeDir(Direction dir)
	{
		int walkDirection = Engine.GetWalkDirection(dir);
		Mobile player = World.Player;
		if (player != null && (player.Direction & 7) != (walkDirection & 7))
		{
			player.Direction = (byte)walkDirection;
			Engine.SendMovementRequest(walkDirection, player.X, player.Y, player.Z, TimeSpan.FromSeconds(0.1));
		}
	}

	public static void StringQueryOkay_OnClick(Gump Sender)
	{
		if (Sender.HasTag("Dialog") && Sender.HasTag("Serial") && Sender.HasTag("Type") && Sender.HasTag("Text"))
		{
			Gumps.Destroy((Gump)Sender.GetTag("Dialog"));
			Network.Send(new PStringQueryResponse((int)Sender.GetTag("Serial"), (short)Sender.GetTag("Type"), ((GTextBox)Sender.GetTag("Text")).String));
		}
	}

	public static void StringQueryCancel_OnClick(Gump Sender)
	{
		if (Sender.HasTag("Dialog") && Sender.HasTag("Serial") && Sender.HasTag("Type"))
		{
			Gumps.Destroy((Gump)Sender.GetTag("Dialog"));
			Network.Send(new PStringQueryCancel((int)Sender.GetTag("Serial"), (short)Sender.GetTag("Type")));
		}
	}

	public static void DestroyGump_OnClick(Gump Sender)
	{
		if (Sender.HasTag("Gump"))
		{
			Gump gump = (Gump)Sender.GetTag("Gump");
			if (gump != null)
			{
				Gumps.Destroy(gump);
			}
		}
	}

	public static void DestroyDialogShowAcctLogin_OnClick(Gump Sender)
	{
		World.Clear();
		Gump gump = null;
		gump = ((!Sender.HasTag("Dialog")) ? Sender.Parent : ((Gump)Sender.GetTag("Dialog")));
		if (gump != null)
		{
			Gumps.Destroy(gump);
			Network.Disconnect();
			Engine.ShowAcctLogin();
		}
	}

	public static void HuePicker_OnHueSelect(int Hue, Gump Sender)
	{
		if (Sender.HasTag("Dialog") && Sender.HasTag("Item Art") && Sender.HasTag("ItemID"))
		{
			Gump gump = (Gump)Sender.GetTag("Dialog");
			Gump g = (Gump)Sender.GetTag("Item Art");
			Gumps.Destroy(g);
			GItemArt gItemArt = new GItemArt(183, 3, (int)Sender.GetTag("ItemID"), Hues.GetItemHue((int)Sender.GetTag("ItemID"), Hue));
			gItemArt.X += (58 - (gItemArt.Image.xMax - gItemArt.Image.xMin)) / 2 - gItemArt.Image.xMin;
			gItemArt.Y += (82 - (gItemArt.Image.yMax - gItemArt.Image.yMin)) / 2 - gItemArt.Image.yMin;
			gump.Children.Add(gItemArt);
			Sender.SetTag("Item Art", gItemArt);
		}
	}

	public static void HuePickerSlider_OnValueChange(double Value, double Old, Gump Sender)
	{
		if (Sender.HasTag("Hue Picker"))
		{
			GHuePicker gHuePicker = (GHuePicker)Sender.GetTag("Hue Picker");
			if (gHuePicker != null)
			{
				gHuePicker.Brightness = (int)Value;
			}
		}
	}

	public static void HuePickerPicker_OnClick(Gump Sender)
	{
		if (Sender.HasTag("Hue Picker") && Sender.HasTag("Brightness Bar"))
		{
			GHuePicker gHuePicker = (GHuePicker)Sender.GetTag("Hue Picker");
			GBrightnessBar gBrightnessBar = (GBrightnessBar)Sender.GetTag("Brightness Bar");
			if (gHuePicker != null && gBrightnessBar != null)
			{
				TargetManager.Client = new HuePickerTargetHandler(gHuePicker, gBrightnessBar);
			}
		}
	}

	public static void HuePickerOk_OnClick(Gump Sender)
	{
		if (!Sender.HasTag("Dialog") || !Sender.HasTag("Hue Picker") || !Sender.HasTag("Serial") || !Sender.HasTag("Relay"))
		{
			return;
		}
		Gump gump = (Gump)Sender.GetTag("Dialog");
		if (gump != null)
		{
			GHuePicker gHuePicker = (GHuePicker)Sender.GetTag("Hue Picker");
			if (gHuePicker == null)
			{
				Gumps.Destroy(gump);
				return;
			}
			int serial = (int)Sender.GetTag("Serial");
			short relay = (short)Sender.GetTag("Relay");
			Network.Send(new PSelectHueResponse(serial, relay, (short)gHuePicker.Hue));
			Gumps.Destroy(gump);
		}
	}

	public static void Help_OnClick(Gump Sender)
	{
		Engine.OpenHelp();
	}

	public static void Journal_OnClick(Gump Sender)
	{
		Engine.OpenJournal();
	}

	public static void Skills_OnClick(Gump Sender)
	{
		Engine.OpenSkills();
	}

	public static void Status_OnClick(Gump Sender)
	{
		int serial = (int)Sender.GetTag("Serial");
		Mobile mobile = World.FindMobile(serial);
		if (mobile != null)
		{
			mobile.QueryStats();
			mobile.OpenStatus(Drag: false);
		}
	}

	public static void LogOut_OnClick(Gump Sender)
	{
		Engine.Quit();
	}

	public static void LogOut_YesNo(Gump sender, bool response)
	{
		if (response)
		{
			Engine.m_Ingame = false;
			Network.Disconnect();
			Engine.ShowAcctLogin();
		}
	}

	public static void Guild_OnClick(Gump Sender)
	{
		Network.Send(new PGuildGumpRequest());
	}

	public static void AttackModeToggle_OnClick(Gump Sender)
	{
		Network.Send(new PSetWarMode(!World.Player.Flags[MobileFlag.Warmode], 32, 0));
	}

	public static void Quit_OnClick(Gump Sender)
	{
		Engine.exiting = true;
	}

	public static void Repeat()
	{
		if (Engine.m_LastCommand != null && Engine.m_LastCommand.Length > 0)
		{
			Engine.commandEntered(Engine.m_LastCommand);
		}
	}

	public static void MouseDown(object sender, MouseEventArgs e)
	{
		if (!Engine.m_EventOk || e == null)
		{
			return;
		}
		Engine.m_LastMouseArgs = e;
		Engine.m_xMouse = e.X;
		Engine.m_yMouse = e.Y;
		if ((!Playback.Active && Engine.m_Ingame && e.Button == MouseButtons.Middle && Macros.Start((Keys)69634)) || Gumps.MouseDown(e.X, e.Y, e.Button))
		{
			return;
		}
		if (!Engine.m_Locked && (e.Button & MouseButtons.Right) == MouseButtons.Right)
		{
			Engine.movingDir = Engine.GetDirection(e.X, e.Y, ref Engine.m_dMouse);
			Engine.amMoving = true;
		}
		else if ((e.Button & MouseButtons.Left) == MouseButtons.Left && (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
		{
			int TileX = 0;
			int TileY = 0;
			GContextMenu.Close();
			Renderer.ResetHitTest();
			ICell cell = Renderer.FindTileFromXY(e.X, e.Y, ref TileX, ref TileY, onlyMobs: false);
			if (cell != null && cell.GetType() == typeof(MobileCell))
			{
				Network.Send(new PPopupRequest((MobileCell)cell));
			}
			else if (cell != null && cell.CellType == typeof(DynamicItem))
			{
				Network.Send(new PPopupRequest(((DynamicItem)cell).m_Item));
			}
		}
		else if (e.Button == MouseButtons.Left)
		{
			DesignContext current = DesignContext.Current;
			if (current != null && current.Entry != null && current.Entry.GetMultiCursor() == null)
			{
				int x = Engine.m_xMouse;
				int y = Engine.m_yMouse;
				current.ComputeTilePosition(ref x, ref y);
				current.BeginDrag(x, y);
			}
		}
	}

	public static void OpenRadar()
	{
		GRadar.Open();
	}

	public static void OpenJournal()
	{
		if (!Engine.m_JournalOpen)
		{
			Engine.m_JournalGump = new GJournal();
			Engine.m_JournalOpen = true;
			Gumps.Desktop.Children.Add(Engine.m_JournalGump);
		}
	}

	public static void OpenHelp()
	{
		Network.Send(new PRequestHelp());
	}

	public static void OpenSkills()
	{
		if (!Engine.m_SkillsOpen)
		{
			Network.Send(new PQuerySkills());
			Engine.PingRequest(sendPing: false);
			Engine.m_SkillsOpen = true;
			Engine.m_SkillsGump = new GSkills();
			Gumps.Desktop.Children.Add(Engine.m_SkillsGump);
		}
	}

	public static void OpenStatus()
	{
		Mobile player = World.Player;
		if (player != null)
		{
			player.QueryStats();
			player.BigStatus = true;
			player.OpenStatus(Drag: false);
		}
	}

	public static void OpenSpellbook(int num)
	{
		Network.Send(new POpenSpellbook(num));
	}

	public static void OpenBackpack()
	{
		World.Player?.Backpack?.Use();
	}

	public static void OpenPaperdoll()
	{
		Network.Send(new POpenPaperdoll());
	}

	public static void Quit()
	{
		GLogOutQuery.Display();
	}

	public static void AllNames()
	{
		if (Engine.m_AllNames != null && !Engine.m_AllNames.ElapsedReset())
		{
			return;
		}
		Engine.m_AllNames = new TimeDelay(1f);
		Mobile player = World.Player;
		if (player == null)
		{
			return;
		}
		foreach (Mobile value2 in World.Mobiles.Values)
		{
			if (value2.Visible && !value2.Player && World.InRange(value2))
			{
				value2.Look();
			}
		}
		if (!TargetManager.IsActive && !player.Ghost && !player.Flags[MobileFlag.Hidden] && !player.Meditating)
		{
			using (ScratchList<Item> scratchList = new ScratchList<Item>())
			{
				List<Item> value = scratchList.Value;
				foreach (Item value3 in World.Items.Values)
				{
					if (value3.Visible && (value3.IsCorpse || value3.IsBones) && World.InRange(value3))
					{
						value3.Look();
						if (value3.InRange(player, 2))
						{
							value.Add(value3);
						}
					}
				}
				if (value.Count > 0)
				{
					value.Sort((Item x, Item y) => PlayerDistanceSorter.Comparer.Compare(x, y));
					value[0].Use();
				}
				return;
			}
		}
		foreach (Item value4 in World.Items.Values)
		{
			if (value4.Visible && (value4.IsCorpse || value4.IsBones) && World.InRange(value4))
			{
				value4.Look();
			}
		}
	}

	public static Item FindItem(int[] itemIDs)
	{
		Mobile player = World.Player;
		if (player == null || player.Ghost)
		{
			return null;
		}
		return player.Backpack?.FindItem(new ItemIDValidator(itemIDs));
	}

	public static Item[] FindItems(int[] itemIDs)
	{
		Mobile player = World.Player;
		if (player == null || player.Ghost)
		{
			return null;
		}
		return player.Backpack?.FindItems(new ItemIDValidator(itemIDs));
	}

	public static Item FindPotion(PotionType type)
	{
		return Engine.FindItem(new int[1] { (int)(3846 + type) });
	}

	public static bool UsePotion(PotionType type)
	{
		Item item = Engine.FindPotion(type);
		if (item == null)
		{
			return false;
		}
		if (type == PotionType.Yellow && DateTime.Now < Engine.m_NextHealPotion)
		{
			if (Engine.m_HealSpam + TimeSpan.FromSeconds(0.5) < DateTime.Now)
			{
				Mobile player = World.Player;
				player.AddTextMessage("", Localization.GetString(500235), Engine.DefaultFont, Hues.Load(34), unremovable: false);
				Engine.m_HealSpam = DateTime.Now;
			}
			return true;
		}
		if (Preferences.Current.Options.ClearHandsBeforePot && type != PotionType.Purple)
		{
			Engine.Dequip(message: false);
		}
		return item.Use();
	}

	public static void OnHealPotion()
	{
		Engine.m_NextHealPotion = DateTime.Now + TimeSpan.FromSeconds(10.0);
	}

	public static void KeyUp(KeyEventArgs e)
	{
		if (Engine.m_EventOk && !Playback.Active && Engine.m_Ingame && World.Player != null && !e.Alt && !e.Control && !e.Shift && e.KeyCode == Keys.Tab)
		{
			e.Handled = Network.Send(new PSetWarMode(warMode: false, 32, 0));
		}
	}

	public static void CancelClick()
	{
		Engine.m_ClickTimer.Stop();
	}

	public static bool AttackLast()
	{
		return TargetManager.LastOffensiveTarget?.Attack() ?? false;
	}

	public static void CountAmmo()
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return;
		}
		Item backpack = player.Backpack;
		if (backpack == null)
		{
			return;
		}
		int[] array = new int[2] { 3903, 7163 };
		string[] array2 = new string[2] { "Arrows", "Bolts" };
		for (int i = 0; i < array.Length; i++)
		{
			Item[] array3 = backpack.FindItems(new ItemIDValidator(array[i]));
			int num = 0;
			for (int j = 0; j < array3.Length; j++)
			{
				num += (ushort)array3[j].Amount;
			}
			Engine.AddTextMessage(array2[i] + ": " + num, Engine.DefaultFont, (num < 5) ? Hues.Load(34) : Engine.DefaultHue);
		}
	}

	public static void CountReagents()
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return;
		}
		Item backpack = player.Backpack;
		if (backpack == null)
		{
			return;
		}
		Reagent[] reagents = Spells.Reagents;
		for (int i = 0; i < reagents.Length; i++)
		{
			Item[] array = backpack.FindItems(new ItemIDValidator(reagents[i].ItemID));
			int num = 0;
			for (int j = 0; j < array.Length; j++)
			{
				num += (ushort)array[j].Amount;
			}
			Engine.AddTextMessage(reagents[i].Name + ": " + num, Engine.DefaultFont, (num < 5) ? Hues.Load(34) : Engine.DefaultHue);
		}
	}

	public static bool SmartPotion()
	{
		return Engine.SmartPotion(100);
	}

	public static bool SmartPotion(int perc)
	{
		Mobile player = World.Player;
		if (player != null)
		{
			if (!player.Ghost)
			{
				if (player.IsPoisoned)
				{
					if (!Engine.UsePotion(PotionType.Orange))
					{
						Engine.AddTextMessage("You do not have any orange potions!", Engine.DefaultFont, Hues.Load(34));
						return false;
					}
				}
				else if (player.MaximumHitPoints > 0 && player.CurrentHitPoints * 100 / player.MaximumHitPoints < perc && !Engine.UsePotion(PotionType.Yellow))
				{
					Engine.AddTextMessage("You do not have any yellow potions!", Engine.DefaultFont, Hues.Load(34));
					return false;
				}
				return true;
			}
			Engine.AddTextMessage("A potion can not help you at this point.");
		}
		return false;
	}

	public static bool BandageSelf()
	{
		Mobile player = World.Player;
		if (player != null)
		{
			if (player.Ghost)
			{
				Engine.AddTextMessage("You are dead.");
			}
			else if (player.CurrentHitPoints == player.MaximumHitPoints && !player.IsPoisoned)
			{
				Engine.AddTextMessage("You do not need to be bandaged.");
			}
			else
			{
				Item backpack = player.Backpack;
				if (backpack == null)
				{
					Engine.AddTextMessage("You do not have a backpack.");
				}
				else
				{
					Item[] array = backpack.FindItems(new ItemIDValidator(3617, 3817));
					if (array.Length != 0)
					{
						Array.Sort(array, AmountSorter.Comparer);
						int num = 0;
						for (int i = 0; i < array.Length; i++)
						{
							num += (ushort)array[i].Amount;
						}
						if (Options.Current.ProtectBandages && GBandageTimer.Active)
						{
							return false;
						}
						new BandageContext(array[0], player).Enqueue();
						num--;
						if (num == 0)
						{
							Engine.AddTextMessage("That was your last bandage!", Engine.DefaultFont, Hues.Load(34));
						}
						else if (num <= 5)
						{
							Engine.AddTextMessage($"You are running very low on bandages! There are {num} remaining.", Engine.DefaultFont, Hues.Load(34));
						}
						else if (num <= 10)
						{
							Engine.AddTextMessage($"You are running low on bandages. There are {num} remaining.", Engine.DefaultFont, Hues.Load(34));
						}
						else
						{
							Engine.AddTextMessage($"You have {num} bandages remaining.");
						}
						return true;
					}
					Engine.AddTextMessage("You have no bandages!", Engine.DefaultFont, Hues.Load(34));
				}
			}
		}
		return false;
	}

	public static void KeyDown(object sender, KeyEventArgs e)
	{
		if (!Engine.m_EventOk || Playback.Active)
		{
			return;
		}
		if (Engine.m_Ingame && Macros.Start(e.KeyCode) && (e.Alt || e.Control || !Preferences.Current.Options.KeyPassthrough))
		{
			e.Handled = true;
			return;
		}
		Keys keyCode = e.KeyCode;
		bool shift = e.Shift;
		bool control = e.Control;
		bool alt = e.Alt;
		if (Engine.m_Ingame && World.Player != null && !alt && !control && !shift && keyCode == Keys.Tab && Gumps.TextFocus == null)
		{
			e.Handled = Network.Send(new PSetWarMode(warMode: true, 32, 0));
		}
		else
		{
			e.Handled = false;
		}
	}

	public static void AddToJournal(JournalEntry je)
	{
		if (Engine.m_JournalGump != null)
		{
			Engine.m_JournalGump.OnEntryAdded();
		}
		Engine.m_Journal.Add(je);
	}

	public static void MouseMove(object sender, MouseEventArgs e)
	{
		if (Engine.m_EventOk && e != null)
		{
			Engine.m_LastMouseArgs = e;
			Engine.m_MouseMoved = true;
			if (Engine.m_xMouse != e.X || Engine.m_yMouse != e.Y)
			{
				Engine.m_Redraw = true;
			}
			Engine.m_xMouse = e.X;
			Engine.m_yMouse = e.Y;
		}
	}

	private static void PopupDelay_OnTick(Timer t)
	{
		object tag = t.GetTag("object");
		GObjectProperties.Display(tag);
		t.Stop();
		Engine.m_PopupDelay = null;
	}

	public static void MouseMoveQueue()
	{
		if (!Engine.m_EventOk)
		{
			return;
		}
		MouseEventArgs lastMouseArgs = Engine.m_LastMouseArgs;
		Engine.m_MouseMoved = false;
		Engine.pointingDir = Engine.GetDirection(lastMouseArgs.X, lastMouseArgs.Y, ref Engine.m_dMouse);
		if (Engine.m_xMouse != lastMouseArgs.X || Engine.m_yMouse != lastMouseArgs.Y)
		{
			Engine.m_Redraw = true;
		}
		Engine.m_xMouse = lastMouseArgs.X;
		Engine.m_yMouse = lastMouseArgs.Y;
		if (Gumps.MouseMove(lastMouseArgs.X, lastMouseArgs.Y, lastMouseArgs.Button))
		{
			return;
		}
		if (!Engine.m_Locked && Engine.amMoving)
		{
			GObjectProperties.Hide();
			if (Engine.m_PopupDelay != null)
			{
				Engine.m_PopupDelay.Stop();
			}
			Engine.m_PopupDelay = null;
			Engine.movingDir = Engine.pointingDir;
		}
		else if (Engine.amMoving && Engine.m_Ingame)
		{
			GObjectProperties.Hide();
			if (Engine.m_PopupDelay != null)
			{
				Engine.m_PopupDelay.Stop();
			}
			Engine.m_PopupDelay = null;
		}
		else if (Gumps.Drag != null)
		{
			GObjectProperties.Hide();
			if (Engine.m_PopupDelay != null)
			{
				Engine.m_PopupDelay.Stop();
			}
			Engine.m_PopupDelay = null;
		}
		else if (lastMouseArgs.Button == MouseButtons.None && World.Serial != 0)
		{
			if (!Engine.ServerFeatures.AOS)
			{
				return;
			}
			int TileX = 0;
			int TileY = 0;
			ICell cell = Renderer.FindTileFromXY(Engine.m_xMouse, Engine.m_yMouse, ref TileX, ref TileY, onlyMobs: true);
			if (World.Player.Flags[MobileFlag.Warmode] && cell != null && cell.CellType == typeof(MobileCell))
			{
				Engine.m_Highlight = ((MobileCell)cell).m_Mobile;
			}
			else
			{
				Engine.m_Highlight = null;
			}
			if (cell is DynamicItem)
			{
				Item item = ((DynamicItem)cell).m_Item;
				if (item.IsMovable)
				{
					if (item.PropertyList == null)
					{
						item.QueryProperties();
						GObjectProperties.Hide();
						if (Engine.m_PopupDelay != null)
						{
							Engine.m_PopupDelay.Stop();
						}
						Engine.m_PopupDelay = null;
					}
					else if (GObjectProperties.Instance == null || GObjectProperties.Instance.Object != item)
					{
						if (Engine.m_PopupDelay == null)
						{
							Engine.m_PopupDelay = new Timer(PopupDelay_OnTick, 250);
							Engine.m_PopupDelay.SetTag("object", item);
							Engine.m_PopupDelay.Start(Now: false);
						}
						else
						{
							Engine.m_PopupDelay.SetTag("object", item);
						}
					}
				}
				else
				{
					GObjectProperties.Hide();
					if (Engine.m_PopupDelay != null)
					{
						Engine.m_PopupDelay.Stop();
					}
					Engine.m_PopupDelay = null;
				}
			}
			else if (cell is MobileCell)
			{
				Mobile mobile = ((MobileCell)cell).m_Mobile;
				if (mobile.PropertyList == null)
				{
					mobile.QueryProperties();
					GObjectProperties.Hide();
					if (Engine.m_PopupDelay != null)
					{
						Engine.m_PopupDelay.Stop();
					}
					Engine.m_PopupDelay = null;
				}
				else if (GObjectProperties.Instance == null || GObjectProperties.Instance.Object != mobile)
				{
					if (Engine.m_PopupDelay == null)
					{
						Engine.m_PopupDelay = new Timer(PopupDelay_OnTick, 250);
						Engine.m_PopupDelay.SetTag("object", mobile);
						Engine.m_PopupDelay.Start(Now: false);
					}
					else
					{
						Engine.m_PopupDelay.SetTag("object", mobile);
					}
				}
			}
			else
			{
				GObjectProperties.Hide();
				if (Engine.m_PopupDelay != null)
				{
					Engine.m_PopupDelay.Stop();
				}
				Engine.m_PopupDelay = null;
			}
		}
		else if (lastMouseArgs.Button == MouseButtons.Left && Engine.m_Ingame)
		{
			if (DesignContext.Current != null && DesignContext.Current.Dragging)
			{
				return;
			}
			GObjectProperties.Hide();
			if (Engine.m_PopupDelay != null)
			{
				Engine.m_PopupDelay.Stop();
			}
			Engine.m_PopupDelay = null;
			if (Engine.m_LastDown > 0)
			{
				int TileX2 = 0;
				int TileY2 = 0;
				ICell cell2 = Renderer.FindTileFromXY(Engine.m_xMouse, Engine.m_yMouse, ref TileX2, ref TileY2, onlyMobs: true);
				if (Engine.m_LastDown < 1073741824)
				{
					if (cell2 == null || cell2.CellType != typeof(MobileCell) || ((MobileCell)cell2).m_Mobile.Serial != Engine.m_LastDown || (Engine.m_LastDownPoint ^ new Point(Engine.m_xMouse, Engine.m_yMouse)) >= 2)
					{
						Mobile mobile2 = World.FindMobile(Engine.m_LastDown);
						if (mobile2 != null)
						{
							mobile2.QueryStats();
							mobile2.OpenStatus(Drag: true);
						}
						Engine.m_LastDown = 0;
					}
				}
				else
				{
					if (cell2 != null && !(cell2.CellType != typeof(DynamicItem)) && ((DynamicItem)cell2).Serial == Engine.m_LastDown && (Engine.m_LastDownPoint ^ new Point(Engine.m_xMouse, Engine.m_yMouse)) < 2)
					{
						return;
					}
					Mobile player = World.Player;
					if (player != null && !player.Ghost)
					{
						Item item2 = World.FindItem(Engine.m_LastDown);
						if (item2 != null)
						{
							Gump gump = item2.OnBeginDrag();
							if (gump.GetType() == typeof(GDragAmount))
							{
								((GDragAmount)gump).ToDestroy = item2;
							}
							else
							{
								item2.RestoreInfo = new RestoreInfo(item2);
								World.Remove(item2);
							}
						}
					}
					Engine.m_LastDown = 0;
				}
			}
			else
			{
				if (Engine.m_LastDown != -1)
				{
					return;
				}
				int TileX3 = 0;
				int TileY3 = 0;
				Renderer.ResetHitTest();
				ICell cell3 = Renderer.FindTileFromXY(Engine.m_xMouse, Engine.m_yMouse, ref TileX3, ref TileY3, onlyMobs: false);
				Engine.m_LastDownPoint = new Point(Engine.m_xMouse, Engine.m_yMouse);
				if (cell3 != null && cell3.GetType() == typeof(MobileCell))
				{
					Engine.m_LastDown = ((MobileCell)cell3).m_Mobile.Serial;
				}
				else
				{
					if (cell3 == null || !(cell3.GetType() == typeof(DynamicItem)))
					{
						return;
					}
					Item item3 = ((DynamicItem)cell3).m_Item;
					if (item3 != null)
					{
						if (item3.IsMovable)
						{
							Engine.m_LastDown = ((DynamicItem)cell3).Serial;
						}
						else
						{
							Engine.m_LastDown = -1;
						}
					}
					else
					{
						Engine.m_LastDown = -1;
					}
				}
			}
		}
		else if (!Engine.m_Locked && Engine.amMoving && (!Engine.amMoving || !Engine.m_Ingame))
		{
			GObjectProperties.Hide();
			if (Engine.m_PopupDelay != null)
			{
				Engine.m_PopupDelay.Stop();
			}
			Engine.m_PopupDelay = null;
			Engine.amMoving = false;
		}
		else if (!Engine.m_Locked && Engine.amMoving)
		{
			GObjectProperties.Hide();
			if (Engine.m_PopupDelay != null)
			{
				Engine.m_PopupDelay.Stop();
			}
			Engine.m_PopupDelay = null;
			Engine.movingDir = Engine.pointingDir;
		}
		else
		{
			GObjectProperties.Hide();
			if (Engine.m_PopupDelay != null)
			{
				Engine.m_PopupDelay.Stop();
			}
			Engine.m_PopupDelay = null;
		}
	}

	public static void Redraw()
	{
		Engine.m_Redraw = true;
	}

	public static void MouseUp(object sender, MouseEventArgs e)
	{
		if (!Engine.m_EventOk || e == null)
		{
			return;
		}
		Engine.m_LastMouseArgs = e;
		Engine.m_LastDown = -1;
		Engine.m_xMouse = e.X;
		Engine.m_yMouse = e.Y;
		Gump drag = Gumps.Drag;
		if (Gumps.MouseUp(e.X, e.Y, e.Button))
		{
			return;
		}
		DesignContext current = DesignContext.Current;
		if (current != null && current.Dragging && e.Button == MouseButtons.Left)
		{
			int x = Engine.m_xMouse;
			int y = Engine.m_yMouse;
			current.ComputeTilePosition(ref x, ref y);
			current.FinishDrag(x, y);
			Engine.CancelClick();
		}
		else if (!Engine.m_Locked && e.Button == MouseButtons.Right && (Control.MouseButtons == MouseButtons.None || Gumps.Drag != null || (current != null && current.Dragging)))
		{
			Engine.amMoving = false;
		}
		else if (drag != null && drag.GetType() == typeof(GDraggedItem))
		{
			Renderer.ResetHitTest();
			GDraggedItem gDraggedItem = (GDraggedItem)drag;
			gDraggedItem.m_IsDragging = false;
			Gumps.Drag = null;
			Gumps.Destroy(gDraggedItem);
			Item item = gDraggedItem.Item;
			int TileX = 0;
			int TileY = 0;
			ICell cell = Renderer.FindTileFromXY(e.X, e.Y, ref TileX, ref TileY);
			if (cell != null)
			{
				if (cell.CellType == typeof(MobileCell))
				{
					Network.Send(new PDropItem(item.Serial, -1, -1, 0, ((MobileCell)cell).m_Mobile.Serial));
				}
				else if (cell.CellType == typeof(DynamicItem))
				{
					Item item2 = ((DynamicItem)cell).m_Item;
					if (item2.IsContainer)
					{
						Network.Send(new PDropItem(item.Serial, -1, -1, 0, item2.Serial));
					}
					else if (item2.IsStackable && item.ID == item2.ID && item.Hue == item2.Hue)
					{
						Network.Send(new PDropItem(item.Serial, item2.X, item2.Y, (sbyte)item2.Z, item2.Serial));
					}
					else
					{
						Network.Send(new PDropItem(item.Serial, item2.X, item2.Y, (sbyte)item2.Z, -1));
					}
				}
				else
				{
					Network.Send(new PDropItem(item.Serial, TileX, TileY, (sbyte)(cell.Z + cell.Height), -1));
				}
			}
		}
		else
		{
			if (!TargetManager.IsActive || drag != null || Gumps.Drag != null || e.Button != MouseButtons.Left || Control.MouseButtons.HasFlag(MouseButtons.Left))
			{
				return;
			}
			int TileX2 = 0;
			int TileY2 = 0;
			ICell cell2 = Renderer.FindTileFromXY(e.X, e.Y, ref TileX2, ref TileY2);
			if (cell2 != null)
			{
				if (cell2 is MobileCell)
				{
					TargetManager.Target(((MobileCell)cell2).m_Mobile);
				}
				else if (cell2 is DynamicItem)
				{
					TargetManager.Target(((DynamicItem)cell2).m_Item);
				}
				else if (cell2 is StaticItem)
				{
					TargetManager.Target(new StaticTarget(TileX2, TileY2, ((StaticItem)cell2).m_Z, ((StaticItem)cell2).m_RealID, ((StaticItem)cell2).m_RealID, ((StaticItem)cell2).m_Hue));
				}
				else if (cell2 is LandTile)
				{
					TargetManager.Target(new GroundTarget(TileX2, TileY2, ((LandTile)cell2).m_Z));
				}
			}
		}
		Engine.CancelClick();
	}

	public static Direction GetDirection(int xFrom, int yFrom, int xTo, int yTo)
	{
		int num = xFrom - xTo;
		int num2 = yFrom - yTo;
		int num3 = (num - num2) * 44;
		int num4 = (num + num2) * 44;
		int num5 = Math.Abs(num3);
		int num6 = Math.Abs(num4);
		if ((num6 >> 1) - num5 >= 0)
		{
			return (num4 <= 0) ? Direction.Down : Direction.Up;
		}
		if ((num5 >> 1) - num6 >= 0)
		{
			return (num3 > 0) ? Direction.Left : Direction.Right;
		}
		if (num3 >= 0 && num4 >= 0)
		{
			return Direction.West;
		}
		if (num3 >= 0 && num4 < 0)
		{
			return Direction.South;
		}
		if (num3 < 0 && num4 < 0)
		{
			return Direction.East;
		}
		return Direction.North;
	}

	public static Direction GetDirection(int x, int y, ref int distance)
	{
		int num = Engine.GameX + Engine.GameWidth / 2 - x;
		int num2 = Engine.GameY + Engine.GameHeight / 2 - y;
		int num3 = Math.Abs(num);
		int num4 = Math.Abs(num2);
		int num5 = (int)((double)Engine.GameWidth / (double)Engine.GameHeight * (double)num4);
		distance = (int)Math.Sqrt(num * num + num5 * num5);
		if ((num4 >> 1) - num3 >= 0)
		{
			if (num2 > 0)
			{
				return Direction.Up;
			}
			return Direction.Down;
		}
		if ((num3 >> 1) - num4 >= 0)
		{
			if (num > 0)
			{
				return Direction.Left;
			}
			return Direction.Right;
		}
		if (num >= 0 && num2 >= 0)
		{
			return Direction.West;
		}
		if (num >= 0 && num2 < 0)
		{
			return Direction.South;
		}
		if (num < 0 && num2 < 0)
		{
			return Direction.East;
		}
		return Direction.North;
	}

	public static Font GetFont(int id)
	{
		if (id < 0 || id >= 10)
		{
			id = 0;
		}
		Font font = Engine.m_Font[id];
		if (font == null)
		{
			font = (Engine.m_Font[id] = new Font(id));
		}
		return font;
	}

	public static UnicodeFont GetUniFont(int id)
	{
		if (id < 0 || id >= 3)
		{
			id = 1;
		}
		UnicodeFont unicodeFont = Engine.m_UniFont[id];
		if (unicodeFont == null)
		{
			unicodeFont = (Engine.m_UniFont[id] = new UnicodeFont(id));
		}
		return unicodeFont;
	}

	public static void ResetDevice()
	{
		if (Engine.m_VertexBuffer != null)
		{
			Engine.m_VertexBuffer.Dispose();
		}
		Engine.m_Device.Reset(Engine.m_PresentParams);
		Engine.OnDeviceReset(null, null);
	}

	public static void OnDeviceReset(object sender, EventArgs e)
	{
		Renderer.m_Version++;
		Engine.m_VertexBuffer = new VertexBuffer(Engine.m_Device, 32768 * TransformedColoredTextured.StrideSize, Usage.Dynamic | Usage.WriteOnly, VertexFormat.PositionRhw | VertexFormat.Diffuse | VertexFormat.Texture1, Pool.Default);
		Engine.m_Device.SetStreamSource(0, Engine.m_VertexBuffer, 0, TransformedColoredTextured.StrideSize);
		Engine.m_Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse | VertexFormat.Texture1;
		Capabilities capabilities = Engine.m_Device.Capabilities;
		ShaderData.DeviceVersion = capabilities.PixelShaderVersion;
		Texture.Square = capabilities.TextureCaps.HasFlag(TextureCaps.SquareOnly);
		Texture.Pow2 = capabilities.TextureCaps.HasFlag(TextureCaps.Pow2);
		Texture.MaxTextureWidth = capabilities.MaxTextureWidth;
		Texture.MaxTextureHeight = capabilities.MaxTextureHeight;
		Texture.MinTextureWidth = 1;
		Texture.MinTextureHeight = 1;
		Texture.CanSysMem = capabilities.DeviceCaps.HasFlag(DeviceCaps.TextureSystemMemory);
		Texture.CanVidMem = capabilities.DeviceCaps.HasFlag(DeviceCaps.TextureVideoMemory);
		Texture.MaxAspect = capabilities.MaxTextureAspectRatio;
		Renderer.Init(capabilities);
		Engine.m_Device.SetRenderState(RenderState.DitherEnable, enable: false);
		Engine.m_Device.SetRenderState(RenderState.NormalizeNormals, enable: false);
		Engine.m_Device.SetRenderState(RenderState.RangeFogEnable, enable: false);
		Engine.m_Device.SetRenderState(RenderState.StencilEnable, enable: false);
		Engine.m_Device.SetRenderState(RenderState.ZEnable, enable: true);
		Engine.m_Device.SetRenderState(RenderState.ZWriteEnable, enable: true);
		Engine.m_Device.SetRenderState(RenderState.CullMode, Cull.Counterclockwise);
		Engine.m_Device.SetRenderState(RenderState.AntialiasedLineEnable, enable: false);
		Engine.m_Device.SetRenderState(RenderState.SpecularEnable, enable: false);
		Engine.m_Device.SetRenderState(RenderState.ShadeMode, ShadeMode.Gouraud);
		Engine.m_Device.SetRenderState(RenderState.Lighting, enable: false);
		Engine.m_Device.SetRenderState(RenderState.VertexBlend, VertexBlend.Disable);
		Engine.m_Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
		Engine.m_Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
		Engine.m_Device.SetRenderState(RenderState.AlphaRef, 1);
		Engine.m_Device.SetRenderState(RenderState.AlphaFunc, Compare.GreaterEqual);
		Engine.m_Device.SetRenderState(RenderState.AlphaBlendEnable, enable: false);
		Engine.m_Device.SetRenderState(RenderState.AlphaTestEnable, enable: false);
		Engine.m_Device.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
		Engine.m_Device.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.Diffuse);
		Engine.m_Device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
		Engine.m_Device.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
		Engine.m_Device.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);
		Engine.m_Device.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
		Renderer.Reset();
	}

	public static void InitDX()
	{
		Engine.m_Fullscreen = Preferences.Current.Layout.Fullscreen;
		PresentParameters presentParameters = new PresentParameters
		{
			SwapEffect = SwapEffect.Discard,
			EnableAutoDepthStencil = true,
			AutoDepthStencilFormat = Format.D16,
			PresentationInterval = PresentInterval.Immediate,
			DeviceWindowHandle = Engine.m_Display.Handle
		};
		if (Engine.m_Fullscreen)
		{
			presentParameters.Windowed = false;
			List<DisplayMode> list = new List<DisplayMode>();
			AdapterCollection adapters = Engine.m_Direct3D.Adapters;
			if (adapters != null && adapters.Count > 0)
			{
				AdapterInformation adapterInformation = adapters[0];
				foreach (DisplayMode displayMode2 in adapterInformation.GetDisplayModes(Format.A8R8G8B8))
				{
					list.Add(displayMode2);
				}
				list.Sort(new DisplayModeComparer(Engine.ScreenWidth, Engine.ScreenHeight, Format.A8R8G8B8));
			}
			if (list.Count == 0)
			{
				throw new Exception("No display modes found");
			}
			DisplayMode displayMode = list[0];
			Debug.Trace("Display Mode: {0}x{1}, {2}, {3}hz", displayMode.Width, displayMode.Height, displayMode.Format, displayMode.RefreshRate);
			presentParameters.BackBufferCount = 1;
			presentParameters.SwapEffect = SwapEffect.Flip;
			presentParameters.BackBufferFormat = displayMode.Format;
			presentParameters.BackBufferWidth = displayMode.Width;
			presentParameters.BackBufferHeight = displayMode.Height;
		}
		else
		{
			presentParameters.Windowed = true;
		}
		PresentParameters[] array = new PresentParameters[3]
		{
			new PresentParameters(presentParameters.BackBufferWidth, presentParameters.BackBufferHeight, presentParameters.BackBufferFormat, presentParameters.BackBufferCount, presentParameters.MultiSampleType, presentParameters.MultiSampleQuality, presentParameters.SwapEffect, presentParameters.DeviceWindowHandle, presentParameters.Windowed, presentParameters.EnableAutoDepthStencil, presentParameters.AutoDepthStencilFormat, presentParameters.PresentFlags, presentParameters.FullScreenRefreshRateInHz, presentParameters.PresentationInterval),
			new PresentParameters(presentParameters.BackBufferWidth, presentParameters.BackBufferHeight, presentParameters.BackBufferFormat, presentParameters.BackBufferCount, presentParameters.MultiSampleType, presentParameters.MultiSampleQuality, presentParameters.SwapEffect, presentParameters.DeviceWindowHandle, presentParameters.Windowed, presentParameters.EnableAutoDepthStencil, presentParameters.AutoDepthStencilFormat, presentParameters.PresentFlags, presentParameters.FullScreenRefreshRateInHz, presentParameters.PresentationInterval),
			new PresentParameters(presentParameters.BackBufferWidth, presentParameters.BackBufferHeight, presentParameters.BackBufferFormat, presentParameters.BackBufferCount, presentParameters.MultiSampleType, presentParameters.MultiSampleQuality, presentParameters.SwapEffect, presentParameters.DeviceWindowHandle, presentParameters.Windowed, presentParameters.EnableAutoDepthStencil, presentParameters.AutoDepthStencilFormat, presentParameters.PresentFlags, presentParameters.FullScreenRefreshRateInHz, presentParameters.PresentationInterval)
		};
		int smoothingMode = Preferences.Current.RenderSettings.SmoothingMode;
		for (int i = 0; i < 3; i++)
		{
			MultisampleType multiSampleType;
			switch ((3 + smoothingMode - i) % 3)
			{
			case 0:
				multiSampleType = MultisampleType.None;
				break;
			case 1:
				multiSampleType = MultisampleType.TwoSamples;
				break;
			case 2:
				multiSampleType = MultisampleType.FourSamples;
				break;
			default:
				continue;
			}
			array[i].MultiSampleType = multiSampleType;
		}
		Engine.m_Direct3D = new Direct3D();
		Exception innerException = null;
		for (int j = 0; j < array.Length; j++)
		{
			Engine.m_PresentParams = array[j];
			try
			{
				try
				{
					Engine.m_Device = new Device(Engine.m_Direct3D, 0, DeviceType.Hardware, Engine.m_Display.Handle, CreateFlags.HardwareVertexProcessing, Engine.m_PresentParams);
				}
				catch
				{
					Engine.m_Device = new Device(Engine.m_Direct3D, 0, DeviceType.Hardware, Engine.m_Display.Handle, CreateFlags.SoftwareVertexProcessing, Engine.m_PresentParams);
				}
			}
			catch (Exception ex)
			{
				innerException = ex;
				continue;
			}
			break;
		}
		if (Engine.m_Device == null)
		{
			throw new ApplicationException("Unable to create Direct3D device.", innerException);
		}
		Renderer.m_Version++;
		Engine.OnDeviceReset(Engine.m_Device, null);
		Debug.Trace("Fullscreen = {0}", Engine.m_Fullscreen);
		Engine.m_rRender = new Rectangle(0, 0, Engine.ScreenWidth, Engine.ScreenHeight);
		if (!Texture.CanSysMem && !Texture.CanVidMem)
		{
			throw new Exception("Device does not support textures in video memory nor system memory.");
		}
	}

	public static void OpenRunebooks()
	{
		foreach (Item item in World.Items.Values)
		{
			if ((item.ID == 8901 || item.ID == 3643 || item.ID == 3834) && item.Hue == 1121)
			{
				new GenericContext(delegate
				{
					Network.Send(new PUseRequest(item));
				}).Enqueue();
			}
		}
	}

	private static Dictionary<string, string> _ParseArgs(string[] args)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (args == null || args.Length == 0)
		{
			return dictionary;
		}
		for (int i = 0; i < args.Length; i++)
		{
			string text = args[i];
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			string text2 = text.Trim();
			if (text2.StartsWith("--"))
			{
				text2 = text2.Substring(2);
			}
			else if (text2.StartsWith("-") || text2.StartsWith("/"))
			{
				text2 = text2.Substring(1);
			}
			int num = text2.IndexOf('=');
			int num2 = text2.IndexOf(':');
			int num3 = -1;
			if (num >= 0 && num2 >= 0)
			{
				num3 = Math.Min(num, num2);
			}
			else if (num >= 0)
			{
				num3 = num;
			}
			else if (num2 >= 0)
			{
				num3 = num2;
			}
			if (num3 > 0)
			{
				string text3 = text2.Substring(0, num3).Trim();
				string value = text2.Substring(num3 + 1).Trim();
				if (!string.IsNullOrWhiteSpace(text3))
				{
					dictionary[text3] = value;
				}
				continue;
			}
			if (i + 1 < args.Length)
			{
				string text4 = args[i + 1];
				if (!string.IsNullOrWhiteSpace(text4) && !text4.StartsWith("-") && !text4.StartsWith("/"))
				{
					dictionary[text2] = text4;
					i++;
					continue;
				}
			}
			dictionary[text2] = string.Empty;
		}
		return dictionary;
	}
}
