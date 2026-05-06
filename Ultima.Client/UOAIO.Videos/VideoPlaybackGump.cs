using System;
using System.Drawing;

namespace UOAIO.Videos;

public sealed class VideoPlaybackGump : Gump
{
	private sealed class VideoPlaybackButton : GButtonNew
	{
		private VideoPlaybackGump _owner;

		private int _type;

		private Texture[] _textures;

		private Texture _activeIcon;

		private static PlaybackState[] states;

		public VideoPlaybackButton(VideoPlaybackGump owner, Bitmap buttonSource, Bitmap iconSource, int type)
			: base(0, 0, 0, 0, 0)
		{
			this._owner = owner;
			this._type = type;
			this._textures = new Texture[3];
			int num = type * 30;
			int num2 = 40;
			this.X = 6 + num;
			this.Y = 94;
			int[] array = new int[3] { 40, 120, 80 };
			for (int i = 0; i < this._textures.Length; i++)
			{
				num2 = array[i];
				using Bitmap bitmap = new Bitmap(30, 30);
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					if (num2 != 0)
					{
						graphics.DrawImage(buttonSource, new Rectangle(0, 0, 30, 30), new Rectangle(num, 0, 30, 30), GraphicsUnit.Pixel);
					}
					graphics.DrawImage(buttonSource, new Rectangle(0, 0, 30, 30), new Rectangle(num, num2, 30, 30), GraphicsUnit.Pixel);
				}
				this._textures[i] = Texture.FromBitmap(bitmap);
			}
			using Bitmap bitmap2 = new Bitmap(30, 30);
			using (Graphics graphics2 = Graphics.FromImage(bitmap2))
			{
				graphics2.DrawImage(iconSource, new Rectangle(0, 0, 30, 30), new Rectangle(type * 30, (type - 1) * 40, 30, 30), GraphicsUnit.Pixel);
			}
			this._activeIcon = Texture.FromBitmap(bitmap2);
		}

		protected override void OnClicked()
		{
			base.OnClicked();
			switch (this._type)
			{
			case 1:
				this._owner._video.Play();
				break;
			case 2:
				this._owner._video.Pause();
				break;
			case 3:
				this._owner._video.Stop();
				break;
			}
		}

		protected internal override void Draw(int x, int y)
		{
			base.Draw(x, y);
			if (base.m_State < 2 && this._owner._video.State == VideoPlaybackButton.states[this._type - 1])
			{
				this._activeIcon.Draw(x, y);
			}
		}

		protected override void Refresh()
		{
			base.m_Image = this._textures[base.m_State];
			if (base.m_Image != null && !base.m_Image.IsEmpty())
			{
				base.m_Width = base.m_Image.Width;
				base.m_Height = base.m_Image.Height;
				base.m_Draw = true;
			}
			else
			{
				base.m_Width = 0;
				base.m_Height = 0;
				base.m_Draw = false;
			}
			base.m_Invalidated = false;
			if (base.m_vCache != null)
			{
				base.m_vCache.Invalidate();
			}
		}

		protected internal override void OnDispose()
		{
			base.OnDispose();
			if (this._textures != null)
			{
				for (int i = 0; i < this._textures.Length; i++)
				{
					this._textures[i].Dispose();
				}
				this._textures = null;
			}
			if (this._activeIcon != null)
			{
				this._activeIcon.Dispose();
				this._activeIcon = null;
			}
		}

		static VideoPlaybackButton()
		{
			VideoPlaybackButton.states = new PlaybackState[3]
			{
				PlaybackState.Playing,
				PlaybackState.Paused,
				PlaybackState.Stopped
			};
		}
	}

	private sealed class VideoTimeLabel : Gump
	{
		private VideoPlaybackGump _owner;

		private Texture[] _textures;

		private int[] digits;

		private static int[] mods;

		public override int Width
		{
			get
			{
				return 100;
			}
			set
			{
			}
		}

		public override int Height
		{
			get
			{
				return 20;
			}
			set
			{
			}
		}

		public VideoTimeLabel(VideoPlaybackGump owner)
			: base(36, 24)
		{
			this._owner = owner;
			this._textures = Texture.FromImageSet("play/images/video-time-font.png", 13, 20, 16, 1);
			int num = (int)Math.Ceiling(this._owner._video.Duration.TotalSeconds);
			int num2 = 3;
			num /= 60;
			if (num > 9)
			{
				num2++;
				num /= 60;
				if (num > 0)
				{
					num2 += (num + 9) / 10;
				}
			}
			this.digits = new int[num2];
		}

		protected internal override void Draw(int X, int Y)
		{
			TimeSpan timeSpan = ((this._owner._video.State != PlaybackState.Stopped) ? this._owner._video.Elapsed : this._owner._video.Duration);
			if (timeSpan < TimeSpan.Zero)
			{
				timeSpan = TimeSpan.Zero;
			}
			int num = (int)timeSpan.TotalSeconds;
			for (int i = 0; i < this.digits.Length; i++)
			{
				int num2 = ((i < VideoTimeLabel.mods.Length) ? VideoTimeLabel.mods[i] : 10);
				this.digits[i] = num % num2;
				num /= num2;
			}
			int num3 = this.digits.Length * 13;
			if (this.digits.Length > 2)
			{
				num3 += 4;
				if (this.digits.Length > 4)
				{
					num3 += 4;
				}
			}
			int num4 = num3;
			for (int j = 0; j < this.digits.Length; j++)
			{
				if (j == 2 || j == 4)
				{
					Texture texture = this._textures[12];
					num4 -= 5;
					texture.Draw(X + num4, Y);
				}
				Texture texture2 = this._textures[this.digits[j]];
				num4 -= 13;
				texture2.Draw(X + num4, Y);
			}
		}

		protected internal override void OnDispose()
		{
			base.OnDispose();
			if (this._textures != null)
			{
				for (int i = 0; i < this._textures.Length; i++)
				{
					this._textures[i].Dispose();
				}
				this._textures = null;
			}
		}

		private Texture Extract(Bitmap source, int x, int y, int width, int height)
		{
			using Bitmap bitmap = new Bitmap(width, height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.DrawImage(source, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
			}
			return Texture.FromBitmap(bitmap);
		}

		static VideoTimeLabel()
		{
			VideoTimeLabel.mods = new int[4] { 10, 6, 10, 6 };
		}
	}

	private sealed class VideoSpeedBar : GThumbSlider
	{
		private VideoPlaybackGump _owner;

		private Texture[] _textures;

		public override int Width
		{
			get
			{
				return 9;
			}
			set
			{
			}
		}

		public override int Height
		{
			get
			{
				return 76;
			}
			set
			{
			}
		}

		public VideoSpeedBar(VideoPlaybackGump owner)
			: base(343, 11, SliderOrientation.Vertical)
		{
			base.Minimum = -5;
			base.Maximum = 5;
			this._owner = owner;
			this._textures = new Texture[2];
			using Bitmap source = Engine.LoadArchivedBitmap("video-rate-slider.png");
			this._textures[0] = this.Extract(source, 0, 0, 16, 23);
			this._textures[1] = this.Extract(source, 18, 0, 16, 23);
		}

		private Texture Extract(Bitmap source, int x, int y, int width, int height)
		{
			using Bitmap bitmap = new Bitmap(width, height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.DrawImage(source, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
			}
			return Texture.FromBitmap(bitmap);
		}

		protected override void OnChanged(int oldValue)
		{
			base.OnChanged(oldValue);
			int num = -base.Value;
			if (num >= 0)
			{
				this._owner._video.UpdateSpeed(1 + num, 1);
			}
			else
			{
				this._owner._video.UpdateSpeed(1, 1 - num);
			}
		}

		protected override int GetThumbSize()
		{
			return 19;
		}

		protected override void DrawThumb(int x, int y, int p)
		{
			this._textures[0].Draw(x - 2, y + p - 1);
		}

		protected internal override void OnDispose()
		{
			base.OnDispose();
			if (this._textures != null)
			{
				for (int i = 0; i < this._textures.Length; i++)
				{
					this._textures[i].Dispose();
				}
				this._textures = null;
			}
		}
	}

	private sealed class VideoProgressBar : Gump
	{
		private VideoPlaybackGump _owner;

		private Texture[] _textures;

		public override int Width
		{
			get
			{
				return 301;
			}
			set
			{
			}
		}

		public override int Height
		{
			get
			{
				return 4;
			}
			set
			{
			}
		}

		public VideoProgressBar(VideoPlaybackGump owner)
			: base(9, 82)
		{
			this._owner = owner;
			this._textures = new Texture[3];
			using Bitmap bitmap = Engine.LoadArchivedBitmap("video-progress.png");
			this._textures[0] = this.Extract(bitmap, 0, 0, 2, bitmap.Height);
			this._textures[1] = this.Extract(bitmap, 2, 0, bitmap.Width - 4, bitmap.Height);
			this._textures[2] = this.Extract(bitmap, bitmap.Width - 2, 0, 2, bitmap.Height);
		}

		private Texture Extract(Bitmap source, int x, int y, int width, int height)
		{
			using Bitmap bitmap = new Bitmap(width, height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.DrawImage(source, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
			}
			return Texture.FromBitmap(bitmap);
		}

		protected internal override void Draw(int x, int y)
		{
			Playback video = this._owner._video;
			int num = (int)(video.Elapsed.Ticks * this.Width / video.Duration.Ticks);
			if (num > this.Width)
			{
				num = this.Width;
			}
			if (num > 4)
			{
				this._textures[0].Draw(x, y);
				this._textures[1].Draw(x + 2, y, num - 4, this._textures[1].Height);
				this._textures[2].Draw(x + num - 2, y);
			}
		}

		protected internal override void OnDispose()
		{
			base.OnDispose();
			if (this._textures != null)
			{
				for (int i = 0; i < this._textures.Length; i++)
				{
					this._textures[i].Dispose();
				}
				this._textures = null;
			}
		}
	}

	private Playback _video;

	private Texture _texture;

	public override int Width
	{
		get
		{
			return (this._texture != null) ? this._texture.Width : 0;
		}
		set
		{
		}
	}

	public override int Height
	{
		get
		{
			return (this._texture != null) ? this._texture.Height : 0;
		}
		set
		{
		}
	}

	public VideoPlaybackGump(Playback video)
		: base(0, 0)
	{
		this._video = video;
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		this._texture = Engine.LoadArchivedTexture("video-window.png");
		base.Children.Add(new VideoSpeedBar(this));
		base.Children.Add(new VideoTimeLabel(this));
		base.Children.Add(new VideoProgressBar(this));
		using Bitmap buttonSource = Engine.LoadArchivedBitmap("video-buttons.png");
		using Bitmap iconSource = Engine.LoadArchivedBitmap("video-button-status.png");
		for (int i = 1; i < 4; i++)
		{
			VideoPlaybackButton toAdd = new VideoPlaybackButton(this, buttonSource, iconSource, i);
			base.Children.Add(toAdd);
		}
	}

	protected internal override void Draw(int X, int Y)
	{
		this._texture.Draw(X, Y);
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return X >= 0 && Y >= 0 && X < this.Width && Y < this.Height;
	}

	protected internal override void OnDispose()
	{
		base.OnDispose();
		if (this._texture != null)
		{
			this._texture.Dispose();
			this._texture = null;
		}
		if (this._video != null)
		{
			this._video.Dispose();
			this._video = null;
		}
	}
}
