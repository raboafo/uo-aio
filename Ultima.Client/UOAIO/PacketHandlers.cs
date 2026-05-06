using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ultima.Client;
using Ultima.Data;
using UOAIO.Events;
using UOAIO.Profiles;
using UOAIO.Prompts;
using UOAIO.Targeting;
using UOAIOPlugins;
using Veritas.Compression;

namespace UOAIO;

public class PacketHandlers
{
	[Flags]
	internal enum EventFlags
	{
		None = 0,
		ConsumeHeal = 1,
		PotionSound = 2,
		HealPotion = 3
	}

	private enum Condition
	{
		Ageless,
		LikeNew,
		Slightly,
		Somewhat,
		Fairly,
		Greatly,
		IDOC
	}

	private enum EffectType
	{
		Moving,
		Lightning,
		Location,
		Fixed
	}

	private enum EffectLayer
	{
		Head = 0,
		RightHand = 1,
		LeftHand = 2,
		Waist = 3,
		LeftFoot = 4,
		RightFoot = 5,
		CenterFeet = 7
	}

	private static PacketHandlerRegistry m_Registry;

	internal static EventFlags m_EventFlags;

	private static int m_PathfindIndex;

	private static string[] m_Args;

	private static Regex m_ArgReplace;

	private static DateTime m_HealStart;

	private static int m_xMapLeft;

	private static int m_xMapRight;

	private static int m_yMapTop;

	private static int m_yMapBottom;

	private static int m_xMapWidth;

	private static int m_yMapHeight;

	private static readonly byte[] rsaCspBlob;

	private static RegionWorld[] _regionWorlds;

	private static string[] m_BuyMenuNames;

	private static int[] m_BuyMenuPrices;

	private static int m_BuyMenuSerial;

	private static byte[] m_CompBuffer;

	private static Dictionary<int, string> m_WorldNames;

	private static int m_LastWorld;

	private static string[] m_IPFReason;

	internal static Queue m_Sequences;

	public static TimeSpan m_MoveDelay;

	internal static object m_CancelTarget;

	internal static DateTime m_CancelTimeout;

	internal static TargetAction m_CancelAction;

	private static byte[] m_Key;

	public static readonly EventBus EventBus;

	private static PacketCallback Unhandled => UnhandledStub;

	public static string[] IPFReason => PacketHandlers.m_IPFReason;

	internal static PacketHandlerRegistry Registry => PacketHandlers.m_Registry;

	internal static void SetEvent(EventFlags eventFlag)
	{
		PacketHandlers.m_EventFlags |= eventFlag;
	}

	internal static bool CheckEvent(EventFlags eventFlag)
	{
		return (PacketHandlers.m_EventFlags & eventFlag) == eventFlag;
	}

	internal static void BeginSlice()
	{
		PacketHandlers.m_EventFlags = EventFlags.None;
	}

	internal static void FinishSlice()
	{
		if (PacketHandlers.CheckEvent(EventFlags.HealPotion))
		{
			Engine.OnHealPotion();
		}
		PacketHandlers.m_EventFlags = EventFlags.None;
	}

	static PacketHandlers()
	{
		PacketHandlers.EventBus = new EventBus();
		PacketHandlers.m_PathfindIndex = 20;
		PacketHandlers.m_ArgReplace = new Regex("~(?<1>\\d+).*?~", RegexOptions.Singleline);
		PacketHandlers.rsaCspBlob = new byte[276]
		{
			6, 2, 0, 0, 0, 164, 0, 0, 82, 83,
			65, 49, 0, 8, 0, 0, 1, 0, 1, 0,
			125, 138, 16, 112, 214, 118, 172, 65, 223, 141,
			198, 212, 79, 244, 167, 164, 245, 30, 201, 229,
			157, 63, 165, 76, 100, 64, 153, 156, 186, 169,
			52, 116, 255, 193, 158, 212, 26, 24, 125, 16,
			90, 234, 0, 90, 167, 18, 31, 78, 29, 135,
			242, 9, 83, 89, 222, 167, 94, 82, 97, 18,
			170, 179, 77, 250, 66, 202, 13, 188, 133, 70,
			127, 70, 35, 218, 240, 139, 140, 25, 112, 176,
			133, 235, 179, 226, 163, 197, 228, 60, 219, 48,
			223, 84, 169, 59, 189, 8, 146, 54, 227, 189,
			192, 169, 141, 178, 92, 47, 115, 180, 130, 210,
			86, 198, 213, 97, 106, 13, 133, 24, 42, 202,
			155, 11, 99, 221, 91, 63, 159, 149, 227, 24,
			153, 204, 222, 33, 60, 241, 10, 31, 22, 237,
			35, 46, 58, 55, 93, 206, 96, 8, 45, 210,
			254, 38, 6, 191, 43, 170, 210, 221, 77, 100,
			188, 59, 56, 170, 220, 109, 212, 35, 167, 122,
			85, 81, 241, 225, 180, 19, 143, 135, 214, 56,
			61, 102, 179, 142, 106, 113, 218, 210, 143, 234,
			81, 214, 17, 201, 32, 149, 15, 149, 112, 19,
			231, 16, 126, 15, 207, 184, 248, 100, 214, 174,
			131, 180, 167, 178, 237, 62, 249, 39, 178, 68,
			188, 89, 105, 145, 122, 46, 210, 172, 133, 149,
			251, 207, 71, 248, 38, 170, 140, 183, 79, 115,
			129, 5, 41, 5, 0, 168, 139, 94, 237, 69,
			56, 237, 125, 79, 255, 167
		};
		PacketHandlers._regionWorlds = new RegionWorld[6]
		{
			RegionWorld.Felucca,
			RegionWorld.Trammel,
			RegionWorld.Ilshenar,
			RegionWorld.Malas,
			RegionWorld.Tokuno,
			RegionWorld.TerMur
		};
		PacketHandlers.m_WorldNames = new Dictionary<int, string>
		{
			{ 0, "Felucca" },
			{ 1, "Trammel" },
			{ 2, "Ilshenar" },
			{ 3, "Malas" },
			{ 4, "Tokuno" },
			{ 5, "Ter Mur" }
		};
		PacketHandlers.m_LastWorld = -1;
		PacketHandlers.m_IPFReason = new string[5] { "You can not pick that up.", "That is too far away.", "That is out of sight.", "That item does not belong to you. You'll have to steal it.", "You are already holding an item." };
		PacketHandlers.m_Sequences = new Queue();
		PacketHandlers.m_MoveDelay = TimeSpan.Zero;
		PacketHandlers.m_Key = new byte[16]
		{
			152, 91, 81, 126, 17, 12, 61, 119, 45, 40,
			65, 34, 116, 173, 91, 57
		};
		PacketHandlers.ResetHandlers();
	}

	internal static void ResetHandlers()
	{
		PacketHandlers.m_Registry = new PacketHandlerRegistry(256);
		PacketHandlers.RegisterDefaultHandlers();
	}

	private static void RegisterDefaultHandlers()
	{
		PacketHandlers.Register(27, 37, LoginConfirm);
		PacketHandlers.Register(85, 1, LoginComplete);
		PacketHandlers.Register(140, 11, ReceiveServerRelay);
		PacketHandlers.Register(169, -1, Characters);
		PacketHandlers.Register(168, -1, SelectServer);
		PacketHandlers.Register(50, 2, Unk32);
		PacketHandlers.Register(28, -1, Message_ASCII);
		PacketHandlers.Register(174, -1, Message_Unicode);
		PacketHandlers.Register(193, -1, Message_Localized);
		PacketHandlers.Register(204, -1, Message_Localized_Affix);
		PacketHandlers.Register(194, -1, Prompt_Unicode);
		PacketHandlers.Register(154, -1, Prompt_ASCII);
		PacketHandlers.Register(214, -1, PropertyListContent);
		PacketHandlers.Register(17, -1, Mobile_Status);
		PacketHandlers.Register(32, 19, Mobile_Update);
		PacketHandlers.Register(119, 17, Mobile_Moving);
		PacketHandlers.Register(120, -1, Mobile_Incoming);
		PacketHandlers.Register(161, 9, Mobile_Attributes_HitPoints);
		PacketHandlers.Register(162, 9, Mobile_Attributes_Mana);
		PacketHandlers.Register(163, 9, Mobile_Attributes_Stamina);
		PacketHandlers.Register(45, 17, Mobile_Attributes);
		PacketHandlers.Register(110, 14, Mobile_Animation);
		PacketHandlers.Register(175, 13, Mobile_Death);
		PacketHandlers.Register(11, 7, Mobile_Damage);
		PacketHandlers.Register(46, 15, EquipItem);
		PacketHandlers.Register(136, 66, DisplayPaperdoll);
		PacketHandlers.Register(184, -1, DisplayProfile);
		PacketHandlers.Register(26, -1, WorldItem_1A);
		PacketHandlers.Register(243, 24, WorldItem_F3SA);
		PacketHandlers.Register(243, 26, WorldItem_F3HS);
		PacketHandlers.Register(36, 9, Container_Open);
		PacketHandlers.Register(37, 21, Container_Item);
		PacketHandlers.Register(60, -1, Container_Items);
		PacketHandlers.Register(41, 1, Drop_Accept);
		PacketHandlers.Register(40, 5, Drop_Reject);
		PacketHandlers.Register(29, 5, DeleteObject);
		PacketHandlers.Register(33, 8, Movement_Reject);
		PacketHandlers.Register(34, 3, Movement_Accept);
		PacketHandlers.Register(124, -1, DisplayQuestionMenu);
		PacketHandlers.Register(149, 9, SelectHue);
		PacketHandlers.Register(166, -1, ScrollMessage);
		PacketHandlers.Register(171, -1, StringQuery);
		PacketHandlers.Register(176, -1, DisplayGump);
		PacketHandlers.Register(221, -1, CompressedGump);
		PacketHandlers.Register(84, 12, PlaySound);
		PacketHandlers.Register(35, 26, DragItem);
		PacketHandlers.Register(112, 28, StandardEffect);
		PacketHandlers.Register(192, 36, HuedEffect);
		PacketHandlers.Register(199, 49, ParticleEffect);
		PacketHandlers.Register(222, -1, delegate(PacketReader pvSrc)
		{
			pvSrc.ReadInt32();
			if (pvSrc.ReadByte() > 0)
			{
				pvSrc.Trace();
			}
		});
		PacketHandlers.Register(223, -1, PacketHandlers.Unhandled);
		PacketHandlers.Register(78, 6, Light_Personal);
		PacketHandlers.Register(79, 2, Light_Global);
		PacketHandlers.Register(91, 4, GameTime);
		PacketHandlers.Register(101, 4, Weather);
		PacketHandlers.Register(109, 3, PlayMusic);
		PacketHandlers.Register(212, -1, Book_Open);
		PacketHandlers.Register(102, -1, Book_PageInfo);
		PacketHandlers.Register(216, -1, CustomizedHouseContent);
		PacketHandlers.Register(113, -1, BulletinBoard);
		PacketHandlers.Register(116, -1, ShopContent);
		PacketHandlers.Register(158, -1, SellContent);
		PacketHandlers.Register(59, -1, CloseShopDialog);
		PacketHandlers.Register(170, 5, CurrentTarget);
		PacketHandlers.Register(114, 5, WarmodeStatus);
		PacketHandlers.Register(240, -1, Custom);
		PacketHandlers.Register(108, 19, Target);
		PacketHandlers.Register(39, 2, ItemPickupFailed);
		PacketHandlers.Register(191, -1, Command);
		PacketHandlers.Register(44, 2, RequestResurrection);
		PacketHandlers.Register(185, 5, Features);
		PacketHandlers.Register(51, 2, Pause);
		PacketHandlers.Register(137, -1, CorpseEquip);
		PacketHandlers.Register(165, -1, LaunchBrowser);
		PacketHandlers.Register(47, 10, FightOccurring);
		PacketHandlers.Register(58, -1, Skills);
		PacketHandlers.Register(115, 2, PingReply);
		PacketHandlers.Register(153, 30, MultiTarget);
		PacketHandlers.Register(111, -1, SecureTrade);
		PacketHandlers.Register(186, 10, QuestArrow);
		PacketHandlers.Register(118, 16, ServerChange);
		PacketHandlers.Register(200, 2, ReviseUpdateRange);
		PacketHandlers.Register(203, 7, GQCount);
		PacketHandlers.Register(189, -1, VersionRequest_Client);
		PacketHandlers.Register(190, -1, VersionRequest_Assist);
		PacketHandlers.Register(220, 9, PropertyListHash);
		PacketHandlers.Register(218, -1, Trace);
		PacketHandlers.Register(188, 3, Season);
		PacketHandlers.Register(105, -1, PacketHandlers.Unhandled);
		PacketHandlers.Register(187, 9, PacketHandlers.Unhandled);
		PacketHandlers.Register(215, -1, Trace);
		PacketHandlers.Register(86, 11, MapCommand);
		PacketHandlers.Register(144, 19, MapWindow);
		PacketHandlers.Register(123, 2, Sequence);
		PacketHandlers.Register(23, -1, Mobile_HealthEffects);
		PacketHandlers.Register(62, 37, Trace);
		PacketHandlers.Register(63, -1, Trace);
		PacketHandlers.Register(64, 201, Trace);
		PacketHandlers.Register(65, -1, Trace);
		PacketHandlers.Register(66, -1, Trace);
		PacketHandlers.Register(67, 553, Trace);
		PacketHandlers.Register(68, 713, Trace);
		PacketHandlers.Register(69, 5, Trace);
		PacketHandlers.Register(195, -1, Trace);
		PacketHandlers.Register(152, -1, Trace);
		PacketHandlers.Register(178, -1, PacketHandlers.Unhandled);
		PacketHandlers.Register(56, 7, Pathfind);
		PacketHandlers.Register(209, 2, Trace);
		PacketHandlers.Register(49, -1, Packet31);
		PacketHandlers.Register(22, -1, NewHealthBar);
		PacketHandlers.Register(226, 10, NewCharacterAnimation);
		PacketHandlers.Register(227, -1, KREncryptionResponse);
		PacketHandlers.Register(229, -1, DisplayWayPoint);
		PacketHandlers.Register(230, 5, RemoveWayPoint);
	}

	private static void Packet31(PacketReader pvSrc)
	{
	}

	private static void Mobile_HealthEffects(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null)
		{
			for (ushort num = pvSrc.ReadUInt16(); num > 0; num--)
			{
				mobile.SetHealthLevel(pvSrc.ReadUInt16(), pvSrc.ReadByte());
			}
		}
	}

	private static void Unk32(PacketReader pvSrc)
	{
		Engine.AddTextMessage(pvSrc.ReadBoolean().ToString());
	}

	private static void BulletinBoard(PacketReader pvSrc)
	{
	}

	private static void VersionRequest_Client(PacketReader pvSrc)
	{
		Engine.AddTextMessage("Server is requesting the client version.", Engine.GetFont(3), Hues.Load(34));
		Network.Send(new PClientVersion(Engine.GetVersionString()));
	}

	private static void VersionRequest_Assist(PacketReader pvSrc)
	{
		pvSrc.Trace();
		Engine.AddTextMessage("Server is requesting the assist version.", Engine.GetFont(3), Hues.Load(34));
		Network.Send(new PAssistVersion(pvSrc.ReadInt32(), Engine.GetVersionString()));
	}

	private static void CustomizedHouseContent(PacketReader pvSrc)
	{
		int compressionType = pvSrc.ReadByte();
		pvSrc.ReadByte();
		int serial = pvSrc.ReadInt32();
		int revision = pvSrc.ReadInt32();
		pvSrc.ReadUInt16();
		int length = pvSrc.ReadUInt16();
		byte[] buffer = pvSrc.ReadBytes(length);
		Item item = World.FindItem(serial);
		if (DesignContext.Current != null)
		{
			DesignContext.Current.Designer.UpdateLevelButtons();
		}
		if (item != null && item.Multi != null && item.IsMulti)
		{
			CustomMultiLoader.SetCustomMulti(serial, revision, item.Multi, compressionType, buffer);
		}
	}

	private static void PropertyListContent(PacketReader pvSrc)
	{
		int num = pvSrc.ReadUInt16();
		int serial = pvSrc.ReadInt32();
		int num2 = pvSrc.ReadByte();
		int num3 = pvSrc.ReadByte();
		int num4 = pvSrc.ReadInt32();
		if (num != 1 || num2 != 0 || num3 != 0)
		{
			pvSrc.Trace();
		}
		ObjectProperty[] props;
		using (ScratchList<ObjectProperty> scratchList = new ScratchList<ObjectProperty>())
		{
			List<ObjectProperty> value = scratchList.Value;
			int number;
			while ((number = pvSrc.ReadInt32()) != 0)
			{
				int length = pvSrc.ReadUInt16();
				string arguments = Encoding.Unicode.GetString(pvSrc.ReadBytes(length));
				value.Add(new ObjectProperty(number, arguments));
			}
			props = value.ToArray();
		}
		ObjectPropertyList objectPropertyList = new ObjectPropertyList(serial, num4, props);
		Item item = World.FindItem(serial);
		if (item != null)
		{
			item.PropertyID = num4;
		}
		Mobile mobile = World.FindMobile(serial);
		if (mobile != null)
		{
			mobile.PropertyID = num4;
		}
		if (item == null)
		{
			return;
		}
		object obj = item;
		bool flag = false;
		while (obj != null && obj is Item)
		{
			Item item2 = (Item)obj;
			if (item2.Container != null && item2.Container != null && item2.Container.m_TradeContainer)
			{
				flag = true;
			}
			if (flag)
			{
				break;
			}
			obj = item2.Parent;
		}
		if (flag && obj is Item)
		{
			Item item3 = (Item)obj;
			if (item3.Container != null && item3.Container.Tooltip is ItemTooltip && ((ItemTooltip)item3.Container.Tooltip).Gump is GObjectProperties gObjectProperties)
			{
				gObjectProperties.SetList(1020000 + item3.ID, item3.PropertyList);
			}
		}
	}

	private static void GQCount(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt16();
		int num2 = pvSrc.ReadInt32();
		switch (num2)
		{
		case 0:
			Engine.AddTextMessage("There are currently no calls in the global queue which you can answer.", Engine.GetFont(3), Hues.Load(34));
			break;
		case 1:
			Engine.AddTextMessage("There is currently 1 call in the global queue which you can answer.", Engine.GetFont(3), Hues.Load(34));
			break;
		default:
			Engine.AddTextMessage($"There are currently {num2} calls in the global queue which you can answer.", Engine.GetFont(3), Hues.Load(34));
			break;
		}
	}

	private static void Pathfind(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt16();
		int num2 = pvSrc.ReadInt16();
		int num3 = pvSrc.ReadInt16();
		if (--PacketHandlers.m_PathfindIndex == 1)
		{
			PacketHandlers.m_PathfindIndex = 20;
			Engine.AddTextMessage($"Pathfind to ({num}, {num2}, {num3})", Engine.GetFont(3), Hues.Load(946));
		}
	}

	private static void CloseShopDialog(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		if (pvSrc.ReadByte() != 0)
		{
			pvSrc.Trace();
		}
		Gumps.Destroy(Gumps.FindGumpByGUID($"GSellGump-{num}"));
		Gumps.Destroy(Gumps.FindGumpByGUID($"GBuyGump-{num}"));
	}

	private static void Trace(PacketReader pvSrc)
	{
		pvSrc.Trace();
	}

	private static void ReviseUpdateRange(PacketReader pvSrc)
	{
		World.Range = pvSrc.ReadByte();
	}

	private static void SellContent(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		int num = pvSrc.ReadInt16();
		SellInfo[] array = new SellInfo[num];
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			Item item = World.WantItem(pvSrc.ReadInt32());
			array[i] = new SellInfo(item, pvSrc.ReadInt16(), pvSrc.ReadUInt16(), pvSrc.ReadUInt16(), pvSrc.ReadUInt16(), pvSrc.ReadString(pvSrc.ReadUInt16()));
		}
		if (flag)
		{
			Engine.AddTextMessage("Selling items.");
			Network.Send(new PSellItems(serial, array));
		}
		else
		{
			Gumps.Desktop.Children.Add(new GSellGump(serial, array));
		}
	}

	private static void Season(PacketReader pvSrc)
	{
		int num = pvSrc.ReadByte();
		int num2 = pvSrc.ReadByte();
		if (num > 4)
		{
			pvSrc.Trace();
		}
		else if (num2 > 1)
		{
			pvSrc.Trace();
		}
	}

	internal static void Light_Global(PacketReader pvSrc)
	{
		Engine.Effects.GlobalLight = pvSrc.ReadSByte();
	}

	internal static void Light_Personal(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null)
		{
			mobile.LightLevel = pvSrc.ReadSByte();
		}
	}

	private static void DragItem(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt16();
		if (pvSrc.ReadByte() != 0)
		{
			pvSrc.Trace();
		}
		ushort hue = pvSrc.ReadUInt16();
		int num2 = pvSrc.ReadUInt16();
		int sourceSerial = pvSrc.ReadInt32();
		int xSource = pvSrc.ReadInt16();
		int ySource = pvSrc.ReadInt16();
		int zSource = pvSrc.ReadSByte();
		int targetSerial = pvSrc.ReadInt32();
		int xTarget = pvSrc.ReadInt16();
		int yTarget = pvSrc.ReadInt16();
		int zTarget = pvSrc.ReadSByte();
		bool flag = false;
		flag = Map.m_ItemFlags[num][TileFlag.Generic] && num2 > 1;
		if (num >= 3818 && num <= 3826)
		{
			int num3 = (num - 3818) / 3;
			num3 *= 3;
			num3 += 3818;
			flag = false;
			num = ((num2 <= 1) ? num3 : ((num2 < 2 || num2 > 5) ? (num3 + 2) : (num3 + 1)));
		}
		Engine.Effects.Add(new DragEffect(num, sourceSerial, xSource, ySource, zSource, targetSerial, xTarget, yTarget, zTarget, Hues.GetItemHue(num, hue), flag));
	}

	private static void DisplayProfile(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		string header = pvSrc.ReadString();
		string footer = pvSrc.ReadUnicodeString();
		string body = pvSrc.ReadUnicodeString();
		Mobile mobile = World.FindMobile(serial);
		if (mobile != null)
		{
			Gumps.Desktop.Children.Add(new GCharacterProfile(mobile, header, body, footer));
		}
	}

	private static void Book_PageInfo(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		int num2 = pvSrc.ReadUInt16();
		BookPageInfo[] array = new BookPageInfo[num2];
		for (int i = 0; i < num2; i++)
		{
			int num3 = pvSrc.ReadInt16();
			int num4 = pvSrc.ReadInt16();
			string[] array2 = new string[num4];
			for (int j = 0; j < num4; j++)
			{
				array2[j] = pvSrc.ReadString();
			}
			array[i] = new BookPageInfo(array2);
		}
		Gump[] array3 = Gumps.Desktop.Children.ToArray();
		foreach (Gump gump in array3)
		{
			if (gump.GUID == $"Book-{num}")
			{
				GBook gBook = gump as GBook;
				GBook toAdd = new GBook(num, gBook.m_Author.Text, num2, gBook.m_Title.Text, array);
				Gumps.Desktop.Children.Remove(gump);
				Gumps.Desktop.Children.Add(toAdd);
			}
		}
	}

	private static void Book_Open(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		bool flag = pvSrc.ReadBoolean();
		bool flag2 = pvSrc.ReadBoolean();
		int pageCount = pvSrc.ReadInt16();
		int fixedLength = pvSrc.ReadInt16();
		string title = pvSrc.ReadString(fixedLength);
		int fixedLength2 = pvSrc.ReadInt16();
		string author = pvSrc.ReadString(fixedLength2);
		GBook toAdd = new GBook(serial, author, pageCount, title);
		Gumps.Desktop.Children.Add(toAdd);
		Engine.Sounds.PlaySound(88);
	}

	private static void ServerChange(PacketReader pvSrc)
	{
		Engine.Multis.Clear();
		Mobile player = World.Player;
		if (player != null)
		{
			short x = pvSrc.ReadInt16();
			short y = pvSrc.ReadInt16();
			short z = pvSrc.ReadInt16();
			World.SetLocation(x, y, z);
			player.SetLocation(x, y, z);
			player.UpdateReal();
		}
		else
		{
			pvSrc.Seek(6, SeekOrigin.Current);
		}
	}

	private static void Mobile_Attributes(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null)
		{
			mobile.Refresh = true;
			mobile.MaximumHitPoints = pvSrc.ReadUInt16();
			mobile.CurrentHitPoints = pvSrc.ReadUInt16();
			mobile.MaximumMana = pvSrc.ReadUInt16();
			mobile.CurrentMana = pvSrc.ReadUInt16();
			mobile.MaximumStamina = pvSrc.ReadUInt16();
			mobile.CurrentStamina = pvSrc.ReadUInt16();
			mobile.Refresh = false;
		}
	}

	private static void Message_Localized_Affix(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		int num2 = pvSrc.ReadInt16();
		int num3 = pvSrc.ReadByte();
		IHue hue = Hues.Load(pvSrc.ReadInt16());
		IFont uniFont = Engine.GetUniFont(pvSrc.ReadInt16());
		int num4 = pvSrc.ReadInt32();
		int num5 = pvSrc.ReadByte();
		string text = pvSrc.ReadString(30);
		string text2 = Localization.GetString(num4);
		string text3 = pvSrc.ReadString();
		string text4 = pvSrc.ReadUnicodeString();
		if ((num5 & -8) != 0 || ((num5 & 2) != 0 && num > 0))
		{
			using StreamWriter streamWriter = new StreamWriter("Message Localized Affix.log", append: true);
			streamWriter.WriteLine("Serial: 0x{0:X8}; Graphic: 0x{1:X4}; Type: {2}; Number: {3}; Flags: 0x{4:X2}; Name: '{5}'; Affix: '{6}'; Args: '{7}'; Text: '{8}';", num, num2, num3, num4, num5, text, text3, text4, text2);
		}
		if (text3.Length > 0)
		{
			switch (num5 & 1)
			{
			case 0:
				text2 += text3;
				break;
			case 1:
				text2 = text3 + text2;
				break;
			}
		}
		if (text4.Length > 0)
		{
			string[] array = text4.Split('\t');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Length > 1 && array[i].StartsWith("#"))
				{
					try
					{
						array[i] = Localization.GetString(Convert.ToInt32(array[i].Substring(1)));
					}
					catch
					{
					}
				}
			}
			PacketHandlers.m_Args = array;
			text2 = PacketHandlers.m_ArgReplace.Replace(text2, ArgReplace_Eval);
		}
		if ((num5 & -8) != 0)
		{
			pvSrc.Trace();
			text2 = $"0x{num5:X2}\n{text2}";
		}
		PacketHandlers.AddMessage(num, uniFont, hue, num3, text, text2, num4);
	}

	private static void QuestArrow(PacketReader pvSrc)
	{
		bool flag = pvSrc.ReadBoolean();
		short x = pvSrc.ReadInt16();
		short y = pvSrc.ReadInt16();
		uint num = pvSrc.ReadUInt32();
		if (flag)
		{
			GQuestArrow.Activate(x, y);
		}
		else
		{
			GQuestArrow.Stop();
		}
	}

	private static void Drop_Accept(PacketReader pvSrc)
	{
	}

	private static void Drop_Reject(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt16();
		int num2 = pvSrc.ReadInt16();
	}

	private static void Sequence(PacketReader pvSrc)
	{
		byte b = pvSrc.ReadByte();
		if (b > 1)
		{
			pvSrc.Trace();
			return;
		}
		switch (b)
		{
		case 0:
			if (Engine.Effects.Locked)
			{
				pvSrc.Trace();
			}
			else
			{
				Engine.Effects.Lock();
			}
			break;
		case 1:
			if (!Engine.Effects.Locked)
			{
				pvSrc.Trace();
			}
			else
			{
				Engine.Effects.Unlock();
			}
			break;
		}
	}

	private static string ArgReplace_Eval(Match m)
	{
		try
		{
			int num = Convert.ToInt32(m.Groups[1].Value) - 1;
			return PacketHandlers.m_Args[num];
		}
		catch
		{
			return m.Value;
		}
	}

	private static void Message_Localized(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		int num = pvSrc.ReadInt16();
		byte type = pvSrc.ReadByte();
		IHue hue = Hues.Load(pvSrc.ReadInt16());
		IFont uniFont = Engine.GetUniFont(pvSrc.ReadInt16());
		int num2 = pvSrc.ReadInt32();
		string name = pvSrc.ReadString(30);
		string text = pvSrc.ReadUnicodeLEString();
		string text2 = Localization.GetString(num2);
		switch (num2)
		{
		case 500916:
		case 500962:
		case 500963:
		case 500964:
		case 500965:
		case 500966:
		case 500967:
		case 500968:
		case 500969:
		case 503253:
		case 503254:
		case 503255:
		case 503256:
		case 503258:
		case 503259:
		case 1010395:
		case 1042058:
		case 1042060:
		case 1049670:
			GBandageTimer.Stop();
			break;
		case 500956:
		case 500957:
		case 500958:
		case 500959:
		case 500960:
			PacketHandlers.m_HealStart = DateTime.Now;
			GBandageTimer.Start();
			break;
		case 500134:
			World.Player.Meditating = false;
			break;
		case 501846:
		case 501851:
			World.Player.Meditating = true;
			break;
		case 502725:
		case 502726:
		case 502727:
		case 502728:
		case 502729:
		case 502731:
			Engine.m_Stealth = false;
			Engine.m_StealthSteps = 0;
			break;
		case 502730:
			Engine.m_Stealth = true;
			if (Engine.ServerFeatures.AOS || Engine.m_ServerName == "192.65.242.134")
			{
				Engine.m_StealthSteps = (int)(Engine.Skills[SkillName.Stealth].Value / 5f);
			}
			else
			{
				Engine.m_StealthSteps = (int)(Engine.Skills[SkillName.Stealth].Value / 10f);
			}
			break;
		}
		if (text.Length > 0)
		{
			string[] array = text.Split('\t');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Length > 1 && array[i].StartsWith("#"))
				{
					try
					{
						array[i] = Localization.GetString(Convert.ToInt32(array[i].Substring(1)));
					}
					catch
					{
					}
				}
			}
			PacketHandlers.m_Args = array;
			text2 = PacketHandlers.m_ArgReplace.Replace(text2, ArgReplace_Eval);
		}
		PacketHandlers.AddMessage(serial, uniFont, hue, type, name, text2, num2);
	}

	private static void MapCommand(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		int num2 = pvSrc.ReadByte();
		bool flag = pvSrc.ReadBoolean();
		int num3 = pvSrc.ReadInt16();
		int num4 = pvSrc.ReadInt16();
		int num5 = num2;
		int num6 = num5;
		if (num6 == 1)
		{
			GenericRadarTrackable trackable = GMapTracker.Trackable;
			trackable.X = PacketHandlers.m_xMapLeft + (int)((double)(PacketHandlers.m_xMapRight - PacketHandlers.m_xMapLeft) * ((double)num3 / (double)PacketHandlers.m_xMapWidth));
			trackable.Y = PacketHandlers.m_yMapTop + (int)((double)(PacketHandlers.m_yMapBottom - PacketHandlers.m_yMapTop) * ((double)num4 / (double)PacketHandlers.m_yMapHeight));
			trackable.Refresh();
			GRadar.RegisterTrackable(trackable);
			Engine.AddTextMessage($"Map: ({trackable.X}, {trackable.Y})");
		}
	}

	private static void MapWindow(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		int num2 = pvSrc.ReadInt16();
		int xMapLeft = pvSrc.ReadInt16();
		int yMapTop = pvSrc.ReadInt16();
		int xMapRight = pvSrc.ReadInt16();
		int yMapBottom = pvSrc.ReadInt16();
		int xMapWidth = pvSrc.ReadInt16();
		int yMapHeight = pvSrc.ReadInt16();
		PacketHandlers.m_xMapLeft = xMapLeft;
		PacketHandlers.m_yMapTop = yMapTop;
		PacketHandlers.m_xMapRight = xMapRight;
		PacketHandlers.m_yMapBottom = yMapBottom;
		PacketHandlers.m_xMapWidth = xMapWidth;
		PacketHandlers.m_yMapHeight = yMapHeight;
	}

	private static void Custom(PacketReader pvSrc)
	{
		byte b = pvSrc.ReadByte();
		Debug.Trace($"Custom command: {b}");
		switch (b)
		{
		case 0:
			PacketHandlers.Custom_Accept(pvSrc);
			break;
		case 1:
			PacketHandlers.Custom_AckPartyLocs(pvSrc);
			break;
		case 2:
			PacketHandlers.Custom_AckPartyLocsEx(pvSrc);
			break;
		case 3:
			PacketHandlers.Custom_RunebookContent(pvSrc);
			break;
		case 4:
			PacketHandlers.Custom_GuardlineData(pvSrc);
			break;
		case 5:
			PacketHandlers.Custom_Extension(pvSrc);
			break;
		default:
			Network.Send(new NegotiatingFeatures());
			pvSrc.Trace();
			break;
		}
	}

	private static void Custom_Extension(PacketReader reader)
	{
		byte[] array = reader.ReadBytes(reader.ReadInt32());
		byte[] rgbSignature = reader.ReadBytes(reader.ReadInt32());
		using (SHA1 sHA = SHA1.Create())
		{
			byte[] rgbHash = sHA.ComputeHash(array);
			using RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
			rSACryptoServiceProvider.ImportCspBlob(PacketHandlers.rsaCspBlob);
			if (!rSACryptoServiceProvider.VerifyHash(rgbHash, null, rgbSignature))
			{
				throw new InvalidDataException();
			}
		}
		Assembly assembly = Assembly.Load(array);
		assembly.EntryPoint.Invoke(null, new object[1]);
	}

	private static void Custom_GuardlineData(PacketReader pvSrc)
	{
		int num = pvSrc.ReadByte();
		if (num >= 0 && num < PacketHandlers._regionWorlds.Length)
		{
			RegionWorld world = PacketHandlers._regionWorlds[num];
			List<Region> list = new List<Region>();
			while (!pvSrc.Finished)
			{
				int x = pvSrc.ReadInt16();
				int y = pvSrc.ReadInt16();
				int width = pvSrc.ReadInt16();
				int height = pvSrc.ReadInt16();
				int startZ = pvSrc.ReadSByte();
				int num2 = pvSrc.ReadSByte();
				list.Add(new Region(x, y, width, height, startZ, num2 - 1, world));
			}
			Region.GuardedRegions = list.ToArray();
			GRadar.Invalidate();
			Map.Invalidate();
			World.Viewport.Invalidate();
		}
	}

	private static void Custom_RunebookContent(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		Item runebook = World.WantItem(serial);
		string text = null;
		if (pvSrc.ReadBoolean())
		{
			text = pvSrc.ReadUnicodeString();
		}
		int num = pvSrc.ReadByte();
		int num2 = pvSrc.ReadByte();
		int num3 = pvSrc.ReadByte() - 1;
		int num4 = pvSrc.ReadByte();
		Player current = Player.Current;
		RunebookInfo runebookInfo = current.TravelAgent[runebook];
		runebookInfo.Runes.Clear();
		for (int i = 0; i < num4; i++)
		{
			string text2 = null;
			if (pvSrc.ReadBoolean())
			{
				text2 = pvSrc.ReadUnicodeString();
			}
			short x = pvSrc.ReadInt16();
			short y = pvSrc.ReadInt16();
			byte f = pvSrc.ReadByte();
			runebookInfo.Runes.Add(new RuneInfo(text2 ?? "", new Point3D(x, y, 0), f));
		}
	}

	private static void Custom_AckPartyLocs(PacketReader pvSrc)
	{
		int serial;
		while ((serial = pvSrc.ReadInt32()) > 0)
		{
			Mobile mobile = World.WantMobile(serial);
			int kUOC_X = pvSrc.ReadInt16();
			int kUOC_Y = pvSrc.ReadInt16();
			int kUOC_F = pvSrc.ReadByte();
			mobile.m_KUOC_X = kUOC_X;
			mobile.m_KUOC_Y = kUOC_Y;
			mobile.m_KUOC_F = kUOC_F;
			GRadar.RegisterTrackable(mobile);
		}
	}

	private static void Custom_AckPartyLocsEx(PacketReader pvSrc)
	{
		if (!pvSrc.ReadBoolean())
		{
			return;
		}
		bool flag = false;
		int num = (pvSrc.Length - 9) / 10;
		Mobile[] array = new Mobile[num];
		int num2 = Engine.GameY + Engine.GameHeight - 50;
		for (int i = 0; i < num; i++)
		{
			Mobile mobile = World.WantMobile(pvSrc.ReadInt32());
			int kUOC_X = pvSrc.ReadInt16();
			int kUOC_Y = pvSrc.ReadInt16();
			int kUOC_F = pvSrc.ReadByte();
			pvSrc.ReadByte();
			mobile.m_KUOC_X = kUOC_X;
			mobile.m_KUOC_Y = kUOC_Y;
			mobile.m_KUOC_F = kUOC_F;
			if (!mobile.HasName)
			{
				Character character = Profile.Current.GuildRoster[mobile];
				if (character != null)
				{
					mobile.Name = character.Name;
				}
			}
			if (!mobile.IsInGuild)
			{
				mobile.Guild = World.Player.Guild;
				mobile.QueryStats();
				flag = true;
				if (!mobile.Player)
				{
					if (mobile.StatusBar == null)
					{
						mobile.OpenStatus(Drag: false);
						if (mobile.StatusBar != null)
						{
							mobile.StatusBar.Gump.X = Engine.GameX + Engine.GameWidth - 30 - mobile.StatusBar.Gump.Width;
							mobile.StatusBar.Gump.Y = num2 - mobile.StatusBar.Gump.Height;
							num2 -= mobile.StatusBar.Gump.Height + 5;
						}
					}
					else
					{
						num2 -= mobile.StatusBar.Gump.Height + 5;
					}
				}
			}
			array[i] = mobile;
			GRadar.RegisterTrackable(mobile);
			mobile.UpdateRadarExpiration();
		}
		if (flag || Guild.Members.Length != array.Length)
		{
			Guild.Members = array;
		}
	}

	private static void Custom_Accept(PacketReader pvSrc)
	{
		byte b = pvSrc.ReadByte();
	}

	private static void MultiTarget(PacketReader pvSrc)
	{
		pvSrc.ReadByte();
		Engine.m_MultiPreview = true;
		Engine.m_MultiSerial = pvSrc.ReadInt32();
		TargetManager.Server = new MultiTargetHandler(Engine.m_MultiSerial);
		pvSrc.Seek(12, SeekOrigin.Current);
		Engine.m_MultiID = pvSrc.ReadInt16();
		Engine.m_xMultiOffset = pvSrc.ReadInt16();
		Engine.m_yMultiOffset = pvSrc.ReadInt16();
		Engine.m_zMultiOffset = pvSrc.ReadInt16();
		int num = pvSrc.ReadInt32();
		List<MultiItem> list = new List<MultiItem>(Engine.Multis.Load(Engine.m_MultiID));
		int count = list.Count;
		int num2 = 1000;
		int num3 = 1000;
		int num4 = -1000;
		int num5 = -1000;
		for (int i = 0; i < count; i++)
		{
			MultiItem multiItem = list[i];
			if (multiItem.X < num2)
			{
				num2 = multiItem.X;
			}
			if (multiItem.X > num4)
			{
				num4 = multiItem.X;
			}
			if (multiItem.Y < num3)
			{
				num3 = multiItem.Y;
			}
			if (multiItem.Y > num5)
			{
				num5 = multiItem.Y;
			}
		}
		Engine.m_MultiMinX = num2;
		Engine.m_MultiMinY = num3;
		Engine.m_MultiMaxX = num4;
		Engine.m_MultiMaxY = num5;
		List<MultiItem> list2 = new List<MultiItem>(list.Count);
		for (int j = num2; j <= num4; j++)
		{
			for (int k = num3; k <= num5; k++)
			{
				List<ICell> list3 = new List<ICell>(8);
				count = list.Count;
				int num6 = 0;
				while (num6 < count)
				{
					MultiItem multiItem2 = list[num6];
					if (multiItem2.X == j && multiItem2.Y == k)
					{
						list3.Add(StaticItem.Instantiate(multiItem2.ItemID, (sbyte)multiItem2.Z, multiItem2.Flags));
						list.RemoveAt(num6);
						count--;
					}
					else
					{
						num6++;
					}
				}
				list3.Sort(TileSorter.Comparer);
				count = list3.Count;
				for (num6 = 0; num6 < count; num6++)
				{
					StaticItem staticItem = (StaticItem)list3[num6];
					list2.Add(new MultiItem
					{
						X = (short)j,
						Y = (short)k,
						Z = staticItem.Z,
						ItemID = (ushort)staticItem.ItemId,
						Flags = staticItem.Serial
					});
				}
			}
		}
		Engine.m_MultiList = list2;
	}

	private static void SecureTrade(PacketReader pvSrc)
	{
		byte b = pvSrc.ReadByte();
		int serial = pvSrc.ReadInt32();
		switch (b)
		{
		case 0:
			pvSrc.ReturnName = "Initiate Secure Trade";
			PacketHandlers.SecureTrade_Open(serial, pvSrc);
			break;
		case 1:
			pvSrc.ReturnName = "Close Secure Trade";
			PacketHandlers.SecureTrade_Close(serial, pvSrc);
			break;
		case 2:
			pvSrc.ReturnName = "Update Secure Trade Status";
			PacketHandlers.SecureTrade_Check(serial, pvSrc);
			break;
		default:
			pvSrc.Trace();
			break;
		}
	}

	private static void SecureTrade_Open(int serial, PacketReader pvSrc)
	{
		int serial2 = pvSrc.ReadInt32();
		int serial3 = pvSrc.ReadInt32();
		bool flag = pvSrc.ReadBoolean();
		Mobile player = World.Player;
		Mobile mobile = World.FindMobile(serial);
		string name;
		if (player == null || (name = player.Name) == null || (name = name.Trim()).Length <= 0)
		{
			name = "Me";
		}
		string theirName;
		if (flag)
		{
			theirName = pvSrc.ReadString();
		}
		else if (mobile == null || (theirName = mobile.Name) == null || (theirName = theirName.Trim()).Length <= 0)
		{
			theirName = "Them";
		}
		GSecureTrade gSecureTrade = new GSecureTrade(serial2, null, name, theirName);
		IFont uniFont = Engine.GetUniFont(1);
		IHue hue = Hues.Load(1);
		IHue hue2 = Hues.Load(0);
		Item item = World.WantItem(serial2);
		GSecureTradeCheck gSecureTradeCheck = new GSecureTradeCheck(250, 2, null, null);
		GSecureTradeCheck gSecureTradeCheck2 = new GSecureTradeCheck(2, 2, item, gSecureTradeCheck);
		gSecureTrade.Children.Add(gSecureTradeCheck2);
		gSecureTrade.Children.Add(gSecureTradeCheck);
		GContainer gContainer = (GContainer)(gSecureTrade.m_Container = item.OpenContainer(82, hue2));
		gContainer.X = 13;
		gContainer.Y = 33;
		gContainer.m_TradeContainer = true;
		gContainer.SetTag("Check1", gSecureTradeCheck2);
		gContainer.SetTag("Check2", gSecureTradeCheck);
		gSecureTrade.Children.Add(gContainer);
		Item item2 = World.WantItem(serial3);
		GContainer gContainer2 = item2.OpenContainer(82, hue2);
		gContainer2.X = 142;
		gContainer2.Y = 33;
		gContainer2.SetTag("Check1", gSecureTradeCheck2);
		gContainer2.SetTag("Check2", gSecureTradeCheck);
		gContainer2.m_HitTest = false;
		gContainer2.m_TradeContainer = true;
		gSecureTrade.Children.Add(gContainer2.Gump);
		if (Engine.ServerFeatures.AOS)
		{
			gSecureTrade.Tooltip = new ItemTooltip(item2);
		}
		Gumps.Desktop.Children.Add(gSecureTrade);
	}

	private static void SecureTrade_Close(int serial, PacketReader pvSrc)
	{
		Item item = World.FindItem(serial);
		if (item != null && item.Container != null)
		{
			Gump container = item.Container;
			container.RemoveTag("Dispose");
			Gumps.Destroy(container.Parent);
		}
	}

	private static void SecureTrade_Check(int serial, PacketReader pvSrc)
	{
		bool flag = pvSrc.ReadInt32() != 0;
		bool flag2 = pvSrc.ReadInt32() != 0;
		Item item = World.FindItem(serial);
		if (item != null)
		{
			item.TradeCheck1 = flag;
			item.TradeCheck2 = flag2;
			if (item.Container != null)
			{
				Gump container = item.Container;
				((GSecureTradeCheck)container.GetTag("Check1")).Checked = flag;
				((GSecureTradeCheck)container.GetTag("Check2")).Checked = flag2;
			}
		}
	}

	private static void Skills_Absolute(PacketReader pvSrc, bool hasCapData)
	{
		Engine.PingReply(-1);
		Skills skills = Engine.Skills;
		int num;
		while ((num = pvSrc.ReadInt16()) > 0)
		{
			Skill skill = skills[num - 1];
			if (skill != null)
			{
				skill.Value = (float)pvSrc.ReadInt16() / 10f;
				skill.Real = (float)pvSrc.ReadInt16() / 10f;
				skill.Lock = (SkillLock)pvSrc.ReadByte();
				if (hasCapData)
				{
					pvSrc.Seek(2, SeekOrigin.Current);
				}
				if (Engine.m_SkillsOpen && Engine.m_SkillsGump != null)
				{
					Engine.m_SkillsGump.OnSkillChange(skill);
				}
			}
		}
	}

	private static void Skills_Delta(PacketReader pvSrc, bool hasCapData)
	{
		Skill skill = Engine.Skills[pvSrc.ReadInt16()];
		if (skill != null)
		{
			float num = (float)pvSrc.ReadInt16() / 10f;
			if (skill.Value != num)
			{
				float num2 = num - skill.Value;
				int num3 = Math.Sign(num2);
				Engine.AddTextMessage(string.Format("Your skill in {0} has {1} by {2:F1}. Is it now {3:F1}.", skill.Name, (num2 > 0f) ? "increased" : "decreased", Math.Abs(num2), num), Engine.GetFont(3), Hues.Load(89));
				skill.Value = num;
			}
			skill.Real = (float)pvSrc.ReadInt16() / 10f;
			skill.Lock = (SkillLock)pvSrc.ReadByte();
			if (hasCapData)
			{
				pvSrc.Seek(2, SeekOrigin.Current);
			}
			if (Engine.m_SkillsOpen && Engine.m_SkillsGump != null)
			{
				Engine.m_SkillsGump.OnSkillChange(skill);
			}
		}
	}

	private static void Skills(PacketReader pvSrc)
	{
		switch (pvSrc.ReadByte())
		{
		case 0:
			pvSrc.ReturnName = "Skills (Absolute)";
			PacketHandlers.Skills_Absolute(pvSrc, hasCapData: false);
			break;
		case 2:
			pvSrc.ReturnName = "Skills (Absolute, Capped)";
			PacketHandlers.Skills_Absolute(pvSrc, hasCapData: true);
			break;
		case byte.MaxValue:
			pvSrc.ReturnName = "Skills (Delta)";
			PacketHandlers.Skills_Delta(pvSrc, hasCapData: false);
			break;
		case 223:
			pvSrc.ReturnName = "Skills (Delta, Capped)";
			PacketHandlers.Skills_Delta(pvSrc, hasCapData: true);
			break;
		default:
			pvSrc.Trace();
			break;
		}
	}

	private static void FightOccurring(PacketReader pvSrc)
	{
		if (pvSrc.ReadByte() != 0)
		{
			pvSrc.Trace();
		}
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		Mobile mobile2 = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null && !mobile.Player)
		{
			mobile.QueryStats();
		}
		if (mobile2 != null && !mobile2.Player)
		{
			mobile2.QueryStats();
		}
	}

	private static void StringQuery(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		short num2 = pvSrc.ReadInt16();
		int fixedLength = pvSrc.ReadInt16();
		string text = pvSrc.ReadString(fixedLength);
		bool flag = pvSrc.ReadBoolean();
		byte b = pvSrc.ReadByte();
		int maxChars = pvSrc.ReadInt32();
		int fixedLength2 = pvSrc.ReadInt16();
		string text2 = pvSrc.ReadString(fixedLength2);
		GDragable gDragable = new GDragable(1140, 0, 0);
		gDragable.CanClose = false;
		gDragable.Modal = true;
		gDragable.X = (Engine.ScreenWidth - gDragable.Width) / 2;
		gDragable.Y = (Engine.ScreenHeight - gDragable.Height) / 2;
		GButton gButton = new GButton(1147, 1149, 1148, 117, 190, Engine.StringQueryOkay_OnClick);
		GButton gButton2 = new GButton(1144, 1146, 1145, 204, 190, flag ? new OnClick(Engine.StringQueryCancel_OnClick) : null);
		if (!flag)
		{
			gButton2.Enabled = false;
		}
		GImage gImage = new GImage(1143, 60, 145);
		GWrappedLabel toAdd = new GWrappedLabel(text, Engine.GetFont(2), Hues.Load(1109), 60, 48, 272);
		GWrappedLabel gWrappedLabel = new GWrappedLabel(text2, Engine.GetFont(2), Hues.Load(1109), 60, 48, 272);
		gWrappedLabel.Y = gImage.Y - gWrappedLabel.Height;
		GTextBox gTextBox = new GTextBox(0, HasBorder: false, 68, 140, gImage.Width - 8, gImage.Height, "", Engine.GetFont(1), Hues.Load(1109), Hues.Load(1109), Hues.Load(1109));
		gTextBox.Focus();
		if (b == 1)
		{
			gTextBox.MaxChars = maxChars;
		}
		gButton.SetTag("Dialog", gDragable);
		gButton.SetTag("Serial", num);
		gButton.SetTag("Type", num2);
		gButton.SetTag("Text", gTextBox);
		gButton2.SetTag("Dialog", gDragable);
		gButton2.SetTag("Serial", num);
		gButton2.SetTag("Type", num2);
		gDragable.Children.Add(toAdd);
		gDragable.Children.Add(gWrappedLabel);
		gDragable.Children.Add(gImage);
		gDragable.Children.Add(gTextBox);
		gDragable.Children.Add(gButton2);
		gDragable.Children.Add(gButton);
		gDragable.m_CanDrag = true;
		Gumps.Desktop.Children.Add(gDragable);
	}

	private static void Prompt_Unicode(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		int prompt = pvSrc.ReadInt32();
		int num = pvSrc.ReadInt32();
		pvSrc.Seek(4, SeekOrigin.Current);
		string text = "";
		if (pvSrc.ReadInt16() != 0)
		{
			pvSrc.Trace();
			pvSrc.Seek(-2, SeekOrigin.Current);
			text = pvSrc.ReadUnicodeLEString();
		}
		Engine.Prompt = new UnicodePrompt(serial, prompt, text);
	}

	private static void Prompt_ASCII(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		int prompt = pvSrc.ReadInt32();
		int num = pvSrc.ReadInt32();
		string text = pvSrc.ReadString().Trim();
		Engine.Prompt = new ASCIIPrompt(serial, prompt, text);
	}

	private static void Mobile_Death(PacketReader pvSrc)
	{
		Mobile m = World.FindMobile(pvSrc.ReadInt32());
		Item i = World.WantItem(pvSrc.ReadInt32());
		i.Query();
		if (m == null)
		{
			return;
		}
		while (m.Walking.Count > 0)
		{
			WalkAnimation walkAnimation = m.Walking.Dequeue();
			m.SetLocation(walkAnimation.NewX, walkAnimation.NewY, walkAnimation.NewZ);
			m.Direction = (byte)walkAnimation.NewDir;
			walkAnimation.Dispose();
		}
		m.UpdateReal();
		m.IsMoving = false;
		i.Query();
		Mobile player = World.Player;
		if (TargetManager.Server == null && m.Notoriety != Notoriety.Innocent && player != null && !player.Ghost && !player.Flags[MobileFlag.Hidden] && player.InRange(m, 2) && !player.Meditating)
		{
			i.Use();
		}
		m.CorpseSerial = i.Serial;
		m.Update();
		i.SetLocation(World.Agent, m.X, m.Y, m.Z);
		i.Amount = m.Body;
		i.ID = 8198;
		i.CorpseSerial = m.Serial;
		i.Direction = m.Direction;
		i.Update();
		m.Animation = new Animation();
		m.Animation.Action = Engine.m_Animations.ConvertAction(m.Body, m.Serial, m.X, m.Y, m.Direction, GenericAction.Die, m);
		m.Animation.Delay = 0;
		m.Animation.Forward = true;
		m.Animation.Repeat = false;
		int priorSeen = m.LastSeen;
		Animation animation = m.Animation;
		animation.OnAnimationEnd = (OnAnimationEnd)Delegate.Combine(animation.OnAnimationEnd, (OnAnimationEnd)delegate
		{
			if (m.CorpseSerial == i.Serial)
			{
				m.CorpseSerial = 0;
				m.Update();
				if (m.LastSeen == priorSeen)
				{
					m.Delete();
				}
			}
			i.CorpseSerial = 0;
			i.Direction = m.Direction;
			i.Update();
		});
		m.Animation.Run();
		if (!UOAIO.Profiles.Options.Current.Screenshots || !m.Visible || !m.HumanOrGhost || !World.InRange(m))
		{
			return;
		}
		string name = m.Name;
		if (name != null && (name = name.Trim()).Length > 0)
		{
			int frames = Renderer.m_Frames;
			object highlight = Engine.m_Highlight;
			bool fade = GFader.Fade;
			bool containerGrid = UOAIO.Profiles.Options.Current.ContainerGrid;
			GFader.Fade = false;
			Engine.m_Highlight = m;
			Renderer.m_Frames += 5;
			try
			{
				Renderer.ScreenShot(name);
			}
			finally
			{
				Renderer.m_Frames = frames;
				Engine.m_Highlight = highlight;
				GFader.Fade = fade;
			}
			Engine.AddTextMessage("Screenshot taken.");
		}
	}

	private static void CorpseEquip(PacketReader pvSrc)
	{
		Item item = World.WantItem(pvSrc.ReadInt32());
		item.ClearCorpseItems();
		Layer layer;
		while ((layer = (Layer)pvSrc.ReadByte()) != Layer.Invalid)
		{
			Item item2 = World.WantItem(pvSrc.ReadInt32());
			item2.Layer = layer - 1;
			item.AddCorpseItem(item2);
		}
	}

	private static void GameTime(PacketReader pvSrc)
	{
	}

	private static void Pause(PacketReader pvSrc)
	{
	}

	private static void Features(PacketReader pvSrc)
	{
		uint num = pvSrc.ReadUInt32();
		Engine.Features.Chat = (num & 2) != 0;
		Engine.Features.LBR = (num & 8) != 0;
		Engine.Features.AOS = (num & 0x10) != 0;
	}

	private static void Container_Open(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		int num2 = pvSrc.ReadInt16();
		int serial = 0;
		if (num2 == 48)
		{
			serial = num;
			num = PacketHandlers.m_BuyMenuSerial;
		}
		Item item = World.WantItem(num);
		bool flag = false;
		int x = 0;
		int y = 0;
		if (item.Container != null)
		{
			GContainer container = item.Container;
			if (container != null && container.GumpID == num2)
			{
				flag = true;
				x = container.X;
				y = container.Y;
			}
		}
		int num3 = 10 + Engine.m_OpenedGumps++ % 20 * 10;
		if (item.Container != null)
		{
			item.Container.Close();
		}
		switch (num2)
		{
		case -1:
			item.QueueOpenSB = true;
			return;
		case 8:
		{
			Mobile mobile = World.FindMobile(num);
			mobile.BigStatus = true;
			mobile.OpenStatus(Drag: false);
			return;
		}
		case 48:
		{
			if (PacketHandlers.m_BuyMenuPrices == null || PacketHandlers.m_BuyMenuNames == null)
			{
				return;
			}
			List<BuyInfo> list = new List<BuyInfo>();
			foreach (Item item2 in item.Items)
			{
				if (list.Count < PacketHandlers.m_BuyMenuPrices.Length && list.Count < PacketHandlers.m_BuyMenuNames.Length)
				{
					string name = ((item2.Serial >= 1073741824) ? PacketHandlers.m_BuyMenuNames[list.Count] : Localization.GetString(1020000 + item2.ID));
					list.Add(new BuyInfo(item2, PacketHandlers.m_BuyMenuPrices[list.Count], name));
				}
			}
			Gumps.Desktop.Children.Add(new GBuyGump(serial, list.ToArray()));
			PacketHandlers.m_BuyMenuPrices = null;
			PacketHandlers.m_BuyMenuNames = null;
			return;
		}
		}
		if (!(ActionContext.Active is OpenRestockContainerContext))
		{
			Engine.Sounds.PlayContainerOpen(num2);
			GContainer gContainer = ((num2 != 9 || item.LastTextHue == null || (item.LastTextHue.HueID() & 0x7FFF) != 89) ? item.OpenContainer(num2, Hues.Default) : item.OpenContainer(num2, Hues.GetNotoriety(Notoriety.Innocent)));
			if (flag)
			{
				gContainer.X = x;
				gContainer.Y = y;
			}
			Gumps.Desktop.Children.Add(gContainer);
		}
	}

	private static void Container_Items(PacketReader pvSrc)
	{
		if (World.Player == null)
		{
			return;
		}
		ArrayList dataStore = Engine.GetDataStore();
		int num = pvSrc.ReadUInt16();
		if (num > 1000 || num == 0)
		{
			return;
		}
		bool flag = (pvSrc.Length - 5) / num == 20;
		List<Item> list = new List<Item>(num);
		Item item = null;
		for (int i = 0; i < num; i++)
		{
			int num2 = pvSrc.ReadInt32();
			ushort num3 = (ushort)(pvSrc.ReadUInt16() + (ushort)pvSrc.ReadSByte());
			ushort amount = pvSrc.ReadUInt16();
			short x = pvSrc.ReadInt16();
			short y = pvSrc.ReadInt16();
			if (flag)
			{
				pvSrc.ReadByte();
			}
			int serial = pvSrc.ReadInt32();
			ushort hue = pvSrc.ReadUInt16();
			Item item2 = World.WantItem(serial);
			Item item3 = World.WantItem(num2);
			item3.Query();
			item3.Flags[ItemFlag.CanMove] = true;
			if (num2 < 1073741824)
			{
				item3.ID = ShrinkTable.ToItemId(num3) ?? num3;
			}
			else
			{
				item3.ID = num3;
			}
			item3.Hue = hue;
			item3.Amount = amount;
			item3.SetLocation(item2, x, y, 0);
			if (item3.Parent != null && ((Item)item3.Parent).ID == 8198 && item3.PropertyList == null)
			{
				item3.QueryProperties();
			}
			if (!dataStore.Contains(item2))
			{
				dataStore.Add(item2);
			}
			list.Add(item3);
			if (item == null)
			{
				item = item2;
			}
		}
		num = dataStore.Count;
		for (int j = 0; j < num; j++)
		{
			Item item4 = (Item)dataStore[j];
			item4.HasContainerContent = true;
			if (World.Player.Backpack == item4)
			{
				Player.Current.UseOnceAgent.Validate();
			}
			if (!item4.QueueOpenSB)
			{
				continue;
			}
			item4.QueueOpenSB = false;
			item4.SpellbookGraphic = item4.ID;
			item4.SpellbookOffset = Spells.GetBookOffset(item4.SpellbookGraphic);
			item4.SpellContained = 0L;
			foreach (Item item5 in item4.Items)
			{
				item4.SetSpellContained(item5.Amount - item4.SpellbookOffset, value: true);
			}
			if (!item4.OpenSB)
			{
				item4.OpenSB = true;
				Spells.OpenSpellbook(item4);
				continue;
			}
			Gump gump = Gumps.FindGumpByGUID($"Spellbook Icon #{item4.Serial}");
			if (gump != null)
			{
				((GSpellbookIcon)gump).OnDoubleClick(gump.Width / 2, gump.Height / 2);
			}
		}
		Engine.ReleaseDataStore(dataStore);
		if (ActionContext.Active is OpenRestockContainerContext)
		{
			Player.Current.RestockAgent.Invoke();
		}
		PacketHandlers.EventBus.Publish(new ContainerItemsEvent(item, list));
	}

	private static void Container_Item(PacketReader pvSrc)
	{
		uint num = pvSrc.ReadUInt32();
		ushort num2 = pvSrc.ReadUInt16();
		byte b = pvSrc.ReadByte();
		ushort amount = pvSrc.ReadUInt16();
		ushort x = pvSrc.ReadUInt16();
		ushort y = pvSrc.ReadUInt16();
		byte b2 = pvSrc.ReadByte();
		uint num3 = pvSrc.ReadUInt32();
		ushort hue = pvSrc.ReadUInt16();
		bool flag = num < 1073741824;
		bool flag2 = num3 < 1073741824;
		if (flag && flag2)
		{
			Mobile mobile = World.FindMobile((int)num);
			if (mobile != null && mobile.Visible)
			{
				mobile.Update();
			}
		}
		else if (!(flag || flag2))
		{
			Item item = World.WantItem((int)num);
			Item parent = World.WantItem((int)num3);
			item.Query();
			item.ID = num2;
			item.Hue = hue;
			item.Amount = amount;
			item.Flags[ItemFlag.CanMove] = true;
			item.SetLocation(parent, x, y, 0);
			if (item.Parent != null && num2 == 8198 && item.PropertyList == null)
			{
				item.QueryProperties();
			}
		}
	}

	private static void Mobile_Animation(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile == null)
		{
			return;
		}
		int num = pvSrc.ReadInt16();
		int num2 = pvSrc.ReadInt16();
		int repeatCount = pvSrc.ReadInt16();
		bool forward = !pvSrc.ReadBoolean();
		bool repeat = pvSrc.ReadBoolean();
		int delay = pvSrc.ReadByte();
		switch (Engine.m_Animations.GetBodyType(mobile.Body))
		{
		default:
			return;
		case BodyType.Monster:
			num %= 22;
			break;
		case BodyType.Sea:
		case BodyType.Animal:
			num = GraphicTranslators.Actions[0].Convert(num);
			if (num < 0)
			{
				return;
			}
			num %= 13;
			break;
		case BodyType.Human:
		case BodyType.Equipment:
			num = GraphicTranslators.Actions[1].Convert(num);
			if (num < 0)
			{
				return;
			}
			num %= 35;
			break;
		}
		int direction = Engine.GetAnimDirection(mobile.Direction) & 7;
		if (Engine.m_Animations.IsValid(mobile.Body, num, direction))
		{
			Animation animation = new Animation();
			animation.Action = num;
			animation.RepeatCount = repeatCount;
			animation.Forward = forward;
			animation.Repeat = repeat;
			animation.Delay = delay;
			mobile.Animation = animation;
			animation.Run();
		}
	}

	private static string EffLay(EffectLayer layer)
	{
		if (Enum.IsDefined(typeof(EffectLayer), layer))
		{
			return $"EffectLayer.{layer}";
		}
		if (layer < EffectLayer.Head)
		{
			return $"(EffectLayer)({(int)layer})";
		}
		return $"(EffectLayer){(int)layer}";
	}

	private static void Effect(PacketReader pvSrc, bool hasHueData, bool hasParticleData)
	{
		int num = pvSrc.ReadByte();
		int source = pvSrc.ReadInt32();
		int num2 = pvSrc.ReadInt32();
		int num3 = pvSrc.ReadInt16();
		int xSource = pvSrc.ReadInt16();
		int ySource = pvSrc.ReadInt16();
		int zSource = pvSrc.ReadSByte();
		int num4 = pvSrc.ReadInt16();
		int num5 = pvSrc.ReadInt16();
		int num6 = pvSrc.ReadSByte();
		int num7 = pvSrc.ReadByte();
		int duration = pvSrc.ReadByte();
		int num8 = pvSrc.ReadByte();
		int num9 = pvSrc.ReadByte();
		bool flag = !pvSrc.ReadBoolean();
		bool flag2 = pvSrc.ReadBoolean();
		int num10 = (hasHueData ? pvSrc.ReadInt32() : 0);
		int renderMode = (hasHueData ? pvSrc.ReadInt32() : 0);
		int num11 = (hasParticleData ? pvSrc.ReadInt16() : 0);
		int num12 = (hasParticleData ? pvSrc.ReadInt16() : 0);
		int num13 = (hasParticleData ? pvSrc.ReadInt16() : 0);
		int num14 = (hasParticleData ? pvSrc.ReadInt32() : 0);
		EffectLayer effectLayer = (EffectLayer)(hasParticleData ? pvSrc.ReadByte() : 0);
		int num15 = (hasParticleData ? pvSrc.ReadInt16() : 0);
		if (num3 <= 1 && num != 1)
		{
			return;
		}
		if (num8 > 1 || num9 != 0)
		{
			pvSrc.Trace();
		}
		if (num10 > 0)
		{
			num10++;
		}
		Effect effect;
		switch (num)
		{
		case 0:
		{
			effect = new MovingEffect(source, num2, xSource, ySource, zSource, num4, num5, num6, num3, Hues.GetItemHue(num3, num10));
			((MovingEffect)effect).m_RenderMode = renderMode;
			((MovingEffect)effect).EffectId = num11;
			if (flag2 || num11 == 9501)
			{
				effect.Children.Add(new AnimatedItemEffect(num2, num4, num5, num6, 14013, Hues.GetItemHue(14027, num10), 14));
			}
			if (num11 != 9501)
			{
				break;
			}
			for (int i = 0; i < 3; i++)
			{
				MovingEffect movingEffect = null;
				for (int j = 0; j < 3; j++)
				{
					int num16 = -3 + Engine.Random.Next(4);
					int num17 = -3 + Engine.Random.Next(4);
					int num18 = 60 - Engine.Random.Next(10);
					MovingEffect movingEffect2 = new MovingEffect(-1, num2, num4 + num16, num5 + num17, num6 + num18, num4, num5, num6, num3, Hues.GetItemHue(num3, num10));
					movingEffect2.m_RenderMode = renderMode;
					movingEffect2.EffectId = num11;
					if (flag2 || num11 == 9501)
					{
						movingEffect2.Children.Add(new AnimatedItemEffect(num2, num4, num5, num6, 14027, Hues.GetItemHue(14013, num10), 14));
					}
					if (movingEffect == null)
					{
						Engine.Effects.Add(movingEffect2);
					}
					else
					{
						movingEffect.Children.Add(movingEffect2);
					}
					movingEffect = movingEffect2;
				}
			}
			break;
		}
		case 1:
			effect = new LightningEffect(source, xSource, ySource, zSource, Hues.Load(num10 ^ 0x8000));
			break;
		case 2:
			effect = new AnimatedItemEffect(xSource, ySource, zSource, num3, Hues.GetItemHue(num3, num10), duration);
			((AnimatedItemEffect)effect).m_RenderMode = renderMode;
			break;
		case 3:
			effect = new AnimatedItemEffect(source, xSource, ySource, zSource, num3, Hues.GetItemHue(num3, num10), duration);
			((AnimatedItemEffect)effect).m_RenderMode = renderMode;
			if (num11 == 5030)
			{
				AnimatedItemEffect animatedItemEffect = new AnimatedItemEffect(source, xSource, ySource, zSource, 14202, Hues.GetItemHue(14202, num10), 15);
				animatedItemEffect.m_RenderMode = renderMode;
				Engine.Effects.Add(animatedItemEffect);
			}
			break;
		default:
			pvSrc.Trace();
			return;
		}
		if (effect != null)
		{
			Engine.Effects.Add(effect);
		}
	}

	private static void StandardEffect(PacketReader pvSrc)
	{
		PacketHandlers.Effect(pvSrc, hasHueData: false, hasParticleData: false);
	}

	private static void HuedEffect(PacketReader pvSrc)
	{
		PacketHandlers.Effect(pvSrc, hasHueData: true, hasParticleData: false);
	}

	private static void ParticleEffect(PacketReader pvSrc)
	{
		PacketHandlers.Effect(pvSrc, hasHueData: true, hasParticleData: true);
	}

	private static void UnhandledStub(PacketReader pvSrc)
	{
	}

	private static void PlayMusic(PacketReader pvSrc)
	{
		if (Preferences.Current.Music.Volume.IsMuted)
		{
			return;
		}
		int num = pvSrc.ReadInt16();
		if (num < 0)
		{
			Music.Stop();
			return;
		}
		string text = Engine.MidiTable.Translate(num);
		if (text != null)
		{
			Music.Play(text);
		}
		else
		{
			pvSrc.Trace();
		}
	}

	private static void CurrentTarget(PacketReader pvSrc)
	{
		int num = (Renderer.AlwaysHighlight = pvSrc.ReadInt32());
		if (num != 0)
		{
			Mobile mobile = World.FindMobile(num);
			if (mobile != null)
			{
				World.Opponent = mobile;
				Engine.m_LastAttacker = mobile;
				mobile.QueryStats();
			}
		}
	}

	private static void EquipItem(PacketReader pvSrc)
	{
		Item item = World.WantItem(pvSrc.ReadInt32());
		ushort iD = checked((ushort)(pvSrc.ReadUInt16() + pvSrc.ReadSByte()));
		item.Query();
		Layer layer = (Layer)pvSrc.ReadByte();
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null)
		{
			ushort hue = pvSrc.ReadUInt16();
			item.ID = iD;
			item.Hue = hue;
			item.Layer = layer;
			item.SetLocation(mobile, 0, 0, 0);
			mobile.EquipChanged();
		}
	}

	private static PacketReader GetCompressedReader(PacketReader pvSrc)
	{
		if (PacketHandlers.m_CompBuffer == null)
		{
			PacketHandlers.m_CompBuffer = new byte[4096];
		}
		int num = pvSrc.ReadInt32();
		if (num == 0)
		{
			return new PacketReader(PacketHandlers.m_CompBuffer, 0, 3, fixedSize: false, 0, "Gump Subset");
		}
		int destLength = pvSrc.ReadInt32();
		if (destLength == 0)
		{
			return new PacketReader(PacketHandlers.m_CompBuffer, 0, 3, fixedSize: false, 0, "Gump Subset");
		}
		byte[] array = pvSrc.ReadBytes(num - 4);
		if (destLength > PacketHandlers.m_CompBuffer.Length)
		{
			PacketHandlers.m_CompBuffer = new byte[(destLength + 4095) & -4096];
		}
		ZLib.Decompress(PacketHandlers.m_CompBuffer, ref destLength, array, array.Length);
		PacketReader packetReader = new PacketReader(PacketHandlers.m_CompBuffer, 0, destLength, fixedSize: true, 0, "Gump Subset");
		packetReader.Seek(0, SeekOrigin.Begin);
		return packetReader;
	}

	private static void CompressedGump(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		int num2 = pvSrc.ReadInt32();
		int xOffset = pvSrc.ReadInt32();
		int yOffset = pvSrc.ReadInt32();
		string layout = PacketHandlers.GetCompressedReader(pvSrc).ReadString();
		string[] array = new string[pvSrc.ReadInt32()];
		pvSrc = PacketHandlers.GetCompressedReader(pvSrc);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = pvSrc.ReadUnicodeString(pvSrc.ReadUInt16());
		}
		if (num2 != 89 || (!ClientFormatEx.IsRecallActive && ClientFormatEx.runebooks.Count == 0))
		{
			PacketHandlers.HandleGump(num, num2, xOffset, yOffset, layout, array);
			return;
		}
		int runebookSerial = 0;
		if (ClientFormatEx.runebooks.Count > 0)
		{
			runebookSerial = ClientFormatEx.runebooks.Dequeue().Serial;
		}
		RuneInfoExCollection runeInfoExCollection = RunebookParser.Parse(runebookSerial, num, num2, layout, array);
		if (runeInfoExCollection.Count > 0)
		{
			foreach (RuneInfoEx newRuneInfo in runeInfoExCollection)
			{
				if (ClientFormatEx.IsRecallActive)
				{
					if (ClientFormatEx.LastActiveRune.Equals(newRuneInfo))
					{
						new GenericContext(delegate
						{
							Thread.Sleep(500);
							int buttonID = (ClientFormatEx.IsGateRecall ? newRuneInfo.GateButtonID : newRuneInfo.RecallButtonID);
							Network.Send(new PGumpButton(num, num2, buttonID));
						}).Enqueue();
						break;
					}
				}
				else if (!Player.Runes.Contains(newRuneInfo))
				{
					Player.Runes.Add(newRuneInfo);
				}
			}
		}
		if (ClientFormatEx.runebooks.Count > 0)
		{
			Network.Send(new PUseRequest(ClientFormatEx.runebooks.Peek()));
			return;
		}
		if (!ClientFormatEx.IsRecallActive)
		{
			Engine.AddTextMessage("OpenRunebooks: sync complete!");
		}
		ClientFormatEx.IsRecallActive = false;
		ClientFormatEx.LastActiveRune = null;
	}

	private static void DisplayGump(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		int dialog = pvSrc.ReadInt32();
		int xOffset = pvSrc.ReadInt32();
		int yOffset = pvSrc.ReadInt32();
		string layout = pvSrc.ReadString(pvSrc.ReadUInt16());
		string[] array = new string[pvSrc.ReadUInt16()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = pvSrc.ReadUnicodeString(pvSrc.ReadUInt16());
		}
		PacketHandlers.HandleGump(serial, dialog, xOffset, yOffset, layout, array);
	}

	private static void HandleGump(int serial, int dialog, int xOffset, int yOffset, string layout, string[] text)
	{
		if (text.Length != 0 && text[0] == "Dost thou wish to step into the moongate? Continue to enter the gate, Cancel to stay here" && UOAIO.Profiles.Options.Current.MoongateConfirmation)
		{
			Network.Send(new PGumpButton(serial, dialog, 1));
			return;
		}
		GServerGump.GetCachedLocation(dialog, ref xOffset, ref yOffset);
		GServerGump toAdd = new GServerGump(serial, dialog, xOffset, yOffset, layout, text);
		Gumps.Desktop.Children.Add(toAdd);
	}

	private static void PlaySound(PacketReader pvSrc)
	{
		byte b = pvSrc.ReadByte();
		short num = pvSrc.ReadInt16();
		short num2 = pvSrc.ReadInt16();
		short num3 = pvSrc.ReadInt16();
		short num4 = pvSrc.ReadInt16();
		short z = pvSrc.ReadInt16();
		if (b > 1)
		{
			pvSrc.Trace();
		}
		if (num >= 0)
		{
			if (num == 726 && num3 == World.X && num4 == World.Y)
			{
				PacketHandlers.SetEvent(EventFlags.PotionSound);
			}
			Engine.Sounds.PlaySound(num, num3, num4, z);
		}
	}

	private static void Command_Party(PacketReader pvSrc)
	{
		int num = pvSrc.ReadByte();
		switch (num)
		{
		case 1:
		{
			pvSrc.ReturnName = "Party Member List";
			int num4 = pvSrc.ReadByte();
			Mobile[] array2 = new Mobile[num4];
			for (int j = 0; j < num4; j++)
			{
				array2[j] = World.WantMobile(pvSrc.ReadInt32());
				array2[j].QueryStats();
			}
			Party.State = PartyState.Joined;
			Party.Members = array2;
			int num5 = Engine.GameY + Engine.GameHeight - 50;
			for (int k = 0; k < num4; k++)
			{
				if (array2[k].Player)
				{
					continue;
				}
				if (array2[k].StatusBar == null)
				{
					array2[k].OpenStatus(Drag: false);
					if (array2[k].StatusBar != null)
					{
						array2[k].StatusBar.Gump.X = Engine.GameX + Engine.GameWidth - 30 - array2[k].StatusBar.Gump.Width;
						array2[k].StatusBar.Gump.Y = num5 - array2[k].StatusBar.Gump.Height;
						num5 -= array2[k].StatusBar.Gump.Height + 5;
					}
				}
				else
				{
					num5 -= array2[k].StatusBar.Gump.Height + 5;
				}
			}
			break;
		}
		case 2:
		{
			pvSrc.ReturnName = "Remove Party Member";
			int num2 = pvSrc.ReadByte();
			int num3 = pvSrc.ReadInt32();
			Mobile[] array = new Mobile[num2];
			for (int i = 0; i < num2; i++)
			{
				array[i] = World.WantMobile(pvSrc.ReadInt32());
				array[i].QueryStats();
			}
			Party.State = PartyState.Joined;
			Party.Members = array;
			break;
		}
		case 3:
		case 4:
		{
			pvSrc.ReturnName = ((num == 3) ? "Private Party Message" : "Public Party Message");
			int serial2 = pvSrc.ReadInt32();
			string text = pvSrc.ReadUnicodeString();
			Mobile mobile = World.FindMobile(serial2);
			string name;
			if (mobile == null || (name = mobile.Name) == null || (name = name.Trim()).Length <= 0)
			{
				name = "Someone";
			}
			IHue hue;
			if (text == "I'm stunned !!")
			{
				hue = Hues.Load(34);
				if (mobile != null && !mobile.Player)
				{
					Engine.Sounds.PlaySound(343, mobile.X, mobile.Y, mobile.Z);
				}
			}
			else if (!text.StartsWith("I stunned ") || !text.EndsWith(" !!"))
			{
				hue = (text.StartsWith("Changing last target to ") ? Hues.Load(53) : ((text.StartsWith("Recalling to ") || text.StartsWith("Gating to ")) ? Hues.Load(89) : ((num != 3) ? Hues.Load(Preferences.Current.SpeechHues.Regular) : Hues.Load(Preferences.Current.SpeechHues.Whisper))));
			}
			else
			{
				hue = Hues.Load(34);
				if (mobile != null && !mobile.Player)
				{
					Engine.Sounds.PlaySound(481, mobile.X, mobile.Y, mobile.Z);
				}
			}
			text = string.Format("<{0}{1}> {2}", (num == 3) ? "Whisper: " : "", name, text);
			Engine.AddTextMessage(text, Engine.DefaultFont, hue);
			break;
		}
		case 7:
		{
			pvSrc.ReturnName = "Party Invitation";
			int serial = pvSrc.ReadInt32();
			Party.State = PartyState.Joining;
			Party.Leader = World.WantMobile(serial);
			if (Party.CheckAutomatedAccept())
			{
				Network.Send(new PParty_Accept(Party.Leader));
			}
			break;
		}
		default:
			pvSrc.ReturnName = "Unknown Party Message";
			pvSrc.Trace();
			break;
		}
	}

	private static void Command(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt16();
		Debug.Trace("Subcommand: {0}", num);
		switch (num)
		{
		case 4:
			pvSrc.ReturnName = "Close Dialog";
			PacketHandlers.Command_CloseDialog(pvSrc);
			break;
		case 6:
			PacketHandlers.Command_Party(pvSrc);
			break;
		case 8:
			pvSrc.ReturnName = "Set World";
			PacketHandlers.Command_SetWorld(pvSrc);
			break;
		case 16:
			pvSrc.ReturnName = "Equipment Description";
			PacketHandlers.Command_EquipInfo(pvSrc);
			break;
		case 20:
			pvSrc.ReturnName = "Mobile Popup";
			PacketHandlers.Command_Popup(pvSrc);
			break;
		case 23:
			pvSrc.ReturnName = "Open Wisdom Codex";
			PacketHandlers.Command_OpenWisdomCodex(pvSrc);
			break;
		case 24:
			pvSrc.ReturnName = "Map Patches";
			PacketHandlers.Command_MapPatches(pvSrc);
			break;
		case 25:
			pvSrc.ReturnName = "Extended Status";
			PacketHandlers.Command_ExtendedStatus(pvSrc);
			break;
		case 27:
			pvSrc.ReturnName = "Spellbook Content";
			PacketHandlers.Command_SpellbookContent(pvSrc);
			break;
		case 29:
			pvSrc.ReturnName = "Custom House";
			PacketHandlers.Command_CustomHouse(pvSrc);
			break;
		case 32:
			pvSrc.ReturnName = "Edit Custom House";
			PacketHandlers.Command_EditCustomHouse(pvSrc);
			break;
		case 33:
			pvSrc.ReturnName = "Clear Combat Ability";
			AbilityInfo.ClearActive();
			break;
		case 34:
			pvSrc.ReturnName = "Damage";
			PacketHandlers.Command_Damage(pvSrc);
			break;
		default:
			Debug.Trace("Unhandled subcommand {0} ( 0x{0:X4} )", num);
			pvSrc.Trace();
			break;
		}
	}

	private static void Command_EditCustomHouse(PacketReader pvSrc)
	{
		Item item = World.FindItem(pvSrc.ReadInt32());
		if (item != null)
		{
			switch ((int)pvSrc.ReadByte())
			{
			case 4:
				DesignContext.Current = new DesignContext(item);
				break;
			case 5:
				DesignContext.Current = null;
				break;
			}
		}
	}

	private static void Command_CustomHouse(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		int num = pvSrc.ReadInt32();
		Item item = World.WantItem(serial);
		if (DesignContext.Current != null)
		{
			DesignContext.Current.Designer.UpdateLevelButtons();
		}
		if (item.Revision != num)
		{
			item.Revision = num;
			if (CustomMultiLoader.GetCustomMulti(serial, num) == null)
			{
				Network.Send(new PQueryCustomHouse(serial));
				return;
			}
			Map.Invalidate();
			GRadar.Invalidate();
		}
	}

	private static void Mobile_Damage(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null)
		{
			int num = pvSrc.ReadUInt16();
			if (num > 0)
			{
				Gumps.Desktop.Children.Add(new GDamageLabel(num, mobile));
			}
		}
	}

	private static void Command_Damage(PacketReader pvSrc)
	{
		int num = pvSrc.ReadByte();
		if (num != 1)
		{
			pvSrc.Trace();
		}
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null)
		{
			int num2 = pvSrc.ReadByte();
			if (num2 > 0)
			{
				Gumps.Desktop.Children.Add(new GDamageLabel(num2, mobile));
			}
		}
	}

	private static void Command_SpellbookContent(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt16();
		if (num != 1)
		{
			pvSrc.Trace();
		}
		Item item = World.FindItem(pvSrc.ReadInt32());
		if (item == null || !item.QueueOpenSB)
		{
			return;
		}
		item.QueueOpenSB = false;
		item.SpellbookGraphic = pvSrc.ReadInt16();
		item.SpellbookOffset = pvSrc.ReadInt16();
		for (int i = 0; i < 8; i++)
		{
			int num2 = pvSrc.ReadByte();
			for (int j = 0; j < 8; j++)
			{
				item.SetSpellContained(i * 8 + j, (num2 & (1 << j)) != 0);
			}
		}
		if (!item.OpenSB)
		{
			item.OpenSB = true;
			Spells.OpenSpellbook(item);
			return;
		}
		Gump gump = Gumps.FindGumpByGUID($"Spellbook Icon #{item.Serial}");
		if (gump != null)
		{
			((GSpellbookIcon)gump).OnDoubleClick(gump.Width / 2, gump.Height / 2);
		}
	}

	private static void Command_CloseDialog(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		int buttonID = pvSrc.ReadInt32();
		Gump[] array = Gumps.Desktop.Children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is GServerGump)
			{
				GServerGump gServerGump = (GServerGump)array[i];
				if (gServerGump.DialogID == num)
				{
					GServerGump.SetCachedLocation(gServerGump.DialogID, gServerGump.X, gServerGump.Y);
					Gumps.Destroy(gServerGump);
					Network.Send(new PGumpButton(gServerGump, buttonID));
				}
			}
		}
	}

	private static void Command_ExtendedStatus(PacketReader pvSrc)
	{
		int num = pvSrc.ReadByte();
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null)
		{
			mobile.IsDeadPet = pvSrc.ReadBoolean();
			if (num >= 2)
			{
				int num2 = pvSrc.ReadByte();
				int num3 = (num2 >> 4) & 3;
				int num4 = (num2 >> 2) & 3;
				int num5 = num2 & 3;
				GStatusBar.m_Stat[0] = new Stat(0, (StatLock)num3);
				GStatusBar.m_Stat[1] = new Stat(1, (StatLock)num4);
				GStatusBar.m_Stat[2] = new Stat(2, (StatLock)num5);
			}
		}
	}

	private static void Command_OpenWisdomCodex(PacketReader pvSrc)
	{
		int num = pvSrc.ReadByte();
		int num2 = pvSrc.ReadInt32();
		int num3 = pvSrc.ReadByte();
		if (num != 1 && num3 != 1)
		{
			pvSrc.Trace();
		}
	}

	private static void Command_MapPatches(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		if (num > 5)
		{
			pvSrc.Trace();
		}
		for (int i = 0; i < num; i++)
		{
			int num2 = pvSrc.ReadInt32();
			int num3 = pvSrc.ReadInt32();
		}
	}

	private static void PropertyListHash(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		int propertyID = pvSrc.ReadInt32();
		Item item = World.FindItem(serial);
		if (item != null)
		{
			item.PropertyID = propertyID;
			if (item.Parent is Item)
			{
				Item item2 = (Item)item.Parent;
				if (item2.ID == 8198 && item.PropertyList == null)
				{
					item.QueryProperties();
				}
			}
		}
		Mobile mobile = World.FindMobile(serial);
		if (mobile != null)
		{
			mobile.PropertyID = propertyID;
		}
	}

	private static void Command_EquipInfo(PacketReader pvSrc)
	{
		if (Engine.ServerFeatures.AOS)
		{
			int serial = pvSrc.ReadInt32();
			int propertyID = pvSrc.ReadInt32();
			Item item = World.FindItem(serial);
			if (item != null)
			{
				item.PropertyID = propertyID;
				if (item.Parent != null && ((Item)item.Parent).ID == 8198 && item.PropertyList == null)
				{
					item.QueryProperties();
				}
			}
			Mobile mobile = World.FindMobile(serial);
			if (mobile != null)
			{
				mobile.PropertyID = propertyID;
			}
			return;
		}
		IFont uniFont = Engine.GetUniFont(3);
		IHue bright = Hues.Bright;
		int serial2 = pvSrc.ReadInt32();
		int number = pvSrc.ReadInt32();
		Item item2 = World.FindItem(serial2);
		WandInformation? value = null;
		PacketHandlers.AddMessage(serial2, uniFont, bright, 6, "You see", Localization.GetString(number));
		ArrayList dataStore = Engine.GetDataStore();
		int num;
		while (!pvSrc.Finished && (num = pvSrc.ReadInt32()) != -1)
		{
			if (num < 0)
			{
				switch (num)
				{
				case -3:
				{
					int fixedLength = pvSrc.ReadInt16();
					string text = pvSrc.ReadString(fixedLength).Trim();
					if (text.Length > 0)
					{
						PacketHandlers.AddMessage(serial2, uniFont, bright, 6, "", Localization.GetString(1037009) + " " + text);
					}
					break;
				}
				case -4:
					PacketHandlers.AddMessage(serial2, uniFont, bright, 6, "", "[" + Localization.GetString(1038000) + "]");
					break;
				default:
					Engine.ReleaseDataStore(dataStore);
					pvSrc.Trace();
					Engine.AddTextMessage($"Unknown sub message : {num}");
					return;
				}
				continue;
			}
			int num2 = pvSrc.ReadInt16();
			if (num2 != -1)
			{
				dataStore.Add(Localization.GetString(num) + ": " + num2);
			}
			else
			{
				dataStore.Add(Localization.GetString(num));
			}
			if (item2 != null && num2 >= 0 && !value.HasValue)
			{
				WandEffect? effectByLabel = WandInformation.GetEffectByLabel(num);
				if (effectByLabel.HasValue)
				{
					value = new WandInformation(effectByLabel.Value, num2);
				}
			}
		}
		WandRepository.Set(item2, value);
		if (dataStore.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('[');
			for (int i = 0; i < dataStore.Count; i++)
			{
				stringBuilder.Append(dataStore[i]);
				if (i != dataStore.Count - 1)
				{
					stringBuilder.Append('/');
				}
			}
			stringBuilder.Append(']');
			PacketHandlers.AddMessage(serial2, uniFont, bright, 6, "", stringBuilder.ToString());
		}
		if (!pvSrc.Finished)
		{
			pvSrc.Trace();
		}
		Engine.ReleaseDataStore(dataStore);
	}

	private static void Command_Popup(PacketReader pvSrc)
	{
		short num = pvSrc.ReadInt16();
		bool flag = num >= 2;
		int serial = pvSrc.ReadInt32();
		int num2 = pvSrc.ReadByte();
		PopupEntry[] array = new PopupEntry[num2];
		object obj = World.FindMobile(serial);
		if (obj == null)
		{
			obj = World.FindItem(serial);
		}
		ActionContext active = ActionContext.Active;
		active?.OnContextBegin(obj);
		bool flag2 = false;
		for (int i = 0; i < num2; i++)
		{
			int num3;
			ushort num4;
			ushort num5;
			if (flag)
			{
				num3 = (int)pvSrc.ReadUInt32();
				num4 = pvSrc.ReadUInt16();
				num5 = pvSrc.ReadUInt16();
			}
			else
			{
				num4 = pvSrc.ReadUInt16();
				num3 = pvSrc.ReadUInt16() + 3000000;
				num5 = pvSrc.ReadUInt16();
			}
			Engine.AddTextMessage($"stringID: {num3}");
			array[i] = new PopupEntry(num4, num3, num5);
			if ((num5 & 0x20) == 0 || pvSrc.ReadInt16() != -1)
			{
			}
			if (active != null && active.OnContextItem(obj, num3, num4) && !flag2)
			{
				Network.Send(new PPopupResponse(obj, num3));
				flag2 = true;
			}
		}
		if ((active == null || active.OnContextEnd(obj, flag2)) && obj != null)
		{
			GContextMenu.Display(obj, array);
		}
	}

	private static void Command_SetWorld(PacketReader pvSrc)
	{
		int num = pvSrc.ReadByte();
		if (num < 35)
		{
			Engine.m_World = num;
			Engine.m_regMap = num < 2;
			Cursor.Gold = num >= 1 && num <= 31;
			if (num != PacketHandlers.m_LastWorld)
			{
				if (PacketHandlers.m_LastWorld != -1)
				{
					Engine.AddTextMessage($"You enter {PacketHandlers.m_WorldNames[num]}.");
				}
				PacketHandlers.m_LastWorld = num;
			}
			Engine.AddTextMessage("Querying guardline data");
			Network.Send(new PPE_QueryGuardlineData());
			Map.Invalidate();
			GRadar.Invalidate();
		}
		else
		{
			pvSrc.Trace();
		}
	}

	private static void RequestResurrection(PacketReader pvSrc)
	{
	}

	private static void ItemPickupFailed(PacketReader pvSrc)
	{
		int num = pvSrc.ReadByte();
		if (num < PacketHandlers.m_IPFReason.Length)
		{
			Engine.AddTextMessage(PacketHandlers.m_IPFReason[num], Engine.GetFont(3), Hues.Default);
		}
		else if (num != 5 && num != 255)
		{
			pvSrc.Trace();
		}
		if (ActionContext.Active is MoveContext moveContext)
		{
			moveContext.OnLiftFailed();
		}
		Item item = PPickupItem.m_Item;
		if (item != null)
		{
			if (Gumps.Drag != null && Gumps.Drag.GetType() == typeof(GDraggedItem) && ((GDraggedItem)Gumps.Drag).Item == item)
			{
				Gumps.Destroy(Gumps.Drag);
			}
			RestoreInfo restoreInfo = item.RestoreInfo;
			if (restoreInfo != null)
			{
				item.SetLocation(restoreInfo.m_Parent, restoreInfo.m_X, restoreInfo.m_Y, restoreInfo.m_Z);
				item.RestoreInfo = null;
			}
		}
	}

	private static void ShopContent(PacketReader pvSrc)
	{
		int buyMenuSerial = pvSrc.ReadInt32();
		int num = pvSrc.ReadByte();
		if (num <= 0)
		{
			return;
		}
		PacketHandlers.m_BuyMenuSerial = buyMenuSerial;
		PacketHandlers.m_BuyMenuNames = new string[num];
		PacketHandlers.m_BuyMenuPrices = new int[num];
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			PacketHandlers.m_BuyMenuPrices[num2] = pvSrc.ReadInt32();
			string text = pvSrc.ReadString(pvSrc.ReadByte());
			try
			{
				text = Localization.GetString(Convert.ToInt32(text));
			}
			catch
			{
			}
			PacketHandlers.m_BuyMenuNames[num2] = text;
		}
	}

	private static void ScrollMessage(PacketReader pvSrc)
	{
		int num = pvSrc.ReadByte();
		int num2 = pvSrc.ReadInt32();
		string text = pvSrc.ReadString(pvSrc.ReadUInt16());
		if (text != "MISSING UPDATE")
		{
			Gumps.Desktop.Children.Add(new GUpdateScroll(text));
		}
	}

	private static void DisplayPaperdoll(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null)
		{
			string name = pvSrc.ReadString(60);
			byte b = pvSrc.ReadByte();
			bool canDrag = mobile.Player || (b & 2) != 0;
			Gumps.OpenPaperdoll(mobile, name, canDrag);
		}
	}

	private static void SelectHue(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		short num2 = pvSrc.ReadInt16();
		short num3 = pvSrc.ReadInt16();
		GAlphaBackground gAlphaBackground = new GAlphaBackground(0, 0, 244, 110);
		gAlphaBackground.m_NonRestrictivePicking = true;
		gAlphaBackground.Center();
		GItemArt gItemArt = new GItemArt(183, 3, num3);
		gItemArt.X += (58 - (gItemArt.Image.xMax - gItemArt.Image.xMin)) / 2 - gItemArt.Image.xMin;
		gItemArt.Y += (82 - (gItemArt.Image.yMax - gItemArt.Image.yMin)) / 2 - gItemArt.Image.yMin;
		gAlphaBackground.Children.Add(gItemArt);
		GHuePicker gHuePicker = new GHuePicker(4, 4);
		gHuePicker.Brightness = 1;
		gHuePicker.SetTag("ItemID", (int)num3);
		gHuePicker.SetTag("Item Art", gItemArt);
		gHuePicker.SetTag("Dialog", gAlphaBackground);
		gHuePicker.OnHueSelect = Engine.HuePicker_OnHueSelect;
		gAlphaBackground.Children.Add(gHuePicker);
		gAlphaBackground.Children.Add(new GSingleBorder(3, 3, 162, 82));
		gAlphaBackground.Children.Add(new GSingleBorder(164, 3, 17, 82));
		GBrightnessBar gBrightnessBar = new GBrightnessBar(165, 4, 15, 80, gHuePicker);
		gAlphaBackground.Children.Add(gBrightnessBar);
		gBrightnessBar.Refresh();
		GFlatButton gFlatButton = new GFlatButton(123, 87, 58, 20, "Picker", Engine.HuePickerPicker_OnClick);
		GFlatButton gFlatButton2 = new GFlatButton(183, 87, 58, 20, "Okay", Engine.HuePickerOk_OnClick);
		gFlatButton2.SetTag("Hue Picker", gHuePicker);
		gFlatButton2.SetTag("Dialog", gAlphaBackground);
		gFlatButton2.SetTag("Serial", num);
		gFlatButton2.SetTag("ItemID", num3);
		gFlatButton2.SetTag("Relay", num2);
		gFlatButton.SetTag("Hue Picker", gHuePicker);
		gFlatButton.SetTag("Brightness Bar", gBrightnessBar);
		gAlphaBackground.Children.Add(gFlatButton);
		gAlphaBackground.Children.Add(gFlatButton2);
		Gumps.Desktop.Children.Add(gAlphaBackground);
		Engine.HuePicker_OnHueSelect(gHuePicker.Hue, gHuePicker);
	}

	private static void LaunchBrowser(PacketReader pvSrc)
	{
		string url = pvSrc.ReadString();
		if (Engine.m_Fullscreen)
		{
			Engine.AddTextMessage("Cannot open browser in fullscreen.");
		}
		else
		{
			Engine.OpenBrowser(url);
		}
	}

	private static void WarmodeStatus(PacketReader pvSrc)
	{
		bool flag = pvSrc.ReadBoolean();
		if (pvSrc.ReadByte() == 0)
		{
			byte b = pvSrc.ReadByte();
			if (b != 32 && b != 50 && b != 0)
			{
				pvSrc.Trace();
			}
			else if (pvSrc.ReadByte() != 0)
			{
				pvSrc.Trace();
			}
		}
		Mobile player = World.Player;
		if (player != null)
		{
			player.Flags[MobileFlag.Warmode] = flag;
			if (!flag)
			{
				Engine.m_Highlight = null;
			}
		}
		Gumps.Invalidate();
		Engine.Redraw();
	}

	private static void DeleteObject(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		if ((num & 0x40000000) == 0)
		{
			World.Remove(World.FindMobile(num));
			return;
		}
		Item item = World.FindItem(num);
		if (item != null && item.ItemId == ItemId.HealPotion && item.IsChildOf(World.Player))
		{
			PacketHandlers.SetEvent(EventFlags.ConsumeHeal);
		}
		World.Remove(item);
	}

	private static ushort NormalizeColor(ushort color)
	{
		ushort num = (ushort)(color & 0x8000);
		ushort num2 = (ushort)(color & 0x4000);
		ushort num3 = (ushort)(color & 0x3FFF);
		if (num3 != 0)
		{
			if (num3 >= 3000)
			{
				return (ushort)(num | num2 | 1);
			}
			return color;
		}
		return num;
	}

	private static void WorldObject(uint serial, ushort visage, byte offset, ushort amount, ushort x, ushort y, short z, byte light, ushort color, byte flags, ushort amount2, int type, int unk)
	{
		bool wasFound = false;
		Item item = World.WantItem((int)serial, ref wasFound);
		if (type == 2)
		{
			int num = visage & 0x3FFF;
			if (item.Multi == null || item.Multi.MultiID != num)
			{
				item.Multi = new Multi(num);
				Engine.Multis.Register(item);
			}
			visage = 1;
		}
		else if (item.Multi != null)
		{
			Engine.Multis.Unregister(item);
			item.Multi = null;
		}
		item.ID = visage;
		item.Amount = amount;
		item.Direction = light;
		item.Hue = color;
		item.Flags.Value = flags;
		item.SetLocation(World.Agent, x, y, z);
		if (!item.Visible && (item.IsCorpse || item.IsBones) && UOAIO.Profiles.Options.Current.IncomingNames)
		{
			item.Look();
		}
		if (visage == 8198 && item.CorpseSerial != 0)
		{
			Mobile mobile = World.FindMobile(item.CorpseSerial);
			if (mobile != null)
			{
				item.Direction = mobile.Direction;
			}
		}
		item.Update();
		PacketHandlers.EventBus.Publish(new WorldObjectUpdateEvent(item));
	}

	private static void WorldObjectHS(uint serial, ushort visage, byte offset, ushort amount, ushort x, ushort y, short z, byte light, ushort color, byte flags, ushort amount2, int type, int unk)
	{
		bool wasFound = false;
		Item item = World.WantItem((int)serial, ref wasFound);
		if (type == 2)
		{
			int num = visage & 0x7FFF;
			if (item.Multi == null || item.Multi.MultiID != num)
			{
				item.Multi = new Multi(num);
				Engine.Multis.Register(item);
			}
			visage = 1;
		}
		else if (item.Multi != null)
		{
			Engine.Multis.Unregister(item);
			item.Multi = null;
		}
		item.ID = visage;
		item.Amount = amount;
		item.Direction = light;
		item.Hue = color;
		item.Flags.Value = flags;
		item.SetLocation(World.Agent, x, y, z);
		if (!item.Visible && (item.IsCorpse || item.IsBones) && UOAIO.Profiles.Options.Current.IncomingNames)
		{
			item.Look();
		}
		if (visage == 8198 && item.CorpseSerial != 0)
		{
			Mobile mobile = World.FindMobile(item.CorpseSerial);
			if (mobile != null)
			{
				item.Direction = mobile.Direction;
			}
		}
		item.Update();
		PacketHandlers.EventBus.Publish(new WorldObjectUpdateEvent(item));
	}

	private static void WorldItem_F3SA(PacketReader packet)
	{
		ushort num = packet.ReadUInt16();
		int type = packet.ReadByte();
		uint serial = packet.ReadUInt32();
		ushort num2 = packet.ReadUInt16();
		byte offset = packet.ReadByte();
		ushort num3 = packet.ReadUInt16();
		ushort num4 = packet.ReadUInt16();
		ushort x = packet.ReadUInt16();
		ushort y = packet.ReadUInt16();
		short z = packet.ReadSByte();
		byte light = packet.ReadByte();
		ushort color = packet.ReadUInt16();
		byte flags = packet.ReadByte();
		if (num2 > ushort.MaxValue)
		{
			num2 = 50;
			color = 20;
		}
		PacketHandlers.WorldObject(serial, num2, offset, num4, x, y, z, light, color, flags, num4, type, 1);
	}

	private static void WorldItem_F3HS(PacketReader packet)
	{
		ushort num = packet.ReadUInt16();
		int type = packet.ReadByte();
		uint serial = packet.ReadUInt32();
		ushort num2 = packet.ReadUInt16();
		byte offset = packet.ReadByte();
		ushort num3 = packet.ReadUInt16();
		ushort num4 = packet.ReadUInt16();
		ushort x = packet.ReadUInt16();
		ushort y = packet.ReadUInt16();
		short z = packet.ReadSByte();
		byte light = packet.ReadByte();
		ushort color = packet.ReadUInt16();
		byte flags = packet.ReadByte();
		int unk = packet.ReadInt16();
		if (num2 > ushort.MaxValue)
		{
			num2 = 50;
			color = 20;
		}
		PacketHandlers.WorldObjectHS(serial, num2, offset, num4, x, y, z, light, color, flags, num4, type, unk);
	}

	private static void WorldItem_1A(PacketReader packet)
	{
		uint num = packet.ReadUInt32();
		ushort num2 = packet.ReadUInt16();
		byte offset = (byte)(((num2 & 0x8000) != 0) ? packet.ReadByte() : 0);
		ushort num3 = (ushort)(((num & 0x80000000u) == 0) ? 1 : packet.ReadUInt16());
		ushort num4 = packet.ReadUInt16();
		ushort num5 = packet.ReadUInt16();
		byte light = (byte)(((num4 & 0x8000) != 0) ? packet.ReadByte() : 0);
		sbyte z = packet.ReadSByte();
		ushort color = (ushort)(((num5 & 0x8000) != 0) ? packet.ReadUInt16() : 0);
		byte b = packet.ReadByte();
		b = (byte)(((num5 & 0x4000) != 0) ? b : 0);
		num &= 0x7FFFFFFF;
		num2 &= 0x7FFF;
		num4 &= 0x7FFF;
		num5 &= 0x3FFF;
		int type = 0;
		if (num2 >= 16384)
		{
			num2 += 49152;
			type = 2;
		}
		PacketHandlers.WorldObject(num, num2, offset, num3, num4, num5, z, light, color, b, num3, type, 1);
	}

	private static void Mobile_Moving(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile == null)
		{
			return;
		}
		bool flag = false;
		mobile.Body = pvSrc.ReadUInt16();
		if (mobile.Body == 1417 || mobile.Body == 1418 || mobile.Body == 1419 || mobile.Body == 1420 || mobile.Body == 1421 || mobile.Body == 1422)
		{
			mobile.Body = 61;
		}
		if (!mobile.Player)
		{
			int x = pvSrc.ReadInt16();
			int y = pvSrc.ReadInt16();
			int z = pvSrc.ReadSByte();
			int num = pvSrc.ReadByte();
			WalkAnimation item = WalkAnimation.PoolInstance(mobile, x, y, z, num);
			mobile.Walking.Enqueue(item);
			if (mobile.Walking.Count > 4)
			{
				WalkAnimation walkAnimation = mobile.Walking.Dequeue();
				mobile.SetLocation((short)walkAnimation.NewX, (short)walkAnimation.NewY, (short)walkAnimation.NewZ);
				walkAnimation.Dispose();
				flag = true;
			}
			mobile.Walking.Peek().Start();
			mobile.SetReal(x, y, z, num);
		}
		else
		{
			pvSrc.Seek(6, SeekOrigin.Current);
		}
		mobile.Hue = pvSrc.ReadUInt16();
		mobile.Flags.Value = pvSrc.ReadByte();
		mobile.Notoriety = (Notoriety)pvSrc.ReadByte();
		mobile.IsMoving = !mobile.Player || Engine.amMoving;
		mobile.LastSeen = Engine.Ticks;
		if (!mobile.Visible)
		{
			mobile.Update();
		}
		else if (flag)
		{
			mobile.Update();
		}
	}

	private static void Movement_Accept(PacketReader pvSrc)
	{
		if (World.Player == null)
		{
			return;
		}
		int num = pvSrc.ReadByte();
		byte b = (byte)(pvSrc.ReadByte() & -65);
		if (b == 0 || b >= 8)
		{
			b = 1;
		}
		World.Player.Notoriety = (Notoriety)b;
		if (PacketHandlers.m_Sequences.Count == 0)
		{
			Engine.AddTextMessage("sequence count");
			Engine.Resync();
		}
		else
		{
			int[] array = (int[])PacketHandlers.m_Sequences.Dequeue();
			if (num != array[0])
			{
				Engine.AddTextMessage("sequence mismatch");
				Engine.Resync();
			}
			else
			{
				World.SetLocation(array[1], array[2], array[3]);
			}
			PacketHandlers.m_MoveDelay -= TimeSpan.FromSeconds((double)array[4] * 0.1);
		}
		Engine.m_WalkAck++;
		PacketHandlers.EventBus.Publish(new MovementAcceptedEvent());
	}

	private static void PingReply(PacketReader pvSrc)
	{
		Engine.PingReply(pvSrc.ReadByte());
	}

	internal static void AddSequence(int seq, int x, int y, int z, TimeSpan speed)
	{
		PacketHandlers.m_Sequences.Enqueue(new int[5]
		{
			seq,
			x,
			y,
			z,
			(int)(speed.TotalSeconds / 0.1)
		});
		PacketHandlers.m_MoveDelay += speed;
	}

	private static void Movement_Reject(PacketReader pvSrc)
	{
		PacketHandlers.m_Sequences.Clear();
		PacketHandlers.m_MoveDelay = TimeSpan.Zero;
		Engine.m_Sequence = 0;
		Engine.m_WalkReq = 0;
		Engine.m_WalkAck = 0;
		Mobile player = World.Player;
		if (player != null)
		{
			pvSrc.ReadByte();
			short x = pvSrc.ReadInt16();
			short y = pvSrc.ReadInt16();
			byte direction = pvSrc.ReadByte();
			player.Direction = direction;
			sbyte z = pvSrc.ReadSByte();
			World.SetLocation(x, y, z);
			player.SetLocation(x, y, z);
			player.MovedTiles = 0;
			player.HorseFootsteps = 0;
			player.IsMoving = false;
			player.Walking.Clear();
			player.UpdateReal();
			player.Update();
		}
		if (Engine.m_Stealth)
		{
			Engine.m_StealthSteps++;
		}
	}

	internal static void ClearTarget(TargetAction action, TimeSpan timeout)
	{
		PacketHandlers.QueueTarget(action, null, timeout);
	}

	internal static void QueueTarget(TargetAction action, object target, TimeSpan timeout)
	{
		PacketHandlers.m_CancelAction = action;
		PacketHandlers.m_CancelTarget = target;
		PacketHandlers.m_CancelTimeout = DateTime.Now + timeout;
	}

	private static void Target(PacketReader pvSrc)
	{
		byte b = pvSrc.ReadByte();
		int targetID = pvSrc.ReadInt32();
		byte b2 = pvSrc.ReadByte();
		pvSrc.ReadInt32();
		pvSrc.ReadInt16();
		pvSrc.ReadInt16();
		pvSrc.ReadByte();
		pvSrc.ReadSByte();
		pvSrc.ReadInt16();
		if (b2 == 3)
		{
			if (TargetManager.Server != null)
			{
				TargetManager.Server = null;
			}
		}
		else
		{
			if (b > 1 || (b2 != 1 && b2 != 2 && b2 > 0))
			{
				pvSrc.Trace();
			}
			ServerTargetHandler serverTargetHandler = (TargetManager.Server = new ServerTargetHandler(targetID, b > 0, (AggressionType)b2));
			TargetActions.Identify();
			TargetManager.ProcessQueue();
			if (PacketHandlers.m_CancelTimeout != DateTime.MinValue)
			{
				if (PacketHandlers.m_CancelTimeout > DateTime.Now && (PacketHandlers.m_CancelAction == TargetAction.Unknown || serverTargetHandler.Action == PacketHandlers.m_CancelAction))
				{
					if (PacketHandlers.m_CancelTarget == null)
					{
						TargetManager.Server.Cancel();
					}
					else
					{
						TargetManager.Server.Target(PacketHandlers.m_CancelTarget);
					}
				}
				PacketHandlers.m_CancelAction = TargetAction.Unknown;
				PacketHandlers.m_CancelTarget = null;
				PacketHandlers.m_CancelTimeout = DateTime.MinValue;
			}
		}
		PacketHandlers.EventBus.Publish(new TargetEvent());
	}

	private static void Mobile_Incoming(PacketReader pvSrc)
	{
		if (World.Player == null)
		{
			return;
		}
		int num = pvSrc.ReadInt32();
		if ((num & -1073741824) != 0)
		{
			pvSrc.Trace();
		}
		ushort num2 = pvSrc.ReadUInt16();
		if (((ulong)num & 0xFFFFFFFF80000000uL) != 0L)
		{
			pvSrc.ReadInt16();
		}
		ushort x = pvSrc.ReadUInt16();
		ushort y = pvSrc.ReadUInt16();
		sbyte z = pvSrc.ReadSByte();
		byte b = pvSrc.ReadByte();
		ushort hue = pvSrc.ReadUInt16();
		byte value = pvSrc.ReadByte();
		Notoriety notoriety = (Notoriety)pvSrc.ReadByte();
		bool wasFound = false;
		Mobile mobile = World.WantMobile(num, ref wasFound);
		bool visible = mobile.Visible;
		List<Item> list = new List<Item>(mobile.Items);
		int serial;
		while ((serial = pvSrc.ReadInt32()) > 0)
		{
			Item item = World.WantItem(serial);
			item.Query();
			ushort num3 = pvSrc.ReadUInt16();
			Layer layer = (Layer)pvSrc.ReadByte();
			ushort hue2 = (ushort)(((num3 & 0x8000) != 0) ? pvSrc.ReadUInt16() : 0);
			item.ID = num3 & 0x7FFF;
			item.Hue = hue2;
			item.Layer = layer;
			item.SetLocation(mobile, 0, 0, 0);
			list.Remove(item);
		}
		foreach (Item item2 in list)
		{
			World.Remove(item2);
		}
		if (mobile.Player)
		{
			b &= 7;
			b |= (byte)(mobile.Direction & 0x80);
		}
		_ = mobile.Direction;
		if (!mobile.Visible && !mobile.Player && UOAIO.Profiles.Options.Current.IncomingNames)
		{
			mobile.Look();
		}
		if (num2 == 1417 || num2 == 1418 || num2 == 1419 || num2 == 1420 || num2 == 1421 || num2 == 1422)
		{
			num2 = 61;
		}
		if (!mobile.Player)
		{
			mobile.SetLocation(World.Agent, x, y, z);
			mobile.Direction = b;
			mobile.Hue = hue;
			mobile.Body = num2;
			mobile.IsMoving = false;
			mobile.MovedTiles = 0;
			mobile.HorseFootsteps = 0;
			mobile.Walking.Clear();
			mobile.UpdateReal();
		}
		mobile.Flags.Value = value;
		mobile.Notoriety = notoriety;
		mobile.LastSeen = Engine.Ticks;
		mobile.EquipChanged();
		mobile.Update();
		if (!visible && !mobile.Player && (num2 == 400 || num2 == 401) && (mobile.StatusBar != null || mobile.IsFriend || mobile.IsInParty || TargetManager.IsAcquirable(World.Player, mobile)))
		{
			mobile.QueryStats();
		}
	}

	private static void Mobile_Attributes_HitPoints(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null)
		{
			mobile.Refresh = true;
			mobile.MaximumHitPoints = pvSrc.ReadUInt16();
			mobile.CurrentHitPoints = pvSrc.ReadUInt16();
			mobile.Refresh = false;
		}
	}

	private static void Mobile_Attributes_Stamina(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null)
		{
			mobile.Refresh = true;
			mobile.MaximumStamina = pvSrc.ReadUInt16();
			mobile.CurrentStamina = pvSrc.ReadUInt16();
			mobile.Refresh = false;
		}
	}

	private static void Mobile_Attributes_Mana(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile != null)
		{
			mobile.Refresh = true;
			mobile.MaximumMana = pvSrc.ReadUInt16();
			mobile.CurrentMana = pvSrc.ReadUInt16();
			mobile.Refresh = false;
		}
	}

	private static void Mobile_Status(PacketReader pvSrc)
	{
		Mobile mobile = World.WantMobile(pvSrc.ReadInt32());
		if (mobile == null)
		{
			return;
		}
		mobile.Refresh = true;
		mobile.Name = pvSrc.ReadString(30);
		mobile.CurrentHitPoints = pvSrc.ReadUInt16();
		mobile.MaximumHitPoints = pvSrc.ReadUInt16();
		mobile.IsPet = pvSrc.ReadBoolean();
		byte b = pvSrc.ReadByte();
		if (b >= 1)
		{
			mobile.Gender = pvSrc.ReadByte();
			mobile.Strength = pvSrc.ReadUInt16();
			mobile.Dexterity = pvSrc.ReadUInt16();
			mobile.Intelligence = pvSrc.ReadUInt16();
			mobile.CurrentStamina = pvSrc.ReadUInt16();
			mobile.MaximumStamina = pvSrc.ReadUInt16();
			mobile.CurrentMana = pvSrc.ReadUInt16();
			mobile.MaximumMana = pvSrc.ReadUInt16();
			mobile.Gold = pvSrc.ReadInt32();
			mobile.Armor = pvSrc.ReadUInt16();
			mobile.Weight = pvSrc.ReadUInt16();
			if (b >= 2)
			{
				if (b >= 5)
				{
					pvSrc.ReadUInt16();
					pvSrc.ReadByte();
				}
				mobile.StatCap = pvSrc.ReadUInt16();
				if (b >= 3)
				{
					mobile.FollowersCur = pvSrc.ReadByte();
					mobile.FollowersMax = pvSrc.ReadByte();
					if (b >= 4)
					{
						mobile.FireResist = pvSrc.ReadInt16();
						mobile.ColdResist = pvSrc.ReadInt16();
						mobile.PoisonResist = pvSrc.ReadInt16();
						mobile.EnergyResist = pvSrc.ReadInt16();
						mobile.Luck = pvSrc.ReadUInt16();
						mobile.DamageMin = pvSrc.ReadUInt16();
						mobile.DamageMax = pvSrc.ReadUInt16();
						mobile.TithingPoints = pvSrc.ReadInt32();
						if (b > 5)
						{
							pvSrc.Trace();
						}
					}
					else
					{
						mobile.FireResist = 0;
						mobile.ColdResist = 0;
						mobile.PoisonResist = 0;
						mobile.EnergyResist = 0;
						mobile.Luck = 0;
						mobile.DamageMin = 0;
						mobile.DamageMax = 0;
					}
				}
				else
				{
					mobile.FollowersCur = 0;
					mobile.FollowersMax = 5;
				}
			}
			else
			{
				mobile.StatCap = 225;
			}
		}
		mobile.Refresh = false;
		if (mobile.IsInGuild && mobile.HasName)
		{
			GuildRoster guildRoster = Profile.Current.GuildRoster;
			Character character = guildRoster[mobile];
			if (character != null)
			{
				character.Name = mobile.Name;
			}
			else
			{
				guildRoster.Characters.Add(new Character(mobile));
			}
		}
	}

	private static void Mobile_Update(PacketReader pvSrc)
	{
		Mobile mobile = World.WantMobile(pvSrc.ReadInt32());
		ushort num = pvSrc.ReadUInt16();
		byte b = pvSrc.ReadByte();
		ushort hue = pvSrc.ReadUInt16();
		byte value = pvSrc.ReadByte();
		short x = pvSrc.ReadInt16();
		short y = pvSrc.ReadInt16();
		short num2 = pvSrc.ReadInt16();
		byte direction = pvSrc.ReadByte();
		sbyte z = pvSrc.ReadSByte();
		if (b != 0 || num2 != 0)
		{
			pvSrc.Trace();
		}
		if (mobile.Player)
		{
			if (Engine.m_InResync)
			{
				Engine.m_InResync = false;
				Engine.AddTextMessage("Resynchronization complete.");
			}
			else if (mobile.InRange(x, y, 18))
			{
			}
			PacketHandlers.m_Sequences.Clear();
			PacketHandlers.m_MoveDelay = TimeSpan.Zero;
			Engine.m_Sequence = 0;
			Engine.m_WalkAck = 0;
			Engine.m_WalkReq = 0;
		}
		if (mobile.Player)
		{
			if ((num == 402 || num == 403) && mobile.Body != 402 && mobile.Body != 403)
			{
				Network.Send(new PSetWarMode(warMode: false, 32, 0));
				Engine.Effects.Add(new DeathEffect());
			}
			else if ((mobile.Body == 402 || mobile.Body == 403) && num != 402 && num != 403)
			{
				Animation animation = (mobile.Animation = new Animation());
				Animation animation3 = animation;
				animation3.Action = 17;
				animation3.Delay = 0;
				animation3.Forward = true;
				animation3.Repeat = false;
				animation3.Run();
				Engine.Effects.Add(new ResurrectEffect());
			}
		}
		if (mobile.Player)
		{
			World.SetLocation(x, y, z);
		}
		mobile.SetLocation(World.Agent, x, y, z);
		if (num == 1417 || num == 1418 || num == 1419 || num == 1420 || num == 1421 || num == 1422)
		{
			num = 61;
		}
		mobile.Body = num;
		mobile.Hue = hue;
		mobile.IsMoving = false;
		mobile.MovedTiles = 0;
		mobile.HorseFootsteps = 0;
		mobile.Walking.Clear();
		mobile.UpdateReal();
		mobile.Direction = direction;
		mobile.Flags.Value = value;
		mobile.LastSeen = Engine.Ticks;
		mobile.Update();
	}

	private static void DisplayQuestionMenu(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		int menuID = pvSrc.ReadInt16();
		string question = pvSrc.ReadString(pvSrc.ReadByte());
		AnswerEntry[] array = new AnswerEntry[pvSrc.ReadByte()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new AnswerEntry(i, pvSrc.ReadInt16(), pvSrc.ReadUInt16(), pvSrc.ReadString(pvSrc.ReadByte()));
		}
		if (array.Length != 0 && array[0].ItemID != 0)
		{
			Gumps.Desktop.Children.Add(new GItemList(serial, menuID, question, array));
		}
		else
		{
			Gumps.Desktop.Children.Add(new GQuestionMenu(serial, menuID, question, array));
		}
	}

	private static void AddMessage(int serial, IFont font, IHue hue, int type, string name, string text)
	{
		PacketHandlers.AddMessage(serial, font, hue, type, name, text, 0);
	}

	private static void AddMessage(int serial, IFont font, IHue hue, int type, string name, string text, int number)
	{
		if (ActionContext.Active is LookContext)
		{
			return;
		}
		name = name.Trim();
		text = text.Trim();
		switch (number)
		{
		case 1004013:
		{
			Mobile player2 = World.Player;
			Engine.Sounds.PlaySound(481, player2.X, player2.Y, player2.Z);
			Mobile lastAttacker = Engine.m_LastAttacker;
			string text2 = "someone";
			if (lastAttacker != null && !string.IsNullOrEmpty(lastAttacker.Name))
			{
				text2 = lastAttacker.Name;
			}
			Party.SendAutomatedMessage("I stunned {0} !!", text2);
			break;
		}
		case 1004014:
		{
			Mobile player = World.Player;
			Engine.Sounds.PlaySound(343, player.X, player.Y, player.Z);
			Party.SendAutomatedMessage("I'm stunned !!");
			break;
		}
		}
		switch (number)
		{
		case 500948:
			TargetActions.Lookahead = TargetAction.Bandage;
			break;
		case 1049632:
			TargetActions.Identify(TargetAction.Bola);
			break;
		case 500236:
			TargetActions.Identify(TargetAction.PurplePotion);
			break;
		case 500819:
			TargetActions.Lookahead = TargetAction.DetectHidden;
			break;
		case 1049541:
			TargetActions.Lookahead = TargetAction.Discord;
			break;
		case 502698:
			TargetActions.Identify(TargetAction.Stealing);
			break;
		}
		if (number == 500119 || number == 1045157 || number == 3000201)
		{
			Engine.DelayedAction();
			ActionContext active = ActionContext.Active;
			if (active != null)
			{
				active.WasDelayed = true;
				return;
			}
		}
		if (serial > 0 && serial < 1073741824)
		{
			Mobile mobile = World.FindMobile(serial);
			if (mobile != null)
			{
				if (!string.IsNullOrEmpty(name))
				{
					mobile.GuessedName = name;
				}
				if (font is Font && type == 0 && text.StartsWith("["))
				{
					string text3 = text;
					if (text3.EndsWith(" (Chaos)"))
					{
						text3 = text3.Substring(0, text3.Length - " (Chaos)".Length);
					}
					else if (text3.EndsWith(" (Order)"))
					{
						text3 = text3.Substring(0, text3.Length - " (Order)".Length);
					}
					if (text3.EndsWith("]"))
					{
						int num = text3.LastIndexOf(", ");
						if (num >= 0)
						{
							num += 2;
							text3 = text3.Substring(num, text3.Length - num - 1);
						}
						else
						{
							text3 = text3.Substring(1, text3.Length - 2);
						}
						mobile.Guild = text3;
					}
				}
				else if (type == 6 && text.StartsWith("(") && text.EndsWith(")"))
				{
					if (text.EndsWith("Minax)") || text.EndsWith("Minax) (Evil)"))
					{
						mobile.Faction = Faction.Minax;
					}
					else if (text.EndsWith("Council of Mages)") || text.EndsWith("Council of Mages) (Hero)"))
					{
						mobile.Faction = Faction.CouncilOfMages;
					}
					else if (text.EndsWith("True Britannians)") || text.EndsWith("True Britannians) (Hero)"))
					{
						mobile.Faction = Faction.TrueBritannians;
					}
					else if (text.EndsWith("Shadowlords)") || text.EndsWith("Shadowlords) (Evil)"))
					{
						mobile.Faction = Faction.Shadowlords;
					}
				}
			}
		}
		bool flag;
		Mobile mobile4;
		Item item2;
		switch (type)
		{
		case 3:
		case 4:
		case 7:
		case 10:
			if (type == 13 || type == 14)
			{
				goto case 0;
			}
			goto IL_04e5;
		case 0:
		case 2:
		case 8:
		case 9:
			if (serial > 0 && serial < 1073741824)
			{
				Mobile mobile3 = World.FindMobile(serial);
				if (mobile3 != null && mobile3.IsIgnored)
				{
					break;
				}
			}
			goto IL_04e5;
		case 13:
			Engine.AddTextMessage(string.Format("[Guild][{0}]: {1}", string.IsNullOrEmpty(name) ? "???" : name, text), font, hue);
			break;
		case 14:
			Engine.AddTextMessage(string.Format("[Alliance][{0}]: {1}", string.IsNullOrEmpty(name) ? "???" : name, text), font, hue);
			break;
		case 1:
			if (name.Length > 0)
			{
				Engine.AddTextMessage(name + ": " + text, font, hue);
			}
			else
			{
				Engine.AddTextMessage(text, font, hue);
			}
			break;
		case 6:
		{
			Mobile mobile2 = World.FindMobile(serial);
			if (mobile2 != null)
			{
				mobile2.AddTextMessage("You see", text, font, hue, unremovable: false);
				break;
			}
			Item item = World.FindItem(serial);
			if (item != null)
			{
				item.AddTextMessage("You see", text, font, hue, unremovable: false);
			}
			else
			{
				Engine.AddTextMessage(text, font, hue);
			}
			break;
		}
		default:
			{
				StreamWriter streamWriter = new StreamWriter("Messages.log", append: true);
				streamWriter.WriteLine("Serial = 0x{0:X8}", serial);
				streamWriter.WriteLine("Font = {0}", font);
				streamWriter.WriteLine("Hue = {0}", hue);
				streamWriter.WriteLine("Type = {0}", type);
				streamWriter.WriteLine("Name = \"{0}\"", name);
				streamWriter.WriteLine("Text = \"{0}\"", text);
				streamWriter.WriteLine(new string('#', 20));
				streamWriter.Flush();
				streamWriter.Close();
				break;
			}
			IL_04e5:
			if (type == 3 || type == 4)
			{
				StreamWriter streamWriter2 = new StreamWriter("Messages.log", append: true);
				streamWriter2.WriteLine("Serial = 0x{0:X8}", serial);
				streamWriter2.WriteLine("Font = {0}", font);
				streamWriter2.WriteLine("Hue = {0}", hue);
				streamWriter2.WriteLine("Type = {0}", type);
				streamWriter2.WriteLine("Name = \"{0}\"", name);
				streamWriter2.WriteLine("Text = \"{0}\"", text);
				streamWriter2.WriteLine(new string('#', 20));
				streamWriter2.Flush();
				streamWriter2.Close();
			}
			flag = false;
			if (type == 10 || (number >= 1060718 && number <= 1060727))
			{
				Spell spellByPower = Spells.GetSpellByPower(text);
				if (spellByPower == null)
				{
					text += " - Unknown";
				}
				else
				{
					if (serial == World.Serial)
					{
						TargetActions.Lookahead = (TargetAction)(spellByPower.SpellID - 1);
					}
					text = text + " - " + spellByPower.Name;
				}
				flag = true;
			}
			mobile4 = World.FindMobile(serial);
			if (mobile4 != null)
			{
				if (flag && !mobile4.Player)
				{
					hue = Hues.GetNotoriety(mobile4.Notoriety, full: false);
				}
				if (mobile4.Player)
				{
					ActionContext active2 = ActionContext.Active;
					if (active2 != null && !active2.OnSpeech(text))
					{
						break;
					}
				}
				if (type == 7)
				{
					MessageManager.ClearMessages(mobile4);
				}
				mobile4.AddTextMessage(name, text, font, hue, type == 10 || type == 7);
				break;
			}
			item2 = World.FindItem(serial);
			if (item2 != null)
			{
				if (type == 7)
				{
					MessageManager.ClearMessages(item2);
				}
				item2.AddTextMessage(name, text, font, hue, type == 10 || type == 7);
			}
			else
			{
				Engine.AddTextMessage(text, font, hue);
			}
			break;
		}
	}

	private static void Message_Unicode(PacketReader pvSrc)
	{
		int serial = pvSrc.ReadInt32();
		int num = pvSrc.ReadInt16();
		int type = pvSrc.ReadByte();
		IHue hue = Hues.Load(pvSrc.ReadInt16());
		IFont uniFont = Engine.GetUniFont(pvSrc.ReadInt16());
		string text = pvSrc.ReadString(4);
		string name = pvSrc.ReadString(30);
		string text2 = pvSrc.ReadUnicodeString();
		PacketHandlers.AddMessage(serial, uniFont, hue, type, name, text2);
	}

	private static void Weather(PacketReader pvSrc)
	{
		int num = pvSrc.ReadByte();
		int num2 = pvSrc.ReadByte();
		int num3 = pvSrc.ReadSByte();
	}

	private static void LoginComplete(PacketReader pvSrc)
	{
		Music.Stop();
		Engine.Unlock();
		Engine.m_Loading = false;
		Engine.m_Ingame = true;
		Cursor.Hourglass = false;
		Engine.ClearScreen();
		_ = World.Player;
		Preferences.Current.Layout.Apply(applyGumps: true);
		Engine.DrawNow();
		Engine.StartPings();
		Network.Send(new POpenPaperdoll());
		World.Player.QueryStats();
		Network.Send(new PChatOpen(""));
		Network.Send(new PClientType());
		Renderer.DrawPing = true;
		GItemCounters.Active = true;
		GRadar.Open();
		Engine.OpenBackpack();
		if (!ShaderData.IsSupported)
		{
			Engine.AddTextMessage("*** Your video card does not support the 2.0 shader model.", 30f);
			Engine.AddTextMessage("*** Game graphics will not be rendered correctly.", 30f);
		}
		Loader.Initialize();
	}

	private static void Message_ASCII(PacketReader pvSrc)
	{
		int num = pvSrc.ReadInt32();
		int num2 = pvSrc.ReadInt16();
		int num3 = pvSrc.ReadByte();
		short num4;
		IHue hue = Hues.Load(num4 = pvSrc.ReadInt16());
		short num5;
		IFont font = Engine.GetFont(num5 = pvSrc.ReadInt16());
		string text = pvSrc.ReadString(30);
		string text2 = pvSrc.ReadString();
		if (World.Player != null && num == 0 && num2 == 0 && num3 == 0 && num4 == -1 && num5 == -1 && text == "SYSTEM")
		{
			Packet packet = new Packet(3);
			packet.m_Stream.Write((byte)32);
			packet.m_Stream.Write(num4);
			packet.m_Stream.Write(num5);
			StringBuilder stringBuilder = new StringBuilder();
			Process currentProcess = Process.GetCurrentProcess();
			string fileName = Path.GetFileName(currentProcess.MainModule.FileName);
			stringBuilder.AppendFormat("{0}{1}E{2}{3} {4} {5}\0", 'C', 'H', 'A', 'T', fileName, Assembly.GetCallingAssembly().GetName().Version);
			for (int i = 0; i < stringBuilder.Length; i++)
			{
				byte toWrite = (byte)(PacketHandlers.m_Key[i % PacketHandlers.m_Key.Length] ^ (byte)stringBuilder[i]);
				packet.m_Stream.Write(toWrite);
			}
			Network.Send(packet);
		}
		else
		{
			PacketHandlers.AddMessage(num, font, hue, num3, text, text2);
		}
	}

	private static void LoginConfirm(PacketReader pvSrc)
	{
		Engine.PingReply(-1);
		World.Clear();
		Map.Invalidate();
		Mobile mobile = World.WantMobile(pvSrc.ReadInt32());
		World.Serial = mobile.Serial;
		Macros.Reset();
		if (pvSrc.ReadInt32() != 0)
		{
			pvSrc.Trace();
		}
		mobile.Body = pvSrc.ReadUInt16();
		short x = pvSrc.ReadInt16();
		short y = pvSrc.ReadInt16();
		short z = pvSrc.ReadInt16();
		World.SetLocation(x, y, z);
		mobile.SetLocation(World.Agent, x, y, z);
		mobile.UpdateReal();
		mobile.Direction = pvSrc.ReadByte();
		mobile.Update();
		Network.Send(new PQuerySkills());
		Engine.PingRequest(sendPing: false);
		Network.Send(new PClientVersion(Engine.GetVersionString()));
		Network.Send(new PScreenSize());
		Network.Send(new PSetLanguage());
		Network.Send(new PUnknownLogin());
		PUpdateRange.Dispatch(null);
		Party.State = PartyState.Alone;
	}

	private static void ReceiveServerRelay(PacketReader pvSrc)
	{
		IPAddress addr = new IPAddress(pvSrc.ReadBytes(4));
		int port = (int) pvSrc.ReadUInt16();
		uint authId = pvSrc.ReadUInt32();
		GameCrypto gameCrypto = new GameCrypto(authId);

		Debug.Trace($"Server relay: connecting to {addr}:{port}");
		if (!Network.Connect(gameCrypto, new IPEndPoint(addr, port)))
		{
			throw new InvalidOperationException("Failed to connect.");
		}

		Network.Send(new PGameSeed(authId));
		Network.Send(new PGameLogin((int)authId, Engine.Shard.Account, Engine.Shard.Password));
		Network.Context._crypto = gameCrypto;
		Network.Flush();
	}

	private static void Characters(PacketReader pvSrc)
	{
		int count = pvSrc.ReadByte();
		List<CharacterInfo> list = new List<CharacterInfo>();
		for (int i = 0; i < count; i++)
		{
			string text = pvSrc.ReadString(30);
			string password = pvSrc.ReadString(30);
			if (text != "")
			{
				CharacterInfo item = new CharacterInfo(text, password, i);
				list.Add(item);
			}
		}
		int num2 = pvSrc.ReadByte();
		for (int j = 0; j < num2; j++)
		{
			pvSrc.ReadByte();
			pvSrc.ReadString(31);
			pvSrc.ReadString(31);
		}
		if (list.Count == 0)
		{
			Network.Close();
			Engine.exiting = true;
		}
		else
		{
			CharacterSelect characterSelect = new CharacterSelect(list.ToList());
			characterSelect.ShowDialog();
		}
	}

	private static void SelectServer(PacketReader pvSrc)
	{
		pvSrc.ReadByte();
		ushort count = pvSrc.ReadUInt16();
		for (int i = 1; i <= count; i++)
		{
			pvSrc.ReadUInt16();
			pvSrc.ReadString(32);
			pvSrc.ReadByte();
			pvSrc.ReadByte();
			pvSrc.ReadInt32();
		}
		if (count < 1)
		{
			throw new Exception("No shards");
		}
		Network.Send(new PSelectServer(0));
		Network.Flush();
	}

	public static void Register(int packetID, int length, PacketCallback callback)
	{
		PacketHandlers.m_Registry.Register(packetID, length, callback);
	}

	private static void NewHealthBar(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile((int)pvSrc.ReadUInt32());
		if (mobile == null)
		{
			return;
		}
		ushort num = pvSrc.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			ushort num2 = pvSrc.ReadUInt16();
			bool value = pvSrc.ReadBoolean();
			switch (num2)
			{
			case 2:
				mobile.Flags[MobileFlag.YellowHits] = value;
				break;
			case 1:
				mobile.Flags[MobileFlag.Poisoned] = value;
				break;
			}
		}
		mobile.StatusBar?.OnRefresh();
	}

	private static void NewCharacterAnimation(PacketReader pvSrc)
	{
		Mobile mobile = World.FindMobile(pvSrc.ReadInt32());
		if (mobile == null)
		{
			return;
		}
		ushort num = (ushort)pvSrc.ReadInt16();
		ushort action = (ushort)pvSrc.ReadInt16();
		byte mode = pvSrc.ReadByte();
		int num2 = PacketHandlers.TranslateNewAnimAction(mobile, num, action, mode);
		if (num2 == 255)
		{
			return;
		}
		switch (Engine.m_Animations.GetBodyType(mobile.Body))
		{
		case BodyType.Monster:
			num2 %= 22;
			break;
		case BodyType.Sea:
		case BodyType.Animal:
			num2 = GraphicTranslators.Actions[0].Convert(num2);
			if (num2 < 0)
			{
				return;
			}
			num2 %= 13;
			break;
		case BodyType.Human:
		case BodyType.Equipment:
			num2 = GraphicTranslators.Actions[1].Convert(num2);
			if (num2 < 0)
			{
				return;
			}
			num2 %= 35;
			break;
		}
		int direction = Engine.GetAnimDirection(mobile.Direction) & 7;
		if (Engine.m_Animations.IsValid(mobile.Body, num2, direction))
		{
			Animation animation = new Animation();
			animation.Action = num2;
			animation.RepeatCount = 1;
			animation.Forward = true;
			animation.Delay = 1;
			animation.Repeat = (num == 1 || num == 2) && mobile.Body == 21;
			if (num2 == 0 || num2 == 2)
			{
				animation.Repeat = true;
			}
			mobile.Animation = animation;
			animation.Run();
		}
	}

	private static int TranslateNewAnimAction(Mobile m, ushort type, ushort action, byte mode)
	{
		BodyType bodyType = Engine.m_Animations.GetBodyType(m.Body);
		bool flag = bodyType == BodyType.Monster;
		bool flag2 = bodyType == BodyType.Animal || bodyType == BodyType.Sea;
		bool flag3 = bodyType == BodyType.Human;
		switch (type)
		{
		case 0:
			if (flag)
			{
				return (new int[4] { 4, 5, 6, 4 })[mode % 4];
			}
			if (flag2)
			{
				if (mode % 2 == 0)
				{
					return 5;
				}
				return 6;
			}
			if (m.IsMounted)
			{
				if (action != 1)
				{
					if (action != 2)
					{
						if (action <= 0)
						{
							return 29;
						}
						return 26;
					}
					return 28;
				}
				return 27;
			}
			return action switch
			{
				1 => 18, 
				2 => 19, 
				3 => 11, 
				4 => 9, 
				5 => 10, 
				6 => 12, 
				7 => 13, 
				8 => 14, 
				_ => 31, 
			};
		case 1:
		case 2:
			if (flag)
			{
				if (mode % 2 == 0)
				{
					return 16;
				}
				return 15;
			}
			if (flag2 || m.IsMounted)
			{
				return 255;
			}
			return 30;
		case 3:
			if (flag)
			{
				if (mode % 2 == 0)
				{
					return 3;
				}
				return 2;
			}
			if (bodyType == BodyType.Sea)
			{
				return 8;
			}
			if (flag2)
			{
				if (mode % 2 == 0)
				{
					return 22;
				}
				return 21;
			}
			if (mode % 2 == 0)
			{
				return 12;
			}
			return 8;
		case 4:
			if (flag)
			{
				return 10;
			}
			if (flag3)
			{
				if (!m.IsMounted)
				{
					return 20;
				}
				return 255;
			}
			return 7;
		case 5:
			if (flag || bodyType == BodyType.Sea)
			{
				if (mode % 2 == 0)
				{
					return 17;
				}
				return 18;
			}
			if (flag3)
			{
				if (!m.IsMounted)
				{
					if (mode % 2 == 0)
					{
						return 5;
					}
					return 6;
				}
				return 255;
			}
			return (new int[3] { 9, 10, 3 })[mode % 3];
		case 6:
		case 14:
			if (flag)
			{
				return 11;
			}
			if (bodyType == BodyType.Sea)
			{
				return 5;
			}
			if (flag2)
			{
				return 3;
			}
			if (!m.IsMounted)
			{
				return 34;
			}
			return 255;
		case 7:
			if (flag)
			{
				if (mode % 2 == 0)
				{
					return 17;
				}
				return 18;
			}
			if (flag3)
			{
				if (!m.IsMounted)
				{
					if (mode % 2 == 0)
					{
						return 9;
					}
					return 10;
				}
				return 255;
			}
			return 255;
		case 8:
			if (flag)
			{
				if (mode % 2 == 0)
				{
					return 12;
				}
				return 13;
			}
			if (flag3)
			{
				if (!m.IsMounted)
				{
					if (mode % 2 == 0)
					{
						return 12;
					}
					return 13;
				}
				return 255;
			}
			return 255;
		case 9:
		case 10:
			if (flag)
			{
				if (type != 9)
				{
					return 13;
				}
				return 20;
			}
			if (flag3)
			{
				if (action != 1)
				{
					return 12;
				}
				return 11;
			}
			return 255;
		case 11:
			if (flag)
			{
				return 12;
			}
			if (flag3 || flag2)
			{
				if (m.IsMounted)
				{
					return 255;
				}
				if (action != 1 && action != 2)
				{
					return 16;
				}
				return 17;
			}
			return 5;
		default:
			return action;
		}
	}

	private static void KREncryptionResponse(PacketReader pvSrc)
	{
	}

	private static void DisplayWayPoint(PacketReader pvSrc)
	{
		pvSrc.ReadUInt32();
		pvSrc.ReadUInt16();
		pvSrc.ReadUInt16();
		pvSrc.ReadSByte();
		pvSrc.ReadByte();
		pvSrc.ReadUInt16();
		pvSrc.ReadUInt16();
		pvSrc.ReadUInt32();
		pvSrc.ReadUnicodeLEString();
	}

	private static void RemoveWayPoint(PacketReader pvSrc)
	{
		pvSrc.ReadUInt32();
	}
}
