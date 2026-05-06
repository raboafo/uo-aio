using System;

namespace UOAIO;

public class GMapTracker : GTracker
{
	private static GenericRadarTrackable _trackable;

	public static GenericRadarTrackable Trackable => GMapTracker._trackable;

	protected internal override void Render(int X, int Y)
	{
		if (!GMapTracker._trackable.HasExpired)
		{
			base.Render(X, Y, GMapTracker._trackable.X, GMapTracker._trackable.Y);
		}
	}

	protected override string GetPluralString(string direction, int distance)
	{
		return "Treasure: " + distance + " tiles " + direction;
	}

	protected override string GetSingularString(string direction)
	{
		return "Treasure: 1 tile " + direction;
	}

	static GMapTracker()
	{
		GMapTracker._trackable = new GenericRadarTrackable(TimeSpan.FromMinutes(5.0));
	}
}
