namespace UOAIO;

public interface IAnimationOwner
{
	Frames GetOwnedFrames(IHue hue, int realID);
}
