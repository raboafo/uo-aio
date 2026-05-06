namespace UOAIO;

public interface IRelayedSwitch
{
	int RelayID { get; }

	bool Active { get; }
}
