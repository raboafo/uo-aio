using System.Collections.Generic;

namespace UOAIO;

public class ObjectPropertyList
{
	private int m_Serial;

	private int m_Number;

	private ObjectProperty[] m_Props;

	private static Dictionary<long, ObjectPropertyList> m_Table;

	public int Serial => this.m_Serial;

	public int Number => this.m_Number;

	public ObjectProperty[] Properties => this.m_Props;

	public static long GetKey(int serial, int number)
	{
		return ((long)serial << 32) | (uint)number;
	}

	public ObjectPropertyList(int serial, int number, ObjectProperty[] props)
	{
		this.m_Serial = serial;
		this.m_Number = number;
		this.m_Props = props;
		ObjectPropertyList.m_Table[ObjectPropertyList.GetKey(serial, number)] = this;
	}

	public static ObjectPropertyList Find(int serial, int number)
	{
		ObjectPropertyList value = null;
		ObjectPropertyList.m_Table.TryGetValue(ObjectPropertyList.GetKey(serial, number), out value);
		return value;
	}

	static ObjectPropertyList()
	{
		ObjectPropertyList.m_Table = new Dictionary<long, ObjectPropertyList>();
	}
}
