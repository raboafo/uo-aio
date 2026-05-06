namespace UOAIO;

public class Stat
{
	public string Name;

	public int ID;

	public StatLock Lock;

	public Stat(int id, StatLock Lock)
	{
		this.ID = id;
		this.Lock = Lock;
	}

	public Stat()
	{
	}
}
