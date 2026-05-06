namespace UOAIO;

public sealed class Entity : IEntity
{
	private int m_Serial;

	public int Serial => this.m_Serial;

	public Entity(int serial)
	{
		this.m_Serial = serial;
	}

	public static implicit operator Entity(int serial)
	{
		return new Entity(serial);
	}
}
