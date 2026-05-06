using System;
using System.Drawing;

namespace UOAIO;

public class RectangleList
{
	private Rectangle[] m_Rects;

	private int m_Count;

	public int Count => this.m_Count;

	public Rectangle this[int index]
	{
		get
		{
			if (index < 0 || index >= this.m_Count)
			{
				throw new IndexOutOfRangeException();
			}
			return this.m_Rects[index];
		}
	}

	public RectangleList()
	{
		this.m_Rects = new Rectangle[8];
	}

	private static Rectangle[] Punch(Rectangle cookie, Rectangle cutter)
	{
		if (!cookie.IntersectsWith(cutter))
		{
			return new Rectangle[1] { cookie };
		}
		int num = cutter.X - cookie.X;
		int num2 = cutter.Y - cookie.Y;
		int num3 = cookie.X + cookie.Width - (cutter.X + cutter.Width);
		int num4 = cookie.Y + cookie.Height - (cutter.Y + cutter.Height);
		int num5 = 0;
		if (num > 0)
		{
			num5++;
		}
		else
		{
			num = 0;
		}
		if (num2 > 0)
		{
			num5++;
		}
		else
		{
			num2 = 0;
		}
		if (num3 > 0)
		{
			num5++;
		}
		else
		{
			num3 = 0;
		}
		if (num4 > 0)
		{
			num5++;
		}
		else
		{
			num4 = 0;
		}
		Rectangle[] array = new Rectangle[num5];
		num5 = 0;
		if (num > 0)
		{
			array[num5++] = new Rectangle(cookie.X, cookie.Y, num, cookie.Height);
		}
		if (num2 > 0)
		{
			array[num5++] = new Rectangle(cookie.X + num, cookie.Y, cookie.Width - num - num3, num2);
		}
		if (num3 > 0)
		{
			array[num5++] = new Rectangle(cutter.X + cutter.Width, cookie.Y, num3, cookie.Height);
		}
		if (num4 > 0)
		{
			array[num5++] = new Rectangle(cookie.X + num, cutter.Y + cutter.Height, cookie.Width - num - num3, num4);
		}
		return array;
	}

	public void Add(Rectangle rect)
	{
		for (int i = 0; i < this.m_Count; i++)
		{
			Rectangle rectangle = this.m_Rects[i];
			if (rect.IntersectsWith(rectangle))
			{
				Rectangle[] array = RectangleList.Punch(rect, rectangle);
				for (int j = 0; j < array.Length; j++)
				{
					this.Add(array[j]);
				}
				return;
			}
		}
		this.InternalAdd(rect);
	}

	public void Remove(Rectangle rect)
	{
		for (int num = this.m_Count - 1; num >= 0; num--)
		{
			Rectangle rectangle = this.m_Rects[num];
			if (rect.IntersectsWith(rectangle))
			{
				this.InternalRemove(num);
				Rectangle[] array = RectangleList.Punch(rectangle, rect);
				for (int i = 0; i < array.Length; i++)
				{
					this.InternalAdd(array[i]);
				}
			}
		}
	}

	public void Clear()
	{
		this.m_Count = 0;
	}

	private void InternalAdd(Rectangle rect)
	{
		if (this.m_Count >= this.m_Rects.Length)
		{
			Rectangle[] rects = this.m_Rects;
			this.m_Rects = new Rectangle[rects.Length * 2];
			for (int i = 0; i < rects.Length; i++)
			{
				this.m_Rects[i] = rects[i];
			}
		}
		this.m_Rects[this.m_Count++] = rect;
	}

	private void InternalRemove(int index)
	{
		this.m_Count--;
		for (int i = index; i < this.m_Count; i++)
		{
			this.m_Rects[i] = this.m_Rects[i + 1];
		}
	}
}
