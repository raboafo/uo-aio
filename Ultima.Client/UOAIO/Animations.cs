using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SharpDX.Direct3D9;
using UOAIO.Assets;
using UOAIO.Profiles;
using Veritas;

namespace UOAIO;

public class Animations
{
	private class Loader
	{
		private Animations m_Owner;

		private Thread m_Thread;

		private int m_Index;

		private const string RelativeApplicationDataPath = "Veritas/Ultima Online/Cache/Anim-{0}";

		private const string RelativeLegacyPath = "data/ultima/cache/anim.{0}.uoi";

		public bool IsAlive => this.m_Thread != null && this.m_Thread.IsAlive;

		public Loader(Animations owner, int index)
		{
			this.m_Owner = owner;
			this.m_Index = index;
			this.m_Thread = new Thread(Thread_Start);
			this.m_Thread.IsBackground = true;
			this.m_Thread.Name = "Background Animation Loader";
		}

		public void Start()
		{
			if (this.m_Thread != null)
			{
				this.m_Thread.Start();
			}
		}

		public void Stop()
		{
			if (this.m_Thread != null && this.m_Thread.IsAlive)
			{
				this.m_Thread.Abort();
			}
		}

		public bool Wait()
		{
			return this.m_Thread == null || this.m_Thread.Join(10);
		}

		private void SetLoadedIndex(Entry3D[] entries, int count)
		{
			if (this.m_Index == 1)
			{
				this.m_Owner.m_Index = entries;
				this.m_Owner.m_Count = count;
			}
			else if (this.m_Index == 2)
			{
				this.m_Owner.m_Index2 = entries;
				this.m_Owner.m_Count2 = count;
			}
			else if (this.m_Index == 3)
			{
				this.m_Owner.m_Index3 = entries;
				this.m_Owner.m_Count3 = count;
			}
			else if (this.m_Index == 4)
			{
				this.m_Owner.m_Index4 = entries;
				this.m_Owner.m_Count4 = count;
			}
			else
			{
				this.m_Owner.m_Index5 = entries;
				this.m_Owner.m_Count5 = count;
			}
		}

		private static string GetCachePath(int index)
		{
			string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), $"Veritas/Ultima Online/Cache/Anim-{index}");
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(text));
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			return text;
		}

		private unsafe void Thread_Start()
		{
			if (AssetSourceManager.Animations.TryLoadIndex(this.m_Index, out var entries, out var count))
			{
				this.SetLoadedIndex(entries, count);
				return;
			}
			string cachePath = Loader.GetCachePath(this.m_Index);
			string path = Engine.FileManager.ResolveMUL(string.Format("anim{0}.mul", (this.m_Index == 1) ? "" : this.m_Index.ToString()));
			string path2 = Engine.FileManager.ResolveMUL(string.Format("anim{0}.idx", (this.m_Index == 1) ? "" : this.m_Index.ToString()));
			if (!File.Exists(path) || !File.Exists(path2))
			{
				if (this.m_Index == 1)
				{
					this.m_Owner.m_Index = new Entry3D[0];
					this.m_Owner.m_Count = 0;
				}
				else if (this.m_Index == 2)
				{
					this.m_Owner.m_Index2 = new Entry3D[0];
					this.m_Owner.m_Count2 = 0;
				}
				else if (this.m_Index == 3)
				{
					this.m_Owner.m_Index3 = new Entry3D[0];
					this.m_Owner.m_Count3 = 0;
				}
				else if (this.m_Index == 4)
				{
					this.m_Owner.m_Index4 = new Entry3D[0];
					this.m_Owner.m_Count4 = 0;
				}
				else
				{
					this.m_Owner.m_Index5 = new Entry3D[0];
					this.m_Owner.m_Count5 = 0;
				}
				return;
			}
			if (!File.Exists(cachePath))
			{
				string text = Engine.FileManager.BasePath($"data/ultima/cache/anim.{this.m_Index}.uoi");
				if (File.Exists(text))
				{
					try
					{
						File.Move(text, cachePath);
					}
					catch
					{
						File.Copy(text, cachePath, overwrite: false);
					}
				}
			}
			if (File.Exists(cachePath))
			{
				using FileStream fileStream = new FileStream(cachePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				if (fileStream.Length >= 21)
				{
					using BinaryReader binaryReader = new BinaryReader(fileStream);
					if (binaryReader.ReadBoolean())
					{
						DateTime timeStamp = Engine.GetTimeStamp(path);
						DateTime timeStamp2 = Engine.GetTimeStamp(path2);
						DateTime dateTime = DateTime.FromFileTime(binaryReader.ReadInt64());
						DateTime dateTime2 = DateTime.FromFileTime(binaryReader.ReadInt64());
						if (timeStamp == dateTime && timeStamp2 == dateTime2)
						{
							int num = binaryReader.ReadInt32();
							if (binaryReader.BaseStream.Length >= 21 + num * 12)
							{
								Entry3D[] array = new Entry3D[num];
								array = new Entry3D[num];
								fixed (Entry3D* buffer = array)
								{
									UnsafeMethods.ReadFile((FileStream)binaryReader.BaseStream, buffer, num * 12);
								}
								if (this.m_Index == 1)
								{
									this.m_Owner.m_Index = array;
									this.m_Owner.m_Count = num;
								}
								else if (this.m_Index == 2)
								{
									this.m_Owner.m_Index2 = array;
									this.m_Owner.m_Count2 = num;
								}
								else if (this.m_Index == 3)
								{
									this.m_Owner.m_Index3 = array;
									this.m_Owner.m_Count3 = num;
								}
								else if (this.m_Index == 4)
								{
									this.m_Owner.m_Index4 = array;
									this.m_Owner.m_Count4 = num;
								}
								else
								{
									this.m_Owner.m_Index5 = array;
									this.m_Owner.m_Count5 = num;
								}
								return;
							}
						}
					}
				}
			}
			using FileStream fileStream2 = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read);
			int num2 = (int)(fileStream2.Length / 12);
			Entry3D[] array2 = new Entry3D[num2];
			fixed (Entry3D* ptr = array2)
			{
				UnsafeMethods.ReadFile(fileStream2, ptr, num2 * 12);
				using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryReader binaryReader2 = new BinaryReader(input);
					Entry3D* ptr2 = ptr;
					for (Entry3D* ptr3 = ptr + num2; ptr2 < ptr3; ptr2++)
					{
						if (ptr2->m_Lookup < 0)
						{
							continue;
						}
						binaryReader2.BaseStream.Seek(ptr2->m_Lookup + 512, SeekOrigin.Begin);
						int num3 = binaryReader2.ReadInt32() & 0xFF;
						int i = 0;
						int num4 = -10000;
						for (; i < num3; i++)
						{
							binaryReader2.BaseStream.Seek(ptr2->m_Lookup + 516 + (i << 2), SeekOrigin.Begin);
							binaryReader2.BaseStream.Seek(ptr2->m_Lookup + 514 + binaryReader2.ReadInt32(), SeekOrigin.Begin);
							int num5 = binaryReader2.ReadInt16();
							int num6 = binaryReader2.ReadInt32() >> 16;
							if (num6 + num5 > num4)
							{
								num4 = num6 + num5;
							}
						}
						ptr2->m_Extra = num3 | (num4 << 8);
					}
				}
				using FileStream output = new FileStream(cachePath, FileMode.Create, FileAccess.Write, FileShare.None);
				using BinaryWriter binaryWriter = new BinaryWriter(output);
				binaryWriter.Write(value: false);
				binaryWriter.Write(Engine.GetTimeStamp(path).ToFileTime());
				binaryWriter.Write(Engine.GetTimeStamp(path2).ToFileTime());
				binaryWriter.Write(num2);
				UnsafeMethods.WriteFile((FileStream)binaryWriter.BaseStream, ptr, num2 * 12);
				binaryWriter.Seek(0, SeekOrigin.Begin);
				binaryWriter.Write(value: true);
			}
			if (this.m_Index == 1)
			{
				this.m_Owner.m_Index = array2;
				this.m_Owner.m_Count = num2;
			}
			else if (this.m_Index == 2)
			{
				this.m_Owner.m_Index2 = array2;
				this.m_Owner.m_Count2 = num2;
			}
			else if (this.m_Index == 3)
			{
				this.m_Owner.m_Index3 = array2;
				this.m_Owner.m_Count3 = num2;
			}
			else if (this.m_Index == 4)
			{
				this.m_Owner.m_Index4 = array2;
				this.m_Owner.m_Count4 = num2;
			}
			else
			{
				this.m_Owner.m_Index5 = array2;
				this.m_Owner.m_Count5 = num2;
			}
		}
	}

	private enum PixelType
	{
		None,
		Inner,
		Outer
	}

	public Entry3D[] m_Index;

	public Entry3D[] m_Index2;

	public Entry3D[] m_Index3;

	public Entry3D[] m_Index4;

	public Entry3D[] m_Index5;

	private int m_Count;

	private int m_Count2;

	private int m_Count3;

	private int m_Count4;

	private int m_Count5;

	private static Stream m_Stream;

	private static Stream m_Stream2;

	private static Stream m_Stream3;

	private static Stream m_Stream4;

	private static Stream m_Stream5;

	private static BodyType[] m_Types;

	private int[] m_Table;

	private static Loader m_Loader;

	private static Loader m_Loader2;

	private static Loader m_Loader3;

	private static Loader m_Loader4;

	private static Loader m_Loader5;

	private int m_Action;

	private int m_BodyID;

	private int m_SA_Body;

	private int m_SA_Dir;

	private MountTable m_MountTable;

	private const int DoubleXor = -2145386496;

	private const int DoubleOpaque = -2147450880;

	private short[] m_Palette = new short[256];

	private int[] m_Palette32 = new int[256];

	private byte[] m_Data;

	private static ushort[] _guassianBlurMatrix;

	public List<Frames> m_Frames = new List<Frames>();

	public static bool IsLoading => Animations.m_Loader.IsAlive || Animations.m_Loader2.IsAlive || Animations.m_Loader3.IsAlive || Animations.m_Loader4.IsAlive || Animations.m_Loader5.IsAlive;

	public MountTable MountTable
	{
		get
		{
			if (this.m_MountTable == null)
			{
				this.m_MountTable = new MountTable();
			}
			return this.m_MountTable;
		}
	}

	public void Translate(ref int bodyID)
	{
		if (this.m_Table == null)
		{
			this.LoadTable();
		}
		if (bodyID <= 0 || bodyID >= this.m_Table.Length)
		{
			bodyID = 0;
		}
		else
		{
			bodyID = this.m_Table[bodyID] & 0x7FFF;
		}
	}

	public void Translate(ref int bodyID, ref IHue h)
	{
		if (this.m_Table == null)
		{
			this.LoadTable();
		}
		if (bodyID <= 0 || bodyID >= this.m_Table.Length)
		{
			bodyID = 0;
			return;
		}
		int num = this.m_Table[bodyID];
		if ((num & int.MinValue) != 0)
		{
			bodyID = num & 0x7FFF;
			if (h == Hues.Default)
			{
				h = Hues.Load((num >> 15) & 0xFFFF);
			}
		}
	}

	public void Translate(ref int bodyID, ref int hue)
	{
		if (this.m_Table == null)
		{
			this.LoadTable();
		}
		if (bodyID <= 0 || bodyID >= this.m_Table.Length)
		{
			bodyID = 0;
			return;
		}
		int num = this.m_Table[bodyID];
		if ((num & int.MinValue) != 0)
		{
			bodyID = num & 0x7FFF;
			if (hue == 0)
			{
				hue = (num >> 15) & 0xFFFF;
			}
		}
	}

	private void LoadTable()
	{
		int num = 400 + (this.m_Index.Length - 35000) / 175;
		this.m_Table = new int[num];
		for (int i = 0; i < num; i++)
		{
			GraphicTranslation graphicTranslation = GraphicTranslators.Bodies[i];
			if (graphicTranslation == null || BodyConverter.Contains(i))
			{
				this.m_Table[i] = i;
			}
			else
			{
				this.m_Table[i] = graphicTranslation.FallbackId | int.MinValue | (((graphicTranslation.FallbackData ^ 0x8000) & 0xFFFF) << 15);
			}
		}
	}

	public static bool WaitLoading()
	{
		if (Animations.m_Loader.IsAlive && !Animations.m_Loader.Wait())
		{
			return false;
		}
		if (Animations.m_Loader2.IsAlive && !Animations.m_Loader2.Wait())
		{
			return false;
		}
		if (Animations.m_Loader3.IsAlive && !Animations.m_Loader3.Wait())
		{
			return false;
		}
		if (Animations.m_Loader4.IsAlive && !Animations.m_Loader4.Wait())
		{
			return false;
		}
		if (Animations.m_Loader5.IsAlive && !Animations.m_Loader5.Wait())
		{
			return false;
		}
		return true;
	}

	public static void StartLoading()
	{
		Animations.m_Loader.Start();
		Animations.m_Loader2.Start();
		Animations.m_Loader3.Start();
		Animations.m_Loader4.Start();
		Animations.m_Loader5.Start();
	}

	public BodyType GetBodyType(int body)
	{
		if (body >= 0 && body < Animations.m_Types.Length)
		{
			return Animations.m_Types[body];
		}
		return BodyType.Empty;
	}

	public Animations()
	{
		ArchivedFile archivedFile = Engine.FileManager.GetArchivedFile("play/config/body-types.dat");
		if (archivedFile != null)
		{
			using BinaryReader binaryReader = new BinaryReader(archivedFile.Download());
			Animations.m_Types = new BodyType[(int)binaryReader.BaseStream.Length];
			for (int i = 0; i < Animations.m_Types.Length; i++)
			{
				Animations.m_Types[i] = (BodyType)binaryReader.ReadByte();
			}
			if (970 < Animations.m_Types.Length)
			{
				Animations.m_Types[970] = BodyType.Human;
			}
		}
		else
		{
			Debug.Error("data/config/body-types.dat does not exist");
			Animations.m_Types = new BodyType[0];
		}
		if (!AssetSourceManager.Animations.IsUoo)
		{
			try
			{
				Animations.m_Stream = Engine.FileManager.OpenMUL(Files.AnimMul);
			}
			catch
			{
			}
			try
			{
				Animations.m_Stream2 = Engine.FileManager.OpenMUL("Anim2.mul");
			}
			catch
			{
			}
			try
			{
				Animations.m_Stream3 = Engine.FileManager.OpenMUL("Anim3.mul");
			}
			catch
			{
			}
			try
			{
				Animations.m_Stream4 = Engine.FileManager.OpenMUL("Anim4.mul");
			}
			catch
			{
			}
			try
			{
				Animations.m_Stream5 = Engine.FileManager.OpenMUL("Anim5.mul");
			}
			catch
			{
			}
		}
		Animations.m_Loader = new Loader(this, 1);
		Animations.m_Loader2 = new Loader(this, 2);
		Animations.m_Loader3 = new Loader(this, 3);
		Animations.m_Loader4 = new Loader(this, 4);
		Animations.m_Loader5 = new Loader(this, 5);
	}

	public Frame GetFrame(IAnimationOwner owner, int BodyID, int ActionID, int Direction, int Frame, int xCenter, int yCenter, IHue h, ref int TextureX, ref int TextureY, bool preserveHue)
	{
		if (BodyID <= 0)
		{
			return UOAIO.Frame.Empty;
		}
		this.m_Action = ActionID;
		Direction &= 7;
		int num = Direction;
		if (Direction > 4)
		{
			num -= (Direction - 4) * 2;
		}
		this.Translate(ref BodyID, ref h);
		this.m_BodyID = BodyID;
		int realIDNoMap = this.GetRealIDNoMap(BodyID, ActionID, num);
		if (realIDNoMap < 0 || realIDNoMap >= this.m_Count)
		{
			return UOAIO.Frame.Empty;
		}
		realIDNoMap <<= 2;
		if (Preferences.Current.RenderSettings.SmoothCharacters)
		{
			realIDNoMap |= 2;
		}
		if (!Preferences.Current.RenderSettings.AnimatedCharacters)
		{
			BodyType bodyType = this.GetBodyType(BodyID);
			bool flag = true;
			switch (bodyType)
			{
			case BodyType.Human:
			case BodyType.Equipment:
				flag = this.m_Action != 21 && this.m_Action != 22;
				break;
			case BodyType.Sea:
			case BodyType.Animal:
				flag = this.m_Action != 8 && this.m_Action != 12;
				break;
			case BodyType.Monster:
				flag = this.m_Action != 2 && this.m_Action != 3;
				break;
			}
			if (flag)
			{
				realIDNoMap |= 1;
				Frame = 0;
			}
		}
		Frames frames = ((owner != null) ? owner.GetOwnedFrames(h, realIDNoMap) : h.GetAnimation(realIDNoMap));
		if (Frame >= frames.FrameCount || Frame < 0)
		{
			return UOAIO.Frame.Empty;
		}
		Frame frame = frames.FrameList[Frame];
		if (frame != null && frame.Image != null && !frame.Image.IsEmpty())
		{
			if (Direction > 4)
			{
				TextureX = xCenter + (frame.CenterX - frame.Image.Width);
			}
			else
			{
				TextureX = xCenter - frame.CenterX;
			}
			TextureY = yCenter - frame.Image.Height - frame.CenterY;
		}
		frame.Image.Flip = Direction > 4;
		return frame;
	}

	public bool IsValid(int bodyID, int action, int direction)
	{
		int realID = this.GetRealID(bodyID, action, direction);
		return this.ConvertRealID(ref realID) switch
		{
			1 => realID >= 0 && realID < this.m_Index.Length && this.m_Index[realID].m_Lookup >= 0, 
			2 => realID >= 0 && realID < this.m_Index2.Length && this.m_Index2[realID].m_Lookup >= 0, 
			3 => realID >= 0 && realID < this.m_Index3.Length && this.m_Index3[realID].m_Lookup >= 0, 
			4 => realID >= 0 && realID < this.m_Index4.Length && this.m_Index4[realID].m_Lookup >= 0, 
			_ => realID >= 0 && realID < this.m_Index5.Length && this.m_Index5[realID].m_Lookup >= 0, 
		};
	}

	public int SafeAction(int desired, int fb1)
	{
		if (this.IsValid(this.m_SA_Body, desired, this.m_SA_Dir))
		{
			return desired;
		}
		return fb1;
	}

	public int SafeAction(int desired, int fb1, int fb2)
	{
		if (this.IsValid(this.m_SA_Body, desired, this.m_SA_Dir))
		{
			return desired;
		}
		if (this.IsValid(this.m_SA_Body, fb1, this.m_SA_Dir))
		{
			return fb1;
		}
		return fb2;
	}

	public int ConvertAction(int BodyID, int Serial, int X, int Y, int Direction, GenericAction g, Mobile m)
	{
		this.Translate(ref BodyID);
		BodyType bodyType = this.GetBodyType(BodyID);
		this.m_SA_Body = BodyID;
		this.m_SA_Dir = Direction;
		if (bodyType == BodyType.Monster)
		{
			switch (g)
			{
			case GenericAction.Walk:
				return 0;
			case GenericAction.Run:
				return 0;
			case GenericAction.Stand:
				return 1;
			case GenericAction.Die:
				return this.SafeAction(2 + ((Direction >> 7) & 1), 2, 3);
			case GenericAction.MountedWalk:
				return 0;
			case GenericAction.MountedRun:
				return 0;
			case GenericAction.MountedStand:
				return 1;
			}
		}
		else if (bodyType == BodyType.Animal || bodyType == BodyType.Sea)
		{
			switch (g)
			{
			case GenericAction.Walk:
				return 0;
			case GenericAction.Run:
				return this.SafeAction(1, 0);
			case GenericAction.Stand:
				return 2;
			case GenericAction.Die:
				return this.SafeAction(8 + ((Direction >> 7) & 1) * 4, 8, 12);
			case GenericAction.MountedWalk:
				return 0;
			case GenericAction.MountedRun:
				return this.SafeAction(1, 0);
			case GenericAction.MountedStand:
				return 2;
			}
		}
		else if (bodyType == BodyType.Human || bodyType == BodyType.Equipment)
		{
			switch (g)
			{
			case GenericAction.Walk:
				if (m != null && m.Warmode)
				{
					return this.SafeAction(15, m.UsingTwoHandedWeapon() ? 1 : 0, 0);
				}
				if (m.UsingTwoHandedWeapon())
				{
					return this.SafeAction(1, 0);
				}
				return this.SafeAction(0, 1, 15);
			case GenericAction.Run:
				if (m != null && m.UsingTwoHandedWeapon())
				{
					return this.SafeAction(3, 2);
				}
				return this.SafeAction(2, 3);
			case GenericAction.Stand:
				if (m != null && m.Warmode)
				{
					if (m.UsingTwoHandedWeapon())
					{
						return this.SafeAction(8, 7, 4);
					}
					return this.SafeAction(7, 8, 4);
				}
				return this.SafeAction(4, 7, 8);
			case GenericAction.Die:
				return this.SafeAction(21 + ((Direction >> 7) & 1), 21, 22);
			case GenericAction.MountedWalk:
				return 23;
			case GenericAction.MountedRun:
				return 24;
			case GenericAction.MountedStand:
				return 25;
			case GenericAction.Sit:
				return 26;
			}
		}
		return 0;
	}

	public int ConvertMountItemToBody(int itemID)
	{
		if (this.m_MountTable == null)
		{
			this.m_MountTable = new MountTable();
		}
		return this.m_MountTable.Translate(itemID);
	}

	public int GetRealIDNoMap(int body, int action, int direction)
	{
		direction &= 7;
		int num = ((body >= 400) ? ((body - 400) * 175 + 35000) : ((body < 200) ? (body * 110) : ((body - 200) * 65 + 22000)));
		if (direction > 4)
		{
			direction -= (direction - 4) * 2;
		}
		return num + (action * 5 + direction);
	}

	public int ConvertRealID(ref int realID)
	{
		int bodyID;
		int num;
		int num2;
		if (realID >= 35000)
		{
			bodyID = 400 + (realID - 35000) / 175;
			num = (realID - 35000) % 175 / 5;
			num2 = (realID - 35000) % 175 % 5;
		}
		else if (realID >= 22000)
		{
			bodyID = 200 + (realID - 22000) / 65;
			num = (realID - 22000) % 65 / 5;
			num2 = (realID - 22000) % 65 % 5;
		}
		else
		{
			bodyID = realID / 110;
			num = realID % 110 / 5;
			num2 = realID % 110 % 5;
		}
		int num3 = BodyConverter.Convert(ref bodyID);
		switch (num3)
		{
		case 2:
			if (bodyID < 200)
			{
				realID = bodyID * 110 + num * 5 + num2;
			}
			else
			{
				realID = 22000 + (bodyID - 200) * 65 + num * 5 + num2;
			}
			break;
		case 3:
			if (bodyID >= 200)
			{
				if (bodyID >= 400)
				{
					realID = (bodyID - 400) * 175 + 35000 + num * 5 + num2;
				}
				else
				{
					realID = (bodyID - 200) * 110 + 22000 + num * 5 + num2;
				}
			}
			else
			{
				realID = bodyID * 65 + 9000 + num * 5 + num2;
			}
			break;
		case 4:
			if (bodyID < 200)
			{
				realID = bodyID * 110 + num * 5 + num2;
			}
			else if (bodyID < 400)
			{
				realID = 22000 + (bodyID - 200) * 65 + num * 5 + num2;
			}
			else
			{
				realID = 35000 + (bodyID - 400) * 175 + num * 5 + num2;
			}
			break;
		case 5:
			if (bodyID < 200 && bodyID != 34)
			{
				realID = bodyID * 110 + num * 5 + num2;
			}
			else if (bodyID < 400)
			{
				realID = 22000 + (bodyID - 200) * 65 + num * 5 + num2;
			}
			else
			{
				realID = 35000 + (bodyID - 400) * 175 + num * 5 + num2;
			}
			break;
		}
		if (bodyID < 200 && bodyID != 34)
		{
			realID = bodyID * 110 + num * 5 + num2;
		}
		else if (bodyID < 400)
		{
			realID = 22000 + (bodyID - 200) * 65 + num * 5 + num2;
		}
		else
		{
			realID = 35000 + (bodyID - 400) * 175 + num * 5 + num2;
		}
		return num3;
	}

	public int GetRealID(int BodyID, int ActionID, int Direction)
	{
		Direction &= 7;
		this.Translate(ref BodyID);
		int num;
		if (BodyID >= 600)
		{
			num = (BodyID - 600) * 285 + 50000;
		}
		num = ((BodyID >= 400) ? ((BodyID - 400) * 175 + 35000) : ((BodyID < 200) ? (BodyID * 110) : ((BodyID - 200) * 65 + 22000)));
		if (Direction > 4)
		{
			Direction -= (Direction - 4) * 2;
		}
		return num + (ActionID * 5 + Direction);
	}

	public void UpdateInstance(long SeedID, object Anim)
	{
	}

	public void DisposeInstance(object Anim)
	{
		Frames frames = (Frames)Anim;
		if (frames != null && frames.FrameList != null)
		{
			int num = frames.FrameList.Length;
			for (int i = 0; i < num; i++)
			{
				if (frames.FrameList[i] != null && frames.FrameList[i].Image != null)
				{
					frames.FrameList[i].Image.Dispose();
					frames.FrameList[i].Image = null;
				}
			}
		}
		Anim = null;
	}

	public unsafe Frames Create(int realID, IHue hue)
	{
		bool flag = (realID & 1) != 0;
		bool flag2 = (realID & 2) != 0;
		realID >>= 2;
		bool flag3 = hue is Hues.ShadowHue;
		int length;
		int lookup;
		int num;
		Stream stream;
		int sourceIndex = this.ConvertRealID(ref realID);
		switch (sourceIndex)
		{
		case 1:
		{
			if (realID < 0 || realID >= this.m_Count || realID >= this.m_Index.Length)
			{
				return Frames.Empty;
			}
			Entry3D entry3D3 = this.m_Index[realID];
			length = entry3D3.m_Length;
			lookup = entry3D3.m_Lookup;
			num = entry3D3.m_Extra & 0xFF;
			stream = Animations.m_Stream;
			break;
		}
		case 2:
		{
			if (realID < 0 || realID >= this.m_Count2 || realID >= this.m_Index2.Length)
			{
				return Frames.Empty;
			}
			Entry3D entry3D4 = this.m_Index2[realID];
			length = entry3D4.m_Length;
			lookup = entry3D4.m_Lookup;
			num = entry3D4.m_Extra & 0xFF;
			stream = Animations.m_Stream2;
			break;
		}
		case 3:
		{
			if (realID < 0 || realID >= this.m_Count3 || realID >= this.m_Index3.Length)
			{
				return Frames.Empty;
			}
			Entry3D entry3D2 = this.m_Index3[realID];
			length = entry3D2.m_Length;
			lookup = entry3D2.m_Lookup;
			num = entry3D2.m_Extra & 0xFF;
			stream = Animations.m_Stream3;
			break;
		}
		case 4:
		{
			if (realID < 0 || realID >= this.m_Count4 || realID >= this.m_Index4.Length)
			{
				return Frames.Empty;
			}
			Entry3D entry3D5 = this.m_Index4[realID];
			length = entry3D5.m_Length;
			lookup = entry3D5.m_Lookup;
			num = entry3D5.m_Extra & 0xFF;
			stream = Animations.m_Stream4;
			break;
		}
		default:
		{
			if (realID < 0 || realID >= this.m_Count5 || realID >= this.m_Index5.Length)
			{
				return Frames.Empty;
			}
			Entry3D entry3D = this.m_Index5[realID];
			length = entry3D.m_Length;
			lookup = entry3D.m_Lookup;
			num = entry3D.m_Extra & 0xFF;
			stream = Animations.m_Stream5;
			break;
		}
		}
		if (flag)
		{
			num = this.ReduceFrameCountForSingleFrameMode(num);
		}
		if (lookup < 0 || num <= 0)
		{
			return Frames.Empty;
		}
		if (stream == null)
		{
			return this.CreateFromUoo(sourceIndex, realID, lookup, num, flag2, flag3, hue);
		}
		if (length <= 0)
		{
			return Frames.Empty;
		}
		if (this.m_Data == null || length > this.m_Data.Length)
		{
			this.m_Data = new byte[length];
		}
		stream.Seek(lookup, SeekOrigin.Begin);
		stream.Read(this.m_Data, 0, length);
		fixed (short* palette = this.m_Palette)
		{
			short* ptr = palette;
			fixed (int* palette2 = this.m_Palette32)
			{
				int* ptr2 = palette2;
				fixed (byte* data = this.m_Data)
				{
					if (!flag3)
					{
						hue.CopyPixels(data, ptr, 256);
					}
					for (int i = 0; i < 256; i++)
					{
						ptr2[i] = Engine.C16232(ptr[i]);
					}
					Frames frames = new Frames();
					frames.FrameCount = num;
					frames.FrameList = new Frame[num];
					for (int j = 0; j < num; j++)
					{
						int num2 = *(int*)(data + 516 + (j << 2));
						byte* ptr3 = data + 512 + num2;
						short* ptr4 = (short*)ptr3;
						int num3 = (flag3 ? 8 : 0);
						int num4 = (flag3 ? 17 : 0);
						int num5 = *ptr4;
						int num6 = ptr4[1];
						int num7 = ptr4[2];
						int num8 = ptr4[3];
						ptr3 += 8;
						frames.FrameList[j] = new Frame();
						frames.FrameList[j].CenterX = num5 + num3;
						frames.FrameList[j].CenterY = num6 - num3;
						if (num7 <= 0 || num8 <= 0)
						{
							frames.FrameList[j].Image = Texture.Empty;
							continue;
						}
						Texture texture = new Texture(num7 + num4, num8 + num4, TextureTransparency.Simple);
						if (texture.IsEmpty())
						{
							frames.FrameList[j].Image = Texture.Empty;
							continue;
						}
						texture._shaderData = hue.ShaderData;
						int num9 = 0;
						short* ptr5 = null;
						int num10 = num3 + num5 - 512;
						int num11 = num6 + num8 + num3 - 512;
						LockData ld = texture.Lock(LockFlags.WriteOnly);
						short* pvSrc = (short*)ld.pvSrc;
						int num12 = ld.Pitch >> 1;
						ushort* pvSrc2 = (ushort*)ld.pvSrc;
						int num13 = ld.Pitch >> 1;
						int num14 = num13 - ld.Width;
						ushort* ptr6 = pvSrc2 + num13 * ld.Height;
						pvSrc += num10;
						pvSrc += num11 * num12;
						if (flag3)
						{
							ushort* ptr7 = pvSrc2;
							while (ptr7 < ptr6)
							{
								*(ptr7++) = 0;
							}
							fixed (ushort* guassianBlurMatrix = Animations._guassianBlurMatrix)
							{
								while ((num9 = *(int*)ptr3) != 2147450879)
								{
									ptr3 += 4;
									if (((num9 >> 12) & 0x200) != 0)
									{
										num9 ^= -2145386496;
										ptr5 = pvSrc + (((num9 >> 12) & 0x3FF) * num12 + ((num9 >> 22) & 0x3FF));
										for (short* ptr8 = ptr5 + (num9 & 0xFFF); ptr5 < ptr8; ptr5++)
										{
											ushort* ptr9 = guassianBlurMatrix;
											for (int k = -num3; k <= num3; k++)
											{
												ushort* ptr10 = (ushort*)(ptr5 + k * num12 - num3);
												ushort* ptr11 = ptr10 + num4;
												while (ptr10 < ptr11)
												{
													ushort* intPtr = ptr10++;
													*intPtr += *(ptr9++);
												}
											}
										}
									}
									ptr3 += num9 & 0xFFF;
								}
								ptr7 = pvSrc2;
								while (ptr7 < ptr6)
								{
									int num15 = *ptr7 * 31 / 22409;
									*(ptr7++) = (ushort)(0x8000 | (num15 << 10) | (num15 << 5) | num15);
								}
							}
						}
						else
						{
							for (; (num9 = *(int*)ptr3) != 2147450879; ptr3 += num9 & 3)
							{
								ptr3 += 4;
								num9 ^= -2145386496;
								ptr5 = pvSrc + (((num9 >> 12) & 0x3FF) * num12 + ((num9 >> 22) & 0x3FF));
								short* ptr8 = ptr5 + (num9 & 0xFFC);
								while (ptr5 < ptr8)
								{
									*ptr5 = ptr[(int)(*ptr3)];
									ptr5[1] = ptr[(int)ptr3[1]];
									ptr5[2] = ptr[(int)ptr3[2]];
									ptr5[3] = ptr[(int)ptr3[3]];
									ptr5 += 4;
									ptr3 += 4;
								}
								switch (num9 & 3)
								{
								case 3:
									ptr5[2] = ptr[(int)ptr3[2]];
									goto case 2;
								case 2:
									ptr5[1] = ptr[(int)ptr3[1]];
									break;
								case 1:
									break;
								default:
									continue;
								}
								*ptr5 = ptr[(int)(*ptr3)];
							}
							if (flag2)
							{
								Texture texture2 = texture;
								texture = new Texture(num7, num8, Format.A8R8G8B8, TextureTransparency.Complex);
								texture._shaderData = hue.ShaderData;
								LockData lockData = texture.Lock(LockFlags.WriteOnly);
								for (int l = 0; l < ld.Height; l++)
								{
									for (int m = 0; m < ld.Width; m++)
									{
										switch (Animations.GetPixelType(ld, m, l))
										{
										case PixelType.Inner:
										{
											ushort c = ((ushort*)((byte*)ld.pvSrc + (nint)(l * (ld.Pitch >> 1)) * (nint)2))[m];
											int* ptr15 = (int*)((byte*)lockData.pvSrc + (nint)(l * (lockData.Pitch >> 2)) * (nint)4) + m;
											*ptr15 = Engine.C16232(c) | -16777216;
											break;
										}
										case PixelType.Outer:
										{
											ushort num16 = ((ushort*)((byte*)ld.pvSrc + (nint)(l * (ld.Pitch >> 1)) * (nint)2))[m];
											int num17 = 0;
											int num18 = 0;
											int num19 = 0;
											int num20 = 0;
											for (int n = -1; n <= 1; n++)
											{
												for (int num21 = -1; num21 <= 1; num21++)
												{
													if (Animations.GetPixelType(ld, m + num21, l + n) == PixelType.Inner)
													{
														ushort* ptr12 = (ushort*)((byte*)ld.pvSrc + (nint)((l + n) * (ld.Pitch >> 1)) * (nint)2) + (m + num21);
														num17 += (*ptr12 >> 10) & 0x1F;
														num18 += (*ptr12 >> 5) & 0x1F;
														num19 += *ptr12 & 0x1F;
														num20++;
													}
												}
											}
											if (num20 > 0)
											{
												num17 = num17 * 255 / (31 * num20);
												num18 = num18 * 255 / (31 * num20);
												num19 = num19 * 255 / (31 * num20);
												float num22 = (float)(Engine.GrayScale(num16) * 255) / 31f / Engine.GrayScale(num17, num18, num19);
												int num23 = (int)((1f + 2f * num22) / 3f * 255f);
												if (num23 < 0)
												{
													num23 = 0;
												}
												else if (num23 > 255)
												{
													num23 = 255;
												}
												int* ptr13 = (int*)((byte*)lockData.pvSrc + (nint)(l * (lockData.Pitch >> 2)) * (nint)4) + m;
												*ptr13 = (num23 << 24) | Engine.Blend32((num17 << 16) | (num18 << 8) | num19, Engine.C16232(num16), 127);
											}
											else
											{
												int* ptr14 = (int*)((byte*)lockData.pvSrc + (nint)(l * (lockData.Pitch >> 2)) * (nint)4) + m;
												*ptr14 = -16777216 | Engine.C16232(num16);
											}
											break;
										}
										}
									}
								}
								texture2.Unlock();
								texture2.Dispose();
							}
						}
						texture.Unlock();
						texture.SetPriority(0);
						frames.FrameList[j].Image = texture;
					}
					this.m_Frames.Add(frames);
					return frames;
				}
			}
		}
	}

	private int ReduceFrameCountForSingleFrameMode(int frameCount)
	{
		BodyType bodyType = this.GetBodyType(this.m_BodyID);
		bool flag = true;
		switch (bodyType)
		{
		case BodyType.Human:
		case BodyType.Equipment:
			flag = this.m_Action != 21 && this.m_Action != 22;
			break;
		case BodyType.Sea:
		case BodyType.Animal:
			flag = this.m_Action != 8 && this.m_Action != 12;
			break;
		case BodyType.Monster:
			flag = this.m_Action != 2 && this.m_Action != 3;
			break;
		}
		if (flag)
		{
			return 1;
		}
		return frameCount;
	}

	private void UpdateEntryMetadata(int index, int realID, int frameCount, int maxHeight)
	{
		Entry3D[] index2 = this.GetIndex(index);
		if (index2 == null || realID < 0 || realID >= index2.Length)
		{
			return;
		}
		if (frameCount < 0)
		{
			frameCount = 0;
		}
		if (maxHeight < 0)
		{
			maxHeight = 0;
		}
		frameCount = Math.Min(frameCount, 255);
		index2[realID].m_Extra = frameCount | (maxHeight << 8);
	}

	private Frames CreateFromUoo(int index, int realID, int lookup, int frameCount, bool smooth, bool shadow, IHue hue)
	{
		if (!AssetSourceManager.Animations.TryGetFrames(lookup, out var framesData, out var maxHeight) || framesData == null || framesData.Length == 0)
		{
			return Frames.Empty;
		}
		if (frameCount <= 0 || frameCount > framesData.Length)
		{
			frameCount = framesData.Length;
		}
		Frames frames = new Frames();
		frames.FrameCount = frameCount;
		frames.FrameList = new Frame[frameCount];
		for (int i = 0; i < frameCount; i++)
		{
			UooAnimationFrameData uooAnimationFrameData = framesData[i];
			frames.FrameList[i] = new Frame();
			int num = (shadow ? 8 : 0);
			frames.FrameList[i].CenterX = uooAnimationFrameData.CenterX + num;
			frames.FrameList[i].CenterY = uooAnimationFrameData.CenterY - num;
			if (uooAnimationFrameData.Pixels == null || uooAnimationFrameData.Width <= 0 || uooAnimationFrameData.Height <= 0 || uooAnimationFrameData.Pixels.Length < uooAnimationFrameData.Width * uooAnimationFrameData.Height)
			{
				frames.FrameList[i].Image = Texture.Empty;
				continue;
			}
			Texture image = (shadow ? this.CreateShadowTextureFromUooFrame(uooAnimationFrameData, hue) : this.CreateTextureFromUooFrame(uooAnimationFrameData, hue, smooth));
			frames.FrameList[i].Image = (image ?? Texture.Empty);
		}
		this.UpdateEntryMetadata(index, realID, framesData.Length, maxHeight);
		this.m_Frames.Add(frames);
		return frames;
	}

	private unsafe Texture CreateTextureFromUooFrame(UooAnimationFrameData frameData, IHue hue, bool smooth)
	{
		Texture texture = new Texture(frameData.Width, frameData.Height, TextureTransparency.Simple);
		if (texture.IsEmpty())
		{
			return Texture.Empty;
		}
		texture._shaderData = hue.ShaderData;
		LockData ld = texture.Lock(LockFlags.WriteOnly);
		ushort* pvSrc = (ushort*)ld.pvSrc;
		int num = ld.Pitch >> 1;
		for (int i = 0; i < frameData.Height; i++)
		{
			ushort* ptr = pvSrc + i * num;
			int num2 = i * frameData.Width;
			for (int j = 0; j < frameData.Width; j++)
			{
				ushort num3 = frameData.Pixels[num2 + j];
				if (num3 != 0)
				{
					num3 |= 0x8000;
					num3 = hue.Pixel(num3);
				}
				ptr[j] = num3;
			}
		}
		if (smooth)
		{
			Texture texture2 = texture;
			texture = new Texture(frameData.Width, frameData.Height, Format.A8R8G8B8, TextureTransparency.Complex);
			texture._shaderData = hue.ShaderData;
			LockData lockData = texture.Lock(LockFlags.WriteOnly);
			for (int k = 0; k < ld.Height; k++)
			{
				for (int l = 0; l < ld.Width; l++)
				{
					switch (Animations.GetPixelType(ld, l, k))
					{
					case PixelType.Inner:
					{
						ushort c = ((ushort*)((byte*)ld.pvSrc + (nint)(k * (ld.Pitch >> 1)) * (nint)2))[l];
						int* ptr4 = (int*)((byte*)lockData.pvSrc + (nint)(k * (lockData.Pitch >> 2)) * (nint)4) + l;
						*ptr4 = Engine.C16232(c) | -16777216;
						break;
					}
					case PixelType.Outer:
					{
						ushort num4 = ((ushort*)((byte*)ld.pvSrc + (nint)(k * (ld.Pitch >> 1)) * (nint)2))[l];
						int num5 = 0;
						int num6 = 0;
						int num7 = 0;
						int num8 = 0;
						for (int m = -1; m <= 1; m++)
						{
							for (int num9 = -1; num9 <= 1; num9++)
							{
								if (Animations.GetPixelType(ld, l + num9, k + m) == PixelType.Inner)
								{
									ushort* ptr2 = (ushort*)((byte*)ld.pvSrc + (nint)((k + m) * (ld.Pitch >> 1)) * (nint)2) + (l + num9);
									num5 += (*ptr2 >> 10) & 0x1F;
									num6 += (*ptr2 >> 5) & 0x1F;
									num7 += *ptr2 & 0x1F;
									num8++;
								}
							}
						}
						if (num8 > 0)
						{
							num5 = num5 * 255 / (31 * num8);
							num6 = num6 * 255 / (31 * num8);
							num7 = num7 * 255 / (31 * num8);
							float num10 = (float)(Engine.GrayScale(num4) * 255) / 31f / Engine.GrayScale(num5, num6, num7);
							int num11 = (int)((1f + 2f * num10) / 3f * 255f);
							if (num11 < 0)
							{
								num11 = 0;
							}
							else if (num11 > 255)
							{
								num11 = 255;
							}
							int* ptr3 = (int*)((byte*)lockData.pvSrc + (nint)(k * (lockData.Pitch >> 2)) * (nint)4) + l;
							*ptr3 = (num11 << 24) | Engine.Blend32((num5 << 16) | (num6 << 8) | num7, Engine.C16232(num4), 127);
						}
						else
						{
							int* ptr5 = (int*)((byte*)lockData.pvSrc + (nint)(k * (lockData.Pitch >> 2)) * (nint)4) + l;
							*ptr5 = -16777216 | Engine.C16232(num4);
						}
						break;
					}
					}
				}
			}
			texture2.Unlock();
			texture2.Dispose();
		}
		texture.Unlock();
		texture.SetPriority(0);
		return texture;
	}

	private unsafe Texture CreateShadowTextureFromUooFrame(UooAnimationFrameData frameData, IHue hue)
	{
		Texture texture = new Texture(frameData.Width + 17, frameData.Height + 17, TextureTransparency.Simple);
		if (texture.IsEmpty())
		{
			return Texture.Empty;
		}
		texture._shaderData = hue.ShaderData;
		LockData lockData = texture.Lock(LockFlags.WriteOnly);
		ushort* pvSrc = (ushort*)lockData.pvSrc;
		int num = lockData.Pitch >> 1;
		int num2 = num * lockData.Height;
		for (int i = 0; i < num2; i++)
		{
			pvSrc[i] = 0;
		}
		fixed (ushort* guassianBlurMatrix = Animations._guassianBlurMatrix)
		{
			for (int j = 0; j < frameData.Height; j++)
			{
				int num3 = j * frameData.Width;
				for (int k = 0; k < frameData.Width; k++)
				{
					if (frameData.Pixels[num3 + k] == 0)
					{
						continue;
					}
					ushort* ptr = guassianBlurMatrix;
					for (int l = -8; l <= 8; l++)
					{
						ushort* ptr2 = pvSrc + (j + 8 + l) * num + k;
						ushort* ptr3 = ptr2 + 17;
						while (ptr2 < ptr3)
						{
							*ptr2 += *(ptr++);
							ptr2++;
						}
					}
				}
			}
		}
		ushort* ptr4 = pvSrc;
		ushort* ptr5 = pvSrc + num2;
		while (ptr4 < ptr5)
		{
			int num4 = *ptr4 * 31 / 22409;
			*ptr4 = (ushort)(0x8000 | (num4 << 10) | (num4 << 5) | num4);
			ptr4++;
		}
		texture.Unlock();
		texture.SetPriority(0);
		return texture;
	}

	private unsafe static PixelType GetPixelType(LockData ld, int x, int y)
	{
		if (x < 0 || x >= ld.Width || y < 0 || y >= ld.Height)
		{
			return PixelType.None;
		}
		ushort* ptr = (ushort*)((byte*)ld.pvSrc + (nint)(y * (ld.Pitch >> 1)) * (nint)2) + x;
		if ((*ptr & 0x8000) == 0)
		{
			return PixelType.None;
		}
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (j == 0 || i == 0)
				{
					if (x + j < 0 || x + j >= ld.Width || y + i < 0 || y + i >= ld.Height)
					{
						return PixelType.Outer;
					}
					ptr = (ushort*)((byte*)ld.pvSrc + (nint)((y + i) * (ld.Pitch >> 1)) * (nint)2) + (x + j);
					if ((*ptr & 0x8000) == 0)
					{
						return PixelType.Outer;
					}
				}
			}
		}
		return PixelType.Inner;
	}

	public void FullCleanup(int timeNow)
	{
		int num = timeNow - 15000;
		for (int num2 = this.m_Frames.Count - 1; num2 >= 0; num2--)
		{
			Frames frames = this.m_Frames[num2];
			if (frames.Disposed || frames.LastAccessTime < num)
			{
				TextureFactory.m_Disposing.Enqueue(frames);
			}
		}
	}

	public void Dispose()
	{
		Animations.m_Loader.Stop();
		Animations.m_Loader2.Stop();
		Animations.m_Loader3.Stop();
		Animations.m_Loader4.Stop();
		Animations.m_Loader5.Stop();
		if (Animations.m_Stream != null)
		{
			Animations.m_Stream.Close();
			Animations.m_Stream = null;
		}
		if (Animations.m_Stream2 != null)
		{
			Animations.m_Stream2.Close();
			Animations.m_Stream2 = null;
		}
		if (Animations.m_Stream3 != null)
		{
			Animations.m_Stream3.Close();
			Animations.m_Stream3 = null;
		}
		if (Animations.m_Stream4 != null)
		{
			Animations.m_Stream4.Close();
			Animations.m_Stream4 = null;
		}
		if (Animations.m_Stream5 != null)
		{
			Animations.m_Stream5.Close();
			Animations.m_Stream5 = null;
		}
		if (this.m_MountTable != null)
		{
			this.m_MountTable.Dispose();
			this.m_MountTable = null;
		}
		this.m_Table = null;
		this.m_Data = null;
		this.m_Index = null;
		this.m_Palette = null;
	}

	public Entry3D[] GetIndex(int index)
	{
		return index switch
		{
			2 => this.m_Index2, 
			3 => this.m_Index3, 
			4 => this.m_Index4, 
			5 => this.m_Index5, 
			_ => this.m_Index, 
		};
	}

	public int GetCount(int index)
	{
		return index switch
		{
			2 => this.m_Count2, 
			3 => this.m_Count3, 
			4 => this.m_Count4, 
			5 => this.m_Count5, 
			_ => this.m_Count, 
		};
	}

	public int GetHeight(int bodyID, int actionID, int direction)
	{
		direction &= 7;
		int realID = this.GetRealID(bodyID, actionID, direction);
		int index = this.ConvertRealID(ref realID);
		Entry3D[] index2 = this.GetIndex(index);
		int count = this.GetCount(index);
		if (realID < 0 || realID >= count || realID >= index2.Length)
		{
			return 0;
		}
		return (index2[realID].m_Extra & -256) >> 8;
	}

	public int GetHeight(int realID)
	{
		int index = this.ConvertRealID(ref realID);
		Entry3D[] index2 = this.GetIndex(index);
		int count = this.GetCount(index);
		if (realID < 0 || realID >= count || realID >= index2.Length)
		{
			return 0;
		}
		return (index2[realID].m_Extra & -256) >> 8;
	}

	public int GetFrameCount(int bodyID, int actionID, int direction)
	{
		direction &= 7;
		int realID = this.GetRealID(bodyID, actionID, direction);
		int index = this.ConvertRealID(ref realID);
		Entry3D[] index2 = this.GetIndex(index);
		int count = this.GetCount(index);
		if (realID < 0 || realID >= count || realID >= index2.Length)
		{
			return 0;
		}
		return index2[realID].m_Extra & 0xFF;
	}

	public int GetFrameCount(int realID)
	{
		int index = this.ConvertRealID(ref realID);
		Entry3D[] index2 = this.GetIndex(index);
		int count = this.GetCount(index);
		if (realID < 0 || realID >= count || realID >= index2.Length)
		{
			return 0;
		}
		return index2[realID].m_Extra & 0xFF;
	}

	static Animations()
	{
		Animations._guassianBlurMatrix = new ushort[289]
		{
			0, 11, 21, 31, 38, 45, 50, 53, 53, 53,
			50, 45, 38, 31, 21, 11, 0, 11, 23, 34,
			44, 53, 60, 65, 68, 69, 68, 65, 60, 53,
			44, 34, 23, 11, 21, 34, 46, 57, 66, 74,
			80, 84, 85, 84, 80, 74, 66, 57, 46, 34,
			21, 31, 44, 57, 68, 79, 88, 95, 100, 101,
			100, 95, 88, 79, 68, 57, 44, 31, 38, 53,
			66, 79, 91, 101, 110, 116, 117, 116, 110, 101,
			91, 79, 66, 53, 38, 45, 60, 74, 88, 101,
			114, 124, 131, 133, 131, 124, 114, 101, 88, 74,
			60, 45, 50, 65, 80, 95, 110, 124, 136, 146,
			149, 146, 136, 124, 110, 95, 80, 65, 50, 53,
			68, 84, 100, 116, 131, 146, 159, 165, 159, 146,
			131, 116, 100, 84, 68, 53, 53, 69, 85, 101,
			117, 133, 149, 165, 181, 165, 149, 133, 117, 101,
			85, 69, 53, 53, 68, 84, 100, 116, 131, 146,
			159, 165, 159, 146, 131, 116, 100, 84, 68, 53,
			50, 65, 80, 95, 110, 124, 136, 146, 149, 146,
			136, 124, 110, 95, 80, 65, 50, 45, 60, 74,
			88, 101, 114, 124, 131, 133, 131, 124, 114, 101,
			88, 74, 60, 45, 38, 53, 66, 79, 91, 101,
			110, 116, 117, 116, 110, 101, 91, 79, 66, 53,
			38, 31, 44, 57, 68, 79, 88, 95, 100, 101,
			100, 95, 88, 79, 68, 57, 44, 31, 21, 34,
			46, 57, 66, 74, 80, 84, 85, 84, 80, 74,
			66, 57, 46, 34, 21, 11, 23, 34, 44, 53,
			60, 65, 68, 69, 68, 65, 60, 53, 44, 34,
			23, 11, 0, 11, 21, 31, 38, 45, 50, 53,
			53, 53, 50, 45, 38, 31, 21, 11, 0
		};
	}
}
