using System;

namespace UOAIO;

public class PaperdollBody
{
	public static readonly PaperdollBody[] Configuration;

	private int _bodyId;

	private int? _gender;

	private PaperdollType _type;

	private PaperdollImage[] _images;

	public int BodyId => this._bodyId;

	public int? Gender => this._gender;

	public PaperdollType Type => this._type;

	public PaperdollImage[] Images => this._images;

	public static PaperdollBody FromMobile(Mobile mob)
	{
		if (mob == null)
		{
			throw new ArgumentNullException("mob");
		}
		PaperdollBody[] configuration = PaperdollBody.Configuration;
		foreach (PaperdollBody paperdollBody in configuration)
		{
			if (paperdollBody.IsMatch(mob))
			{
				return paperdollBody;
			}
		}
		return null;
	}

	public PaperdollBody(int bodyId, int? gender, PaperdollType type, params PaperdollImage[] images)
	{
		this._bodyId = bodyId;
		this._gender = gender;
		this._type = type;
		this._images = images;
	}

	public bool IsMatch(Mobile mob)
	{
		if (mob.Body == this._bodyId)
		{
			if (this._gender.HasValue && mob.Gender != this._gender.Value)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	static PaperdollBody()
	{
		PaperdollBody.Configuration = new PaperdollBody[6]
		{
			new PaperdollBody(400, null, PaperdollType.Regular, new PaperdollImage(12, null, 1f)),
			new PaperdollBody(401, null, PaperdollType.Regular, new PaperdollImage(13, null, 1f)),
			new PaperdollBody(402, null, PaperdollType.Ghost, new PaperdollImage(12, null, 0.25f)),
			new PaperdollBody(403, null, PaperdollType.Ghost, new PaperdollImage(13, null, 0.25f)),
			new PaperdollBody(987, 0, PaperdollType.Regular, new PaperdollImage(12, 33770, 1f), new PaperdollImage(50987, null, 1f)),
			new PaperdollBody(987, 1, PaperdollType.Regular, new PaperdollImage(13, 33770, 1f), new PaperdollImage(60987, null, 1f))
		};
	}
}
