using System.IO;
using System.Text;

namespace UOAIO;

public class Skills
{
	private Skill[] m_Skills;

	private SkillGroup[] m_Groups;

	public const int Count = 256;

	public const int Mask = 255;

	public SkillGroup[] Groups => this.m_Groups;

	public Skill this[SkillName name]
	{
		get
		{
			if (name < SkillName.Alchemy || (int)name >= this.m_Skills.Length)
			{
				return null;
			}
			return this.m_Skills[(int)name];
		}
	}

	public Skill this[int SkillID]
	{
		get
		{
			if (SkillID < 0 || SkillID >= this.m_Skills.Length)
			{
				return null;
			}
			return this.m_Skills[SkillID];
		}
		set
		{
			this.m_Skills[SkillID] = value;
		}
	}

	public int GetSkill(string Name)
	{
		int num = -1;
		while (++num < 256)
		{
			if (this.m_Skills[num].Name == Name)
			{
				return num;
			}
		}
		return -1;
	}

	public unsafe Skills()
	{
		byte[] array = new byte[3072];
		Stream stream = Engine.FileManager.OpenMUL(Files.SkillIdx);
		UnsafeMethods.ReadFile((FileStream)stream, array, 0, 3072);
		stream.Close();
		byte[] array2 = null;
		Stream stream2 = Engine.FileManager.OpenMUL(Files.SkillMul);
		array2 = new byte[stream2.Length];
		UnsafeMethods.ReadFile((FileStream)stream2, array2, 0, array2.Length);
		stream2.Close();
		fixed (byte* ptr = array)
		{
			int* ptr2 = (int*)ptr;
			fixed (byte* ptr3 = array2)
			{
				this.m_Skills = new Skill[256];
				int num = 0;
				while (num < 256)
				{
					int num2 = *ptr2;
					if (num2 < 0)
					{
						ptr2 += 3;
						num++;
						continue;
					}
					byte* ptr4 = ptr3 + num2;
					int num3 = ptr2[1];
					if (num3 < 1)
					{
						ptr2 += 3;
						num++;
						continue;
					}
					bool action = *(ptr4++) != 0;
					StringBuilder stringBuilder;
					if (num3 < 1)
					{
						stringBuilder = new StringBuilder();
					}
					else
					{
						num3--;
						stringBuilder = new StringBuilder(num3);
						for (int i = 0; i < num3 && ptr4[i] != 0; i++)
						{
							stringBuilder.Append((char)ptr4[i]);
						}
					}
					this.m_Skills[num] = new Skill(num, action, stringBuilder.ToString());
					ptr2 += 3;
					num++;
				}
			}
		}
		string path = Engine.FileManager.ResolveMUL("SkillGrp.mul");
		if (File.Exists(path))
		{
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			int num4 = binaryReader.ReadInt32();
			bool flag = false;
			if (num4 == -1)
			{
				flag = true;
				num4 = binaryReader.ReadInt32();
			}
			this.m_Groups = new SkillGroup[num4];
			this.m_Groups[0] = new SkillGroup("Miscellaneous", 0);
			for (int j = 1; j < num4; j++)
			{
				fileStream.Seek((flag ? 8 : 4) + (j - 1) * (flag ? 34 : 17), SeekOrigin.Begin);
				StringBuilder stringBuilder2 = new StringBuilder(18);
				if (flag)
				{
					int num5;
					while ((num5 = binaryReader.ReadInt16()) != 0)
					{
						stringBuilder2.Append((char)num5);
					}
				}
				else
				{
					int num5;
					while ((num5 = binaryReader.ReadByte()) != 0)
					{
						stringBuilder2.Append((char)num5);
					}
				}
				this.m_Groups[j] = new SkillGroup(stringBuilder2.ToString(), j);
			}
			fileStream.Seek((flag ? 8 : 4) + (num4 - 1) * (flag ? 34 : 17), SeekOrigin.Begin);
			for (int k = 0; k < 256; k++)
			{
				Skill skill = this.m_Skills[k];
				if (skill != null)
				{
					try
					{
						int num6 = binaryReader.ReadInt32();
						skill.Group = this.m_Groups[num6];
						skill.Group.Skills.Add(skill);
					}
					catch
					{
						break;
					}
					continue;
				}
				break;
			}
			binaryReader.Close();
		}
		else
		{
			this.m_Groups = new SkillGroup[1];
			this.m_Groups[0] = new SkillGroup("Skills", 0);
			for (int l = 0; l < 256; l++)
			{
				this.m_Skills[l].Group = this.m_Groups[0];
			}
		}
	}
}
