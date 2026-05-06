namespace UOAIO;

public interface IRadarTrackable
{
	int X { get; }

	int Y { get; }

	int Facet { get; }

	int Color { get; }

	string Name { get; }

	bool HasExpired { get; }
}
