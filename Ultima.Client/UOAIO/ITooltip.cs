namespace UOAIO;

public interface ITooltip
{
	float Delay { get; set; }

	Gump GetGump();
}
